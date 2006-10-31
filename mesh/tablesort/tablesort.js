/**
 * TableSort
 *
 * Author:
 *   Original version:
 *     Copyright Stuart Langridge MIT License
 *     See: www.kryogenix.org/code/browser/sorttable/
 *
 *   Modified by:
 *     Michel Emond
 *     Net Integration Technologies
 *     www.net-itech.com
 *
 * Description:
 *   Turns a simple html table into a sortable table. The columns headers become
 *   clickable and the sorting order toggles between ascending and descending.
 *   Different data types (letters, numbers, currency, and dates) are
 *   automatically supported.
 *
 *   Restrictions:
 *     - The table must have headers so there's something to click on.
 *     - The table's HTML must be well formed.
 *     - Moreover, if the table includes checkboxes or radio buttons, the FORM
 *       and TABLE tags (including TR and TD) must be correctly nested in order
 *       to form strict valid html. Otherwise, form elements gets ignored once
 *       the form is submitted.
 *
 * Dependencies:
 *   shared.js
 *   tablesort.css
 *
 * Global objects:
 *   TableSort   Constructor method
 *   tableSort   TableSort object
 *
 * Syntax:
 *   In order to activate TableSort on a table, add a "tablesort" attribute
 *   having the value "1", and make sure the table has a unique id.
 *
 * Examples:
 *   <table id="mytable" tablesort="1" border="1">
 */

/*global document */

