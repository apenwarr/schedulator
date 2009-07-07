/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.Text;
using Wv;
using Wv.Extensions;

namespace Wv
{
    public class WvUrl
    {
	public string method, user = null, password = null, host = null;
	public int port = 0;
	public string path;
	
	public WvUrl(string method,
		     string user, string password,
		     string host, int port,
		     string path)
	{
	    this.method = method;
	    this.user = user;
	    this.password = password;
	    this.host = host;
	    this.port = port;
	    this.path = path;
	}
	
	// input is of form:
	//    method://user:password@host:port/path/path/path
	// where all the parts are optional, eg:
	//    method://host:port
	//    method:path
	//    path  (equivalent to file:path)
	public WvUrl(string url)
	{
	    wv.assert(url != null);
	    
	    // backslashes are just another way of writing slashes, when you're
	    // talking about a URL.
	    url = url.Replace('\\', '/');
	    
	    int colon = url.IndexOf(':');
	    int slashslash = url.IndexOf("//");
	    
	    if (colon >= 0 && (slashslash < 0 || colon < slashslash))
	    {
		method = url.Substring(0, colon);
		url = url.Substring(colon+1);
	    }
	    else
		method = "file";
	    
	    // now all that's left is the part after method:
	    if (url.StartsWith("//"))
	    {
		url = url.Substring(2);
		int slash = url.IndexOf('/');
		if (slash >= 0)
		{
		    path = url.Substring(slash);
		    url = url.Substring(0, slash);
		}
		else
		    path = "";
		    
		int at = url.IndexOf('@');
		if (at >= 0)
		{
		    string userpass = url.Substring(0, at);
		    url = url.Substring(at+1);
		    
		    int ucolon = userpass.IndexOf(':');
		    if (ucolon >= 0)
		    {
			password = userpass.Substring(ucolon+1);
			user = userpass.Substring(0, ucolon);
		    }
		    else
			user = userpass;
		}
		
		// now all that's left is the host:port part
		int pcolon = url.IndexOf(':');
		if (pcolon >= 0)
		{
		    port = url.Substring(pcolon+1).atoi();
		    host = url.Substring(0, pcolon);
		}
		else
		    host = url;
	    }
	    else
	    {
		// no host part at all
		path = url;
	    }
	    
	    if (host == "") host = null;
	    if (user == "") user = null;
	    if (password == "") password = null;
	    
	    wv.assert(method != null);
	    wv.assert(path != null);
	}
	
	
	public string ToString(bool show_password)
	{
	    var sb = new StringBuilder(200);
	    
	    sb.Append(method + ":");
	    
	    // we intentionally ignore the username/password if there's
	    // no hostname; what would it mean?
	    if (host != null || port != 0)
	    {
		sb.Append("//");
		
		if (user != null || (password != null && show_password))
		{
		    if (user != null)
			sb.Append(user);
		    if (password != null && show_password)
			sb.AppendFormat(":{0}", password);

		    sb.Append("@");
		}
		
		if (host != null)
		    sb.Append(host);
		if (port != 0)
		    sb.AppendFormat(":{0}", port);
		
		if (path != "" && !path.StartsWith("/"))
		    sb.Append("/");
	    }
	    
	    sb.Append(path);
	    return sb.ToString();
	}
	
	
	public override string ToString()
	{
	    return ToString(false);
	}
    }
}
