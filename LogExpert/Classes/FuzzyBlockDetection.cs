using LogExpert.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogExpert.Classes
{
	public class FuzzyBlockDetection
	{
		private static readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly List<int> _lineHashList = new List<int>();

		public void TestStatistic(PatternArgs patternArgs, ILogWindowSearch logWindow)
		{
			int beginLine = patternArgs.startLine;
			_logger.Info("TestStatistics() called with start line " + beginLine);

			logWindow.PatternArgs = patternArgs;

			int num = beginLine + 1;

			logWindow.ProgressEventArgs.MinValue = 0;
			logWindow.ProgressEventArgs.MaxValue = logWindow.DataGridView.RowCount;
			logWindow.ProgressEventArgs.Value = beginLine;
			logWindow.ProgressEventArgs.Visible = true;
			logWindow.SendProgressBarUpdate();

			PrepareDict(logWindow);

			Dictionary<int, int> processedLinesDict = new Dictionary<int, int>();
			List<PatternBlock> blockList = new List<PatternBlock>();
			int blockId = 0;
			logWindow.IsSearching = true;
			logWindow.ShouldCancel = false;
			int searchLine = -1;
			for (int i = beginLine; i < num && !logWindow.ShouldCancel; ++i)
			{
				if (processedLinesDict.ContainsKey(i))
				{
					continue;
				}

				PatternBlock block;
				int maxBlockLen = patternArgs.endLine - patternArgs.startLine;
				_logger.Debug("TestStatistic(): i=" + i + " searchLine=" + searchLine);
				searchLine++;
				logWindow.UpdateProgressBar(searchLine);
				while (!logWindow.ShouldCancel && (block = DetectBlock(i, searchLine, maxBlockLen, logWindow.PatternArgs.maxDiffInBlock, logWindow.PatternArgs.maxMisses, processedLinesDict, logWindow)) != null)
				{
					_logger.Debug("Found block: " + block);
					if (block.Weigth >= logWindow.PatternArgs.minWeight)
					{
						blockList.Add(block);
						AddBlockTargetLinesToDict(processedLinesDict, block);
						block.BlockId = blockId;

						searchLine = block.TargetEnd + 1;
					}
					else
					{
						searchLine = block.TargetStart + 1;
					}
					logWindow.UpdateProgressBar(searchLine);
				}
				blockId++;
			}
			logWindow.IsSearching = false;
			logWindow.ProgressEventArgs.MinValue = 0;
			logWindow.ProgressEventArgs.MaxValue = 0;
			logWindow.ProgressEventArgs.Value = 0;
			logWindow.ProgressEventArgs.Visible = false;
			logWindow.SendProgressBarUpdate();
			logWindow.PatternWindow.SetBlockList(blockList, logWindow.PatternArgs);
			_logger.Info("TestStatistics() ended");
		}

		private void AddBlockTargetLinesToDict(Dictionary<int, int> dict, PatternBlock block)
		{
			foreach (int lineNum in block.TargetLines.Keys)
			{
				if (!dict.ContainsKey(lineNum))
				{
					dict.Add(lineNum, lineNum);
				}
			}
		}

		private PatternBlock DetectBlock(int startNum, int startLineToSearch, int maxBlockLen, int maxDiffInBlock, int maxMisses, Dictionary<int, int> processedLinesDict, ILogWindowSearch logWindow)
		{
			int targetLine = FindSimilarLine(startNum, startLineToSearch, processedLinesDict, logWindow);
			if (targetLine == -1)
			{
				return null;
			}

			PatternBlock block = new PatternBlock();
			block.StartLine = startNum;
			int srcLine = block.StartLine;
			block.TargetStart = targetLine;
			int srcMisses = 0;
			block.SrcLines.Add(srcLine, srcLine);
			int len = 0;
			QualityInfo qi = new QualityInfo();
			qi.Quality = block.Weigth;
			block.QualityInfoList[targetLine] = qi;

			while (!logWindow.ShouldCancel)
			{
				srcLine++;
				len++;
				if (maxBlockLen > 0 && len > maxBlockLen)
				{
					break;
				}
				int nextTargetLine = FindSimilarLine(srcLine, targetLine + 1, processedLinesDict, logWindow);
				if (nextTargetLine > -1 && nextTargetLine - targetLine - 1 <= maxDiffInBlock)
				{
					block.Weigth += maxDiffInBlock - (nextTargetLine - targetLine - 1) + 1;
					block.EndLine = srcLine;
					block.SrcLines.Add(srcLine, srcLine);
					if (nextTargetLine - targetLine > 1)
					{
						int tempWeight = block.Weigth;
						for (int tl = targetLine + 1; tl < nextTargetLine; ++tl)
						{
							qi = new QualityInfo();
							qi.Quality = --tempWeight;
							block.QualityInfoList[tl] = qi;
						}
					}
					targetLine = nextTargetLine;
					qi = new QualityInfo();
					qi.Quality = block.Weigth;
					block.QualityInfoList[targetLine] = qi;
				}
				else
				{
					srcMisses++;
					block.Weigth--;
					targetLine++;
					qi = new QualityInfo();
					qi.Quality = block.Weigth;
					block.QualityInfoList[targetLine] = qi;
					if (srcMisses > maxMisses)
					{
						break;
					}
				}
			}
			block.TargetEnd = targetLine;
			qi = new QualityInfo();
			qi.Quality = block.Weigth;
			block.QualityInfoList[targetLine] = qi;
			for (int k = block.TargetStart; k <= block.TargetEnd; ++k)
			{
				block.TargetLines.Add(k, k);
			}
			return block;
		}

		private void PrepareDict(ILogWindowSearch logWindow)
		{
			this._lineHashList.Clear();
			Regex regex = new Regex("\\d");
			Regex regex2 = new Regex("\\S");

			int num = logWindow.LineCount;
			for (int i = 0; i < num; ++i)
			{
				string msg = GetMsgForLine(i, logWindow);
				if (msg != null)
				{
					msg = msg.ToLower();
					msg = regex.Replace(msg, "0");
					msg = regex2.Replace(msg, " ");
					char[] chars = msg.ToCharArray();
					int value = 0;
					int numOfE = 0;
					int numOfA = 0;
					int numOfI = 0;
					for (int j = 0; j < chars.Length; ++j)
					{
						value += chars[j];
						switch (chars[j])
						{
							case 'e':
								numOfE++;
								break;

							case 'a':
								numOfA++;
								break;

							case 'i':
								numOfI++;
								break;
						}
					}
					value += numOfE * 30;
					value += numOfA * 20;
					value += numOfI * 10;
					_lineHashList.Add(value);
				}
			}
		}

		private int FindSimilarLine(int srcLine, int startLine, Dictionary<int, int> processedLinesDict, ILogWindowSearch logWindow)
		{
			int threshold = logWindow.PatternArgs.fuzzy;

			bool prepared = false;
			Regex regex = null;
			Regex regex2 = null;
			string msgToFind = null;
			CultureInfo culture = CultureInfo.CurrentCulture;

			int num = logWindow.LineCount;
			for (int i = startLine; i < num; ++i)
			{
				if (processedLinesDict.ContainsKey(i))
				{
					continue;
				}

				if (!prepared)
				{
					msgToFind = GetMsgForLine(srcLine, logWindow);
					regex = new Regex("\\d");
					regex2 = new Regex("\\W");
					msgToFind = msgToFind.ToLower(culture);
					msgToFind = regex.Replace(msgToFind, "0");
					msgToFind = regex2.Replace(msgToFind, " ");
					prepared = true;
				}
				string msg = GetMsgForLine(i, logWindow);
				if (msg != null)
				{
					msg = regex.Replace(msg, "0");
					msg = regex2.Replace(msg, " ");
					int lenDiff = Math.Abs(msg.Length - msgToFind.Length);
					if (lenDiff > threshold)
					{
						continue;
					}
					msg = msg.ToLower(culture);
					int distance = Classes.YetiLevenshtein.Distance(msgToFind, msg);
					if (distance < threshold)
					{
						return i;
					}
				}
			}
			return -1;
		}

		private string GetMsgForLine(int i, ILogWindowSearch logWindow)
		{
			string line = logWindow.GetLogLine(i);
			ILogLineColumnizer columnizer = logWindow.CurrentColumnizer;
			ColumnizerCallback callback = new ColumnizerCallback(logWindow.CurrentLogWindows);
			string[] cols = columnizer.SplitLine(callback, line);
			return cols[columnizer.GetColumnCount() - 1];
		}
	}
}