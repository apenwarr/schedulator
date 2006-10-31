/**
 * DynamicStatus
 *
 * Author:
 *   Michel Emond
 *   Net Integration Technologies
 *   www.net-itech.com
 *
 * Description:
 *   Uses JavaScript's XmlHttp object to dynamically update the status of
 *   different widgets.
 *
 *   DynamicStatus provides support for three kinds of widgets: read-only
 *   widgets, regular form fields, and EditInPlace widgets.
 *
 *   The read-only widgets are graphical or text objects specific to
 *   DynamicStatus. Their purpose is only to provide information, they are not
 *   input widgets.
 *
 *   In order to combine the dynamic update with input widgets, DynamicStatus
 *   simply enhance regular form fields. Also, when the value of a form field is
 *   changed by a user, the form field is marked as 'dirty', and this field will
 *   not be changed anymore by DynamicStatus (until the page is reloaded).
 *
 *   In order to combine DynamicStatus and EditInPlace, DynamicStatus adds its
 *   own properties to already existing EditInPlace widgets. When the widget is
 *   in 'edit' mode, it is seen as 'dirty' and the value will not be changed
 *   (until the cancel button is pressed).
 *
 * Dependencies:
 *   shared.js
 *   json.js
 *   dynamicstatus.css
 *   editinplace.js (in some cases)
 *   powersearch.js (in some cases)
 *
 * Global objects:
 *   DynamicStatus   Constructor method
 *   dynamicStatus   DynamicStatus object
 *
 * Syntax:
 *   In order to define a read-only widget, put an empty span and set the 'id'
 *   and 'dyntype' attributes, 'id' being a unique identification key and
 *   'dyntype' being one of the following values: 'gauge', 'light', 'text', or
 *   'progressbar'.
 *
 *   Additionnaly, with the 'progressbar' type, you may also provide a 'width'
 *   and 'height' attribute to define the size of the widget. If missing, those
 *   values will default respectively to 200 and 15 pixels.
 *
 *   In order to activate the dynamic update on a form field, set the 'dynkey'
 *   attribute on the form field, 'dynkey' being a unique identification key. It
 *   must also be unique across the 'id' attribute of the read-only widgets.
 *
 *   Supported HTML form fields are: input (of type text, checkbox, and radio),
 *   textarea, and select (with or without the 'multiple' attribute).
 *
 *   In order to add the DynamicStatus properties on an EditInPlace widget, the
 *   'editinplace.js' library must absolutely be loaded up before
 *   'dynamicstatus.js'. Then, set the 'dynkey' attribute on the span, along
 *   with the other EditInPlace attributes, 'dynkey' being a unique
 *   identification key. It must also be unique across the 'id' attribute of the
 *   read-only widgets, and the 'dynkey' attribute of the dynamic form fields.
 *
 *   See the examples below.
 *
 *   Restrictions:
 *     - When using EditInPlace, load 'editinplace.js' before
 *       'dynamicstatus.js'.
 *     - The value of the 'id' or 'dynkey' attributes must not contain commas.
 *     - The onChange event of the form fields is overwritten.
 *
 * Backend script:
 *   DynamicStatus relies on a backend script providing the status updates. The
 *   value of dataScript must absolutely be set, in a custom configuration, to
 *   the location and name of this script.
 *
 *   Setting dataScript in a custom configuration must be done like this:
 *     widgetsConfig.dynamicStatus.dataScript = '<script name>';
 *
 *   See shared.js for information about custom configuration.
 *
 *   DynamicStatus will send two parameters to the backend script in the form
 *   of an HTTP GET request:
 *
 *     'keys':  string, this is a comma separated list of the keys DynamicStatus
 *              is requesting, the backend script should not return any other
 *              values
 *
 *     'first': boolean, optional, if present and true, it means this is the
 *              very first call to the backend script and all the 'keys' must be
 *              returned right away.
 *
 *   DynamicStatus expects the results to be returned in a format that will be
 *   automatically parsed as a hash in JavaScript. It has to look like this:
 *
 *     {"key1":"status1","key2":"status2",...}
 *
 *   The ',...' at the end means that more key/status pairs can be added but
 *   must not be present in a real returned value. Make sure to correctly escape
 *   '\' and '"' with a '\' in order for the string to be valid in JavaScript.
 *
 *   Depending on the type, the expected status values are:
 *
 *     Read-only widgets:
 *
 *       'gauge'
 *         A number from 0 to 100.
 *
 *       'light'
 *         Either 'off', 'green', 'yellow', or 'red'.
 *
 *       'text'
 *         Any string; an empty string will get the widget to occupy no space.
 *
 *       'progressbar'
 *         A number from 0 to 100, followed by ':' and then any string; the
 *         string will appear at the right of the progressbar
 *
 *       About HTML entities: For 'text' and 'progressbar', since the insertion
 *       of HTML is allowed, any special HTML characters must be converted to
 *       their equivalent entities by the backend script.
 *
 *     Form fields:
 *
 *       'select' (with the 'multiple' attribute) and 'checkbox'
 *         A comma separated list of values. Commas "," and backslash "\"
 *         must be escaped with a backslash "\". Do not escape characters other
 *         than commas "," and backslash "\". Those characters must be escaped
 *         for the value itself. See previous comment about characters to escape
 *         in the final string.
 *
 *       'input' (text, checkbox, and radio), 'textarea', and 'select' (without
 *       the 'multiple' attribute)
 *         Any string.
 *
 *     EditInPlace:
 *
 *       Any string, or comma separated list of values, depending on the type of
 *       form field. See documentation about the updateInitValue public function
 *       in editinplace.js.
 *
 *   Make sure to include an 'Expires' http header with a date in the past.
 *   Otherwise, IE gets confused when it is set to 'automatically' detect new
 *   versions.
 *
 * Examples:
 *   Read-only widgets:
 *     <span id="mygauge" dyntype="gauge"></span>
 *     <span id="mylight" dyntype="light"></span>
 *     <span id="mytext" dyntype="text"></span>
 *     <span id="mybar" dyntype="progressbar" width="500" height="20"></span>
 *
 *   Regular form fields:
 *     <input type="radio" name="myRadio" dynkey="radio1" value="foo"> foo
 *     <input type="radio" name="myRadio" dynkey="radio1" value="bar"> bar
 *
 *     <input type="text" name="myTextField1" dynkey="field1">
 *
 *   EditInPlace widget:
 *     <span id="myTextField2" editinplace="text" size="50"
 *       dynkey="field2">text content</span>
 */

