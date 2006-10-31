/**
 * shared.js
 *
 * Author (when not specified):
 *   Michel Emond
 *   Net Integration Technologies
 *   www.net-itech.com
 *
 * Description:
 *   Centralized collection of useful functions. See the description of each
 *   function for more details.
 *
 * Dependencies:
 *   None
 *
 * Global objects:
 *   Various global functions
 *   _onLoad                      Array
 *   _xmlHttpRequestObjects       Hash
 *   _xmlHttpRequestObjectsIndex  Numeric
 *
 * How to use:
 *   When required, include the shared.js file like this before any other
 *   widget's .js file:
 *   <script src="shared.js" type="text/javascript"></script>
 *
 *   Restrictions:
 *     - This widgets library is expected to be kept in the same directory
 *       structure as the original repository.
 *
 * Examples:
 *   <script src="../shared.js" type="text/javascript"></script>
 */

/*global ActiveXObject, XMLHttpRequest, document, navigator */

/**
 * Shared configuration system
 *
 * The widgetsConfig global variable is used to store the widgets various
 * configuration settings. It is allowed (and sometimes required) to add or
 * change values, but no keys should be removed from this initial default
 * configuration tree.
 *
 * In order to set keys of widgetsConfig, custom configuration files should use
 * the JSON notation and the setWidgetsCustomConfig() function like this:
 *
 *   In a file named config.js, local to the web application:
 *     setWidgetsCustomConfig(
 *         function () {
 *             widgetsConfig.[...];
 *             widgetsConfig.[...];
 *             [...]
 *         }
 *     );
 *
 *   And then in the .html file of the web application:
 *     <script src="config.js" type="text/javascript"></script>
 *
 *   Note that due to the nature of JavaScript, the names of the keys are case
 *   sensitive.
 *
 * Restrictions:
 *   - Custom configuration files must absolutely be included after shared.js.
 */
var widgetsConfig = {};
var widgetsCustomConfig = [];
function setWidgetsCustomConfig(fcn)
{
    widgetsCustomConfig.push(fcn);
}
function widgetsConfigInit()
{
    // Custom images location
    //   By default, the location of the images used by the widgets is computed
    //   from the location of shared.js. Use this property to use images from a
    //   different location. In all cases, images are expected to be found in
    //   the same directory structure as the original repository. See example
    //   below.
    widgetsConfig.imgBasePath = '';
    var scriptTags = document.getElementsByTagName('script');
    var sharedTag, src;
    for (var i = 0; i < scriptTags.length; i++) {
        src = scriptTags[i].src;
        if (src.substr(src.length-9) == 'shared.js') {
            sharedTag = scriptTags[i];
            break;
        }
    }
    if (sharedTag) {
        widgetsConfig.imgBasePath = sharedTag.src.substr(0,
            sharedTag.src.lastIndexOf('/')+1);
    }

    widgetsConfig.editInPlace = {
        // Image maps
        'imageMap':{},
        'cancelImgName':'cancel.gif',
        'cancelImgWidth':16,
        'cancelImgHeight':16,
        'doCancelLink':true
    };

    widgetsConfig.dynamicStatus = {
        // Script to get the data from, required for DynamicStatus to operate
        'dataScript':''
    };

    widgetsConfig.powerSearch = {
        // Drop list maximum length
        'listMaxLength':10,
        // Drop list shows up on text field focus
        'showListOnFocus':false,
        // Drop list shows up only when text is typed, takes precedence on
        // showListOnFocus
        'requireTextToShowList':false
    };

    widgetsConfig.crazyTable = {
        // Settings can be assigned globally or to individual crazytables.
        // Repeat the crazytable settings under this key using the crazytable
        // source name as the hash index.
        'sourceConfig':{},

        // Content for when a CrazyTable cannot initialize
        'initFailedMsg':'CrazyTable Initialization Failed',

        // Script to get the data from, required for CrazyTable to operate
        'dataScript':'',

        // Hierarchy
            // Expand/collapse can be disabled, hierarchy is then be loaded up
            // in one shot
            'hierarchyFoldable':true,
            // Whether nodes are clickable or not (the +/- sign is always
            // clickable)
            'hierarchyNodesClickable':true,
            // Ability to auto collapse sub nodes when collapsing a node
            'hierarchyKeepExpandState':true,
            // Ability to expand nodes down to a certain level at
            // initialization, -1 for none
            'hierarchyAutoExpandMaxLevel':-1,

        // Rows
            // Duplicate rows can be disabled (duplicates are then ignored)
            'rowsDuplicateAllowed':true,
            // Actions on multiple nodes can be enabled
            'rowsTaggingEnabled':false,

        // Columns
            // Columns order, use empty array for all cols
            'colsOrder':[],
            // Columns toggling can be enabled
            'colsTogglingEnabled':false,
            // Make some columns available only in one mode (hierarchy or global
            // sort), use empty array for all cols
            'colsInHierarchy':[],
            'colsInGlobalSort':[],

        // Sort
            // Ability to enable sorting
            'sortEnabled':false,
            // Ability to allow sorting on some columns only, use empty array
            // for all cols
            'sortCols':[],
            // Restrict sorting mode: 'globalsort', 'hierarchy', or 'both'
            'sortInMode':'both',

        // Search
            // Search can be enabled
            'searchEnabled':false,
            // Restrict searching mode: 'globalsort', 'hierarchy', or 'both'
            'searchInMode':'both',
            // Search can use only some cols, use empty array for all cols
            'searchCols':[],
            // Search autocompletion can be enabled
            'searchAutoCompleteEnabled':false,
            // Columns used as content for search autocompletion, use empty
            // array for all cols
            'searchAutoCompleteCols':[]
    };
}


