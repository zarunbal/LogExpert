using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using LogExpert.Classes;
using System.Linq;

namespace LogExpert
{
	public class Persister
	{
		#region Const

		private const string PERSISTENCE_EXTENSION = ".lxp";

		#endregion Const

		#region Static Fields

		//TODO Zarunbal: think about this
		private static readonly Regex _directoryCharsRegex = new Regex(
			string.Format("{0}|{1}|{2}",
				Regex.Escape(Path.DirectorySeparatorChar.ToString()),
				Regex.Escape(Path.AltDirectorySeparatorChar.ToString()),
				Regex.Escape(Path.VolumeSeparatorChar.ToString())));

		#endregion Static Fields

		#region Public Methods

		/// <summary>
		/// Loads the persistence options out of the given persistence file name.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static PersistenceData LoadOptionsOnly(string fileName)
		{
			//TODO Zarunbal: check if this works as designed
			PersistenceData output = Load(fileName);

			return output;
		}

		public static string SavePersistenceData(String logFileName, PersistenceData persistenceData, Preferences preferences)
		{
			string fileName = BuildPersisterFileName(logFileName, preferences);
			if (preferences.saveLocation == SessionSaveLocation.SameDir)
			{
				// make to log file in .lxp file relative
				string filePart = Path.GetFileName(persistenceData.FileName);
				persistenceData.FileName = filePart;
			}
			Save(fileName, persistenceData);
			return fileName;
		}

		public static string SavePersistenceDataWithFixedName(String persistenceFileName, PersistenceData persistenceData)
		{
			Save(persistenceFileName, persistenceData);
			return persistenceFileName;
		}

		public static PersistenceData LoadPersistenceData(string logFileName, Preferences preferences)
		{
			string fileName = BuildPersisterFileName(logFileName, preferences);
			return Load(fileName);
		}

		public static PersistenceData LoadPersistenceDataOptionsOnly(string logFileName, Preferences preferences)
		{
			string fileName = BuildPersisterFileName(logFileName, preferences);
			return LoadOptionsOnly(fileName);
		}

		public static PersistenceData LoadPersistenceDataOptionsOnlyFromFixedFile(string persistenceFile)
		{
			return LoadOptionsOnly(persistenceFile);
		}

		public static PersistenceData LoadPersistenceDataFromFixedFile(string persistenceFile)
		{
			return Load(persistenceFile);
		}

		#endregion Public Methods

		#region Private Methods

		private static string BuildPersisterFileName(string logFileName, Preferences preferences)
		{
			string dir = null;
			string file = null;
			switch (preferences.saveLocation)
			{
				case SessionSaveLocation.SameDir:
				default:
					FileInfo fileInfo = new FileInfo(logFileName);
					dir = fileInfo.DirectoryName;
					file = fileInfo.DirectoryName + Path.DirectorySeparatorChar + fileInfo.Name + PERSISTENCE_EXTENSION;
					break;

				case SessionSaveLocation.DocumentsDir:
					dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
						  Path.DirectorySeparatorChar +
						  "LogExpert";
					file = dir + Path.DirectorySeparatorChar + BuildSessionFileNameFromPath(logFileName);
					break;

				case SessionSaveLocation.OwnDir:
					dir = preferences.saveDirectory;
					file = dir + Path.DirectorySeparatorChar + BuildSessionFileNameFromPath(logFileName);
					break;
			}
			if (!Directory.Exists(dir))
			{
				try
				{
					Directory.CreateDirectory(dir);
				}
				catch (Exception e)
				{
					MessageBox.Show(e.Message, "LogExpert");
				}
			}
			return file;
		}

		private static string BuildSessionFileNameFromPath(string logFileName)
		{
			string result = _directoryCharsRegex.Replace(logFileName, "_");
			result += PERSISTENCE_EXTENSION;
			return result;
		}

		private static void Save(string fileName, PersistenceData persistenceData)
		{
			ConfigurationUtils.Save<PersistenceData>(fileName, persistenceData);
		}

		private static PersistenceData Load(string fileName)
		{
			FileInfo file = new FileInfo(fileName);

			if (file.Exists)
			{
				PersistenceData output = ConfigurationUtils.Load<PersistenceData>(fileName);

				//HACK to workaround circular reference in bookmark overlay
				output.BookmarkList.Select(a => a.Overlay.Bookmark = a).ToArray();

				return output;
			}
			else
			{
				throw new FileNotFoundException("File not found", fileName);
			}
		}

		#endregion Private Methods
	}
}