using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace LogExpert
{
    public static class ProjectPersister
    {
        #region Public methods

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
            XmlNodeList layoutElements = xmlDoc.GetElementsByTagName("layout");
            if (layoutElements.Count > 0)
            {
                projectData.tabLayoutXml = layoutElements[0].InnerXml;
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

            if (projectData.tabLayoutXml != null)
            {
                XmlElement layoutElement = xmlDoc.CreateElement("layout");
                layoutElement.InnerXml = projectData.tabLayoutXml;
                rootElement.AppendChild(layoutElement);
            }

            xmlDoc.Save(projectFileName);
        }

        #endregion

        #region Private Methods

        private static void SaveProjectMembers(XmlDocument xmlDoc, XmlNode membersNode, List<string> memberList)
        {
            foreach (string fileName in memberList)
            {
                XmlElement memberElement = xmlDoc.CreateElement("member");
                membersNode.AppendChild(memberElement);
                memberElement.SetAttribute("fileName", fileName);
            }
        }

        #endregion
    }
}