/*global addEventHandler, clearDomObj, document, editInPlace, getRequestObject,
         isDefined, isObject, JSON, powerSearch, setTimeout, splitEscapedList,
         widgetsConfig */

function DynamicStatus()
{
    var self = this;
    this.isReady = true;

    if (!document.getElementsByTagName) {
        this.isReady = false;
        return;
    }

    var dynamicReadOnly = {};
    var dynamicFields = {};
    var dynamicDirtyState = {};
    var dynamicEditInPlace = {};
    var dynamicWidgetsList = '';

    function dynamicStatusGetHash(list)
    {
        var hash = {};

        var values = splitEscapedList(list);
        for (var i = 0; i < values.length; i++) {
            hash[values[i]] = 1;
        }

        return hash;
    }

    function formFieldOnChangeHandler(obj)
    {
        var dynkey = obj.getAttribute('dynkey');
        if (!dynkey) { return; }
        dynamicDirtyState[dynkey] = true;
    }

    this.init = function ()
    {
        var barHeight, barWidth, dynkey, fields, i, img, progBar, progBorder, s;
        var dynType, table, tbody, td, textSpan, thisSpan, tr;
        var widgetsArray = [];

        if (widgetsConfig.dynamicStatus.dataScript === '') { return; }

        // dynamic form fields
        var fieldTypes = ["input","select","textarea"];
        for (s in fieldTypes) {
            fields = document.getElementsByTagName(fieldTypes[s]);
            for (i = 0; i < fields.length; i++) {
                dynkey = fields[i].getAttribute('dynkey');
                if (dynkey !== null) {
                    if (fieldTypes[s] != 'input' || fields[i].type == 'text') {
                        dynamicFields[dynkey] = fields[i];
                        dynamicDirtyState[dynkey] = false;
                        addEventHandler(fields[i], 'onchange',
                            formFieldOnChangeHandler);
                        widgetsArray.push(dynkey);
                    } else if (fields[i].type == 'radio' ||
                               fields[i].type == 'checkbox') {
                        if (dynamicFields[dynkey] === undefined) {
                            dynamicFields[dynkey] = [];
                            dynamicDirtyState[dynkey] = false;
                            widgetsArray.push(dynkey);
                        }
                        dynamicFields[dynkey].push(fields[i]);
                        addEventHandler(fields[i], 'onchange',
                            formFieldOnChangeHandler);
                    }
                }
            }
        }

        // dynamic read-only widgets and dynamic EditInPlace widgets
        var spans = document.getElementsByTagName("span");
        for (i = 0; i < spans.length; i++) {
            thisSpan = spans[i];
            if (thisSpan.id === '') { continue; }

            // dynamic EditInPlace widgets
            if (isDefined('editInPlace') && editInPlace.isReady) {
                dynkey = thisSpan.getAttribute('dynkey');
                if (dynkey !== null && editInPlace.spans[thisSpan.id]) {
                    dynamicEditInPlace[dynkey] = thisSpan.id;
                    widgetsArray.push(dynkey);
                    continue;
                }
            }

            // dynamic read-only widgets
            dynType = thisSpan.getAttribute('dyntype');
            if (dynType === null) { continue; }

            dynamicReadOnly[thisSpan.id] = thisSpan;
            thisSpan = clearDomObj(thisSpan);
            widgetsArray.push(thisSpan.id);

            // gauge
            if (dynType == 'gauge') {
                img = document.createElement("IMG");
                img.setAttribute("src",
                    widgetsConfig.imgBasePath+"dynamicstatus/gauge/0.gif");
                img.setAttribute("width", "109");
                img.setAttribute("height", "18");
                thisSpan.appendChild(img);

            // light
            } else if (dynType == 'light') {
                img = document.createElement("IMG");
                img.setAttribute("src",
                    widgetsConfig.imgBasePath+"dynamicstatus/light/off.gif");
                img.setAttribute("width", "17");
                img.setAttribute("height", "17");
                thisSpan.appendChild(img);

            // text
            } else if (dynType == 'text') {
                // nothing to do

            // progress bar
            } else if (dynType == 'progressbar') {
                barWidth = thisSpan.getAttribute('width');
                if (barWidth === null || isNaN(barWidth)) {
                    barWidth = 200;
                }
                thisSpan.setAttribute('width', barWidth);
                barHeight = thisSpan.getAttribute('height');
                if (barHeight === null || isNaN(barHeight)) {
                    barHeight = 15;
                }

                progBorder = document.createElement("DIV");
                progBorder.className = 'progBorder';
                progBorder.style.fontSize = '1px'; // IE is silly
                progBorder.style.height = barHeight+'px';
                progBorder.style.width = barWidth+'px';
                progBorder.style.margin = '0';
                progBorder.style.padding = '0';

                progBar = document.createElement("DIV");
                progBar.className = 'progBar';
                progBar.style.height = (barHeight-4)+'px';
                progBar.style.width = '0';
                progBar.style.margin = '2px';
                progBar.style.padding = '0';
                progBorder.appendChild(progBar);

                textSpan = document.createElement("SPAN");

                table = document.createElement("TABLE");
                table.cellspacing = 0;
                table.cellpadding = 0;
                table.border = 0;
                tbody = document.createElement('TBODY');
                tr = document.createElement("TR");
                td = document.createElement("TD");
                td.appendChild(progBorder);
                tr.appendChild(td);
                td = document.createElement("TD");
                td.appendChild(textSpan);
                tr.appendChild(td);
                tbody.appendChild(tr);
                table.appendChild(tbody);
                thisSpan.appendChild(table);
            }
        }

        dynamicWidgetsList = widgetsArray.join(',');

        self.sendRequest(true);
    };

    this.sendRequest = function (firstRequest)
    {
        var requestObj = getRequestObject();
        var url = widgetsConfig.dynamicStatus.dataScript + '?keys=' +
                  dynamicWidgetsList + (firstRequest ? '&first=1' : '');

        var completeHandler = function () {
            setTimeout('dynamicStatus.sendRequest()'+'', 250);
        };
        var failOpenHandler = function () {
            setTimeout('dynamicStatus.sendRequest('+firstRequest+')', 120000);
        };

        requestObj.send(url, self.dynamicStatusUpdate, completeHandler,
                        failOpenHandler);
    };

    this.dynamicStatusUpdate = function (statusInfo)
    {
        if (statusInfo === '') { return; }

        var status = JSON.parse(statusInfo);
        if (!isObject(status)) { return; }

        var barText, divs, fieldObj, i, spanObj, dynType, temp;
        var textSpan, value, values;

        for (var objId in status) {
            // dynamic EditInPlace widgets
            if (dynamicEditInPlace[objId]) {
                editInPlace.updateInitValue(dynamicEditInPlace[objId],
                                            status[objId]);

            // dynamic form fields
            } else if (dynamicFields[objId] && !dynamicDirtyState[objId]) {
                fieldObj = dynamicFields[objId];

                if (fieldObj.type == 'text' || fieldObj.type == 'textarea') {
                    // special handling for PowerSearch fields
                    if (!isDefined('powerSearch') || !powerSearch.isReady ||
                        !powerSearch.silentUpdate(fieldObj.name,
                                                  status[objId])) {
                        fieldObj.value = status[objId];
                    }

                } else if (fieldObj.type == 'select-one') {
                    var newIndex = -1;
                    for (i = 0; i < fieldObj.options.length; i++) {
                        if (fieldObj.options[i].value == status[objId]) {
                            newIndex = i;
                            break;
                        }
                    }
                    fieldObj.selectedIndex = newIndex;

                } else if (fieldObj.type == 'select-multiple') {
                    values = dynamicStatusGetHash(status[objId]);
                    for (i = 0; i < fieldObj.options.length; i++) {
                        fieldObj.options[i].selected =
                            (values[fieldObj.options[i].value] !== undefined);
                    }

                } else if (fieldObj.constructor == Array &&
                           fieldObj.length > 0) {
                    if (fieldObj[0].type == 'checkbox') {
                        values = dynamicStatusGetHash(status[objId]);
                        for (i = 0; i < fieldObj.length; i++) {
                            fieldObj[i].checked =
                                (values[fieldObj[i].value] !== undefined);
                        }
                    } else if (fieldObj[0].type == 'radio') {
                        for (i = 0; i < fieldObj.length; i++) {
                            fieldObj[i].checked =
                                (fieldObj[i].value == status[objId]);
                        }
                    }
                }

            // dynamic read-only widgets
            } else if (dynamicReadOnly[objId]) {
                spanObj = dynamicReadOnly[objId];
                dynType = spanObj.getAttribute('dyntype');

                // gauge
                if (dynType == 'gauge') {
                    if (spanObj.childNodes.length == 1) {
                        value = Math.round(status[objId] / 5) * 5;
                        value = (isNaN(value) || value < 0) ? 0 :
                                    (value > 100) ? 100 : value;
                        spanObj.childNodes[0].src = widgetsConfig.imgBasePath+
                            'dynamicstatus/gauge/'+value+'.gif';
                    }

                // light
                } else if (dynType == 'light') {
                    if (spanObj.childNodes.length == 1) {
                        value = status[objId].toLowerCase();
                        if (value != 'green' && value != 'yellow' &&
                            value != 'red') {
                            value = 'off';
                        }
                        spanObj.childNodes[0].src = widgetsConfig.imgBasePath+
                            'dynamicstatus/light/'+value+'.gif';
                    }

                // text
                } else if (dynType == 'text') {
                    spanObj.innerHTML = status[objId];
                    spanObj.style.display = (status[objId] === '') ? 'none' :'';

                // progress bar
                } else if (dynType == 'progressbar') {
                    // extract percentage and text from returned value
                    temp = status[objId].split(':');
                    value = (temp.length > 0) ? temp.shift() : 0;
                    value = (isNaN(value) || value < 0) ? 0 :
                                (value > 100) ? 100 : value;
                    barText = (temp.length > 0) ? temp.join(':') : '';
                    // set progress bar width
                    value = value / 100 * (spanObj.getAttribute('width')-4);
                    divs = spanObj.getElementsByTagName('div');
                    for (i = 0; i < divs.length; i ++) {
                        if (divs[i].className == 'progBar') {
                            divs[i].style.width = value+'px';
                        }
                    }
                    // set caption text
                    textSpan = spanObj.getElementsByTagName('span');
                    if (textSpan.length > 0) {
                        textSpan[0].innerHTML = barText;
                    }
                }
            }
        }
    };
}

var dynamicStatus = new DynamicStatus();

if (dynamicStatus.isReady) {
    // initialization function called on window.onLoad event, see shared.js
    _onLoad.push('dynamicStatus.init()');
}
