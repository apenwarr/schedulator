//fixme: add a way for the database to provide the width in % of each column

/**
 * Crazy Table
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
 *   <span crazytable="db"></span>
 *
 */

// initialization function called on window.onLoad event, see shared.js
_onLoad.push('crazytableInit()');

// flag so other libraries know that Hierarchy has been loaded up
if (document.getElementsByTagName && getXmlHttp()) {
// fixme: make sure the check is on the required functions
    var CRAZYTABLE = true;
}

// configuration params, can be changed
var _crazytableGetDataScript = 'getdata.php';


// initialization stuff, do not change
var _crazytable = {};
var _isIE = false;

function crazytableInit()
{
    if (typeof(CRAZYTABLE) != 'boolean') { return; }

    var source, table, thisSpan;
    var spans = document.getElementsByTagName("span");

    if (spans.length < 1) { return; }

    _isIE = !(navigator &&
              navigator.userAgent.toLowerCase().indexOf("msie") == -1);

    for (var i = 0; i < spans.length; i++) {
        thisSpan = spans[i];
        source = thisSpan.getAttribute('crazytable');
        if (source !== null) {
            thisSpan.setAttribute('id', 'crazytable' + source);
            table = getTable();
            thisSpan = clearDomObj(thisSpan);
            thisSpan.appendChild(table);

            _crazytable[source] = {};
            _crazytable[source]['span'] = thisSpan;
            _crazytable[source]['xmlHttp'] = null;
            _crazytable[source]['data'] = new Array();
            _crazytable[source]['rows'] = new Array();

            sendCrazytableDataRequest(source, 0, '0');
        }
    }

// fixme: use addEventHandler here and in dynamictatus.js
/*
    window.onunload = function() {
        if (_hierarchyXmlHttp) {
            _hierarchyXmlHttp.abort();
        }
    };
*/
}

function sendCrazytableDataRequest(source, getId, rowId)
{
    var xmlHttp = _crazytable[source].xmlHttp;

    if (xmlHttp && xmlHttp.readyState !== 0) {
        xmlHttp.abort();
    }
    if (!(xmlHttp = getXmlHttp())) { return; }

    try {
        xmlHttp.open('GET', _crazytableGetDataScript +
                     '?source=' + source + '&id=' + getId + '&rowid=' + rowId, true);
    } catch(e) {
// fixme: do something here when unable to send request, maybe server is down...
        return;
    }

    xmlHttp.onreadystatechange = function() {
        if (xmlHttp.readyState == 4) {
            // gets here in case of response or timeout
            if (xmlHttp.responseText && xmlHttp.status == 200) {
                updateCrazytableData(source, xmlHttp.responseText);
            }
        }
    };

    try {
        xmlHttp.send(null);
    } catch(e) {
alert('catch');
// fixme: check with dynamicstatus.js...
    }

    return;
}

function setNode(source, node)
{
    var newNode = {};
    newNode.id = node.id;
    newNode.name = node.name;
    newNode.cols = new Array();
    for (var i in node.cols) {
        newNode.cols[i] = node.cols[i];
    }
    newNode.nodes = new Array();
    newNode.nodescount = node.nodescount;

    _crazytable[source].data[node.id] = newNode;
}

function updateCrazytableData(source, returnedData)
{
    if (returnedData === '') { return; }

    try {
        var data = eval('('+returnedData+')');
    } catch(e) {
        return;
    }

    var i;

    for (i = 0; i < data.nodes.length; i++) {
        setNode(source, data.nodes[i]);
    }

    if (data.request == 'getsubnodes') {
        for (i = 0; i < data.nodes.length; i++) {
            if (data.nodes[i].id > 0) {
                _crazytable[source].data[data.id].nodes.push(data.nodes[i].id);
            }
        }
        if (data.nodes.length > 0) {
            expandHierarchyRow(source, data.id, data.rowid);
        }
    } else if (data.request == 'getpage') {
    }
}

function getTable()
{
    var table = document.createElement('TABLE');
    var tbody = document.createElement('TBODY');
    table.appendChild(tbody);
    table.cellSpacing = 0;
    table.cellPadding = 0;
    table.border = 0;

    return table;
}

