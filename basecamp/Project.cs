using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

/*
<project>
  <id type="integer">#{id}</id>
  <name>#{name}</name>
  <created-on type="datetime">#{created_on}</created-on>
  <status>#{status}</status>
  <last-changed-on type="datetiem">#{last_changed_on}</last-changed-on>
  <company>
    <id type="integer">#{id}</id>
    <name>#{name}</name>
  </company>

  <!-- if user is administrator, or show_announcement is true -->
  <announcement>#{announcement}</announcement>

  <!-- if user is administrator -->
  <start-page>#{start_page}</start-page>
  <show-writeboards type="boolean">#{show_writeboards}</show-writeboards>
  <show-announcement type="boolean">#{show_announcement}</show-announcement>
</project>
*/
public class Project {
    private Company _Company;
    public Company Company {
        get {
            return _Company;
        }
        set {
            _Company = value;
        }
    }    
    private string _Status;
    public string Status {
        get {
            return _Status;
        }
        set {
            _Status = value;
        }
    }
    private DateTime _Created;
    public DateTime Created {
        get {
            return _Created;
        }
        set {
            _Created = value;
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

    private DateTime _LastChanged;
    public DateTime LastChanged {
        get {
            return _LastChanged;
        }
        set {
            _LastChanged = value;
        }
    }

    public Project(int id, string name, DateTime created, string status, DateTime lastChanged, Company company) {
        _ID = id;
        _Name = name;
        _Created = created;
        _Status = status;
        _LastChanged = lastChanged;
        _Company = company;
    }

    public static IList<Project> Parse(XmlNodeList projectNodes) {
        IList<Project> projects = new List<Project>();

        foreach (XmlElement node in projectNodes) {
            int id = int.Parse(node.GetElementsByTagName("id").Item(0).InnerText);
            string name = node.GetElementsByTagName("name").Item(0).InnerText;
            DateTime created = DateTime.Parse(node.GetElementsByTagName("created-on").Item(0).InnerText);
            string status = node.GetElementsByTagName("status").Item(0).InnerText;
            IList<Company> companies = Company.Parse(node.SelectNodes("company"), true);
            DateTime lastChanged = DateTime.Parse(node.GetElementsByTagName("last-changed-on").Item(0).InnerText);

            Project p = new Project(id, name, created, status, lastChanged, companies[0]);

            projects.Add(p);
        }

        return projects;
    }
}