using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;

namespace LogExpert
{
	public class Util
	{
		public static string GetNameFromPath(string fileName)
		{
			int i = fileName.LastIndexOf('\\');
			if (i < 0)
			{
				i = fileName.LastIndexOf('/');
			}
			if (i < 0)
			{
				i = -1;
			}
			return fileName.Substring(i + 1);
		}

		public static string StripExtension(string fileName)
		{
			int i = fileName.LastIndexOf('.');
			if (i < 0)
			{
				i = fileName.Length - 1;
			}
			return fileName.Substring(0, i);
		}

		public static string GetExtension(string fileName)
		{
			int i = fileName.LastIndexOf('.');
			if (i < 0 || i >= fileName.Length - 1)
			{
				return "";
			}
			else
			{
				return fileName.Substring(i + 1);
			}
		}

		public static string GetFileSizeAsText(long size)
		{
			if (size < 1024)
			{
				return "" + size + " bytes";
			}
			else if (size < 1024 * 1024)
			{
				return "" + (size / 1024) + " KB";
			}
			else
			{
				return "" + String.Format("{0:0.00}", ((double)size / 1048576.0)) + " MB";
			}
		}
		
		public static ILogLineColumnizer FindColumnizerByName(string name, IList<ILogLineColumnizer> list)
		{
			foreach (ILogLineColumnizer columnizer in list)
			{
				if (columnizer.GetName().Equals(name))
				{
					return columnizer;
				}
			}
			return null;
		}
		
		public static ILogLineColumnizer CloneColumnizer(ILogLineColumnizer columnizer)
		{
			if (columnizer == null)
			{
				return null;
			}
			ConstructorInfo cti = columnizer.GetType().GetConstructor(Type.EmptyTypes);
			if (cti != null)
			{
				object o = cti.Invoke(new object[] { });
				if (o is IColumnizerConfigurator)
				{
					((IColumnizerConfigurator)o).LoadConfig(ConfigManager.ConfigDir);
				}
				return (ILogLineColumnizer)o;
			}
			return null;
		}
		
		/// <summary>
		/// Returns true, if the given string is null or empty or contains only spaces
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		public static bool IsNullOrSpaces(string toTest)
		{
			return toTest == null || toTest.Trim().Length == 0;
		}
		
		[Conditional("DEBUG")]
		public static void AssertTrue(bool condition, string msg)
		{
			if (!condition)
			{
				MessageBox.Show("Assertion: " + msg);
				throw new Exception(msg);
			}
		}
		
		public string GetWordFromPos(int xPos, string text, Graphics g, Font font)
		{
			string[] words = text.Split(new char[] { ' ', '.', ':', ';' });
			int i = 0;
			int index = 0;
			List<CharacterRange> crList = new List<CharacterRange>();
			for (i = 0; i < words.Length; ++i)
			{
				crList.Add(new CharacterRange(index, words[i].Length));
				index += words[i].Length;
			}
			CharacterRange[] crArray = crList.ToArray();
			StringFormat stringFormat = new StringFormat(StringFormat.GenericTypographic);
			stringFormat.Trimming = StringTrimming.None;
			stringFormat.FormatFlags = StringFormatFlags.NoClip;
			stringFormat.SetMeasurableCharacterRanges(crArray);
			RectangleF rect = new RectangleF(0, 0, 3000, 20);
			Region[] stringRegions = g.MeasureCharacterRanges(text,
				font, rect, stringFormat);
			bool found = false;
			i = 0;
			foreach (Region regio in stringRegions)
			{
				if (regio.IsVisible(xPos, 3, g))
				{
					found = true;
					break;
				}
				i++;
			}
			if (found)
			{
				return words[i];
			}
			else
			{
				return null;
			}
		}
	}
}