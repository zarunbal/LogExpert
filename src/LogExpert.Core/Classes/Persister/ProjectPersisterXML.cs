using System.Xml;

namespace LogExpert.Core.Classes.Persister;

public static class ProjectPersisterXML
{
    #region Public methods

    public static ProjectData LoadProjectData (string projectFileName)
    {
        var projectData = new ProjectData();
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(projectFileName);
        var fileList = xmlDoc.GetElementsByTagName("member");

        foreach (XmlNode fileNode in fileList)
        {
            var fileElement = fileNode as XmlElement;
            var fileName = fileElement.GetAttribute("fileName");
            projectData.FileNames.Add(fileName);
        }

        var layoutElements = xmlDoc.GetElementsByTagName("layout");
        if (layoutElements.Count > 0)
        {
            projectData.TabLayoutXml = layoutElements[0].InnerXml;
        }

        return projectData;
    }

    #endregion
}