function TableSort()
{
    this.isReady = true;

    if (!document.getElementsByTagName) {
        this.isReady = false;
        return;
    }

    var sortColumnIndex;

    function makeSortable(table)
    {
        if (table.rows && table.rows.length > 0) {
            var firstRow = table.rows[0];
        }
        if (!firstRow) { return; }
        // We have a first row: assume it's the header, and make its contents
        // clickable links.
        for (var i = 0; i < firstRow.cells.length; i++) {
            var cell = firstRow.cells[i];
            var txt = getInnerText(cell);
            cell.innerHTML = '<a href="#" class="sortheader" '+
                'onclick="tableSort.resort(this);return false;">'+txt+
                '<span class="sortarrow">&nbsp;&nbsp;&nbsp;</span></a>';
        }
    }

    function getInnerText(el)
    {
        if (typeof el == "string") { return el; }
        if (typeof el === "undefined") { return el; }
        if (el.innerText) { return el.innerText; } //Not needed but it is faster
        var str = "";

        var cs = el.childNodes;
        var l = cs.length;
        for (var i = 0; i < l; i++) {
            switch (cs[i].nodeType) {
                case 1: //ELEMENT_NODE
                    str += getInnerText(cs[i]);
                    break;
                case 3: //TEXT_NODE
                    str += cs[i].nodeValue;
                    break;
            }
        }
        return str;
    }

    function getParent(el, pTagName)
    {
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

    function sortDate(a, b)
    {
        // y2k notes: two digit years less than 50 are treated as 20XX, greater
        // than 50 are treated as 19XX
        var aa = getInnerText(a.cells[sortColumnIndex]);
        var bb = getInnerText(b.cells[sortColumnIndex]);
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
        if (dt1 == dt2) { return 0; }
        if (dt1 < dt2) { return -1; }
        return 1;
    }

    function sortCurrency(a, b)
    {
        var aa = getInnerText(a.cells[sortColumnIndex]).replace(/[^0-9.]/g,'');
        var bb = getInnerText(b.cells[sortColumnIndex]).replace(/[^0-9.]/g,'');
        return parseFloat(aa) - parseFloat(bb);
    }

    function sortNumeric(a, b)
    {
        var aa = parseFloat(getInnerText(a.cells[sortColumnIndex]));
        if (isNaN(aa)) { aa = 0; }
        var bb = parseFloat(getInnerText(b.cells[sortColumnIndex]));
        if (isNaN(bb)) { bb = 0; }
        return aa - bb;
    }

    function sortCaseInsensitive(a, b)
    {
        var aa = getInnerText(a.cells[sortColumnIndex]).toLowerCase();
        var bb = getInnerText(b.cells[sortColumnIndex]).toLowerCase();
        if (aa == bb) { return 0; }
        if (aa < bb) { return -1; }
        return 1;
    }

    function sortDefault(a, b)
    {
        var aa = getInnerText(a.cells[sortColumnIndex]);
        var bb = getInnerText(b.cells[sortColumnIndex]);
        if (aa == bb) { return 0; }
        if (aa < bb) { return -1; }
        return 1;
    }

    this.init = function ()
    {
        // Find all tables with attribute tablesort and make them sortable
        var tbls = document.getElementsByTagName("table");
        for (var i = 0; i < tbls.length; i++) {
            if (tbls[i].getAttribute('tablesort') && tbls[i].id) {
                tbls[i].className = 'tablesort';
                makeSortable(tbls[i]);
            }
        }
    };

    this.resort = function (lnk)
    {
        // get the span
        var span, i, ci;
        for (ci = 0; ci < lnk.childNodes.length; ci++) {
            if (lnk.childNodes[ci].tagName &&
                lnk.childNodes[ci].tagName.toLowerCase() == 'span') {
                span = lnk.childNodes[ci];
            }
        }
        //var spantext = getInnerText(span); unused?
        var td = lnk.parentNode;
        var column = td.cellIndex;
        var table = getParent(td,'table');

        // Work out a type for the column
        if (table.rows.length <= 1) { return; }
        var itm = getInnerText(table.rows[1].cells[column]);
        var sortfn = sortCaseInsensitive;
        if (itm.match(/^\d\d[\/-]\d\d[\/-]\d\d\d\d$/)) { sortfn = sortDate; }
        if (itm.match(/^\d\d[\/-]\d\d[\/-]\d\d$/)) { sortfn = sortDate; }
        if (itm.match(/^[$]/)) { sortfn = sortCurrency; }
        if (itm.match(/^-|(-?[\d\.]+( KB)?)$/)) { sortfn = sortNumeric; }
        sortColumnIndex = column;
        var firstRow = [];
        var newRows = [];
        for (i = 0; i < table.rows[0].length; i++) {
            firstRow[i] = table.rows[0][i];
        }
        for (var j = 1; j < table.rows.length; j++) {
            newRows[j - 1] = table.rows[j];
        }

        newRows.sort(sortfn);

        var sortArrow;
        if (span.getAttribute("sortdir") == 'down') {
            sortArrow = '&nbsp;&nbsp;&uarr;';
            newRows.reverse();
            span.setAttribute('sortdir','up');
        } else {
            sortArrow = '&nbsp;&nbsp;&darr;';
            span.setAttribute('sortdir','down');
        }

        // We appendChild rows that already exist to the tbody, so it moves them
        // rather than creating new ones don't do sortbottom rows
        for (i = 0; i < newRows.length; i++) {
            if (!newRows[i].className || (newRows[i].className &&
                (newRows[i].className.indexOf('sortbottom') == -1))) {
                table.tBodies[0].appendChild(newRows[i]);
            }
        }
        // do sortbottom rows only
        for (i = 0; i < newRows.length; i++) {
            if (newRows[i].className &&
                (newRows[i].className.indexOf('sortbottom') != -1)) {
                table.tBodies[0].appendChild(newRows[i]);
            }
        }

        // Delete any other arrows there may be showing
        var allspans = document.getElementsByTagName("span");
        for (ci = 0; ci < allspans.length; ci++) {
            if (allspans[ci].className == 'sortarrow') {
                // in the same table as us?
                if (getParent(allspans[ci],"table") == getParent(lnk,"table")) {
                    allspans[ci].innerHTML = '&nbsp;&nbsp;&nbsp;';
                }
            }
        }

        span.innerHTML = sortArrow;
    };
}

var tableSort = new TableSort();

if (tableSort.isReady) {
    // initialization function called on window.onLoad event, see shared.js
    _onLoad.push('tableSort.init()');
}
