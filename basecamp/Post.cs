using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

/*
<post>
  <id type="integer">#{id}</id>
  <title>#{title}</title>
  <body>#{body}</body>
  <posted-on type="datetime">#{posted_on}</posted-on>
  <project-id type="integer">#{project_id}</project-id>
  <category-id type="integer">#{category_id}</category-id>
  <author-id type="integer">#{author_id}</author-id>
  <milestone-id type="integer">#{milestone_id}</milestone-id>
  <comments-count type="integer">#{comments_count}</comments-count>
  <attachments-count type="integer">#{attachments_count}</attachments-count>
  <use-textile type="boolean">#{use_textile}</use-textile>
  <extended-body>#{extended_body}</extended-body>
  <display-body>#{display_body}</display-body>
  <display-extended-body>#{display_extended_body}</display-extended-body>

  <!-- if user can see private posts -->
  <private type="boolean">#{private}</private>
</post>
*/
public class Post {
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

    private string _Body;
    public string Body
    {
    	get
    	{
    		return _Body;
    	}
    	set
    	{
    		_Body = value;
    	}
    }

    private DateTime _Posted;
    public DateTime Posted {
        get {
            return _Posted;
        }
        set {
            _Posted = value;
        }
    }

    private int _CategoryID;
    public int CategoryID {
        get {
            return _CategoryID;
        }
        set {
            _CategoryID = value;
        }
    }