/**
 * Queuing system for functions to run on window.onLoad event.
 */
var _onLoad = [];
addEventHandler(window, 'onload', doOnLoad, true);

function doOnLoad()
{
    var i;

    // widgets configuration system initialization
    widgetsConfigInit();

    // custom configurations
    for (i = 0; i < widgetsCustomConfig.length; i++) {
        widgetsCustomConfig[i]();
    }

    // widgets initialization
    for (i = 0; i < _onLoad.length; i++) {
        eval(_onLoad[i]);
    }
}


function isDefined(a)
{
    return typeof(window[a]) !== "undefined";
}
function isFunction(a)
{
    return typeof a == 'function';
}
function isObject(a)
{
    return (a && typeof a == 'object') || isFunction(a);
}
function isArray(a)
{
    return isObject(a) && a.constructor == Array;
}


/**
* Return whether or not an object is floating. An object is floating when it
* has not been grafted to the rest of the page body.
*
* Note about nodeType:
*   When the object is floating, parentNode is null in Mozilla, but IE defines
*   parentNode and parentNode.nodeType is equal to 11 (DOCUMENT_FRAGMENT_NODE).
*/
function isFloating(obj)
{
    return obj && (!obj.parentNode || obj.parentNode.nodeType == 11);
}


/**
 * Strip whitespace and non breakable spaces (ascii 0xa0) from the beginning and
 * end of a string. Return an empty string if the given argument is not a
 * string.
 */
function trim(s)
{
    if (typeof(s) != 'string') { return ''; }

    var nbsp = unescape('%a0');
    var c;

    c = s.substring(0, 1);
    while (c == ' ' || c == nbsp || c == "\n") {
        s = s.substring(1, s.length);
        c = s.substring(0, 1);
    }

    c = s.substring(s.length-1, s.length);
    while (c == ' ' || c == nbsp || c == "\n") {
        s = s.substring(0, s.length-1);
        c = s.substring(s.length-1, s.length);
    }

    return s;
}


/**
 * Wraps text without breaking the words if possible.
 */
function wordWrap(text, width)
{
    if (width === undefined) { width = 75; }

    var i, j, line, temp, words, word;
    var paragraphs = text.split('\n');
    var output = '';

    for (i = 0; i < paragraphs.length; i++)
    {
        words = paragraphs[i].split(' ');
        word = '';
        line = '';
        temp = '';

        for (j = 0; j < words.length; j++) {
            word = words[j];
            if (word.length > width) {
                if (line !== '') { line += ' '; }
                output += line+word.substring(0, width-line.length)+'<br>';
                line = word.substr(width-line.length, word.length);
            } else {
                temp = line + ' ' + word;
                if (temp.length > width) {
                    output += line + '<br>';
                    line = word;
                } else {
                    line = temp;
                }
            }
        }

        output += line + '<br>';
    }

    return output;
}


