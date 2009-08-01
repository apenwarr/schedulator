using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Collections.Generic;

public class BasecampManager {
    #region Private Variables

    private bool m_SecureMode;
    private string m_Url;
    private string m_Username;
    private string m_Password;

    #endregion

    #region Accessors

    /// <summary>
    ///		Gets or sets a value indicating whether a secure connection should be used.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if [secure mode]; otherwise, <c>false</c>.
    /// </value>
    public bool SecureMode {
        get { return m_SecureMode; }
        set { m_SecureMode = value; }
    }

    /// <summary>
    ///		Gets or sets the URL used to connect to basecamp.  The URL does not need to be prefixed with the protocol
    /// </summary>
    /// <example>
    ///		yourcompany.grouphub.com
    ///	</example>
    public string Url {
        get { return m_Url; }
        set { m_Url = value; }
    }

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string Username {
        get { return m_Username; }
        set { m_Username = value; }
    }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password {
        get { return m_Password; }
        set { m_Password = value; }
    }

    #endregion

    #region Constructor

    public BasecampManager() {
        m_Url = System.Configuration.ConfigurationManager.AppSettings["basecamp_projectURL"];
        m_Username = System.Configuration.ConfigurationManager.AppSettings["basecamp_userName"];
        m_Password = System.Configuration.ConfigurationManager.AppSettings["basecamp_password"];
        m_SecureMode = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["basecamp_secureMode"]);
    }

    public BasecampManager(string url, string username, string password, bool secureMode) {
        m_Url = url;
        m_Username = username;
        m_Password = password;
        m_SecureMode = secureMode;
    }

    public bool IsInitialized() {
        bool result = false;
        if (m_Username != "" && m_Password != "" && m_Url != "") {
            result = true;
        }

        return result;
    }

    #endregion

    #region Generic Request
    /// <summary>
    ///		Sends a request using the basecamp API
    /// </summary>
    /// <param name="command">Eg. /projects/list</param>
    /// <param name="request">XML data</param>
    /// <returns>XmlDocument with requested data</returns>
    public XmlDocument SendRequest(string command, string request) {
        XmlDocument result = null;
        if (IsInitialized()) {
            result = new XmlDocument();

            HttpWebRequest webRequest = null;
            WebResponse webResponse = null;
            try {
                string prefix = (m_SecureMode) ? "https://" : "http://";
                string url = string.Concat(prefix, m_Url, command);

                webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";
                webRequest.ContentType = "text/xml";
                webRequest.ServicePoint.Expect100Continue = false;

                string UsernameAndPassword = string.Concat(m_Username, ":", m_Password);
                string EncryptedDetails = Convert.ToBase64String(Encoding.ASCII.GetBytes(UsernameAndPassword));
                webRequest.Headers.Add("Authorization", "Basic " + EncryptedDetails);

                using (StreamWriter sw = new StreamWriter(webRequest.GetRequestStream())) {
                    sw.WriteLine(request);
                }
                webResponse = webRequest.GetResponse();
                using (StreamReader sr = new StreamReader(webResponse.GetResponseStream())) {
                    result.Load(sr.BaseStream);
                    sr.Close();
                }
            }
            catch (Exception ex) {
                string ErrorXml = string.Format("<error>{0}</error>", ex.ToString());
                result.LoadXml(ErrorXml);
            }
            finally {
                if (webRequest != null)
                    webRequest.GetRequestStream().Close();

                if (webResponse != null)
                    webResponse.GetResponseStream().Close();
            }
        }
        return result;
    }
    #endregion

    #region Requests
    #region Categories
    public IList<AttachmentCategory> GetAttachmentCategories(int projectID) {
        string cmd = string.Format("/projects/{0}/attachment_categories", projectID);

        IList<AttachmentCategory> categories = null;
        if (IsInitialized()) {
            XmlDocument xml = SendRequest(cmd, "");

            categories = AttachmentCategory.Parse(xml.SelectNodes("//attachment-category"));
        }

        return categories;
    }
    public IList<PostCategory> GetPostCategories(int projectID) {
        string cmd = string.Format("/projects/{0}/post_categories", projectID);

        IList<PostCategory> categories = null;
        if (IsInitialized()) {
            XmlDocument xml = SendRequest(cmd, "");

            categories = PostCategory.Parse(xml.SelectNodes("//post-category"), false);
        }

        return categories;
    }

    #endregion

    #region Messages
    public Post DeleteMessage(int postID) {
        string cmd = string.Format("/msg/delete/{0}", postID);

        XmlDocument xml = SendRequest(cmd, "");
        Post post = null;
        IList<Post> posts = Post.Parse(xml.SelectNodes("//post"));
        if (posts.Count > 0) {
            post = posts[0];
        }

        return post;
    }

    public Post UpdateMessage(int postID, int categoryID, string title, string body, string extendedBody, bool useTextile, string notificationUserIDs) {
        string cmd = string.Format("/msg/update/{0}", postID);
        string request = Post.UpdateMessageRequest(categoryID, title, body, extendedBody, useTextile, notificationUserIDs);

        XmlDocument xml = SendRequest(cmd, request);
        Post post = null;
        IList<Post> posts = Post.Parse(xml.SelectNodes("//post"));
        if (posts.Count > 0) {
            post = posts[0];
        }

        return post;
    }

    public Post CreateMessage(int projectID, int categoryID, string title, string body, string extendedBody, bool useTextile, string notificationUserIDs) {
        string cmd = string.Format("/projects/{0}/msg/create", projectID);
        string request = Post.CreateMessageRequest(categoryID, title, body, extendedBody, useTextile, notificationUserIDs);

        XmlDocument xml = SendRequest(cmd, request);
        Post post = null;
        IList<Post> posts = Post.Parse(xml.SelectNodes("//post"));
        if (posts.Count > 0) {
            post = posts[0];
        }

        return post;
    }

    public IList<Post> GetMessages(string messageIDs) {
        string cmd = string.Format("/msg/get/{0}", messageIDs);

        IList<Post> posts = null;
        if (IsInitialized()) {
            XmlDocument xml = SendRequest(cmd, "");

            posts = Post.Parse(xml.SelectNodes("//post"));
        }

        return posts;
    }
    #endregion
    
    #region Comments
    public Comment DeleteComment(int postID) {
        string cmd = string.Format("/msg/delete_comment/{0}", postID);

        XmlDocument xml = SendRequest(cmd, "");
        Comment comment = null;
        IList<Comment> comments = Comment.Parse(xml.SelectNodes("//comment"));
        if (comments.Count > 0) {
            comment = comments[0];
        }

        return comment;
    }

    public Comment UpdateComment(int postID, string body) {
        string cmd = "/msg/update_comment";
        string request = Comment.UpdateCommentRequest(postID, body);

        XmlDocument xml = SendRequest(cmd, request);
        Comment comment = null;
        IList<Comment> comments = Comment.Parse(xml.SelectNodes("//comment"));
        if (comments.Count > 0) {
            comment = comments[0];
        }

        return comment;
    }

    public Comment CreateComment(int projectID, int postID, string body) {
        string cmd = "/msg/create_comment";
        string request = Comment.CreateCommentRequest(postID, body);
        XmlDocument xml = SendRequest(cmd, request);
        Comment comment = null;
        IList<Comment> comments = Comment.Parse(xml.SelectNodes("//comment"));
        if (comments.Count > 0) {
            comment = comments[0];
        }

        return comment;
    }

    public IList<Comment> GetComments(int messageID) {
        string cmd = string.Format("/msg/comments/{0}", messageID);

        IList<Comment> comments = null;
        if (IsInitialized()) {
            XmlDocument xml = SendRequest(cmd, "");

            comments = Comment.Parse(xml.SelectNodes("//comment"));
        }

        return comments;
    }
    #endregion

    #region Milestones
    public IList<Milestone> GetMilestones(int projectID) {
        string cmd = string.Format("/projects/{0}/milestones/list", projectID);

        IList<Milestone> milestones = null;
        if (IsInitialized()) {
            XmlDocument xml = SendRequest(cmd, "");

            milestones = Milestone.Parse(xml.SelectNodes("//milestone"));
        }

        return milestones;
    }
    #endregion

    #region Companies
    public IList<Company> GetCompanies() {
        string cmd = string.Format("/contacts/companies");

        IList<Company> companies = null;
        if (IsInitialized()) {
            XmlDocument xml = SendRequest(cmd, "");

            companies = Company.Parse(xml.SelectNodes("//company"), false);
        }

        return companies;
    }

    public Company GetCompany(int id) {
        string cmd = string.Format("/contacts/company/{0}", id);

        Company company = null;
        if (IsInitialized()) {
            XmlDocument xml = SendRequest(cmd, "");

            IList<Company> companies = Company.Parse(xml.SelectNodes("//company"), false);
            if (companies.Count > 0) {
                company = companies[0];
            }
        }

        return company;
    }
    #endregion

    #region People
    public IList<Person> GetPeople(int companyID) {
        string cmd = string.Format("/contacts/people/{0}", companyID);

        IList<Person> people = null;
        if (IsInitialized()) {
            XmlDocument xml = SendRequest(cmd, "");

            people = Person.Parse(xml.SelectNodes("//person"));
        }

        return people;
    }

    public IList<Person> GetPeople(int projectID, int companyID) {
        string cmd = string.Format("/projects/{0}/contacts/people/{1}", projectID, companyID);

        IList<Person> people = null;
        if (IsInitialized()) {
            XmlDocument xml = SendRequest(cmd, "");

            people = Person.Parse(xml.SelectNodes("//person"));
        }

        return people;
    }

    public Person GetPerson(int id) {
        string cmd = string.Format("/contacts/person/{0}", id);

        Person person = null;
        if (IsInitialized()) {
            XmlDocument xml = SendRequest(cmd, "");

            IList<Person> people = Person.Parse(xml.SelectNodes("//person"));
            if (people.Count > 0) {
                person = people[0];
            }
        }

        return person;
    }


    #endregion

    #region Projects
    public IList<Project> GetProjects() {
        string cmd = "/project/list";

        IList<Project> projects = null;
        if (IsInitialized()) {
            XmlDocument xml = SendRequest(cmd, "");

            projects = Project.Parse(xml.SelectNodes("//project"));
        }

        return projects;
    }
    #endregion

    #region Todo Lists
    public IList<TodoList> GetToLists(int projectID) {
        string cmd = string.Format("/projects/{0}/todos/lists", projectID);

        IList<TodoList> lists = null;
        if (IsInitialized()) {
            XmlDocument xml = SendRequest(cmd, "");

            lists = TodoList.Parse(xml.SelectNodes("//todo-list"));
        }

        return lists;
    }

    public TodoList GetTodoList(int id) {
        string cmd = string.Format("/todos/list/{0}", id);

        TodoList list = null;
        if (IsInitialized()) {
            XmlDocument xml = SendRequest(cmd, "");

            IList<TodoList> lists = TodoList.Parse(xml.SelectNodes("//todo-list"));
            if (lists.Count > 0) {
                list = lists[0];
            }
        }

        return list;
    }
    #endregion

    #region Message Archives
    public IList<AbbreviatedPost> GetMessageArchive(int projectID, int categoryID) {
        string cmd = string.Format("/projects/{0}/msg/cat/{1}/archive", projectID, categoryID);

        IList<AbbreviatedPost> posts = null;
        if (IsInitialized()) {
            XmlDocument xml = SendRequest(cmd, "");

            posts = AbbreviatedPost.Parse(xml.SelectNodes("//post"));
        }

        return posts;
    }

    public IList<AbbreviatedPost> GetMessageArchiveByProject(int projectID) {
        string cmd = string.Format("/projects/{0}/msg/archive", projectID);

        IList<AbbreviatedPost> posts = null;
        if (IsInitialized()) {
            XmlDocument xml = SendRequest(cmd, "");

            posts = AbbreviatedPost.Parse(xml.SelectNodes("//post"));
        }

        return posts;
    }
    #endregion
    #endregion
}