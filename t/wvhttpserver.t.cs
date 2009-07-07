/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
#include "wvtest.cs.h"
using System;
using Wv.Test;
using Wv;

[TestFixture]
public class WvHttpTests
{
    [Test] [Category("http")] 
    public void http_request_tests()
    {
        string path = "/testuri";
        string query = "parm1=1&parm2=2";
        string request = String.Format("{0}?{1}", path, query);
        WvHttpRequest t = new WvHttpRequest(request);

        WVPASSEQ(t.request_uri, request);
        WVPASSEQ(t.path, path);
        WVPASSEQ(t.query_string, query);
        WVPASS(t.request_method == Wv.Web.Cgi.Method.Get);

        t.parse_header("Test-Header: asdf");
        WVPASSEQ(t.headers["test-header"], "asdf");
    }
    
    [Test] [Category("http")]
    public void http_cgi_request_tests()
    {
        string path = "/testuri";
        string query = "parm1=1&parm2=2";
        string request = String.Format("{0}?{1}", path, query);
        Environment.SetEnvironmentVariable("SCRIPT_NAME", path);
        Environment.SetEnvironmentVariable("QUERY_STRING", query);
        Environment.SetEnvironmentVariable("REQUEST_URI", request);
        Environment.SetEnvironmentVariable("HTTP_USER_AGENT", "asdf");
        Environment.SetEnvironmentVariable("REQUEST_METHOD", "GET");
        WvHttpCgiRequest t = new WvHttpCgiRequest();

        WVPASSEQ(t.request_uri, request);
        WVPASS(t.request_method == Wv.Web.Cgi.Method.Get);
        WVPASSEQ(t.path, path);
        WVPASSEQ(t.query_string, query);
        WVPASSEQ(t.headers["user-agent"], "asdf");
    }
}
