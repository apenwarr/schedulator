using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Basecamp {

/*
<person>
  <id type="integer">#{id}</id>
  <first-name>#{first_name}</first-name>
  <last-name>#{last_name}</last-name>
  <title>#{title}</title>
  <email-address>#{email_address}</email-address}
  <im-handle>#{im_handle}</im-handle>
  <im-service>#{im_service}</im-service>
  <phone-number-office>#{phone_number_office}</phone-number-office>
  <phone-number-office-ext>#{phone_number_office_ext}</phone-number-office-ext>
  <phone-number-mobile>#{phone_number_mobile}</phone-number-mobile>
  <phone-number-home>#{phone_number_home}</phone-number-home>
  <phone-number-fax>#{phone_number_fax}</phone-number-fax>
  <last-login type="datetime">#{last_login}</last-login>
  <client-id type="integer">#{client_id}</client-id>

  <!-- if user is an administrator, or is self -->
  <user-name>#{user_name}</user-name>

  <!-- if user is self -->
  <password>#{password}</password>
  <token>#{token}</token>

  <!-- if user is an administrator -->
  <administrator type="boolean">#{administrator}</administrator>
  <deleted type="boolean">#{deleted}</deleted>
  <has-access-to-new-projects type="boolean">#{has_access_to_new_projects}</has-access-to-new-projects>
</person>
*/
public class Person {
    private int _ID;
    public int ID {
        get {
            return _ID;
        }
        set {
            _ID = value;
        }
    }

    private string _FirstName;
    public string FirstName {
        get {
            return _FirstName;
        }
        set {
            _FirstName = value;
        }
    }

    private string _LastName;
    public string LastName {
        get {
            return _LastName;
        }
        set {
            _LastName = value;
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

    private string _Email;
    public string Email {
        get {
            return _Email;
        }
        set {
            _Email = value;
        }
    }

    private string _IMHandle;
    public string IMHandle {
        get {
            return _IMHandle;
        }
        set {
            _IMHandle = value;
        }
    }

    private string _IMService;
    public string IMService {
        get {
            return _IMService;
        }
        set {
            _IMService = value;
        }
    }

    private string _PhoneNumberOffice;
    public string phoneNumberOffice {
        get {
            return _PhoneNumberOffice;
        }
        set {
            _PhoneNumberOffice = value;
        }
    }

    private string _PhoneNumberOfficeExt;
    public string PhoneNumberOfficeExt {
        get {
            return _PhoneNumberOfficeExt;
        }
        set {
            _PhoneNumberOfficeExt = value;
        }
    }

    private string _PhoneNumberMobile;
    public string PhoneNumberMobile {
        get {
            return _PhoneNumberMobile;
        }
        set {
            _PhoneNumberMobile = value;
        }
    }

    private string _PhoneNumberHome;
    public string PhoneNumberHome {
        get {
            return _PhoneNumberHome;
        }
        set {
            _PhoneNumberHome = value;
        }
    }

    private string _PhoneNumberFax;
    public string PhoneNumberFax {
        get {
            return _PhoneNumberFax;
        }
        set {
            _PhoneNumberFax = value;
        }
    }
    private DateTime _LastLogin;
    public DateTime LastLogin {
        get {
            return _LastLogin;
        }
        set {
            _LastLogin = value;
        }
    }

    private int _ClientID;
    public int ClientID {
        get {
            return _ClientID;
        }
        set {
            _ClientID = value;
        }
    }

    private string _UserName;
    public string UserName {
        get {
            return _UserName;
        }
        set {
            _UserName = value;
        }
    }

    private bool _IsAdministrator;
    public bool IsAdministrator {
        get {
            return _IsAdministrator;
        }
        set {
            _IsAdministrator = value;
        }
    }

    private bool _IsDeleted;
    public bool IsDeleted {
        get {
            return _IsDeleted;
        }
        set {
            _IsDeleted = value;
        }
    }

    private bool _HasAccessToNewProjects;
    public bool HasAccessToNewProjects {
        get {
            return _HasAccessToNewProjects;
        }
        set {
            _HasAccessToNewProjects = value;
        }
    }


    public Person(int id, string firstName, string lastName, string title, string email, string imHandle, string imService, string phoneNumberOffice, string phoneNumberOfficeExt, string phoneNumberHome, string phoneNumberMobile, string phoneNumberFax, DateTime lastLogin, int clientID, string userName, bool isAdministrator, bool isDeleted, bool hasAccessToNewProjects) {
        _ID = id;
        _FirstName = firstName;
        _LastName = lastName;
        _Title = title;
        _Email = email;
        _IMHandle = imHandle;
        _IMService = imService;
        _PhoneNumberOffice = phoneNumberOffice;
        _PhoneNumberOfficeExt = phoneNumberOfficeExt;
        _PhoneNumberMobile = phoneNumberMobile;
        _PhoneNumberHome = phoneNumberHome;
        _PhoneNumberFax = phoneNumberFax;
        _LastLogin = lastLogin;
        _ClientID = clientID;
        _UserName = userName;
        _IsAdministrator = IsAdministrator;
        _IsDeleted = isDeleted;
        _HasAccessToNewProjects = hasAccessToNewProjects;
    }

    public static IList<Person> Parse(XmlNodeList peopleNodes) {
        IList<Person> people = new List<Person>();

        foreach (XmlElement node in peopleNodes) {
            int id = int.Parse(node.GetElementsByTagName("id").Item(0).InnerText);
            string firstName = node.GetElementsByTagName("first-name").Item(0).InnerText;
            string lastName = node.GetElementsByTagName("last-name").Item(0).InnerText;
            string title = node.GetElementsByTagName("title").Item(0).InnerText;
            string email = node.GetElementsByTagName("email-address").Item(0).InnerText;
            string imHandle = node.GetElementsByTagName("im-handle").Item(0).InnerText;
            string imService = node.GetElementsByTagName("im-service").Item(0).InnerText;
            string office = node.GetElementsByTagName("phone-number-office").Item(0).InnerText;
            string officeExt = node.GetElementsByTagName("phone-number-office-ext").Item(0).InnerText;
            string mobile = node.GetElementsByTagName("phone-number-mobile").Item(0).InnerText;
            string home = node.GetElementsByTagName("phone-number-home").Item(0).InnerText;
            string fax = node.GetElementsByTagName("phone-number-fax").Item(0).InnerText;
            
            int clientID = int.Parse(node.GetElementsByTagName("client-id").Item(0).InnerText);
            string userName = "";
            bool isAdministrator = false;
            bool isDeleted = false;
            bool hasAccessToNewProjects = false;
            DateTime lastLogin = DateTime.Parse("1/1/1900");
            try {
                lastLogin = DateTime.Parse(node.GetElementsByTagName("last-login").Item(0).InnerText);
                userName = node.GetElementsByTagName("user-name").Item(0).InnerText;
                isAdministrator = bool.Parse(node.GetElementsByTagName("administrator").Item(0).InnerText);
                isDeleted = bool.Parse(node.GetElementsByTagName("deleted").Item(0).InnerText);
                hasAccessToNewProjects = bool.Parse(node.GetElementsByTagName("has-access-to-new-projects").Item(0).InnerText);
            }
            catch {
            }
            Person p = new Person(id, firstName, lastName, title, email, imHandle, imService, office, officeExt, home, mobile, fax, lastLogin, clientID, userName, isAdministrator, isDeleted, hasAccessToNewProjects);

            people.Add(p);
        }

        return people;
    }





}

}
