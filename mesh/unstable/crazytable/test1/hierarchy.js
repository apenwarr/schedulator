/**
 * Hierarchy
 *
 * Author:
 *   Michel Emond
 *   Net Integration Technologies
 *   www.net-itech.com
 *
 * Description:
 *
 * Dependencies:
 *   shared.js
 *
 * Syntax:
 *
 *   Restrictions:
 *     - The root node id must be 0 (zero).
 *     - It is assumed that the server will provide valid data with parent/child
 *       relationships that won't lead into cycles.
 *     - The server must return an Expires header to work around the cache.
 *
 * Examples:
 *   <span hierarchy="db"></span>
 *
 */

/*global alert, clearDomObj, navigator */

// initialization function called on window.onLoad event, see shared.js
_onLoad.push('hierarchyInit()');

// flag so other libraries know that Hierarchy has been loaded up
if (document.getElementById && document.getElementsByTagName && getXmlHttp()) {
    var HIERARCHY = true;
}

// configuration params, can be changed
var _getHierarchyDataScript = 'getdata.php';
var _keepExpandState = true;
var _autoExpandMaxLevel = -1; // -1 for none


// initialization stuff, do not change
var _hierarchySpan = null;
var _hierarchyXmlHttp = null;
var _hierarchyData = new Array();
var _hierarchyRows = new Array();
var _hierarchySource = '';

function hierarchyInit()
{
    if (typeof(HIERARCHY) != 'boolean') { return; }

    var tbody, table, thisSpan;
    var spans = document.getElementsByTagName("span");

    if (spans.length < 1) { return; }
    //for (var i = 0; i < spans.length; i++) {
    var i = 0;
        thisSpan = spans[i];
        if (thisSpan.getAttribute('hierarchy') !== null) {
            thisSpan.setAttribute('id', 'hierarchy' +
                thisSpan.getAttribute('hierarchy'));
            table = document.createElement('TABLE');
            tbody = document.createElement('TBODY');
            table.appendChild(tbody);
            table.cellSpacing = 2;
            table.cellPadding = 1;
            table.border = 0; //1;
            thisSpan = clearDomObj(thisSpan);
            thisSpan.appendChild(table);

            _hierarchySpan = thisSpan;
            _hierarchySource = thisSpan.getAttribute('hierarchy');
        }
    //}

// fixme: use addEventHandler here and in dynamictatus.js
/*
    window.onunload = function() {
        if (_hierarchyXmlHttp) {
            _hierarchyXmlHttp.abort();
        }
    };
*/

    sendHierarchyDataRequest(0);
}

function sendHierarchyDataRequest(getId, rowPath)
{
    if (_hierarchyXmlHttp && _hierarchyXmlHttp.readyState !== 0) {
        _hierarchyXmlHttp.abort();
    }
    if (!(_hierarchyXmlHttp = getXmlHttp())) { return; }

    if (rowPath === undefined) {
        rowPath = getId;
    }

    try {
        _hierarchyXmlHttp.open('GET', _getHierarchyDataScript +
                               '?source=' + _hierarchySource +
                               '&id=' + escape(getId) +
                               '&rowpath=' + escape(rowPath), true);
    } catch(e) {
// fixme: do something here when unable to send request, maybe server is down...
        return;
    }

    _hierarchyXmlHttp.onreadystatechange = function() {
        if (_hierarchyXmlHttp.readyState == 4) {
            // gets here in case of response or timeout
            if (_hierarchyXmlHttp.responseText &&
                _hierarchyXmlHttp.status == 200) {
                updateHierarchyData(_hierarchyXmlHttp.responseText);
            }
        }
    };

    try {
        _hierarchyXmlHttp.send(null);
    } catch(e) {
alert('catch');
// fixme: do something sensible here
    }

    return;
}

function setNode(nodeData)
{
    if (!_hierarchyData[nodeData.id]) {
        _hierarchyData[nodeData.id] = {};
    }

    var node = _hierarchyData[nodeData.id];

    if (node.nodes && node.nodes.length > 0) {
        return;
    }

    var i;

    node.id = nodeData.id;
    node.name = nodeData.name;
    node.values = new Array();
    for (i in nodeData.values) {
        node.values[i] = nodeData.values[i];
    }
    node.nodes = new Array();
    for (i in nodeData.nodes) {
        node.nodes.push(nodeData.nodes[i]);
    }
    node.nodescount = nodeData.nodescount;
}

