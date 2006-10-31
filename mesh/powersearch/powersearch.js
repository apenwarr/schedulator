/**
 * PowerSearch
 *
 * Author:
 *   Michel Emond
 *   Net Integration Technologies
 *   www.net-itech.com
 *
 * Description:
 *   Adds auto completion to text form fields.
 *
 * Dependencies:
 *   shared.js
 *   powersearch.css
 *
 * Global objects:
 *   PowerSearch   Constructor method
 *   powerSearch   PowerSearch object
 *
 * Syntax:
 *   In order to activate PowerSearch on a text field, add a "powersearch"
 *   attribute containing a comma separated list of the values for the auto
 *   completion. Values must have their single quote and backslash escaped with
 *   a backslash.
 *
 *   Restrictions:
 *     - The onBlur, onFocus, and onKeyUp events of the form fields are
 *       overwritten.
 *     - PowerSearch works only on single line text fields, it can not be used
 *       with textarea.
 *
 * Custom configuration
 *   PowerSearch has three parameters available in order to change its
 *   appearance and behavior: listMaxLength, showListOnFocus, and
 *   requireTextToShowList, their respective default values being 10, false, and
 *   false.
 *
 *   Setting them in a custom configuration must be done like this:
 *     widgetsConfig.powerSearch.listMaxLength = <number>;
 *     widgetsConfig.powerSearch.showListOnFocus = <true or false>;
 *     widgetsConfig.powerSearch.requireTextToShowList = <true or false>;
 *
 *   See shared.js for information about custom configuration.
 *
 * Examples:
 *   <input powersearch="foo,bar,baz" size="55" name="searchBox">
 *   <input powersearch="hello world,back\\slash,a\, b\, and c" size="55" name="searchBox">
 *
 *   The last example will produce a list of three elements:
 *     - hello world
 *     - back\slash
 *     - a, b, and c
 */

/*global addEventHandler, document, fromHtmlEntities, getOffset, setTimeout,
         splitEscapedList, toHtmlEntities, widgetsConfig, window */

