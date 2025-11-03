using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Xml;

using LogExpert.Core.Classes.Filter;
using LogExpert.Core.Entities;

using NLog;

namespace LogExpert.Core.Classes.Persister;

public static class PersisterXML
{
    #region Fields

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Methods

    private static List<FilterTabData> ReadFilterTabs (XmlElement startNode)
    {
        List<FilterTabData> dataList = [];
        XmlNode filterTabsNode = startNode.SelectSingleNode("filterTabs");
        if (filterTabsNode != null)
        {
            XmlNodeList filterTabNodeList = filterTabsNode.ChildNodes; // all "filterTab" nodes

            foreach (XmlNode node in filterTabNodeList)
            {
                PersistenceData persistenceData = ReadPersistenceDataFromNode(node);
                XmlNode filterNode = node.SelectSingleNode("tabFilter");

                if (filterNode != null)
                {
                    List<FilterParams> filterList = ReadFilter(filterNode as XmlElement);
                    FilterTabData data = new()
                    {
                        PersistenceData = persistenceData,
                        FilterParams = filterList[0] // there's only 1
                    };

                    dataList.Add(data);
                }
            }
        }

        return dataList;
    }

    private static List<FilterParams> ReadFilter (XmlElement startNode)
    {
        List<FilterParams> filterList = [];
        XmlNode filtersNode = startNode.SelectSingleNode("filters");
        if (filtersNode != null)
        {
            XmlNodeList filterNodeList = filtersNode.ChildNodes; // all "filter" nodes
            foreach (XmlNode node in filterNodeList)
            {
                foreach (XmlNode subNode in node.ChildNodes)
                {
                    if (subNode.Name.Equals("params", StringComparison.OrdinalIgnoreCase))
                    {
                        var base64Text = subNode.InnerText;
                        var data = Convert.FromBase64String(base64Text);
                        MemoryStream stream = new(data);

                        try
                        {
                            FilterParams filterParams = JsonSerializer.Deserialize<FilterParams>(stream);
                            filterParams.Init();
                            filterList.Add(filterParams);
                        }
                        catch (JsonException ex)
                        {
                            _logger.Error($"Error while deserializing filter params. Exception Message: {ex.Message}");
                        }
                    }
                }
            }
        }

        return filterList;
    }

    private static PersistenceData LoadInternal (string fileName)
    {
        XmlDocument xmlDoc = new();
        xmlDoc.Load(fileName);
        XmlNode fileNode = xmlDoc.SelectSingleNode("logexpert/file");
        PersistenceData persistenceData = new();
        if (fileNode != null)
        {
            persistenceData = ReadPersistenceDataFromNode(fileNode);
        }

        return persistenceData;
    }

    private static PersistenceData ReadPersistenceDataFromNode (XmlNode node)
    {
        PersistenceData persistenceData = new();
        var fileElement = node as XmlElement;
        persistenceData.BookmarkList = ReadBookmarks(fileElement);
        persistenceData.RowHeightList = ReadRowHeightList(fileElement);
        ReadOptions(fileElement, persistenceData);
        persistenceData.FileName = fileElement.GetAttribute("fileName");
        var sLineCount = fileElement.GetAttribute("lineCount");
        if (sLineCount != null && sLineCount.Length > 0)
        {
            persistenceData.LineCount = int.Parse(sLineCount);
        }

        persistenceData.FilterParamsList = ReadFilter(fileElement);
        persistenceData.FilterTabDataList = ReadFilterTabs(fileElement);
        persistenceData.Encoding = ReadEncoding(fileElement);
        return persistenceData;
    }

    private static Encoding ReadEncoding (XmlElement fileElement)
    {
        XmlNode encodingNode = fileElement.SelectSingleNode("encoding");
        if (encodingNode != null)
        {
            XmlAttribute encAttr = encodingNode.Attributes["name"];
            try
            {
                return encAttr == null ? null : Encoding.GetEncoding(encAttr.Value);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e);
                return Encoding.Default;
            }
            catch (NotSupportedException e)
            {
                _logger.Error(e);
                return Encoding.Default;
            }
        }