/**
 * Convert special characters to html entities. The translations performed are:
 *   '&' (ampersand) becomes '&amp;'
 *   '"' (double quote) becomes '&quot;',
 *   '<' (less than) becomes '&lt;'
 *   '>' (greater than) becomes '&gt;'
 */
function toHtmlEntities(text)
{
    text = text.replace(/&/g, '&amp;');
    text = text.replace(/>/g, '&gt;');
    text = text.replace(/</g, '&lt;');
    text = text.replace(/"/g, '&quot;');
    return text;
}


/**
 * Convert html entities to special characters, performs the opposite conversion
 * of toHtmlEntities.
 */
function fromHtmlEntities(text)
{
    text = text.replace(/&gt;/g, '>');
    text = text.replace(/&lt;/g, '<');
    text = text.replace(/&quot;/g, '"');
    text = text.replace(/&amp;/g, '&');
    return text;
}


/**
 * Returns the integer part of any given value, or null if the value is not a
 * number.
 */
function toInteger(val)
{
    val = trim(val);
    if (isNaN(val) || val === '') {
        return null;
    } else {
        return Math.floor(val);
    }
}


function escapeQuotes(s)
{
    if (typeof(s) != 'string') { return s; }
    s = s.replace(/\\/g, '\\\\');
    s = s.replace(/'/g, '\\\'');
    s = s.replace(/"/g, '\\"');
    return s;
}


/**
 * Splits a comma separated list in wich each element have their single quote
 * and backslask escaped with a backslash.
 */
function splitEscapedList(list)
{
    list = list.replace(/\\,/g, '\\|');
    list = list.replace(/\\\\\|/g, '\\\\,');
    list = list.split(',');
    for (var i = 0; i < list.length; i++) {
        list[i] = list[i].replace(/\\\|/g, ',');
        list[i] = list[i].replace(/\\\\/g, '\\');
    }
    return list;
}


Array.prototype.hasValue = function (v) {
    var r = false;
    for (var i in this) {
        if (this[i] == v) {
            r = true;
            break;
        }
    }
    return r;
};


/**
 * Get position and size of an object.
 */
function getOffset(obj)
{
    var offset = {};

    offset.height = obj.offsetHeight;

    offset.width = obj.offsetWidth;
    if (navigator && navigator.userAgent.toLowerCase().indexOf("msie") == -1) {
        offset.width -= 1 * 2; // 1 = border width of list div
    }

    offset.top = 0;
    offset.left = 0;
    while (obj) {
        offset.top += obj.offsetTop;
        offset.left += obj.offsetLeft;
        obj = obj.offsetParent;
    }

    return offset;
}


/**
 * Remove all children of a DOM object.
 */
function clearDomObj(obj)
{
    while (obj.childNodes.length > 0) {
        obj.removeChild(obj.childNodes[0]);
    }
    return obj;
}


/**
 * Event handler system allowing multiple handlers to be assigned to the same
 * event of an object. Handlers are user defined functions and are called by
 * this system with two parameters: this and event.
 *
 * In order to add an event handler, use the addEventHandler() function. Its
 * parameters are:
 *
 *   obj          object    Form field on wich the handler is added.
 *   eventName    string    Name of the event, e.g. 'onblur', 'onkeyup'...
 *   fcn          function  Function to handle the event.
 *   hasPriority  boolean   Whether or not the handler should be called first.
 *                          This parameter is optional, default to false.
 */
function addEventHandler(obj, eventName, fcn, hasPriority)
{
    eventName = eventName.toLowerCase();
    var handlers = '_' + eventName;
    if (obj[handlers] === undefined || obj[handlers] === null) {
        obj[handlers] = [];
        obj[eventName] = function (event) {
            for (i = 0; i < this[handlers].length; i++) {
                this[handlers][i](this, event);
            }
        };
    }

    if (hasPriority) {
        obj[handlers].unshift(fcn);
    } else {
        obj[handlers].push(fcn);
    }
}


/**
 * Handles xmlHttp requests objects, from creation to destruction.
 * Automatically aborts any pending requests on window.onUnload event.
 *
 * Usage:
 *   First, get a request object with getRequestObject(). Leave the "id"
 *   parameter unspecified in order to get a brand-new object. Request objects
 *   are automatically destroyed upon successful response.
 *
 *   Then use the "send" method of this object, using the following parameters:
 *     url              string    required, request url with GET parameters
 *     responseHandler  function  required, function handling the request
 *                                response, called with the received responseText
 *     completeHandler  function  optional, function called when readyState
 *                                becomes complete, i.e. equals 4.
 *     failOpenHandler  function  optional, function called if open fails.
 *     failSendHandler  function  optional, function called if send fails.
 *
 * Example:
 *   function receiveRequest(text) {
 *       ...
 *   }
 *   var myRequestObj = getRequestObject();
 *   requestObj.send('getdata.php?source=db1', receiveRequest);
 */
var _xmlHttpRequestObjects = {};
var _xmlHttpRequestObjectsIndex = 0;
addEventHandler(window, 'onunload', abortPendingObjects, false);
function getRequestObject(id)
{
    if (id && _xmlHttpRequestObjects[id]) {
        if (_xmlHttpRequestObjects[id].xmlHttp.readyState !== 0) {
            _xmlHttpRequestObjects[id].xmlHttp.abort();
        }
    } else {
        id = _xmlHttpRequestObjectsIndex++;
        _xmlHttpRequestObjects[id] = new RequestObject(id);
    }

    return _xmlHttpRequestObjects[id];
}
// Aborts any pending requests before closing the current window. This prevents
// onreadystatechange to fire up and execute code that doesn't exist anymore
// resulting in a javascript exception.
function abortPendingObjects()
{
    for (var i in _xmlHttpRequestObjects) {
        if (_xmlHttpRequestObjects[i].xmlHttp.readyState !== 0) {
            _xmlHttpRequestObjects[i].xmlHttp.abort();
        }
    }
}
// main class
function RequestObject(id)
{
    var self = this;

    this.id = id;
    this.xmlHttp = getXmlHttp();

    this.send = function (url, responseHandler, completeHandler,
                          failOpenHandler, failSendHandler) {
        if (!url || !responseHandler) { return; }

        self.xmlHttp.onreadystatechange = function () {
            if (self.xmlHttp.readyState == 4) {
                // gets here in case of response or timeout
                if (self.xmlHttp.responseText && self.xmlHttp.status == 200) {
                    responseHandler(self.xmlHttp.responseText);
                    delete _xmlHttpRequestObjects[self.id];
                }
                if (completeHandler) {
                    completeHandler();
                }
            }
        };

        try {
            self.xmlHttp.open('GET', url, true);
        } catch(e) {
            if (failOpenHandler) {
                failOpenHandler();
            } else {
                return;
            }
        }
        try {
            self.xmlHttp.send(null);
        } catch(e) {
            if (failSendHandler) {
                failSendHandler();
            } else {
                return;
            }
        }
    };
}
// Return an xmlHttpRequest object.
// Values of readyState:
//   0 = uninitialized
//   1 = loading
//   2 = loaded
//   3 = interactive
//   4 = complete
function getXmlHttp()
{
    var xmlHttp = null;
    try {
        xmlHttp = new ActiveXObject("Msxml2.XMLHTTP");
    } catch(e) {
        try {
            xmlHttp = new ActiveXObject("Microsoft.XMLHTTP");
        } catch(E) {
            xmlHttp = null;
        }
    }
    if (!xmlHttp && typeof XMLHttpRequest != "undefined") {
        xmlHttp = new XMLHttpRequest();
    }
    return xmlHttp;
}


/**
 * Experimental dump function
 */
function dump(v, m, l)
{
    l = (l === undefined) ? 0 : parseInt(l, 10);
    if (m > 0 && l > m) { return 'structure too deep<br>'; }
    var s = '';
    if (typeof(v) == 'string' || typeof(v) == 'boolean' || typeof(v) == 'number') {
        s = v + '<br>';
    } else if (typeof(v) == 'object') {
        if (!v) {
            s = 'empty object<br>';
        } else {
            var i;
            var pp = ''; for (i = 0; i < 8; i++) { pp += '&nbsp;'; }
            var p = ''; for (i = 0; i < l; i++) { p += pp; }
            s += ((v.length === undefined) ? 'Hash' : 'Array') + '<br>' +p+ '(<br>';
            for (var e in v) {
                s += p + pp + '[' + e + '] => ' + dump(v[e], m, l+1);
            }
            s += p + ')<br><br>';
        }
    } else {
        s = typeof(v) + '<br>';
    }
    return s;
}
