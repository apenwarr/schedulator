using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Basecamp {

/*
<comment id="#{id}">
  <post_id>#{post_id}</post_id>
  <creator_name>#{creator_name}</creator_name>
  <creator_id>#{creator_id}</creator_id>
  <body>#{body}</body>
  <posted_on>#{posted_on}</posted_on>
</comment>
*/
public class Comment {
    private int _PostID;
    public int PostID {
        get {
            return _PostID;
        }
        set {
            _PostID = value;
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

    private string _Body;
    public string Body {
        get {
            return _Body;
        }
        set {
            _Body = value;
        }
    }

    private DateTime _PostedOn;
    public DateTime PostedOn {
        get {
            return _PostedOn;
        }
        set {
            _PostedOn = value;
        }
    }

    public Comment(int postID, int authorID, string body, DateTime postedOn) {
        _PostID = postID;
        _AuthorID = authorID;
        _Body = body;
        _PostedOn = postedOn;
    }

    public static IList<Comment> Parse(XmlNodeList commentNodes) {
        IList<Comment> comments = new List<Comment>();
        Comment c = null;

        foreach (XmlElement node in commentNodes) {
            int postID = int.Parse(node.GetElementsByTagName("post-id").Item(0).InnerText);
            int authorID = int.Parse(node.GetElementsByTagName("author-id").Item(0).InnerText);
            string body = node.GetElementsByTagName("body").Item(0).InnerText;
            DateTime postedOn = DateTime.Parse(node.GetElementsByTagName("posted-on").Item(0).InnerText);

            c = new Comment(postID, authorID, body, postedOn);
            
            comments.Add(c);
        }

        return comments;
    }
    
    /*
    <request>
      <comment>
        <post-id>#{post_id}</post-id>
        <body>#{body}</body>
      </comment>
    </request>
    */
    public static string CreateCommentRequest(int postID, string body) {
        System.IO.StringWriter sw = new System.IO.StringWriter();
        XmlWriter w = new XmlTextWriter(sw);
        
        w.WriteStartElement("request");
            w.WriteStartElement("comment");

                w.WriteStartElement("post-id");
                w.WriteString(postID.ToString());
                w.WriteEndElement(); //end postID

                w.WriteStartElement("body");
                w.WriteString(body);
                w.WriteEndElement(); //end body

            w.WriteEndElement(); //end comment

        w.WriteEndElement(); //end request

        return sw.ToString();
    }

    /*
    <request>
      <comment_id>#{comment_id}</comment_id>
      <comment>
        <body>#{body}</body>
      </comment>
    </request>
    */
    public static string UpdateCommentRequest(int postID, string body) {
        System.IO.StringWriter sw = new System.IO.StringWriter();
        XmlWriter w = new XmlTextWriter(sw);
        
        w.WriteStartElement("request");
            w.WriteStartElement("comment_id");
            w.WriteString(postID.ToString());
            w.WriteEndElement();

            w.WriteStartElement("comment");

                w.WriteStartElement("body");
                w.WriteString(body);
                w.WriteEndElement(); //end body

            w.WriteEndElement(); //end comment

        w.WriteEndElement(); //end request
        

        return sw.ToString();
    }

}

}
