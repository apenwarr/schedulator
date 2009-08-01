using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

/*
<todo-list>
  <id type="integer">#{id}</id>
  <name>#{name}</name>
  <description>#{description}</description>
  <project-id type="integer">#{project_id}</project-id>
  <milestone-id type="integer">#{milestone_id}</milestone-id>
  <position type="integer">#{position}</position>

  <!-- if user can see private lists -->
  <private type="boolean">#{private}</private>

  <!-- if todo-items are included in the response -->
  <todo-items>
    <todo-item>
      ...
    </todo-item>
    <todo-item>
      ...
    </todo-item>
    ...
  </todo-items>
</todo-list>
*/
public class TodoList {
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

    private string _Description;
    public string Description {
        get {
            return _Description;
        }
        set {
            _Description = value;
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

    private int _MilestoneID;
    public int MilestoneID {
        get {
            return _MilestoneID;
        }
        set {
            _MilestoneID = value;
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

    private IList<TodoItem> _TodoItems;
    public IList<TodoItem> TodoItems {
        get {
            return _TodoItems;
        }
        set {
            _TodoItems = value;
        }
    }

    public TodoList(int id, string name, string description, int projectID, int milestoneID, int position, IList<TodoItem> todoItems) {
        _ID = id;
        _Name = name;
        _Description = description;
        _ProjectID = projectID;
        _MilestoneID = milestoneID;
        _Position = position;
        _TodoItems = todoItems;
    }

    public static IList<TodoList> Parse(XmlNodeList listNodes) {
        IList<TodoList> lists = new List<TodoList>();
        TodoList l = null;

        foreach (XmlElement node in listNodes) {
            int id = int.Parse(node.GetElementsByTagName("id").Item(0).InnerText);
            string name = node.GetElementsByTagName("name").Item(0).InnerText;
            string description = node.GetElementsByTagName("description").Item(0).InnerText;
            int projectID = int.Parse(node.GetElementsByTagName("project-id").Item(0).InnerText);
            int position = int.Parse(node.GetElementsByTagName("position").Item(0).InnerText);

            int milestoneID = 0;

            try {
                milestoneID = int.Parse(node.GetElementsByTagName("milestone-id").Item(0).InnerText);
            }
            catch { }
            
            IList<TodoItem> items = TodoItem.Parse(node.SelectNodes("//todo-item"));
            l = new TodoList(id, name, description, projectID, milestoneID, position, items);

            lists.Add(l);
        }

        return lists;
    }
}