        return null;
    }

    private static SortedList<int, Entities.Bookmark> ReadBookmarks (XmlElement startNode)
    {
        SortedList<int, Entities.Bookmark> bookmarkList = [];
        XmlNode boomarksNode = startNode.SelectSingleNode("bookmarks");
        if (boomarksNode != null)
        {
            XmlNodeList bookmarkNodeList = boomarksNode.ChildNodes; // all "bookmark" nodes
            foreach (XmlNode node in bookmarkNodeList)
            {
                string text = null;
                string posX = null;
                string posY = null;
                string line = null;

                foreach (XmlAttribute attr in node.Attributes)
                {
                    if (attr.Name.Equals("line", StringComparison.OrdinalIgnoreCase))
                    {
                        line = attr.InnerText;
                    }
                }

                foreach (XmlNode subNode in node.ChildNodes)
                {
                    if (subNode.Name.Equals("text", StringComparison.OrdinalIgnoreCase))
                    {
                        text = subNode.InnerText;
                    }
                    else if (subNode.Name.Equals("posX", StringComparison.OrdinalIgnoreCase))
                    {
                        posX = subNode.InnerText;
                    }
                    else if (subNode.Name.Equals("posY", StringComparison.OrdinalIgnoreCase))
                    {
                        posY = subNode.InnerText;
                    }
                }

                if (line == null || posX == null || posY == null)
                {
                    _logger.Error($"Invalid XML format for bookmark: {node.InnerText}");
                    continue;
                }

                var lineNum = int.Parse(line);

                Entities.Bookmark bookmark = new(lineNum)
                {
                    OverlayOffset = new Size(int.Parse(posX), int.Parse(posY))
                };

                if (text != null)
                {
                    bookmark.Text = text;
                }

                bookmarkList.Add(lineNum, bookmark);
            }
        }

        return bookmarkList;
    }

    private static SortedList<int, RowHeightEntry> ReadRowHeightList (XmlElement startNode)
    {
        SortedList<int, RowHeightEntry> rowHeightList = [];
        XmlNode rowHeightsNode = startNode.SelectSingleNode("rowheights");
        if (rowHeightsNode != null)
        {
            XmlNodeList rowHeightNodeList = rowHeightsNode.ChildNodes; // all "rowheight" nodes
            foreach (XmlNode node in rowHeightNodeList)
            {
                string height = null;
                string line = null;
                foreach (XmlAttribute attr in node.Attributes)
                {
                    if (attr.Name.Equals("line", StringComparison.OrdinalIgnoreCase))
                    {
                        line = attr.InnerText;
                    }
                    else if (attr.Name.Equals("height", StringComparison.OrdinalIgnoreCase))
                    {
                        height = attr.InnerText;
                    }
                }

                var lineNum = int.Parse(line);
                var heightValue = int.Parse(height);
                rowHeightList.Add(lineNum, new RowHeightEntry(lineNum, heightValue));
            }
        }

        return rowHeightList;
    }

    private static void ReadOptions (XmlElement startNode, PersistenceData persistenceData)
    {
        XmlNode optionsNode = startNode.SelectSingleNode("options");
        var value = GetOptionsAttribute(optionsNode, "multifile", "enabled");
        persistenceData.MultiFile = value != null && value.Equals("1", StringComparison.OrdinalIgnoreCase);
        persistenceData.MultiFilePattern = GetOptionsAttribute(optionsNode, "multifile", "pattern");
        value = GetOptionsAttribute(optionsNode, "multifile", "maxDays");
        try
        {
            persistenceData.MultiFileMaxDays = value != null ? short.Parse(value) : 0;
        }
        catch (Exception)
        {
            persistenceData.MultiFileMaxDays = 0;
        }

        XmlNode multiFileNode = optionsNode.SelectSingleNode("multifile");
        if (multiFileNode != null)
        {
            XmlNodeList multiFileNodeList = multiFileNode.ChildNodes; // all "fileEntry" nodes
            foreach (XmlNode node in multiFileNodeList)
            {
                string fileName = null;
                foreach (XmlAttribute attr in node.Attributes)
                {
                    if (attr.Name.Equals("fileName", StringComparison.OrdinalIgnoreCase))
                    {
                        fileName = attr.InnerText;
                    }
                }

                persistenceData.MultiFileNames.Add(fileName);
            }
        }

        value = GetOptionsAttribute(optionsNode, "currentline", "line");
        if (value != null)
        {
            persistenceData.CurrentLine = int.Parse(value);
        }

        value = GetOptionsAttribute(optionsNode, "firstDisplayedLine", "line");
        if (value != null)
        {
            persistenceData.FirstDisplayedLine = int.Parse(value);
        }

        value = GetOptionsAttribute(optionsNode, "filter", "visible");
        persistenceData.FilterVisible = value != null && value.Equals("1", StringComparison.OrdinalIgnoreCase);
        value = GetOptionsAttribute(optionsNode, "filter", "advanced");
        persistenceData.FilterAdvanced = value != null && value.Equals("1", StringComparison.OrdinalIgnoreCase);
        value = GetOptionsAttribute(optionsNode, "filter", "position");
        if (value != null)
        {
            persistenceData.FilterPosition = int.Parse(value);
        }

        value = GetOptionsAttribute(optionsNode, "bookmarklist", "visible");
        persistenceData.BookmarkListVisible = value != null && value.Equals("1", StringComparison.OrdinalIgnoreCase);
        value = GetOptionsAttribute(optionsNode, "bookmarklist", "position");
        if (value != null)
        {
            persistenceData.BookmarkListPosition = int.Parse(value);
        }

        value = GetOptionsAttribute(optionsNode, "followTail", "enabled");
        persistenceData.FollowTail = value != null && value.Equals("1", StringComparison.OrdinalIgnoreCase);

        value = GetOptionsAttribute(optionsNode, "bookmarkCommentColumn", "visible");
        persistenceData.ShowBookmarkCommentColumn = value != null && value.Equals("1", StringComparison.OrdinalIgnoreCase);

        value = GetOptionsAttribute(optionsNode, "filterSaveList", "visible");
        persistenceData.FilterSaveListVisible = value != null && value.Equals("1", StringComparison.OrdinalIgnoreCase);

        XmlNode tabNode = startNode.SelectSingleNode("tab");
        if (tabNode != null)
        {
            persistenceData.TabName = (tabNode as XmlElement).GetAttribute("name");
        }

        XmlNode columnizerNode = startNode.SelectSingleNode("columnizer");
        if (columnizerNode != null)
        {
            persistenceData.ColumnizerName = (columnizerNode as XmlElement).GetAttribute("name");
        }

        XmlNode highlightGroupNode = startNode.SelectSingleNode("highlightGroup");
        if (highlightGroupNode != null)
        {
            persistenceData.HighlightGroupName = (highlightGroupNode as XmlElement).GetAttribute("name");
        }
    }

    private static string GetOptionsAttribute (XmlNode optionsNode, string elementName, string attrName)
    {
        XmlNode node = optionsNode.SelectSingleNode(elementName);
        if (node == null)
        {
            return null;
        }

        if (node is XmlElement)
        {
            var value = (node as XmlElement).GetAttribute(attrName);
            return value;
        }
        else
        {
            return null;
        }
    }

    public static PersistenceData Load (string fileName)
    {
        return LoadInternal(fileName);
    }

    #endregion
}