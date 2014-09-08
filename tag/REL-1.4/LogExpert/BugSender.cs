using System;
using System.Collections.Generic;
using System.Text;
using Bugzproxy;
using System.Windows.Forms;
using System.Reflection;

namespace LogExpert
{
  internal class BugSender
  {
    String errorText;
    String stackTrace;
    String comment;
    String ccMail;

    String host = "www.logfile-viewer.de";
    uint port = 80;
    String path = "bugzilla/";

    internal BugSender(String errorText, String stackTrace, String comment, String ccMail)
    {
      this.errorText = errorText;
      this.stackTrace = stackTrace;
      this.comment = comment;
      this.ccMail = ccMail;
    }

    public int SendBug()
    {
      SystemInfo info = new SystemInfo();
      String bugDescription =
                              this.comment + "\n\n" +
                              this.errorText + "\n" +
                              this.stackTrace + "\n\n" +
                              info.Info + "\n" +
                              "LogExpert version: " + AssemblyVersion + "/" + AssemblyBuild;
      Server server = new Server(this.host, this.port, this.path);
      server.Login("autoreporter@log-expert.de", "b1u1gz1i1l1l1a1", true);
      int [] ids = server.GetSelectableProductIds();
      if (ids == null || ids.Length == 0)
      {
        throw new Exception("No selectable products found on bugzilla installation.");
      }
      Product product = server.GetProduct(ids[0]);

      if (!this.ccMail.Contains("@") || !this.ccMail.Contains("."))
      {
        this.ccMail = "";
      }
      string[] ccList = this.ccMail.Length > 0 ? new string[] { this.ccMail } : null;

      Bug bug = product.CreateBug(null, // alias
                        "Main Application", // component 
                        AssemblyVersion, // version
                        "All", // operating system
                        "All", // platform 
                        this.errorText, // summary
                        bugDescription, // initial description
                        "P2", // priority
                        "normal", // severity
                        null, // status
                        null, // milestone
                        null, // assigned to
                        ccList, // list of cc's
                        null  // quality contact
                        );
      return bug.Id;
    }

    public string AssemblyVersion
    {
      get
      {
        return Assembly.GetExecutingAssembly().GetName().Version.Major + "." +
               Assembly.GetExecutingAssembly().GetName().Version.Minor;
        //return Assembly.GetExecutingAssembly().GetName().Version.ToString();
      }
    }

    public string AssemblyBuild
    {
      get
      {
        return Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();
      }
    }


  }
}