function getCell(colWidth, colSpan, isHeader)
{
    if (isHeader === undefined) { isHeader = false; }

    var cell;
    var cellTag = (isHeader ? 'TH' : 'TD');

    if (_isIE) {
        // IE is silly
        cell = document.createElement('<' + cellTag +
            (isHeader ? ' nowrap="nowrap"' : '') +
            ' colspan="' + colSpan + '">');
    } else {
        cell = document.createElement(cellTag);
        if (isHeader) {
            cell.setAttribute('nowrap', 'nowrap');
        }
        cell.setAttribute('colspan', colSpan);
    }

    cell.setAttribute('width', colWidth);

    if (colSpan == 1) {
        cell.style.borderBottom = '1px solid black';
    }

    return cell;
}

function getImage(name, id)
{
    var imgId = '';
    if (id !== undefined) {
        imgId = 'id="hl' + id + '"';
    }
    return '<img src="images/' + name + '.png" ' + imgId + ' alt="" title="" ' +
           'width="15" height="18" border="0">';
}

function getNodeClickTag(source, nodeId, rowId)
{
//fixme: fix this to avoid using javascript:;
    return '<a onClick="nodeClick(\''+escapeQuotes(source)+'\',' +
           nodeId + ', \'' + rowId + '\'); return false;" href="#">';
}

function getHierarchyLines(source, nodeId, rowId, rowHinfo)
{
    var r = '';

    var node = _crazytable[source].data[nodeId];
    var isLastNode;
    for (var i = 0; i < rowHinfo.length; i++) {
        isLastNode = (rowHinfo.substr(i, 1) == '1');
        if (i == (rowHinfo.length-1)) {
            if (node.nodescount > 0) {
                r += getNodeClickTag(source, nodeId, rowId) +
                     getImage(isLastNode ? 'hl2p' : 'hl3p', rowId) + '</a>';
            } else {
                r += getImage(isLastNode ? 'hl2' : 'hl3', rowId);
            }
            r += '&nbsp;';
        } else {
            r += getImage(isLastNode ? 'hl0' : 'hl1');
        }
    }

    return r;
}

function createRow(source, nodeId, rowHinfo, isHeader)
{
    var node = _crazytable[source].data[nodeId];
    if (isHeader === undefined) { isHeader = false; }

    // duplicate rows gets a distinct id
    var counter = 0;
    var rowId = String(nodeId);
    while (_crazytable[source].rows[rowId]) {
        counter++;
        rowId = String(nodeId) + String(counter);
    }

    var cell, returnRow, row, subRow, table, tbody;

    row = document.createElement('TR');
    row.setAttribute('id', rowId);
    row.setAttribute('nodeid', nodeId);
    row.setAttribute('hinfo', rowHinfo);
    row.setAttribute('isexpanded', 0);
    returnRow = row;

    var colCount = _crazytable[source].data[0].cols.length;
    var colWidth = Math.floor(100 / (colCount+1)) + '%';

    if (node.nodescount > 0) {
        cell = getCell(colWidth, colCount+1);
        row.appendChild(cell);

        table = getTable();
        cell.appendChild(table);
        tbody = table.childNodes[0];

        row = document.createElement('TR');
        tbody.appendChild(row);

        subRow = document.createElement('TR');
        tbody.appendChild(subRow);

        cell = getCell(colWidth, colCount+1);
        subRow.appendChild(cell);

        table = getTable();
        cell.appendChild(table);
    }

    row.setAttribute('valign', 'top');
    if (!isHeader) {
        row.onmouseover = function() { this.style.backgroundColor = '#e5e5e5'; };
        row.onmouseout =  function() { this.style.backgroundColor = 'white'; };
    }

    cell = getCell(colWidth, 1, isHeader);
    if (isHeader) {
        cell.innerHTML = '&nbsp;';
    } else {
        cell.innerHTML = getHierarchyLines(source, nodeId, rowId, rowHinfo) +
                         (node.nodescount > 0 ?
                          getNodeClickTag(source, nodeId, rowId) : '') +
                         node.name + (node.nodescount > 0 ? '</a>' : '') +
                         '&nbsp;';
    }
    row.appendChild(cell);

    for (var i = 0; i < node.cols.length; i++) {
        cell = getCell(colWidth, 1, isHeader);
        cell.innerHTML = colWidth +' '+node.cols[i];
        row.appendChild(cell);
    }

    if (isHeader) {
        row.className = 'crazy-table-header-row';
    }

    return returnRow;
}

function nodeClick(source, nodeId, rowId)
{
    var node = _crazytable[source].data[nodeId];
    var row = _crazytable[source].rows[rowId];

    if (row.getAttribute('isexpanded') == 1) {
        collapseHierarchyRow(source, nodeId);
    } else if (node.nodescount > node.nodes.length) {
        sendCrazytableDataRequest(source, nodeId, rowId);
    } else {
        expandHierarchyRow(source, nodeId, rowId);
    }
}

