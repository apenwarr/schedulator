// initialization functions called on window.onLoad event, see shared.js
_onLoad.push('sortables_init()');
_onLoad.push('cols_toggle_init()');


// ------------------------- cols toggle -------------------------------

function cols_toggle_init()
{
    if (!document.getElementsByTagName || !document.getElementById) {
        return;
    }

    // column selection span
    var span = document.createElement("SPAN");
    span.setAttribute("id", "selcols_span");
    span.setAttribute("class", "selcols_span");
    span.style.position = 'absolute'; // IE need this to be set here
    document.body.appendChild(span);

    var tbls = document.getElementsByTagName("table");
    for (var ti=0;ti<tbls.length;ti++) {
        var thisTbl = tbls[ti];
        if (thisTbl.getAttribute('sortable') &&
            thisTbl.id && thisTbl.rows.length > 1) {
            var img = document.createElement("IMG");
            img.setAttribute("src", "selcols.png");
            img.setAttribute("align", "right");
            img.setAttribute("width", "21");
            img.setAttribute("height", "22");
            img.setAttribute("border", "0");
            img.setAttribute("id", "img_selcols"+thisTbl.id);

            var tooltip = 'Click to select columns to display';
            var a = document.createElement("A");
            a.setAttribute("href", "javascript:openColsSelector('"+thisTbl.id+"')");
            a.setAttribute("alt", tooltip);
            a.setAttribute("title", tooltip);
            a.setAttribute("onmouseout", "window.status='';");
            a.setAttribute("onmouseover", "window.status='"+tooltip+"';return true;");
            a.appendChild(img);

            var cell = thisTbl.rows[0].cells[thisTbl.rows[0].cells.length-1];
            var html = cell.innerHTML;
            cell.innerHTML = '';
            cell.appendChild(a);
            cell.innerHTML += html;
        }
    }
}

var curTableId;

function openColsSelector(tableId)
{
    var layer = document.getElementById('selcols_span');

    if (layer.style.visibility == 'hidden' || curTableId != tableId) {
        openSelector(tableId);
    } else {
        layer.style.visibility = 'hidden';
        curTableId = '';
    }
}

function toggleCol(tableId, col, forceDisplay)
{
    var thisTbl = document.getElementById(tableId);
    if (thisTbl.rows.length === 0) { return; }

    var headersRow = (thisTbl.rows[0].cells[0].colSpan > 1) ? 1 : 0;
    if (thisTbl.rows.length < (headersRow + 1)) { return; }

    var doDisplay;
    if (thisTbl.rows[headersRow].cells[col].style.display == 'none') {
        doDisplay = '';
    } else {
        if (forceDisplay) { return; }
        doDisplay = 'none';
    }

    for (var i = headersRow; i < thisTbl.rows.length; i++) {
        thisTbl.rows[i].cells[col].style.display = doDisplay;
    }

    if (!forceDisplay) {
        openSelector(tableId);
    }
}

function openSelector(tableId)
{
    var row, cell, img, link, thisCell;

    var layer = document.getElementById('selcols_span');
    if (layer.firstChild) {
        layer.removeChild(layer.firstChild);
    }

    var table = document.createElement('TABLE');
    var tbody = document.createElement('TBODY');
    table.appendChild(tbody);
    table.className = 'selcols_table';
    table.cellSpacing = 0;
    table.cellPadding = 2;
    table.border = 0;

    var thisTbl = document.getElementById(tableId);

    var headersRow = (thisTbl.rows[0].cells[0].colSpan > 1) ? 1 : 0;

    if (thisTbl.rows.length > headersRow) {
        for (var i = 0; i < thisTbl.rows[headersRow].cells.length; i++) {
            thisCell = thisTbl.rows[headersRow].cells[i];

            row = document.createElement('TR');

            cell = document.createElement('TD');
            cell.innerHTML = (thisCell.style.display == 'none') ? '' : '&radic;';
            row.appendChild(cell);

            cell = document.createElement('TD');
            link = document.createElement('A');
            link.href = "javascript:toggleCol("+tableId+","+i+");";
            link.innerHTML = trim(ts_getInnerText(thisCell));
            cell.appendChild(link);
            row.appendChild(cell);

            tbody.appendChild(row);
        }
    }

    layer.appendChild(table);

    if (curTableId != tableId) {
        // We're showing this popup for the first time, so try to
        // position it next to the image anchor.
        var el = document.getElementById('img_selcols'+tableId);
        var p = getAbsolutePosition(el);
        p.x = p.x + el.offsetWidth - layer.offsetWidth;
        // useless crap, remove this eventually
        //p.x = p.x + el.offsetWidth - img.offsetWidth - 10;
        p.y += el.offsetHeight;

        layer.style.left = p.x + 'px';
        layer.style.top = p.y + 'px';
    }
    curTableId = tableId;

    layer.style.display = 'block';
    layer.style.visibility = 'visible';
}

