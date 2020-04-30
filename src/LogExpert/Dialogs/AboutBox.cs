using System;
using System.Reflection;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    internal partial class AboutBox : Form
    {
        #region Fields

        private readonly Assembly _assembly;

        #endregion

        #region cTor

        public AboutBox()
        {
            InitializeComponent();
            _assembly = Assembly.GetExecutingAssembly();

            Text = $@"About {AssemblyTitle}";
            labelProductName.Text = AssemblyProduct;
            labelVersion.Text = AssemblyVersion;
            labelCopyright.Text = AssemblyCopyright;
            //this.labelCompanyName.Text = AssemblyCompany;
            textBoxDescription.Text = AssemblyDescription +
@"

Credits:

DockPanel control (c) 2007 Weifen Luo 
Early bird test: Mathias Dräger

LogExpert uses modules from:
http://sourceforge.net/projects/dockpanelsuite/
http://www.xml-rpc.net/";

            string link = "https://github.com/zarunbal/LogExpert/";
            linkLabelURL.Links.Add(new LinkLabel.Link(0, link.Length, link));
        }

        #endregion

        #region Properties

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = _assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute) attributes[0];
                    if (titleAttribute.Title != string.Empty)
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
                object[] attributes = _assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyDescriptionAttribute) attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = _assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyProductAttribute) attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = _assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyCopyrightAttribute) attributes[0]).Copyright;
            }
        }

        #endregion

        #region Events handler

        private void linkLabelURL_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string target = e.Link.LinkData as string;
            System.Diagnostics.Process.Start(target);
        }

        #endregion
    }
}