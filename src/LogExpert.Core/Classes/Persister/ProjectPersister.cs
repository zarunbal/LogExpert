using System.Collections.Generic;
using System.Xml;

namespace LogExpert.Core.Classes.Persister;

public static class ProjectPersister
{
    #region Public methods

    public static ProjectData LoadProjectData(string projectFileName)
    {
        ProjectData projectData = new();
        XmlDocument xmlDoc = new();
        xmlDoc.Load(projectFileName);
        var fileList = xmlDoc.GetElementsByTagName("member");
        foreach (XmlNode fileNode in fileList)
        {
            var fileElement = fileNode as XmlElement;
            var fileName = fileElement.GetAttribute("fileName");
            projectData.MemberList.Add(fileName);
        }

        var layoutElements = xmlDoc.GetElementsByTagName("layout");
        if (layoutElements.Count > 0)
        {
            projectData.TabLayoutXml = layoutElements[0].InnerXml;
        }

        return projectData;
    }


    public static void SaveProjectData(string projectFileName, ProjectData projectData)
    {
        XmlDocument xmlDoc = new();
        var rootElement = xmlDoc.CreateElement("logexpert");
        xmlDoc.AppendChild(rootElement);
        var projectElement = xmlDoc.CreateElement("project");
        rootElement.AppendChild(projectElement);
        var membersElement = xmlDoc.CreateElement("members");
        projectElement.AppendChild(membersElement);
        SaveProjectMembers(xmlDoc, membersElement, projectData.MemberList);

        if (projectData.TabLayoutXml != null)
        {
            var layoutElement = xmlDoc.CreateElement("layout");
            layoutElement.InnerXml = projectData.TabLayoutXml;
            rootElement.AppendChild(layoutElement);
        }

        xmlDoc.Save(projectFileName);
    }

    #endregion

    #region Private Methods

    private static void SaveProjectMembers(XmlDocument xmlDoc, XmlNode membersNode, List<string> memberList)
    {
        foreach (var fileName in memberList)
        {
            var memberElement = xmlDoc.CreateElement("member");
            membersNode.AppendChild(memberElement);
            memberElement.SetAttribute("fileName", fileName);
        }
    }

    #endregion
}