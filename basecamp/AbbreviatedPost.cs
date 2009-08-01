using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Basecamp {

/*
<post>
  <id type="integer">#{id}</id>
  <title>#{title}</title>
  <posted-on type="datetime">#{posted_on}</posted-on>
  <attachments-count type="integer">#{attachments_count}</attachments-count>
  <category>
    <id type="integer">#{id}</id>
    <name>#{name}</name>
  </category>
</post>
*/
public class AbbreviatedPost {
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

    private DateTime _Posted;
    public DateTime Posted {
        get {
            return _Posted;
        }
        set {
            _Posted = value;
        }
    }

    private PostCategory _Category;
    public PostCategory Category {
        get {
            return _Category;
        }
        set {
            _Category = value;
        }
    }

    public AbbreviatedPost(int id, string title, DateTime posted, PostCategory category) {
        _ID = id;
        _Title = title;
        _Posted = posted;
        _Category = category;
    }

    public static IList<AbbreviatedPost> Parse(XmlNodeList postNodes) {
        IList<AbbreviatedPost> abbreviatedPosts = new List<AbbreviatedPost>();

        foreach (XmlElement node in postNodes) {
            int id = int.Parse(node.GetElementsByTagName("id").Item(0).InnerText);
            string title = node.GetElementsByTagName("title").Item(0).InnerText;
            DateTime posted = DateTime.Parse(node.GetElementsByTagName("posted-on").Item(0).InnerText);
            IList<PostCategory> categories = PostCategory.Parse(node.SelectNodes("category"), true);

            AbbreviatedPost p = new AbbreviatedPost(id, title, posted, categories[0]);

            abbreviatedPosts.Add(p);
        }

        return abbreviatedPosts;
    }

}

}