function getAbsolutePosition(el)
{
    var r = {x: el.offsetLeft, y: el.offsetTop};
    if (el.offsetParent) {
        var tmp = getAbsolutePosition(el.offsetParent);
        r.x += tmp.x;
        r.y += tmp.y;
    }
    return r;
}


// ------------------------- sort table --------------------------------

/**
 * Copyright Stuart Langridge MIT License
 * See: www.kryogenix.org/code/browser/sorttable/
 *
 * The following code is a heavily modified version of the original.
 */

// Do not uncomment this line, sortables_init is not called from here anymore.
//addEvent(window, "load", sortables_init);

var SORT_COLUMN_INDEX;

function sortables_init() {
    // Find all tables with attribute sortable and make them sortable
    if (!document.getElementsByTagName) { return; }
    var tbls = document.getElementsByTagName("table");
    for (var ti=0;ti<tbls.length;ti++) {
        var thisTbl = tbls[ti];
        if (thisTbl.getAttribute('sortable') && thisTbl.id) {
            thisTbl.className = 'sortable';
            ts_makeSortable(thisTbl);
        }
    }
}

function ts_makeSortable(table) {
    if (table.rows && table.rows.length > 0) {
        var firstRow = table.rows[0];
        if (firstRow.cells[0].colSpan > 1 && table.rows.length > 1) {
            firstRow = table.rows[1];
        }
    }
    if (!firstRow) { return; }
    // We have a first row: assume it's the header, and make its contents clickable links
    for (var i=0;i<firstRow.cells.length;i++) {
        var cell = firstRow.cells[i];
        var txt = ts_getInnerText(cell);
        cell.innerHTML = '<a href="#" class="sortheader" onclick="ts_resortTable(this);return false;">'+txt+'<span class="sortarrow">&nbsp;&nbsp;&nbsp;</span></a>';
    }
}

function ts_getInnerText(el) {
    if (typeof el == "string") { return el; }
    if (typeof el === "undefined") { return el; }
    if (el.innerText) { return el.innerText; } //Not needed but it is faster
    var str = "";

    var cs = el.childNodes;
    var l = cs.length;
    for (var i = 0; i < l; i++) {
        switch (cs[i].nodeType) {
            case 1: //ELEMENT_NODE
                str += ts_getInnerText(cs[i]);
                break;
            case 3: //TEXT_NODE
                str += cs[i].nodeValue;
                break;
        }
    }
    return str;
}

function ts_resortTable(lnk) {
    // get the span
    var span, i, ci;
    for (ci=0;ci<lnk.childNodes.length;ci++) {
        if (lnk.childNodes[ci].tagName &&
            lnk.childNodes[ci].tagName.toLowerCase() == 'span') {
            span = lnk.childNodes[ci];
        }
    }
    var spantext = ts_getInnerText(span);
    var td = lnk.parentNode;
    var column = td.cellIndex;
    var table = getParent(td,'table');

    // use 2nd row as columns headers if top header exists
    var r = 0;
    if (table.rows[r].cells[0].colSpan > 1 && table.rows.length > 1) {
        r = 1;
    }

    // adjust cellIndex when required
    // in IE, cellIndex is different than the index when the cell is hidden
    var doAdjust = false;
    for (i = 0; i < table.rows[r].cells.length; i++) {
        if (table.rows[r].cells[i].cellIndex != i) {
            doAdjust = true;
            break;
        }
    }
    if (doAdjust) {
        var newColumn = column;
        var count = 0;
        for (i = 0; i < table.rows[r].cells.length; i++) {
            if (table.rows[r].cells[i].style.display == 'none') {
                newColumn++;
            } else {
                count++;
            }
            if (count == (column+1)) { break; }
        }
        column = newColumn;
    }

    // Work out a type for the column
    if (table.rows.length <= (r+1)) { return; }
    var itm = ts_getInnerText(table.rows[r+1].cells[column]);
    sortfn = ts_sort_caseinsensitive;
    if (itm.match(/^\d\d[\/-]\d\d[\/-]\d\d\d\d$/)) { sortfn = ts_sort_date; }
    if (itm.match(/^\d\d[\/-]\d\d[\/-]\d\d$/)) { sortfn = ts_sort_date; }
    if (itm.match(/^[£$]/)) { sortfn = ts_sort_currency; }
    if (itm.match(/^-|[\d\.]+( KB)?$/)) { sortfn = ts_sort_numeric; }
    SORT_COLUMN_INDEX = column;
    var firstRow = new Array();
    var newRows = new Array();
    for (i=0;i<table.rows[r].length;i++) { firstRow[i] = table.rows[r][i]; }
    var k = 0;
    for (var j=r+1;j<table.rows.length;j++) {
        if (table.rows[j].style.display != 'none') {
            newRows[k] = table.rows[j];
            k++;
        }
    }

    newRows.sort(sortfn);

    var ARROW;
    if (span.getAttribute("sortdir") == 'down') {
        ARROW = '&nbsp;&nbsp;&uarr;';
        newRows.reverse();
        span.setAttribute('sortdir','up');
    } else {
        ARROW = '&nbsp;&nbsp;&darr;';
        span.setAttribute('sortdir','down');
    }

    // We appendChild rows that already exist to the tbody, so it moves them
    // rather than creating new ones don't do sortbottom rows
    for (i=0;i<newRows.length;i++) {
        if (!newRows[i].className || (newRows[i].className &&
            (newRows[i].className.indexOf('sortbottom') == -1))) {
            table.tBodies[0].appendChild(newRows[i]);
        }
    }
    // do sortbottom rows only
    for (i=0;i<newRows.length;i++) {
        if (newRows[i].className &&
            (newRows[i].className.indexOf('sortbottom') != -1)) {
            table.tBodies[0].appendChild(newRows[i]);
        }
    }

    // Delete any other arrows there may be showing
    var allspans = document.getElementsByTagName("span");
    for (ci=0;ci<allspans.length;ci++) {
        if (allspans[ci].className == 'sortarrow') {
            // in the same table as us?
            if (getParent(allspans[ci],"table") == getParent(lnk,"table")) {
                allspans[ci].innerHTML = '&nbsp;&nbsp;&nbsp;';
            }
        }
    }

    span.innerHTML = ARROW;
}

