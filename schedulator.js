
// For each row in table id 'ixTable', if its class matches 'class', set
// style property 'propname' to 'val'
function changeStyleInTableRows(ixTable, myclass, propname, val) 
{
    var t = document.getElementById(ixTable);
    for (i = 0; i < t.rows.length; i++) 
    {
    	var at = t.rows[i].attributes["milestone"];
        if (at && at.nodeValue == myclass) 
            t.rows[i].style[propname] = val;
    }
}

function hideBugs(ixClass, ixTable) 
{
    var hidelink = document.getElementById(ixClass + "_hidelink");
    var showlink = document.getElementById(ixClass + "_showlink");

    showlink.style["display"] = "";
    hidelink.style["display"] = "none";

    changeStyleInTableRows(ixTable, ixClass, "display", "none");
}

function showBugs(ixClass, ixTable)
{
    var hidelink = document.getElementById(ixClass + "_hidelink");
    var showlink = document.getElementById(ixClass + "_showlink");

    hidelink.style["display"] = "";
    showlink.style["display"] = "none";

    changeStyleInTableRows(ixTable, ixClass, "display", '');
}