function PowerSearch()
{
    var self = this;
    this.isReady = true;

    if (!document.getElementById || !document.getElementsByTagName) {
        this.isReady = false;
        return;
    }

    var searchBox = {};
    var searchBoxNames = [];
    var searchValues = {};
    var prevSearchBoxValue = {};
    var prevTypedSearchBoxValue = {};
    var listDiv = {};
    var listSelectedItem = {};
    var unameLnk = {};
    var nameLnk = {}; // fixme might not be useful after all...
    var unameCol = {};
    var nameCol = {};
    var dynTableId = {};
    var tableIsIndexed = {};

    function getChildNode(el, pTagName)
    {
        if (el === null) { return null; }

        for (var i = 0; i < el.childNodes.length; i++) {
            if (el.childNodes[i].tagName &&
                el.childNodes[i].tagName.toLowerCase() ==
                    pTagName.toLowerCase()) {
                return el.childNodes[i];
            }
        }

        return null;
    }

    function onKeyUpHandler(name, event)
    {
        if (!event && window.event) {
            event = window.event;
        }

        switch (event.keyCode) {
            case 8: // backspace
            case 35: // end
            case 36: // home
            case 37: // left
            case 39: // right
            case 46: // delete
                prevTypedSearchBoxValue[name] = searchBox[name].value;
                break;

            case 27: // escape
                hideListDiv(name);
                break;

            case 38: // up
            case 40: // down
                if (listDiv[name].style.visibility != "visible" &&
                    !self.displayListDiv(name)) {
                    break;
                }

                var divs = listDiv[name].getElementsByTagName("div");
                if (event.keyCode == 38) {
                    listSelectedItem[name]--;
                    if (listSelectedItem[name] < 0) {
                        listSelectedItem[name] = divs.length-1;
                    }
                } else {
                    listSelectedItem[name]++;
                    if (listSelectedItem[name] > (divs.length-1)) {
                        listSelectedItem[name] = 0;
                    }
                }
                highlightListElement(name);
                useListElementValue(name, divs[listSelectedItem[name]]);
                prevTypedSearchBoxValue[name] = searchBox[name].value;

                break;
        }
    }

    function caseSort(a, b)
    {
        var anew = a.toLowerCase();
        var bnew = b.toLowerCase();
        if (anew < bnew) { return -1; }
        if (anew > bnew) { return 1; }
        return 0;
    }

    function setListDivPosition(name)
    {
        if (listDiv[name]) {
            var offset = getOffset(searchBox[name]);
            listDiv[name].style.left = offset.left + "px";
            listDiv[name].style.top = offset.top + offset.height - 1 + "px";
            listDiv[name].style.width = offset.width + "px";
        }
    }

    function hideListDiv(name)
    {
        listDiv[name].style.visibility = "hidden";
    }

    function showListDiv(name)
    {
        listDiv[name].style.visibility = "visible";
        setListDivPosition(name);
    }

    function highlightListElement(name)
    {
        var divs = listDiv[name].getElementsByTagName("div");
        for (var i = 0; i < divs.length; i++) {
            divs[i].className = (listSelectedItem[name] == i) ?
                'pwrsrch-item1' : 'pwrsrch-item0';
        }
    }

    function useListElementValue(name, obj)
    {
        var spans = obj.getElementsByTagName("span");
        if (spans) {
            for (var i = 0; i < spans.length; i++) {
                if (spans[i].className == 'pwrsrch-span2') {
                    searchBox[name].value =
                        fromHtmlEntities(spans[i].innerHTML);
                    // prevents the list from being updated
                    prevSearchBoxValue[name] = searchBox[name].value;
                }
            }
        }
    }

    function filterTable(name, search, unameCol, nameCol)
    {
        var j, r, row;
        var table = document.getElementById(dynTableId[name]);

        // use 2nd row as columns headers if top header exists
        if (table.rows[r].cells[0].colSpan > 1 && table.rows.length > 1) {
            r = 1;
        }

        search = search.toLowerCase();

        // preprocess strings for faster filtering
        if (!tableIsIndexed[name]) {
            //alert('start indexing');
            for (j = r + 1; j < table.rows.length; j++) {
                row = table.rows[j];
                row.setAttribute('filterString',
                    row.cells[unameCol].innerHTML.toLowerCase() + ' ' +
                    row.cells[nameCol].innerHTML.toLowerCase());
            }
            tableIsIndexed[name] = true;
            //alert('done indexing');
        }

        //alert('start filtering');
        for (j = r + 1; j < table.rows.length; j++) {
            row = table.rows[j];
            if (row.getAttribute('filterString').indexOf(search) == -1) {
                row.style.display = 'none';
            } else if (row.style.display == 'none') {
                row.style.display = '';
            }
        }
        //alert('done filtering');
    }

    /* Special wrappers for addEventHandler, see shared.js. */
    function onKeyUpHandlerHandler(obj, event)
    {
        onKeyUpHandler(obj.name, event);
    }
    function hideListDivHandler(obj)
    {
        hideListDiv(obj.name);
    }

    this.init = function ()
    {
        var inputs = document.getElementsByTagName("input");
        for (var i = 0; i < inputs.length; i++) {
            self.widgetInit(inputs[i]);
        }

        setTimeout('powerSearch.updateLists()'+'', 10);
    };

    this.widgetInit = function (thisInput)
    {
        var powerValues = thisInput.getAttribute('powersearch');
        if (!powerValues) { return; }

        searchBox[thisInput.name] = thisInput;
        searchBoxNames.push(thisInput.name);

        // tries to link the powersearch to a dynamic table
        var temp = powerValues.split(',');
        var j, tableObj, tempValues;
        if (temp.length > 2 && (tableObj = document.getElementById(temp[0]))) {

            dynTableId[thisInput.name] = temp[0];
            unameCol[thisInput.name] = temp[1]; // username column
            nameCol[thisInput.name] = temp[2];  // full name column

            // gathers names and unames, main and column headers are skipped
            var unames = [];
            var names = [];
            var skipHeaders = false;
            for (j = 0; j < tableObj.rows.length; j++) {
                var row = tableObj.rows[j];
                if (row.cells.length > 0 && row.cells[0].colSpan == 1) {
                    if (!skipHeaders) {
                        unameLnk[thisInput.name] =
                         getChildNode(row.cells[unameCol[thisInput.name]], 'a');
                        nameLnk[thisInput.name] =
                         getChildNode(row.cells[nameCol[thisInput.name]], 'a');
                        skipHeaders = true;
                        continue;
                    }
                    if (row.cells.length >= unameCol[thisInput.name]-1 &&
                        row.cells.length >= nameCol[thisInput.name]-1) {
                        var temp_uname =
                            row.cells[unameCol[thisInput.name]].innerHTML;
                        var temp_name =
                            row.cells[nameCol[thisInput.name]].innerHTML;
                        unames.push(temp_uname + ' (' + temp_name +')');
                        names.push(temp_name + ' (' + temp_uname +')');
                    }
                }
            }

            // builds final values list
            unames.sort(caseSort);
            names.sort(caseSort);
            tempValues = [];
            for (j = 0; j < names.length; j++) {
                tempValues.push(names[j]);
            }
            for (j = 0; j < unames.length; j++) {
                tempValues.push(unames[j]);
            }

        } else {

            // powersearch isn't linked to a dynamic table, or table could not
            // be found; values provided in the html tag are used to populate
            // the drop list values array

            tempValues = splitEscapedList(powerValues);
            tempValues.sort(caseSort);
            // remove duplicates in a case sensitive way
            j = 1;
            while (j < tempValues.length) {
                if (tempValues[j] == tempValues[j-1]) {
                    tempValues.splice(j, 1);
                } else {
                    j++;
                }
            }

        }
        searchValues[thisInput.name] = tempValues;

        // disables browser's autocomplete feature
        thisInput.autocomplete = "off";

        addEventHandler(thisInput, 'onblur', hideListDivHandler, true);
        addEventHandler(thisInput, 'onkeyup', onKeyUpHandlerHandler, true);

        if (widgetsConfig.powerSearch.showListOnFocus) {
            thisInput.onfocus = function() { self.displayListDiv(this.name); };
        }
        prevSearchBoxValue[thisInput.name] = thisInput.value;
        prevTypedSearchBoxValue[thisInput.name] = thisInput.value;
        tableIsIndexed[thisInput.name] = false; // dynamic table setting
        listSelectedItem[thisInput.name] = -1;
        listDiv[thisInput.name] = document.createElement("DIV");
        listDiv[thisInput.name].id = "listDiv";
        listDiv[thisInput.name].className = 'pwrsrch-list';
        setListDivPosition(thisInput.name);
        document.body.appendChild(listDiv[thisInput.name]);
    };

    this.updateLists = function ()
    {
        for (var i = 0; i < searchBoxNames.length; i++) {
            var name = searchBoxNames[i];
            if (searchBox[name].value != prevSearchBoxValue[name]) {
                self.displayListDiv(name);

                // this line goes with the experimental stuff...
                //var listIsDisplayed = self.displayListDiv(name);

                prevSearchBoxValue[name] = searchBox[name].value;

                /* experimental stuff...
                // dynamic table update
                if (unameLnk[name]) {
                    // forces uname and name cols to be visible
                    toggleCol(dynTableId[name], unameCol[name], true);
                    toggleCol(dynTableId[name], nameCol[name], true);
                    // filter table with search parameters
                    if (!listIsDisplayed) {
                        prevTypedSearchBoxValue[name] = searchBox[name].value;
                    }
                    filterTable(name, prevTypedSearchBoxValue[name],
                                unameCol[name], nameCol[name]);
                }
                */
            }
        }

        setTimeout('powerSearch.updateLists()'+'', 250);
    };

    /**
    * Populates the list based on the content of the search box, auto complete
    * the search box value and display the list if necessary.
    *
    * Returns whether or not the list has been displayed.
    */
    this.displayListDiv = function (name)
    {
        var list = [];
        var s = searchBox[name].value.toLowerCase();
        var i;

        // prevents showing the list when the search box is empty
        if (widgetsConfig.powerSearch.requireTextToShowList && s === '') {
            hideListDiv(name);
            return false;
        }

        for (i = 0; i < searchValues[name].length; i++) {
            if (searchValues[name][i].toLowerCase().indexOf(s) === 0) {
                list.push(searchValues[name][i]);
            }
            if (list.length >= widgetsConfig.powerSearch.listMaxLength) { break; }
        }

        if (list.length === 0) {
            hideListDiv(name);
            return false;
        }

        listSelectedItem[name] = -1;

        // auto complete with the first result when typing in new characters
        if (prevTypedSearchBoxValue[name].substr(0,
            searchBox[name].value.length) == searchBox[name].value) {
            prevTypedSearchBoxValue[name] = searchBox[name].value;
        }
        if (searchBox[name].value.length > prevSearchBoxValue[name].length ||
            prevTypedSearchBoxValue[name] != searchBox[name].value) {
            var v = searchBox[name].value;
            prevTypedSearchBoxValue[name] = v;
            // auto complete the search box only if necessary
            if (v.length < list[0].length) {
                searchBox[name].value =
                    v + list[0].substr(v.length, list[0].length);
                if(searchBox[name].createTextRange) {
                    var temp = searchBox[name].createTextRange();
                    temp.moveStart("character", v.length);
                    temp.select();
                } else if(searchBox[name].setSelectionRange) {
                    searchBox[name].setSelectionRange(v.length,
                        searchBox[name].value.length);
                }
            }
            listSelectedItem[name] = 0;
        }

        while (listDiv[name].childNodes.length > 0) {
            listDiv[name].removeChild(listDiv[name].childNodes[0]);
        }

        for (i = 0; i < list.length; i++) {
            var div = document.createElement("DIV");
            div.className = (i == listSelectedItem[name] ?
                'pwrsrch-item1' : 'pwrsrch-item0');
            div.onmouseover = function () { self.mouseOverHandler(name, this);};
            div.onmouseout = function() { this.className = 'pwrsrch-item0'; };
            div.onmousedown = function() { self.mouseDownHandler(name, this); };

            var span1 = document.createElement("SPAN");
            span1.className = 'pwrsrch-span1';

            var span2 = document.createElement("SPAN");
            span2.className = 'pwrsrch-span2';
            span2.innerHTML = toHtmlEntities(list[i]);

            span1.appendChild(span2);
            div.appendChild(span1);
            listDiv[name].appendChild(div);
        }

        showListDiv(name);

        return true;
    };

    this.mouseOverHandler = function (name, obj)
    {
        listSelectedItem[name] = -1;
        highlightListElement(name);
        obj.className = 'pwrsrch-item1';
    };

    this.mouseDownHandler = function (name, obj)
    {
        useListElementValue(name, obj);
    };

    /**
    * Public function to allow other libraries to update a PowerSearch form
    * field without triggering the drop list to show up.
    *
    * Returns whether or not the field was updated.
    */
    this.silentUpdate = function (name, value)
    {
        if (searchBox[name]) {
            searchBox[name].value = value;
            prevSearchBoxValue[name] = value;
            return true;
        }
        return false;
    };
}

var powerSearch = new PowerSearch();

if (powerSearch.isReady) {
    // initialization function called on window.onLoad event, see shared.js
    _onLoad.push('powerSearch.init()');
}
