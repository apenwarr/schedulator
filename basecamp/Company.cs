using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

/*
<company>
  <id type="integer">#{id}</id>
  <name>#{name}</name>
  <address-one>#{address_one}</address-one>
  <address-two>#{address_two}</address-two>
  <city>#{city}</city>
  <state>#{state}</state>
  <zip>#{zip}</zip>
  <country>#{country}</country>
  <web-address>#{web_address}</web-address>
  <phone-number-office>#{phone_number_office></phone-number-office>
  <phone-number-fax>#{phone_number_fax}</phone-number-fax>
  <time-zone-id>#{time_zone_id}</time-zone-id>
  <can-see-private type="boolean">#{can_see_private}</can-see-private>

  <!-- for non-client companies -->
  <url-name>#{url_name}</url-name>
</company>
*/
public class Company {
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

    private string _Address1;
    public string Address1 {
        get {
            return _Address1;
        }
        set {
            _Address1 = value;
        }
    }

    private string _Address2;
    public string Address2 {
        get {
            return _Address2;
        }
        set {
            _Address2 = value;
        }
    }

    private string _City;
    public string City {
        get {
            return _City;
        }
        set {
            _City = value;
        }
    }

    private string _State;
    public string State {
        get {
            return _State;
        }
        set {
            _State = value;
        }
    }

    private string _Zip;
    public string Zip {
        get {
            return _Zip;
        }
        set {
            _Zip = value;
        }
    }

    private string _Country;
    public string Country {
        get {
            return _Country;
        }
        set {
            _Country = value;
        }
    }

    private string _WebAddress;
    public string WebAddress {
        get {
            return _WebAddress;
        }
        set {
            _WebAddress = value;
        }
    }

    private string _PhoneNumberOffice;
    public string PhoneNumberOffice {
        get {
            return _PhoneNumberOffice;
        }
        set {
            _PhoneNumberOffice = value;
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

    private string _TimeZone;
    public string TimeZone {
        get {
            return _TimeZone;
        }
        set {
            _TimeZone = value;
        }
    }

    private bool _CanSeePrivate;
    public bool CanSeePrivate {
        get {
            return _CanSeePrivate;
        }
        set {
            _CanSeePrivate = value;
        }
    }

    private string _URLName;
    public string URLName {
        get {
            return _URLName;
        }
        set {
            _URLName = value;
        }
    }


    //abbreviated version
    public Company(int id, string name) {
        _ID = id;
        _Name = name;
    }

    //full version
    public Company(int id, string name, string address1, string address2, string city, string state, string zip, string country, string webAddress, string phoneNumberOffice, string phoneNumberFax, string timeZone, bool canSeePrivate, string urlName) {
        _ID = id;
        _Name = name;
        _Address1 = address1;
        _Address2 = address2;
        _City = city;
        _State = state;
        _Zip = zip;
        _Country = country;
        _WebAddress = webAddress;
        _PhoneNumberOffice = phoneNumberOffice;
        _PhoneNumberFax = phoneNumberFax;
        _TimeZone = timeZone;
        _CanSeePrivate = canSeePrivate;
        _URLName = urlName;
    }

    public static IList<Company> Parse(XmlNodeList companyNodes, bool loadAbbreviatedVersion) {
        IList<Company> companies = new List<Company>();
        Company c = null;

        foreach (XmlElement node in companyNodes) {
            int id = int.Parse(node.GetElementsByTagName("id").Item(0).InnerText);
            string name = node.GetElementsByTagName("name").Item(0).InnerText;

            if (loadAbbreviatedVersion) {
                c = new Company(id, name);
            }
            else {
                string address1 = node.GetElementsByTagName("address-one").Item(0).InnerText;
                string address2 = node.GetElementsByTagName("address-two").Item(0).InnerText;
                string city = node.GetElementsByTagName("city").Item(0).InnerText;
                string state = node.GetElementsByTagName("state").Item(0).InnerText;
                string zip = node.GetElementsByTagName("zip").Item(0).InnerText;
                string country = node.GetElementsByTagName("country").Item(0).InnerText;
                string webAddress = node.GetElementsByTagName("web-address").Item(0).InnerText;
                string office = node.GetElementsByTagName("phone-number-office").Item(0).InnerText;
                string fax = node.GetElementsByTagName("phone-number-fax").Item(0).InnerText;
                string timeZone = node.GetElementsByTagName("time-zone-id").Item(0).InnerText;
                string urlName = node.GetElementsByTagName("url-name").Item(0).InnerText;

                bool canSeePrivate = false;
                try {
                    canSeePrivate = bool.Parse(node.GetElementsByTagName("can-see-private").Item(0).InnerText);
                }
                catch { }

                c = new Company(id, name, address1, address2, city, state, zip, country, webAddress, office, fax, timeZone, canSeePrivate, urlName);
            }

            if (c != null) {
                companies.Add(c);
            }
        }

        return companies;
    }

}
