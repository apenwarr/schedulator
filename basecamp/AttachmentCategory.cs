using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

/*
<attachment-category>
  <id type="integer">#{id}</id>
  <name>#{name}</name>
  <project-id type="integer">#{project_id}</project-id>
  <elements-count type="integer">#{elements_count}</elements-count>
</attachment-category>
*/
public class AttachmentCategory {
    private int _ElementsCount;
    public int ElementsCount {
        get {
            return _ElementsCount;
        }
        set {
            _ElementsCount = value;
        }
    }
    private int _ID;
    public int ID {
        get {
            return _ID;
        }
        set {
            _ID = value;
        }
    }

    private string _Name;
    public string Name {
        get {
            return _Name;
        }
        set {
            _Name = value;
        }
    }

    private int _ProjectID;
    public int ProjectID {
        get {
            return _ProjectID;
        }
        set {
            _ProjectID = value;
        }
    }

    //abbreviated version
    public AttachmentCategory(int id, string name) {
        _ID = id;
        _Name = name;
    }

    //full version
    public AttachmentCategory(int id, string name, int projectID, int elementsCount) {
        _ID = id;
        _Name = name;
        _ProjectID = projectID;
        _ElementsCount = elementsCount;
    }

    public static IList<AttachmentCategory> Parse(XmlNodeList categoryNodes) {
        IList<AttachmentCategory> categories = new List<AttachmentCategory>();
        AttachmentCategory c = null;

        foreach (XmlElement node in categoryNodes) {
            int id = int.Parse(node.GetElementsByTagName("id").Item(0).InnerText);
            string name = node.GetElementsByTagName("name").Item(0).InnerText;
            int elementsCount = int.Parse(node.GetElementsByTagName("elements-count").Item(0).InnerText);
            int projectID = int.Parse(node.GetElementsByTagName("project-id").Item(0).InnerText);

            c = new AttachmentCategory(id, name, projectID, elementsCount);
            
            categories.Add(c);
        }

        return categories;
    }

}