    private int _AuthorID;
    public int AuthorID {
        get {
            return _AuthorID;
        }
        set {
            _AuthorID = value;
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

    private int _CommentsCount;
    public int CommentsCount {
        get {
            return _CommentsCount;
        }
        set {
            _CommentsCount = value;
        }
    }

    private int _AttachmentsCount;
    public int AttachmentsCount {
        get {
            return _AttachmentsCount;
        }
        set {
            _AttachmentsCount = value;
        }
    }

    private bool _UseTextile;
    public bool useTextile {
        get {
            return _UseTextile;
        }
        set {
            _UseTextile = value;
        }
    }

    private string _ExtendedBody;
    public string ExtendedBody {
        get {
            return _ExtendedBody;
        }
        set {
            _ExtendedBody = value;
        }
    }

    private string _DisplayBody;
    public string DisplayBody {
        get {
            return _DisplayBody;
        }
        set {
            _DisplayBody = value;
        }
    }

    private string _DisplayExtendedBody;
    public string DisplayExtendedBody {
        get {
            return _DisplayExtendedBody;
        }
        set {
            _DisplayExtendedBody = value;
        }
    }

    private bool _IsPrivate;
    public bool IsPrivate {
        get {
            return _IsPrivate;
        }
        set {
            _IsPrivate = value;
        }
    }

    public Post(int id, string title, string body, DateTime posted, int projectID, int categoryID, int authorID, int milestoneID, int commentsCount, int attachmentsCount, bool useTextile, string extendedBody, string displayBody, string displayExtendedBody, bool isPrivate) {
        _ID = id;
        _Title = title;
        _Body = body;
        _Posted = posted;
        _ProjectID = projectID;
        _CategoryID = categoryID;
        _AuthorID = authorID;
        _MilestoneID = milestoneID;
        _CommentsCount = commentsCount;
        _AttachmentsCount = attachmentsCount;
        _UseTextile = useTextile;
        _ExtendedBody = extendedBody;
        _DisplayBody = displayBody;
        _DisplayExtendedBody = displayExtendedBody;
        _IsPrivate = isPrivate;
    }

    public static IList<Post> Parse(XmlNodeList postNodes) {
        IList<Post> posts = new List<Post>();
        Post p = null;

        foreach (XmlElement node in postNodes) {
            int id = int.Parse(node.GetElementsByTagName("id").Item(0).InnerText);
            string title = node.GetElementsByTagName("title").Item(0).InnerText;
            string body = node.GetElementsByTagName("body").Item(0).InnerText;
            DateTime posted = DateTime.Parse(node.GetElementsByTagName("posted-on").Item(0).InnerText);
            int projectID = int.Parse(node.GetElementsByTagName("project-id").Item(0).InnerText);
            int categoryID = int.Parse(node.GetElementsByTagName("category-id").Item(0).InnerText);
            int authorID = int.Parse(node.GetElementsByTagName("author-id").Item(0).InnerText);
            int milestoneID = int.Parse(node.GetElementsByTagName("milestone-id").Item(0).InnerText);
            int commentsCount = int.Parse(node.GetElementsByTagName("comments-count").Item(0).InnerText);
            int attachmentsCount = int.Parse(node.GetElementsByTagName("attachments-count").Item(0).InnerText);
            bool useTextile = bool.Parse(node.GetElementsByTagName("use-textile").Item(0).InnerText);
            string extendedBody = node.GetElementsByTagName("extended-body").Item(0).InnerText;
            string displayBody = node.GetElementsByTagName("display-body").Item(0).InnerText;
            string displayExtendedBody = node.GetElementsByTagName("display-extended-body").Item(0).InnerText;
            bool isPrivate = false;
            try {
                isPrivate = bool.Parse(node.GetElementsByTagName("private").Item(0).InnerText);
            }
            catch { }

            p = new Post(id, title, body, posted, projectID, categoryID, authorID, milestoneID, commentsCount, attachmentsCount, useTextile, extendedBody, displayBody, displayExtendedBody, isPrivate);

            posts.Add(p);
        }

        return posts;
    }

    /*
    <request>
      <post>
        <category-id>#{category_id}</category-id>
        <title>#{title}</title>
        <body>#{body}</body>
        <extended-body>#{extended_body}</extended-body>
        <use-textile>1</use_textile> <!-- omit to not use textile -->
        <private>1</private> <!-- only for firm employees -->
      </post>
      <notify>#{person_id}</notify>
      <notify>#{person_id}</notify>
      ...
      <attachments>
        <name>#{name}</name> <!-- optional -->
        <file>
          <file>#{temp_id}</file> <!-- the id of the previously uploaded file -->
          <content-type>#{content_type}</content-type>
          <original_filename>#{original_filename}</original-filename>
        </file>
      </attachments>
      <attachments>...</attachments>
      ...
    </request>
    */
    public static string CreateMessageRequest(int categoryID, string title, string body, string extendedBody, bool useTextile, string noticationUserIDs) {
        System.IO.StringWriter sw = new System.IO.StringWriter();
        XmlWriter w = new XmlTextWriter(sw);
        w.WriteStartElement("request");
            w.WriteStartElement("post");

                w.WriteStartElement("category-id");
                w.WriteString(categoryID.ToString());
                w.WriteEndElement(); //end category

                w.WriteStartElement("title");
                w.WriteString(title);
                w.WriteEndElement(); //end title

                w.WriteStartElement("body");
                w.WriteString(body);
                w.WriteEndElement(); //end body

                w.WriteStartElement("extended-body");
                w.WriteString(extendedBody);
                w.WriteEndElement(); //end extendedBody

                if (useTextile) {
                    w.WriteStartElement("use-textile");
                    w.WriteString(useTextile.ToString().ToLower());
                    w.WriteEndElement(); //end useTextile
                }

            w.WriteEndElement(); //end post

            // create notifications
            if (noticationUserIDs.Length > 0) { 
                foreach (string notifyID in noticationUserIDs.Split(',')) {
                    w.WriteStartElement("notify");
                    w.WriteString(notifyID);
                    w.WriteEndElement();
                }
            }
            w.WriteEndElement(); //end request
        
        return sw.ToString();
    }

    /*
    <request>
      <post>
        <category-id>#{category_id}</category-id>
        <title>#{title}</title>
        <body>#{body}</body>
        <extended-body>#{extended_body}</extended-body>
        <use-textile>1</use-textile> <!-- omit to not use textile -->
        <private>1</private> <!-- only for firm employees -->
      </post>
      <notify>#{person_id}</notify>
      <notify>#{person_id}</notify>
      ...
    </request>
    */
        public static string UpdateMessageRequest(int categoryID, string title, string body, string extendedBody, bool useTextile, string noticationUserIDs) {
        System.IO.StringWriter sw = new System.IO.StringWriter();
        XmlWriter w = new XmlTextWriter(sw);
        w.WriteStartElement("request");
            w.WriteStartElement("post");

                w.WriteStartElement("category-id");
                w.WriteString(categoryID.ToString());
                w.WriteEndElement(); //end category

                w.WriteStartElement("title");
                w.WriteString(title);
                w.WriteEndElement(); //end title

                w.WriteStartElement("body");
                w.WriteString(body);
                w.WriteEndElement(); //end body

                w.WriteStartElement("extended-body");
                w.WriteString(extendedBody);
                w.WriteEndElement(); //end extendedBody

                if (useTextile) { 
                    w.WriteStartElement("use-textile");
                    w.WriteString(useTextile.ToString());
                    w.WriteEndElement(); //end useTextile
                }

            w.WriteEndElement(); //end post

            // create notifications
            if (noticationUserIDs.Length > 0) {
                foreach (string notifyID in noticationUserIDs.Split(',')) {
                    w.WriteStartElement("notify");
                    w.WriteString(notifyID);
                    w.WriteEndElement();
                }
            }
            w.WriteEndElement(); //end request
        

        return sw.ToString();
    }
}