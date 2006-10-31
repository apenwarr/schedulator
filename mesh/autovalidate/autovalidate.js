/**
 * AutoValidate
 *
 * Author:
 *   Michel Emond
 *   Net Integration Technologies
 *   www.net-itech.com
 *
 * Description:
 *   Allows automatic validation on text fields for integer values,
 *   ip addresses, and domain names.
 *
 * Dependencies:
 *   shared.js
 *   autovalidate.css
 *
 * Global objects:
 *   AutoValidate   Constructor method
 *   autoValidate   AutoValidate object
 *
 * Syntax:
 *   In order to activate AutoValidate on a text field, add an
 *   "autovalidate" attribute containing one the following values:
 *   "integer", "ipaddress", "domainname", or "ipdomainname".
 *
 *   When using "integer", surrond the input tag with a span in order to fully
 *   activate the "spin control" feature. Minimum and maximum values may be
 *   specified with the optional attributes "minvalue" and "maxvalue". Values
 *   outside of the valid span will be automatically converted to the closest
 *   valid value or, when provided, to the value of the optional attribute
 *   "defvalue". The up and down arrows and the up and down keys will increase
 *   or decrease the value of the widget. By default, the increase/decrease step
 *   is by 1. A different step size may be specified with the optional
 *   attributes "stepsize".
 *
 *   When using "ipaddress", the input tag must absolutely be surronded by a
 *   span.
 *
 *   When using "domainname", additionnal allowed characters may be specified
 *   with the optional attribute "allowchars". By default, allowed characters
 *   are letters, numbers, "-", "_", and ".".
 *
 *   When using "ipdomainname", the input tag must absolutely be surronded by a
 *   span, and the optional attribute "allowchars" may be used (see previous
 *   explanation for "domainname").
 *
 *   Restrictions:
 *     - The onChange, onKeyPress, and onKeyDown events of the form fields are
 *       overwritten.
 *     - When surronding with a span, the input tag must absolutely be alone
 *       inside, without any spaces or carriage returns between the tags.
 *     - AutoValidate works only on single line text fields, it can not be used
 *       with textarea.
 *
 * Examples:
 *   <span><input name="byte" autovalidate="integer" minvalue="0"
 *       maxvalue="255" stepsize="5" size="5" value="42"></span>
 *   <span><input name="ip" autovalidate="ipaddress" size="20"
 *       value="127.0.0.1"></span>
 *   <input name="mydomain" autovalidate="domainname" size="30"
 *       value="localhost" allowchars="*">
 */

/*global addEventHandler, document, navigator, setTimeout, toInteger,
         widgetsConfig, window */

