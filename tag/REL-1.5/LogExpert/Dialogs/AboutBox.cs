using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
  partial class AboutBox : Form
  {
    public AboutBox()
    {
      InitializeComponent();
      this.Text = String.Format("About {0}", AssemblyTitle);
      this.labelProductName.Text = AssemblyProduct;
      this.labelVersion.Text = String.Format("Version {0} / {1}", AssemblyVersion, AssemblyBuild);
      this.labelCopyright.Text = AssemblyCopyright;
      //this.labelCompanyName.Text = AssemblyCompany;
      this.textBoxDescription.Text = AssemblyDescription +
        "\r\n\r\nCredits:\r\n\r\n" +
        "DockPanel control (c) 2007 Weifen Luo \r\n" +
        "Early bird test: Mathias Dräger\r\n" +
        "\r\n" +
        "LogExpert uses modules from:\r\n" +
        "http://sourceforge.net/projects/dockpanelsuite/\r\n" +
        "http://sourceforge.net/projects/bugzproxy/\r\n" +
        "http://www.xml-rpc.net/";
      string link = "http://www.log-expert.de/";
      this.linkLabel1.Links.Add(new LinkLabel.Link(0, link.Length, link));


    }

    #region Assembly Attribute Accessors

    public string AssemblyTitle
    {
      get
      {
        object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
        if (attributes.Length > 0)
        {
          AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
          if (titleAttribute.Title != "")
          {
            return titleAttribute.Title;
          }
        }
        return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
      }
    }

    public string AssemblyVersion
    {
      get
      {
        return Assembly.GetExecutingAssembly().GetName().Version.Major + "." +
               Assembly.GetExecutingAssembly().GetName().Version.Minor;
//        return Assembly.GetExecutingAssembly().GetName().Version.ToString();
      }
    }

    public string AssemblyBuild
    {
      get
      {
        return Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();
      }
    }

    public string AssemblyDescription
    {
      get
      {
        object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
        if (attributes.Length == 0)
        {
          return "";
        }
        return ((AssemblyDescriptionAttribute)attributes[0]).Description;
      }
    }

    public string AssemblyProduct
    {
      get
      {
        object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
        if (attributes.Length == 0)
        {
          return "";
        }
        return ((AssemblyProductAttribute)attributes[0]).Product;
      }
    }

    public string AssemblyCopyright
    {
      get
      {
        object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
        if (attributes.Length == 0)
        {
          return "";
        }
        return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
      }
    }

    public string AssemblyCompany
    {
      get
      {
        object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
        if (attributes.Length == 0)
        {
          return "";
        }
        return ((AssemblyCompanyAttribute)attributes[0]).Company;
      }
    }
    #endregion

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

  }
}