function setRow(nodeData, rowpath, expandedNodesHash, hinfo)
{
    var node = _hierarchyData[nodeData.id];
    if (node === undefined) { return; }

    if (node.rows === undefined) {
        node.rows = {};
    }
    if (node.rows[rowpath] === undefined) {
        node.rows[rowpath] = {};
    }

    if (node.rows[rowpath].isexpanded === undefined) {
        node.rows[rowpath].isexpanded =
            (expandedNodesHash[node.id] &&
             (_autoExpandMaxLevel == -1 ||
              rowpath.split('/').length <= (_autoExpandMaxLevel+1))) ?
            true : false;
        // fixme: try to typecast expandedNodesHash[] in boolean or something...
        // this is ugly!
        if (node.rows[rowpath].isexpanded) {
            nodeData.nodes = node.nodes;
            nodeData.nodescount = node.nodescount;
        }
    }

    if (node.rows[rowpath].hinfo === undefined) {
        if (hinfo === undefined) { hinfo = ''; }
        node.rows[rowpath].hinfo = hinfo;
    } else {
        hinfo = node.rows[rowpath].hinfo;
    }

    var c = 1;
    var tempData, thisHinfo;
    for (var i in nodeData.nodes) {
        tempData = {'id':nodeData.nodes[i],'nodes':[],'nodescount':0};
        thisHinfo = hinfo + (c == nodeData.nodescount ? '1' : '0');
        setRow(tempData, rowpath+'/'+nodeData.nodes[i], expandedNodesHash,
               thisHinfo);
        c++;
    }
}

function updateHierarchyData(returnedData)
{
    if (returnedData === '') { return; }

    try {
        var data = eval('('+returnedData+')');
    }
    catch (e) {
        return;
    }

    var i;

    for (i = 0; i < data.nodes.length; i++) {
        setNode(data.nodes[i]);
    }

    var expandedNodesHash = {};
    var nodeId;
    for (i in data.expandednodes) {
        nodeId = data.expandednodes[i];
        if (_hierarchyData[nodeId] &&
            _hierarchyData[nodeId].nodes.length > 0) {
            expandedNodesHash[nodeId] = true;
        }
    }

    if (data.nodes.length > 0) {
        setRow(data.nodes[0], data.rowpath, expandedNodesHash);
        expandHierarchyRow(data.rowpath);
    }
}

function getImage(name, id)
{
    var imgId = '';
    if (id !== undefined) {
        imgId = 'id="hl' + id + '"';
    }
    return '<img src="images/' + name + '" ' + imgId + ' alt="" title="" ' +
           'width="15" height="18" border="0">';
}

function getImageName(isLastImg, isLastItem, isExpandable, isCollapsed)
{
    var name;
    if (isLastImg) {
        name = (isLastItem ? 'hl2' : 'hl3') +
               (isExpandable ? (isCollapsed ? 'p' : 'm') : '');
    } else {
        name = (isLastItem ? 'hl0' : 'hl1');
    }
    return name + '.png';
}

function getHierarchyLines(rowpath)
{
    var nodeId = rowpath.split('/').pop();
    var node = _hierarchyData[nodeId];

    var r = '';

    var imgId, isExpandable, isLastImg, isLastItem, s;

    var hinfoLength = node.rows[rowpath].hinfo.length;
    for (var i = 0; i < hinfoLength; i++) {
        isLastImg = (i == (hinfoLength-1));
        isLastItem = (node.rows[rowpath].hinfo.substr(i, 1) === '1');
        isExpandable = (node.nodescount > 0);
        imgId = (isLastImg && isExpandable) ? rowpath : undefined;

        s = getImage(getImageName(isLastImg, isLastItem, isExpandable, true), imgId);
        if (isLastImg) {
            if (isExpandable) {
                s = '<a onClick="rowClick(\''+rowpath+'\');return false;" ' +
                    'href="javascript:;">' + s + '</a>';
            }
            s += '&nbsp;';
        }

        r += s;
    }

    return r;
}

function createRow(rowpath, isHeader)
{
    var nodeId = rowpath.split('/').pop();
    var node = _hierarchyData[nodeId];
    if (isHeader === undefined) { isHeader = false; }

    var row = document.createElement('TR');
    row.setAttribute('id', rowpath);

    var cell;
    var cellTag = (isHeader ? 'TH' : 'TD');

    var isIE = !(navigator &&
                 navigator.userAgent.toLowerCase().indexOf("msie") == -1);

    if (isIE) {
        // IE is silly
        cell = document.createElement('<' + cellTag + ' nowrap="nowrap">');
    } else {
        cell = document.createElement(cellTag);
        cell.setAttribute('nowrap', 'nowrap');
    }

    if (isHeader) {
        cell.innerHTML = '<img src="images/thread.gif"> Name';
    } else {
        cell.innerHTML = getHierarchyLines(rowpath) +
                         (node.nodescount > 0 ?
                          '<a onClick="rowClick(\'' + rowpath + '\');' +
                          'return false;" href="javascript:;">' : '') +
                         node.name + (node.nodescount > 0 ? '</a>' : '') +
                         '&nbsp;';
    }
    row.appendChild(cell);

    for (var i = 0; i < node.values.length; i++) {
        if (isIE) {
            // IE is silly
            cell = document.createElement('<' + cellTag +
                (isHeader ? ' nowrap="nowrap"' : '') + '>');
        } else {
            cell = document.createElement(cellTag);
            if (isHeader) {
                cell.setAttribute('nowrap', 'nowrap');
            }
        }
        cell.setAttribute('valign', 'top');
        cell.innerHTML = node.values[i];
        row.appendChild(cell);
    }

    return row;
}

