using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

/*
<milestone>
  <id type="integer">#{id}</id>
  <title>#{title}</title>
  <deadline type="date">#{deadline}</deadline>
  <completed type="boolean">#{true|false}</completed>
  <project-id type="integer">#{project_id}</project-id>
  <created-on type="datetime">#{created_on}</created-on>
  <creator-id type="integer">#{creator_id}</creator-id>
  <responsible-party-id type="integer">#{responsible_party_id}</responsible-party-id>
  <responsible-party-type>#{responsible_party_type}</responsible-party-type>

  <!-- if the milestone has been completed -->
  <completed-on type="datetime">#{completed_on}</completed-on>
  <completer-id type="integer">#{completer_id}</completer-id>
</milestone>
*/
public class Milestone {
    private int _ID;
    public int ID {
        get {
            return _ID;
        }
        set {
            _ID = value;
        }
    }

    private string _Title;
    public string Title {
        get {
            return _Title;
        }
        set {
            _Title = value;
        }
    }

    private DateTime _Due;
    public DateTime Due {
        get {
            return _Due;
        }
        set {
            _Due = value;
        }
    }

    private bool _Completed;
    public bool Completed {
        get {
            return _Completed;
        }
        set {
            _Completed = value;
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

    private DateTime _Created;
    public DateTime Created {
        get {
            return _Created;
        }
        set {
            _Created = value;
        }
    }

    private int _CreatedByPersonID;
    public int CreatedByPersonID {
        get {
            return _CreatedByPersonID;
        }
        set {
            _CreatedByPersonID = value;
        }
    }

    private int _ResponsiblePartyID;
    public int ResponsiblePartyID {
        get {
            return _ResponsiblePartyID;
        }
        set {
            _ResponsiblePartyID = value;
        }
    }

    private string _ResponsiblePartyType;
    public string ResponsiblePartyType {
        get {
            return _ResponsiblePartyType;
        }
        set {
            _ResponsiblePartyType = value;
        }
    }

    private DateTime _CompletedOn;
    public DateTime CompletedOn {
        get {
            return _CompletedOn;
        }
        set {
            _CompletedOn = value;
        }
    }

    private int _CompletedID;
    public int CompletedID {
        get {
            return _CompletedID;
        }
        set {
            _CompletedID = value;
        }
    }

    public Milestone(int id, string title, DateTime due, bool completed, int projectID, DateTime created, int createdByPersonID, int responsiblePartyID, string responsiblePartyType, DateTime completedOn, int completedID) {
        _ID = id;
        _Title = title;
        _Due = due;
        _Completed = completed;
        _ProjectID = projectID;
        _Created = created;
        _CreatedByPersonID = createdByPersonID;
        _ResponsiblePartyID = responsiblePartyID;
        _ResponsiblePartyType = responsiblePartyType;
        _CompletedOn = completedOn;
        _CompletedID = completedID;
    }

    public static IList<Milestone> Parse(XmlNodeList milestoneNodes) {
        IList<Milestone> milestones = new List<Milestone>();

        foreach (XmlElement node in milestoneNodes) {
            int id = int.Parse(node.GetElementsByTagName("id").Item(0).InnerText);
            string title = node.GetElementsByTagName("title").Item(0).InnerText;
            DateTime due = DateTime.Parse(node.GetElementsByTagName("deadline").Item(0).InnerText);
            bool completed = bool.Parse(node.GetElementsByTagName("completed").Item(0).InnerText);
            int projectID = int.Parse(node.GetElementsByTagName("project-id").Item(0).InnerText);
            DateTime createdOn = DateTime.Parse(node.GetElementsByTagName("created-on").Item(0).InnerText);
            int createdByPersonID = int.Parse(node.GetElementsByTagName("creator-id").Item(0).InnerText);
            int responsiblePartyID = int.Parse(node.GetElementsByTagName("responsible-party-id").Item(0).InnerText);
            string responsiblePartyType = node.GetElementsByTagName("responsible-party-type").Item(0).InnerText;
            DateTime completedOn = DateTime.Parse(node.GetElementsByTagName("completed-on").Item(0).InnerText);
            int completedID = 0;

            try {
                completedID = int.Parse(node.GetElementsByTagName("completed-id").Item(0).InnerText);
            }
            catch { } 

            Milestone m = new Milestone(id, title, due, completed, projectID, createdOn, createdByPersonID, responsiblePartyID, responsiblePartyType, completedOn, completedID);

            milestones.Add(m);
        }

        return milestones;
    }


}
