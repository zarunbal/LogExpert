using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace LogExpert
{
	public class ParamParser
	{
		private string argLine;

		public ParamParser(string argTemplate)
		{
			argLine = argTemplate;
		}

		public string ReplaceParams(string logLine, int lineNum, string fileName)
		{
			FileInfo fileInfo = new FileInfo(fileName);
			StringBuilder builder = new StringBuilder(this.argLine);

			builder.Replace("%L", lineNum.ToString());
			builder.Replace("%P", FormatPath(fileInfo.DirectoryName));
			builder.Replace("%N", FormatPath(fileInfo.Name));
			builder.Replace("%F", FormatPath(fileInfo.FullName));
			builder.Replace("%E", FormatPath(fileInfo.FullName));

			string stripped = StripExtension(fileInfo.Name);
			builder.Replace("%M",FormatPath(stripped));
			
			int sPos = 0;
			string reg;
			string replace;
			
			do
			{
				reg = GetNextGroup(builder, ref sPos);
				replace = GetNextGroup(builder, ref sPos);
				if (reg != null && replace != null)
				{
					string result = Regex.Replace(logLine, reg, replace);
					builder.Insert(sPos, result);
				}
			}
			while (replace != null);
			
			return builder.ToString();
		}

		private string GetNextGroup(StringBuilder builder, ref int sPos)
		{
			int count = 0;
			int ePos;

			while (sPos < builder.Length)
			{
				if (builder[sPos] == '{')
				{
					ePos = sPos + 1;
					count = 1;

					while (ePos < builder.Length)
					{
						if (builder[ePos] == '{')
						{
							count++;
						}
						if (builder[ePos] == '}')
						{
							count--;
						}
						if (count == 0)
						{
							string reg = builder.ToString(sPos + 1, ePos - sPos - 1);
							builder.Remove(sPos, ePos - sPos + 1);
							return reg;
						}
						ePos++;
					}
				}
				sPos++;
			}
			return null;
		}

		private static string StripExtension(string fileName)
		{
			int i = fileName.LastIndexOf('.');
			if (i < 0)
			{
				i = fileName.Length - 1;
			}
			return fileName.Substring(0, i);
		}

		private static string FormatPath(string path)
		{
			if (path.Contains(" "))
			{
				return string.Format("\"{0}\"", path);
			}
			return	path;
		}

	}
}