function rowClick(rowpath)
{
    var nodeId = rowpath.split('/').pop();
    var node = _hierarchyData[nodeId];

    if (node.rows[rowpath].isexpanded) {
        collapseHierarchyRow(rowpath);
    } else if (node.nodescount > node.nodes.length) {
        sendHierarchyDataRequest(nodeId, rowpath);
    } else {
        var tempData = {'id':node.id, 'nodes':node.nodes,
                        'nodescount':node.nodescount};
        setRow(tempData, rowpath, {}, node.rows[rowpath].hinfo);
        expandHierarchyRow(rowpath);
    }
}

function setRowsColor(row)
{
    var classNames = new Array('hierarchytableevenrow', 'hierarchytableoddrow');
    var curIndex = (row.className == classNames[0] ? 1 : 0);

    while ((row = row.nextSibling)) {
        row.className = classNames[curIndex];
        curIndex = 1 - curIndex;
    }
}

function expandHierarchyRow(rowpath)
{
    var curId, curRowpath, i, isLastItem, nextRow, node, nodeImg, row;
    var tempRowpath;

    var table = _hierarchySpan.childNodes[0];
    var tbody = table.childNodes[0];

    if (table.rows.length === 0) {
        row = createRow(rowpath, true);
        row.className = 'hierarchytableheaderrow';
        tbody.appendChild(row);
        _hierarchyRows[rowpath] = row;
    }

    var expandQueue = new Array();
    expandQueue.push(rowpath);

    while ((curRowpath = expandQueue.shift())) {
        curId = curRowpath.split('/').pop();
        node = _hierarchyData[curId];

        row = _hierarchyRows[curRowpath];
        nextRow = row.nextSibling;

        for (i in node.nodes) {
            tempRowpath = curRowpath+'/'+node.nodes[i];

            if (!(row = _hierarchyRows[tempRowpath])) {
                row = createRow(tempRowpath, false);
                _hierarchyRows[tempRowpath] = row;
            }

            if (nextRow) {
                tbody.insertBefore(row, nextRow);
            } else {
                tbody.appendChild(row);
            }

            if (_hierarchyData[node.nodes[i]].rows[tempRowpath].isexpanded) {
                expandQueue.push(tempRowpath);
            }
        }

        node.rows[curRowpath].isexpanded = true;

        // if any row other than the root
        if (curRowpath.split('/').length > 1) {
            nodeImg = document.getElementById('hl' + curRowpath);
            if (nodeImg) {
                isLastItem = (node.rows[curRowpath].hinfo.substr(-1) === '1');
                nodeImg.src = 'images/' +
                              getImageName(true, isLastItem, true, false);
            }
        }
    }

    setRowsColor(_hierarchyRows[rowpath]);
}

function collapseHierarchyRow(rowpath)
{
    var curNode, curNodeId, isLastItem, nextRow, nodeImg;

    var table = _hierarchySpan.childNodes[0];
    var tbody = table.childNodes[0];

    var nodeId = rowpath.split('/').pop();
    var node = _hierarchyData[nodeId];

    var row = _hierarchyRows[rowpath].nextSibling;
    curNodeId = row.id.split('/').pop();
    curNode = _hierarchyData[curNodeId];

    while (row && curNode.rows[row.id].hinfo.length >
           node.rows[rowpath].hinfo.length) {
        if (!_keepExpandState && curNode.nodescount > 0) {
            curNode.rows[row.id].isexpanded = false;
            nodeImg = document.getElementById('hl' + row.id);
            if (nodeImg) {
                isLastItem = (curNode.rows[row.id].hinfo.substr(-1) === '1');
                nodeImg.src = 'images/' +
                              getImageName(true, isLastItem, true, true);
            }
        }

        nextRow = row.nextSibling;
        tbody.removeChild(row);
        row = nextRow;
        if (row) {
            curNodeId = row.id.split('/').pop();
            curNode = _hierarchyData[curNodeId];
        }
    }

    node.rows[rowpath].isexpanded = false;

    // if any row other than the root
    if (rowpath.split('/').length > 1) {
        nodeImg = document.getElementById('hl' + rowpath);
        if (nodeImg) {
            isLastItem = (node.rows[rowpath].hinfo.substr(-1) === '1');
            nodeImg.src = 'images/' +
                          getImageName(true, isLastItem, true, true);
        }
    }

    setRowsColor(_hierarchyRows[rowpath]);
}
