using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Basecamp {

/*
<todo-item>
  <id type="integer">#{id}</id>
  <content>#{content}</content>
  <position type="integer">#{position}</position>
  <created-on type="datetime">#{created_on}</created-on>
  <creator-id type="integer">#{creator_id}</creator-id>
  <completed type="boolean">#{completed}</completed>

  <!-- if the item has a responsible party -->
  <responsible-party-type>#{responsible_party_type}</responsible-party-type>
  <responsible-party-id type="integer">#{responsible_party_id}</responsible-party-id>

  <!-- if the item has been completed -->
  <completed-on type="datetime">#{completed_on}</completed-on>
  <completer-id type="integer">#{completer_id}</completer-id>
</todo-item>
*/
public class TodoItem {
    private int _ID;
    public int ID {
        get {
            return _ID;
        }
        set {
            _ID = value;
        }
    }

    private string _Content;
    public string Content {
        get {
            return _Content;
        }
        set {
            _Content = value;
        }
    }

    private int _Position;
    public int Position {
        get {
            return _Position;
        }
        set {
            _Position = value;
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
    public int CreatedByPersonID
    {
    	get
    	{
    		return _CreatedByPersonID;
    	}
    	set
    	{
    		_CreatedByPersonID = value;
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

    private string _ResponsiblePartyType;
    public string ResponsiblePartyType {
        get {
            return _ResponsiblePartyType;
        }
        set {
            _ResponsiblePartyType = value;
        }
    }

    private int _ResponsiblePartyPersonID;
    public int ResponsiblePartyPersonID {
        get {
            return _ResponsiblePartyPersonID;
        }
        set {
            _ResponsiblePartyPersonID = value;
        }
    }    

    private DateTime _DateCompleted;
    public DateTime DateCompleted {
        get {
            return _DateCompleted;
        }
        set {
            _DateCompleted = value;
        }
    }

    private int _CompletedByPersonID;
    public int CompletedByPersonID {
        get {
            return _CompletedByPersonID;
        }
        set {
            _CompletedByPersonID = value;
        }
    }

    public TodoItem(int id, string content, int position, DateTime created, int createdByPersonID, bool completed, string responsiblePartyType, int responsiblePartyPersonID, DateTime completedOn, int completedByPersonID) {
        _ID = id;
        _Content = content;
        _Position = position;
        _Created = created;
        _CreatedByPersonID = createdByPersonID;
        _Completed = completed;
        _ResponsiblePartyType = responsiblePartyType;
        _ResponsiblePartyPersonID = responsiblePartyPersonID;
        _DateCompleted = DateCompleted;
        _CompletedByPersonID = completedByPersonID;
    }

    public static IList<TodoItem> Parse(XmlNodeList itemNodes) {
        IList<TodoItem> items = new List<TodoItem>();
        TodoItem i = null;

        foreach (XmlElement node in itemNodes) {
            int id = int.Parse(node.GetElementsByTagName("id").Item(0).InnerText);
            string content = node.GetElementsByTagName("content").Item(0).InnerText;
            int position = int.Parse(node.GetElementsByTagName("position").Item(0).InnerText);
            DateTime createdOn = DateTime.Parse(node.GetElementsByTagName("created-on").Item(0).InnerText);
            int createdByPersonID = int.Parse(node.GetElementsByTagName("creator-id").Item(0).InnerText);
            bool completed = bool.Parse(node.GetElementsByTagName("completed").Item(0).InnerText);
            string responsiblePartyType = node.GetElementsByTagName("responsible-party-type").Item(0).InnerText;
            int responsiblePartyPersonID = int.Parse(node.GetElementsByTagName("responsible-party-id").Item(0).InnerText);
            DateTime completedOn = DateTime.Parse("1/1/1900");
            int completedByPersonID = 0;

            try {
                completedOn = DateTime.Parse(node.GetElementsByTagName("completed-on").Item(0).InnerText);
                completedByPersonID = int.Parse(node.GetElementsByTagName("completer-id").Item(0).InnerText);
            }
            catch { }

            i = new TodoItem(id, content, position, createdOn, createdByPersonID, completed, responsiblePartyType, responsiblePartyPersonID, completedOn, completedByPersonID);

            items.Add(i);
        }

        return items;
    }

}

}
