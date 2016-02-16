using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace LogExpert.Classes
{
	public static class ConfigurationUtils
	{
		public static string Prefix { get; set; }

		public static T Load<T>()
			where T : new()
		{
			return LoadInternal<T>(false);
		}

		public static T Load<T>(string fileName)
			where T : new()
		{
			return LoadInternal<T>(false, fileName);
		}

		public static void Save<T>(string fileName, T toSave)
			where T : new()
		{
			FileInfo file = new FileInfo(fileName);

			SaveInternal<T>(file, toSave);
		}

		private static T LoadInternal<T>(bool escape)
			where T : new()
		{
			Type type = typeof(T);

			string filename = string.Format("{0}.xml", type.Name);
			return LoadInternal<T>(escape, filename);

		}

		private static T LoadInternal<T>(bool escape, string filename) 
			where T : new()
		{
			FileInfo file = new FileInfo(Path.Combine(Environment2.StartupPath, filename));

			if (file.Exists)
			{
				//Try to read in config, if there is an error delete the file and create a new one (there was some config incompatible changes)
				try
				{
					using (var fileStream = file.OpenRead())
					{
						XmlSerializer serializer = new XmlSerializer(typeof(T));
						return (T)serializer.Deserialize(fileStream);
					}
				}
				catch (Exception ex)
				{
					if (escape)
					{
						throw new Exception("Failed to create and read in config", ex);
					}
					file.Delete();
					return LoadInternal<T>(true, filename);
				}
			}
			else
			{
				T conf = new T();

				SaveInternal(file, conf);

				return conf;
			}
		}

		private static void SaveInternal<T>(FileInfo file, T conf) where T : new()
		{
			using (var filestream = file.Create())
			{
				XmlSerializer serializer = new XmlSerializer(typeof(T));

				serializer.Serialize(filestream, conf);

				filestream.Flush();
				filestream.Close();
			}
		}
	}
}
