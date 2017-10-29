using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    internal partial class AboutBox : Form
    {
        private readonly Assembly _assembly;

        #region cTor

        public AboutBox()
        {
            InitializeComponent();
            this.Text = string.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = AssemblyVersion;
            this.labelCopyright.Text = AssemblyCopyright;
            //this.labelCompanyName.Text = AssemblyCompany;
            this.textBoxDescription.Text = AssemblyDescription +
                                           "\r\n\r\nCredits:\r\n\r\n" +
                                           "DockPanel control (c) 2007 Weifen Luo \r\n" +
                                           "Early bird test: Mathias Dräger\r\n" +
                                           "\r\n" +
                                           "LogExpert uses modules from:\r\n" +
                                           "http://sourceforge.net/projects/dockpanelsuite/\r\n" +
                                           "http://www.xml-rpc.net/";
            string link = "http://www.log-expert.de/";
            this.linkLabel1.Links.Add(new LinkLabel.Link(0, link.Length, link));
            _assembly = Assembly.GetExecutingAssembly();
        }

        #endregion

        #region Events handler

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string target = e.Link.LinkData as string;
            System.Diagnostics.Process.Start(target);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SystemInfo info = new SystemInfo();
            MessageBox.Show(info.Info, "System info");
        }

        #endregion

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = _assembly
                    .GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute) attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(_assembly.CodeBase);
            }
        }

        public string AssemblyVersion => _assembly.GetName().Version.ToString(3);

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = _assembly
                    .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute) attributes[0]).Description;
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
                    return "";
                }
                return ((AssemblyProductAttribute) attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = _assembly
                    .GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute) attributes[0]).Copyright;
            }
        }

        #endregion
    }
}