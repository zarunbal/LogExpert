using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace LogExpert
{
	public class Eminus : IContextMenuEntry, ILogExpertPluginConfigurator
	{
		private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		private EminusConfig config = new EminusConfig();
		private EminusConfig tmpConfig = new EminusConfig();
		private EminusConfigDlg dlg;
		private const string CFG_FILE_NAME = "\\eminus.dat";

		#region IContextMenuEntry Member

		public string GetMenuText(IList<int> logLines, ILogLineColumnizer columnizer, ILogExpertCallback callback)
		{
			if (logLines.Count == 1 && BuildParam(callback.GetLogLine(logLines[0])) != null)
			{
				return "Load class in Eclipse";
			}
			else
			{
				return "_Load class in Eclipse";
			}
		}

		public void MenuSelected(IList<int> logLines, ILogLineColumnizer columnizer, ILogExpertCallback callback)
		{
			if (logLines.Count != 1)
				return;

			XmlDocument doc = BuildParam(callback.GetLogLine(logLines[0]));
			if (doc == null)
			{
				MessageBox.Show("Cannot parse Java stack trace line", "LogExpert");
			}
			else
			{
				try
				{
					TcpClient client = new TcpClient(this.config.host, this.config.port);
					NetworkStream stream = client.GetStream();
					StreamWriter writer = new StreamWriter(stream);
					doc.Save(writer);
					writer.Flush();
					stream.Flush();
					writer.Close();
					stream.Close(500);
					client.Close();
				}
				catch (Exception e)
				{
					_logger.Error(e);
					MessageBox.Show(e.Message, "LogExpert");
				}
			}
		}

		#endregion IContextMenuEntry Member

		private XmlDocument BuildParam(string line)
		{
			// no Java stacktrace but some special logging of our applications at work:
			if (line.IndexOf("Exception of type") != -1 || line.IndexOf("Nested:") != -1)
			{
				int pos = line.IndexOf("created in ");
				if (pos == -1)
					return null;
				pos += "created in ".Length;
				int endPos = line.IndexOf(".", pos);
				if (endPos == -1)
					return null;
				string className = line.Substring(pos, endPos - pos);
				pos = line.IndexOf(":", pos);
				if (pos == -1)
					return null;
				string lineNum = line.Substring(pos + 1);
				XmlDocument doc = BuildXmlDocument(className, lineNum);
				return doc;
			}
			if (line.IndexOf("at ") != -1)
			{
				string str = line.Trim();
				string className = null;
				string lineNum = null;
				int pos = str.IndexOf("at ") + 3;
				str = str.Substring(pos);   // remove 'at '
				int idx = str.IndexOfAny(new char[] { '(', '$', '<' });
				if (idx != -1)
				{
					if (str[idx] == '$')
					{
						className = str.Substring(0, idx);
					}
					else
					{
						pos = str.LastIndexOf('.', idx);
						if (pos == -1)
						{
							return null;
						}
						className = str.Substring(0, pos);
					}
					idx = str.LastIndexOf(':');
					if (idx == -1)
					{
						return null;
					}
					pos = str.IndexOf(')', idx);
					if (pos == -1)
					{
						return null;
					}
					lineNum = str.Substring(idx + 1, pos - idx - 1);
				}
				/*
				 * <?xml version="1.0" encoding="UTF-8"?>
					<loadclass>
						<!-- full qualified java class name -->
						<classname></classname>
						<!-- line number one based -->
						<linenumber></linenumber>
					</loadclass>
				 */

				XmlDocument doc = BuildXmlDocument(className, lineNum);
				return doc;
			}
			return null;
		}

		private XmlDocument BuildXmlDocument(string className, string lineNum)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
			XmlElement rootElement = xmlDoc.CreateElement("eminus");
			xmlDoc.AppendChild(rootElement);
			rootElement.SetAttribute("authKey", this.config.password);

			XmlElement loadElement = xmlDoc.CreateElement("loadclass");
			loadElement.SetAttribute("mode", "dialog");
			rootElement.AppendChild(loadElement);

			XmlElement elemClassName = xmlDoc.CreateElement("classname");
			XmlElement elemLineNum = xmlDoc.CreateElement("linenumber");
			elemClassName.InnerText = className;
			elemLineNum.InnerText = lineNum;
			loadElement.AppendChild(elemClassName);
			loadElement.AppendChild(elemLineNum);
			return xmlDoc;
		}

		#region ILogExpertPluginConfigurator Member

		public void LoadConfig(string configDir)
		{
			string configPath = configDir + CFG_FILE_NAME;

			if (!File.Exists(configPath))
			{
				this.config = new EminusConfig();
			}
			else
			{
				Stream fs = File.OpenRead(configPath);
				BinaryFormatter formatter = new BinaryFormatter();
				try
				{
					this.config = (EminusConfig)formatter.Deserialize(fs);
				}
				catch (SerializationException e)
				{
					MessageBox.Show(e.Message, "Deserialize");
					this.config = new EminusConfig();
				}
				finally
				{
					fs.Close();
				}
			}
		}

		public void SaveConfig(string configDir)
		{
			string configPath = configDir + CFG_FILE_NAME;
			if (this.dlg != null)
			{
				this.dlg.ApplyChanges();
			}
			this.config = this.tmpConfig.Clone();
			BinaryFormatter formatter = new BinaryFormatter();
			Stream fs = new FileStream(configPath, FileMode.Create, FileAccess.Write);
			formatter.Serialize(fs, this.config);
			fs.Close();
		}

		public bool HasEmbeddedForm()
		{
			return true;
		}

		public void ShowConfigForm(Panel panel)
		{
			this.dlg = new EminusConfigDlg(this.tmpConfig);
			this.dlg.Parent = panel;
			this.dlg.Show();
		}

		/// <summary>
		/// Implemented only for demonstration purposes. This function is called when the config button
		/// is pressed (HasEmbeddedForm() must return false for this).
		/// </summary>
		/// <param name="owner"></param>
		public void ShowConfigDialog(Form owner)
		{
			this.dlg = new EminusConfigDlg(this.tmpConfig);
			this.dlg.TopLevel = true;
			this.dlg.Owner = owner;
			this.dlg.ShowDialog();
			this.dlg.ApplyChanges();
		}

		public void HideConfigForm()
		{
			if (this.dlg != null)
			{
				this.dlg.ApplyChanges();
				this.dlg.Hide();
				this.dlg.Dispose();
				this.dlg = null;
			}
		}

		public void StartConfig()
		{
			this.tmpConfig = this.config.Clone();
		}

		#endregion ILogExpertPluginConfigurator Member

		public string Text
		{
			get { return "eminus"; }
		}
	}
}