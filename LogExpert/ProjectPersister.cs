using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace LogExpert
{
  public class ProjectData
  {
    public List<string> memberList = new List<string>();
  }

  public class ProjectPersister
  {

    public static ProjectData LoadProjectData(string projectFileName)
    {
      ProjectData projectData = new ProjectData();
      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.Load(projectFileName);
      XmlNodeList fileList = xmlDoc.GetElementsByTagName("member");
      foreach (XmlNode fileNode in fileList)
      {
        XmlElement fileElement = fileNode as XmlElement;
        string fileName = fileElement.GetAttribute("fileName");
        projectData.memberList.Add(fileName);
      }
      return projectData;
    }


    public static void SaveProjectData(string projectFileName, ProjectData projectData)
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlElement rootElement = xmlDoc.CreateElement("logexpert");
      xmlDoc.AppendChild(rootElement);
      XmlElement projectElement = xmlDoc.CreateElement("project");
      rootElement.AppendChild(projectElement);
      XmlElement membersElement = xmlDoc.CreateElement("members");
      projectElement.AppendChild(membersElement);
      SaveProjectMembers(xmlDoc, membersElement, projectData.memberList);
      xmlDoc.Save(projectFileName);
    }

    private static void SaveProjectMembers(XmlDocument xmlDoc, XmlNode membersNode, List<string> memberList)
    {
      foreach (string fileName in memberList)
      {
        XmlElement memberElement = xmlDoc.CreateElement("member");
        membersNode.AppendChild(memberElement);
        memberElement.SetAttribute("fileName", fileName);
      }
    }

  }
}