function setRowsColor(source, row)
{

// fixme: remove setRowsColor if unused
return;

    row = _crazytable[source].rows['0'];

    var classNames = new Array('crazy-table-even-row', 'crazy-table-odd-row');

    var curClass, rowId;
    var rowStack = new Array();
    var isFirst = true;
    while (row) {
        rowId = row.id;
        if ((isBranch = (row.cells[0].getAttribute('colspan') != 1))) {
            subTable = row.cells[0].childNodes[0];
            styleRow = subTable.rows[0];
        } else {
            styleRow = row;
        }

        if (isFirst) {
            curClass = (styleRow.className == classNames[0] ? 1 : 0);
            isFirst = false;
        } else {
            styleRow.className = classNames[curClass];
            curClass = 1 - curClass;
        }

        if (isBranch &&
            subTable.rows[1].style.display != 'none' &&
            subTable.rows[1].cells[0].childNodes[0].rows.length > 0) {
            rowStack.push(rowId);
            row = subTable.rows[1].cells[0].childNodes[0].rows[0];
        } else {
            row = row.nextSibling;
            while (row == null && rowStack.length > 0) {
                row = _crazytable[source].rows[rowStack.pop()];
                row = row.nextSibling;
            }
        }
    }
}

function expandHierarchyRow(source, nodeId, rowId)
{
    var table = _crazytable[source].span.childNodes[0];
    var tbody = table.childNodes[0];

// fixme: check for the current state of the table, if not in hierarchy state,
// empty the html table except for the headers and set the nodeId to 0

    if (table.rows.length === 0) {
        var tempRow = createRow(source, 0, '', true);
        tbody.appendChild(tempRow);
        _crazytable[source].rows['0'] = tempRow;
    }

    var row = _crazytable[source].rows[rowId];

    try { // fixme: is this try really necessary?
        var subRow = row.cells[0].childNodes[0].rows[1];
        var subTable = subRow.cells[0].childNodes[0];
        var subTbody = subTable.childNodes[0];
    } catch(e) {
        return;
    }

    var node = _crazytable[source].data[nodeId];
    var rowHinfo = row.getAttribute('hinfo');

    if (subTable.rows.length > 0) {
        subRow.style.display = '';
    } else {
        var c = 1;
        var hinfo, newRow;
        for (var i in node.nodes) {
            hinfo = rowHinfo + (c == node.nodescount ? '1' : '0');
            newRow = createRow(source, node.nodes[i], hinfo, false);
            _crazytable[source].rows[newRow.getAttribute('id')] = newRow;
            subTbody.appendChild(newRow);
            c++;
        }
    }

    if (rowId !== '0') {
        var nodeImg = document.getElementById('hl' + rowId);
        if (nodeImg) {
            nodeImg.src = 'images/' +
                          (rowHinfo.substr(-1) === '0' ? 'hl3m' : 'hl2m') +
                          '.png';
        }
    }

    // this fixes a display bug in Mozilla...
    subRow.style.display = 'none';
    subRow.style.display = '';

    row.setAttribute('isexpanded', 1);

    setRowsColor(source, row);
}

function collapseHierarchyRow(source, rowId)
{
    var row = _crazytable[source].rows[rowId];
    try { // fixme: is this try really necessary?
        var subRow = row.cells[0].childNodes[0].rows[1];
    } catch(e) {
        return;
    }

    subRow.style.display = 'none';

    if (rowId !== '0') {
        var nodeImg = document.getElementById('hl' + rowId);
        if (nodeImg) {
            var rowHinfo = row.getAttribute('hinfo');
            nodeImg.src = 'images/' +
                          (rowHinfo.substr(-1) === '0' ? 'hl3p' : 'hl2p') +
                          '.png';
        }
    }

    row.setAttribute('isexpanded', 0);

    setRowsColor(source, row);
}


// fixme: remove this...
_ex = {};

function massExpand(source)
{
    trs = document.getElementsByTagName('tr');

    for (i = 1; i < trs.length; i++) {
        tr = trs[i];

        rowid = tr.getAttribute('id');
        id = tr.getAttribute('nodeid');

        if (rowid && !_ex[rowid]) {
            nodeClick(source, id, rowid);
            _ex[rowid] = 1;
            break;
        }
    }
}
