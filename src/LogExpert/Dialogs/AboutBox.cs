using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    internal partial class AboutBox : Form
    {
        #region Private Fields

        private readonly Assembly _assembly;

        #endregion

        #region Ctor

        public AboutBox()
        {
            InitializeComponent();
            _assembly = Assembly.GetExecutingAssembly();

            Text = string.Format("About {0}", AssemblyTitle);
            labelProductName.Text = AssemblyProduct;
            labelVersion.Text = AssemblyVersion;
            labelCopyright.Text = AssemblyCopyright;

// this.labelCompanyName.Text = AssemblyCompany;
            textBoxDescription.Text = AssemblyDescription +
                                      "\r\n\r\nCredits:\r\n\r\n" +
                                      "DockPanel control (c) 2007 Weifen Luo \r\n" +
                                      "Early bird test: Mathias Dräger\r\n" +
                                      "\r\n" +
                                      "LogExpert uses modules from:\r\n" +
                                      "http://sourceforge.net/projects/dockpanelsuite/\r\n" +
                                      "http://www.xml-rpc.net/";
            string link = "http://www.log-expert.de/";
            linkLabel1.Links.Add(new LinkLabel.Link(0, link.Length, link));
        }

        #endregion

        #region Properties / Indexers

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = _assembly
                    .GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }

                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = _assembly
                    .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }

                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = _assembly
                    .GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }

                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = _assembly
                    .GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != string.Empty)
                    {
                        return titleAttribute.Title;
                    }
                }

                return Path.GetFileNameWithoutExtension(_assembly.CodeBase);
            }
        }

        public string AssemblyVersion => _assembly.GetName().Version.ToString(3);

        #endregion

        #region Private Methods

        private void button2_Click(object sender, EventArgs e)
        {
            SystemInfo info = new SystemInfo();
            MessageBox.Show(info.Info, "System info");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string target = e.Link.LinkData as string;
            Process.Start(target);
        }

        #endregion
    }
}