function getParent(el, pTagName) {
    if (el === null) {
        return null;
    } else if (el.nodeType == 1 &&
               el.tagName.toLowerCase() == pTagName.toLowerCase()) {
        // Gecko bug, supposed to be uppercase
        return el;
    } else {
        return getParent(el.parentNode, pTagName);
    }
}

function ts_sort_date(a,b) {
    // y2k notes: two digit years less than 50 are treated as 20XX, greater
    // than 50 are treated as 19XX
    var aa = ts_getInnerText(a.cells[SORT_COLUMN_INDEX]);
    var bb = ts_getInnerText(b.cells[SORT_COLUMN_INDEX]);
    var dt1, dt2, yr;
    if (aa.length == 10) {
        dt1 = aa.substr(6,4)+aa.substr(3,2)+aa.substr(0,2);
    } else {
        yr = aa.substr(6,2);
        if (parseInt(yr, 10) < 50) { yr = '20'+yr; } else { yr = '19'+yr; }
        dt1 = yr+aa.substr(3,2)+aa.substr(0,2);
    }
    if (bb.length == 10) {
        dt2 = bb.substr(6,4)+bb.substr(3,2)+bb.substr(0,2);
    } else {
        yr = bb.substr(6,2);
        if (parseInt(yr, 10) < 50) { yr = '20'+yr; } else { yr = '19'+yr; }
        dt2 = yr+bb.substr(3,2)+bb.substr(0,2);
    }
    if (dt1==dt2) { return 0; }
    if (dt1<dt2) { return -1; }
    return 1;
}

function ts_sort_currency(a,b) {
    var aa = ts_getInnerText(a.cells[SORT_COLUMN_INDEX]).replace(/[^0-9.]/g,'');
    var bb = ts_getInnerText(b.cells[SORT_COLUMN_INDEX]).replace(/[^0-9.]/g,'');
    return parseFloat(aa) - parseFloat(bb);
}

function ts_sort_numeric(a,b) {
    var aa = parseFloat(ts_getInnerText(a.cells[SORT_COLUMN_INDEX]));
    if (isNaN(aa)) { aa = 0; }
    var bb = parseFloat(ts_getInnerText(b.cells[SORT_COLUMN_INDEX]));
    if (isNaN(bb)) { bb = 0; }
    return aa-bb;
}

function ts_sort_caseinsensitive(a,b) {
    var aa = ts_getInnerText(a.cells[SORT_COLUMN_INDEX]).toLowerCase();
    var bb = ts_getInnerText(b.cells[SORT_COLUMN_INDEX]).toLowerCase();
    if (aa==bb) { return 0; }
    if (aa<bb) { return -1; }
    return 1;
}

function ts_sort_default(a,b) {
    var aa = ts_getInnerText(a.cells[SORT_COLUMN_INDEX]);
    var bb = ts_getInnerText(b.cells[SORT_COLUMN_INDEX]);
    if (aa==bb) { return 0; }
    if (aa<bb) { return -1; }
    return 1;
}