function AutoValidate()
{
    var self = this;
    this.isReady = true;

    if (!document.getElementById || !document.getElementsByTagName) {
        this.isReady = false;
        return;
    }

    function spinOnKeyPressHandler(textField, event)
    {
        if (!event && window.event) {
            event = window.event;
        }
        // up key (38) or down key (40)
        if (event.keyCode == 38 || event.keyCode == 40) {
            spinValidateValue(textField, (event.keyCode == 38 ? 1 : -1));
        }
    }

    function getSpinButton(name, thisInputId)
    {
        var increment = (name == 'up' ? 1 : -1);

        var img = document.createElement("IMG");
        img.setAttribute("src",
            widgetsConfig.imgBasePath+'autovalidate/'+name+".png");
        img.setAttribute("width", "15");
        img.setAttribute("height", "8");
        img.className = 'spinButtonReleased';
        img.onmouseup = function(event) {
            this.className = 'spinButtonReleased';
        };
        img.onmouseout = function(event) {
            this.className = 'spinButtonReleased';
        };
        img.onmousedown = function(event) {
            this.className = 'spinButtonPushed';
        };

        var spinBtn = document.createElement("A");
        spinBtn.setAttribute('textFieldId', thisInputId);
        spinBtn.setAttribute('increment', increment);
        spinBtn.onmousedown = function(event) {
            var textFieldId = this.getAttribute('textFieldId');
            var textField = document.getElementById(textFieldId);
            var tempDate = new Date();
            var timethread = tempDate.getTime();
            textField.setAttribute('spinUpdate', timethread);
            var increment = Math.floor(this.getAttribute('increment'));
            self.spinUpdate(textFieldId, timethread, increment, 50);
        };
        spinBtn.onmouseup = function(event) {
            var textFieldId = this.getAttribute('textFieldId');
            var textField = document.getElementById(textFieldId);
            textField.setAttribute('spinUpdate', '');
            textField.focus();
            textField.select();
        };
        spinBtn.onmouseout = function(event) {
            var textFieldId = this.getAttribute('textFieldId');
            var textField = document.getElementById(textFieldId);
            if (textField.getAttribute('spinUpdate')) {
                textField.setAttribute('spinUpdate', '');
                textField.focus();
                textField.select();
            }
        };
        spinBtn.appendChild(img);

        return spinBtn;
    }

    function spinValidateValue(textField, increment)
    {
        var curValue = toInteger(textField.value);
        var minValue = textField.getAttribute('minvalue');
        var maxValue = textField.getAttribute('maxvalue');
        var defValue = textField.getAttribute('defvalue');
        var stepSize = textField.getAttribute('stepsize');

        if (curValue === null) {
            if (isNaN(increment)) {
                // got here by onChange
                if (defValue === '') {
                    if (minValue !== '' && minValue > 0) {
                        defValue = minValue;
                    } else if (maxValue !== '' && maxValue < 0) {
                        defValue = maxValue;
                    } else {
                        defValue = 0;
                    }
                }
                curValue = defValue;
            } else {
                curValue = (increment == 1 ? minValue : maxValue);
            }

        } else if (maxValue !== '' && curValue > maxValue) {
            curValue = (defValue === '' ? maxValue : defValue);

        } else if (minValue !== '' && curValue < minValue) {
            curValue = (defValue === '' ? minValue : defValue);

        } else if (
            (increment == 1 && (maxValue === '' || curValue < maxValue)) ||
            (increment == -1 && (minValue === '' || curValue > minValue))) {
            if (Math.abs(stepSize) == 1) {
                curValue += increment;
            } else {
                curValue = (((increment == 1) ? Math.floor :
                    Math.ceil)(curValue / stepSize) + increment) * stepSize;
                if (maxValue !== '' && curValue > maxValue) {
                    curValue = maxValue;
                } else if (minValue !== '' && curValue < minValue) {
                    curValue = minValue;
                }
            }
            curValue = curValue;
        }

        textField.value = curValue;
        return true;
    }

    function ipaddressOnChangeHandler(obj)
    {
        var isValidIp = true;
        var alertSpan = obj.parentNode.childNodes[1];
        var regExp = new RegExp('^([0-9]{1,3})\\.([0-9]{1,3})\\.'+
            '([0-9]{1,3})\\.([0-9]{1,3})(\\/([0-9]{1,2}))?$');
        var match = obj.value.match(regExp);
        if (!match ||
            match[1] < 0 || match[1] > 255 ||
            match[2] < 0 || match[2] > 255 ||
            match[3] < 0 || match[3] > 255 ||
            match[4] < 0 || match[4] > 255 ||
            (match[6] !== undefined &&
            (match[6] < 0 || match[6] > 32))) {
            alertSpan.innerHTML = ' Invalid Value';
            isValidIp = false;
        } else {
            alertSpan.innerHTML = '';
        }
        obj.parentNode.childNodes[1].style.visibility =
            isValidIp ? 'hidden' : 'visible';
    }

    function domainnameOnChangeHandler(obj)
    {
        var allowChars = obj.getAttribute('allowchars');
        if (!allowChars) { allowChars = ''; }
        allowChars = allowChars.replace(/([\\\|\(\)\[\{\^\$\*\+\?\.])/g,
                                        '\\$1');
        var regExp = new RegExp('[^-a-z0-9_\\.'+allowChars+']', 'gi');
        obj.value = obj.value.replace(regExp, '');
        //obj.value = obj.value.replace(/\.+/gi, '.');
    }

    function ipdomainnameOnChangeHandler(obj)
    {
        var isValidIp = true;
        var alertSpan = obj.parentNode.childNodes[1];
        var regExp = new RegExp('^([0-9]+)\\.([0-9]+)\\.'+
            '([0-9]+)\\.([0-9]+)(\\/(.*$))?$');
        var match = obj.value.match(regExp);
        if (!match) {
            // fixes domain name value
            var allowChars = obj.getAttribute('allowchars');
            if (!allowChars) { allowChars = ''; }
            allowChars = allowChars.replace(/([\\\|\(\)\[\{\^\$\*\+\?\.])/g,
                                            '\\$1');
            regExp = new RegExp('[^-a-z0-9_\\.\\/'+allowChars+']', 'gi');
            obj.value = obj.value.replace(regExp, '');
        } else if (
            // validates ip address
            match[1] < 0 || match[1] > 255 ||
            match[2] < 0 || match[2] > 255 ||
            match[3] < 0 || match[3] > 255 ||
            match[4] < 0 || match[4] > 255 ||
            match[6] === '' ||
            (match[6] !== undefined &&
            (isNaN(match[6]) ||
            (match[6] < 0 || match[6] > 32)))) {
                alertSpan.innerHTML = ' Invalid Value';
                isValidIp = false;
        } else {
                alertSpan.innerHTML = '';
        }
        obj.parentNode.childNodes[1].style.visibility =
            isValidIp ? 'hidden' : 'visible';
    }

    this.init = function ()
    {
        var inputs = document.getElementsByTagName("input");
        for (var i = 0; i < inputs.length; i++) {
            self.widgetInit(inputs[i]);
        }
    };

    this.spinUpdate = function (fieldId, timethread, increment, delay)
    {
        var field = document.getElementById(fieldId);
        if (!field) { return; }
        if (field.getAttribute('spinUpdate') == timethread) {
            if (spinValidateValue(field, increment)) {
                var tempDate = new Date();
                var useDelay = ((tempDate.getTime() - timethread) < 500) ?
                    500 : delay;
                setTimeout("autoValidate.spinUpdate('"+fieldId+"', "+timethread+
                           ", "+increment+", "+delay+");", useDelay);
            } else {
                field.setAttribute('spinUpdate', '');
            }
        }
    };

    this.widgetInit = function (thisInput)
    {
        var alertspan, autoValidateType, cell, defValue, downLnk, maxValue;
        var minValue, row, span, stepSize, table, tbody, upLnk;

        var isNS4 = (navigator.appName == "Netscape");

        autoValidateType = thisInput.getAttribute('autovalidate');
        if (!autoValidateType) { return; }
        autoValidateType = autoValidateType.toLowerCase();

        if (autoValidateType && thisInput.type == 'text' &&
            thisInput.name !== '') {
            if (!thisInput.id) { thisInput.id = thisInput.name; }

            // ip address
            if (autoValidateType == 'ipaddress') {
                span = thisInput.parentNode;
                if (span.tagName.toLowerCase() == 'span' &&
                    span.childNodes.length == 1) {
                    alertspan = document.createElement("SPAN");
                    alertspan.style.color = 'red';
                    alertspan.style.fontWeight = 'bold';
                    alertspan.style.visibility = 'hidden';
                    alertspan.innerHTML = '';
                    span.appendChild(alertspan);

                    addEventHandler(thisInput, 'onchange',
                        ipaddressOnChangeHandler);
                    thisInput.onkeypress = function(event) {
                        if (!event && window.event) {
                            event = window.event;
                        }
                        var isNS4 = (navigator.appName == "Netscape");
                        var key = isNS4 ? event.which : event.keyCode;
                        if ((!isNS4 || key > 31) && !event.ctrlKey &&
                            String.fromCharCode(key).match(/[0-9\.\/]/gi) === null) {
                            if (isNS4) {
                                return false;
                            } else {
                                event.returnValue = false;
                            }
                        }
                    };
                }

            // domain name
            } else if (autoValidateType == 'domainname') {
                addEventHandler(thisInput, 'onchange',
                    domainnameOnChangeHandler);
                thisInput.onkeypress = function(event) {
                    if (!event && window.event) {
                        event = window.event;
                    }
                    var isNS4 = (navigator.appName == "Netscape");
                    var key = isNS4 ? event.which : event.keyCode;
                    var allowChars = this.getAttribute('allowchars');
                    if (!allowChars) { allowChars = ''; }
                    allowChars = allowChars.replace(
                        /([\\\|\(\)\[\{\^\$\*\+\?\.])/g, '\\$1');
                    var regExp = new RegExp('[-a-z0-9_\\.'+allowChars+']',
                                            'gi');
                    if ((!isNS4 || key > 31) &&
                        String.fromCharCode(key).match(regExp) === null) {
                        if (isNS4) {
                            return false;
                        } else {
                            event.returnValue = false;
                        }
                    }
                };

            // ip address AND domain name
            } else if (autoValidateType == 'ipdomainname') {
                span = thisInput.parentNode;
                if (span.tagName.toLowerCase() == 'span' &&
                    span.childNodes.length == 1) {
                    alertspan = document.createElement("SPAN");
                    alertspan.style.color = 'red';
                    alertspan.style.fontWeight = 'bold';
                    alertspan.style.visibility = 'hidden';
                    alertspan.innerHTML = '';
                    span.appendChild(alertspan);

                    addEventHandler(thisInput, 'onchange',
                        ipdomainnameOnChangeHandler);
                    thisInput.onkeypress = function(event) {
                        if (!event && window.event) {
                            event = window.event;
                        }
                        var isNS4 = (navigator.appName == "Netscape");
                        var key = isNS4 ? event.which : event.keyCode;
                        var allowChars = this.getAttribute('allowchars');
                        if (!allowChars) { allowChars = ''; }
                        allowChars = allowChars.replace(
                            /([\\\|\(\)\[\{\^\$\*\+\?\.])/g, '\\$1');
                        var regExp = new RegExp('[-a-z0-9_\\.\\/'+allowChars+
                                                ']', 'gi');
                        if ((!isNS4 || key > 31) &&
                            String.fromCharCode(key).match(regExp) === null) {
                            if (isNS4) {
                                return false;
                            } else {
                                event.returnValue = false;
                            }
                        }
                    };
                }

            // integer
            } else if (autoValidateType == 'integer') {
                minValue = toInteger(thisInput.getAttribute('minvalue'));
                maxValue = toInteger(thisInput.getAttribute('maxvalue'));
                defValue = toInteger(thisInput.getAttribute('defvalue'));
                stepSize = toInteger(thisInput.getAttribute('stepsize'));
                if (minValue !== null && maxValue !== null &&
                    minValue > maxValue) {
                    minValue = '';
                    maxValue = '';
                } else {
                    if (minValue === null) { minValue = ''; }
                    if (maxValue === null) { maxValue = ''; }
                }
                if (defValue === null) { defValue = ''; }
                if (stepSize === null) { stepSize = 1; }
                thisInput.setAttribute('minvalue', minValue);
                thisInput.setAttribute('maxvalue', maxValue);
                thisInput.setAttribute('defvalue', defValue);
                thisInput.setAttribute('stepsize', stepSize);
                thisInput.setAttribute('spinUpdate', '');
                if (isNS4) {
                    thisInput.onkeypress = function(event) {
                        spinOnKeyPressHandler(this, event);
                        if (event.which > 31 &&
                            String.fromCharCode(event.which).match(/[-0-9]/gi) === null) {
                            return false;
                        }
                    };
                } else {
                    thisInput.onkeydown = function(event) {
                        spinOnKeyPressHandler(this, event);
                    };
                    thisInput.onkeypress = function(event) {
                        event = window.event;
                        if (String.fromCharCode(event.keyCode).match(/[-0-9]/gi) === null) {
                            event.returnValue = false;
                        }
                    };
                }
                addEventHandler(thisInput, 'onchange', spinValidateValue);

                // up/down buttons added if text field is alone inside a span
                span = thisInput.parentNode;
                if (span.tagName.toLowerCase() == 'span' &&
                    span.childNodes.length == 1) {

                    upLnk = getSpinButton('up', thisInput.id);
                    downLnk = getSpinButton('down', thisInput.id);

                    table = document.createElement('TABLE');
                    tbody = document.createElement('TBODY');
                    table.appendChild(tbody);
                    table.cellSpacing = 0;
                    table.cellPadding = 0;
                    table.border = 0;

                    row = document.createElement('TR');
                    cell = document.createElement('TD');
                    cell.rowSpan = 2;
                    cell.appendChild(thisInput);
                    row.appendChild(cell);
                    cell = document.createElement('TD');
                    cell.appendChild(upLnk);
                    row.appendChild(cell);
                    tbody.appendChild(row);

                    row = document.createElement('TR');
                    cell = document.createElement('TD');
                    cell.appendChild(downLnk);
                    row.appendChild(cell);
                    tbody.appendChild(row);

                    span.appendChild(table);
                }
            }
        }
    };
}

var autoValidate = new AutoValidate();

if (autoValidate.isReady) {
    // initialization function called on window.onLoad event, see shared.js
    _onLoad.push('autoValidate.init()');
}
