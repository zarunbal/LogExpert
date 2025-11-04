using System.Xml;

using NLog;

namespace LogExpert.Core.Classes.Persister;

public static class ProjectPersisterXML
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Loads project data from the specified XML file.
    /// </summary>
    /// <remarks>The method reads the XML file to extract file names and layout information. If the XML file
    /// contains a <c>layout</c> element, its inner XML is stored in the <c>TabLayoutXml</c> property of the returned
    /// <see cref="ProjectData"/> object. If any exception occurs during the loading process, an error is logged, and an
    /// empty <see cref="ProjectData"/> object is returned.</remarks>
    /// <param name="projectFileName">The path to the XML file containing the project data.</param>
    /// <returns>A <see cref="ProjectData"/> object populated with file names and layout information from the XML file. If an
    /// error occurs during loading, an empty <see cref="ProjectData"/> object is returned.</returns>
    public static ProjectData LoadProjectData (string projectFileName)
    {
        var projectData = new ProjectData();
        var xmlDoc = new XmlDocument();
        try
        {
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
        catch (Exception xmlParsingException) when (xmlParsingException is XmlException or
                                                                           UnauthorizedAccessException or
                                                                           IOException)
        {
            _logger.Error(xmlParsingException, $"Error loading persistence data from {projectFileName}, unknown format, parsing xml or json was not possible");
            return new ProjectData();
        }
    }
}