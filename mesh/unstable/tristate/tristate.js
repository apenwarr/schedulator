/**
 * TriState
 *
 * Author:
 *   Michel Emond
 *   Net Integration Technologies
 *   www.net-itech.com
 *
 * Description:
 *   Allows to turn regular checkboxes into tri-state checkboxes, i.e. with
 *   checked, unchecked, and mixed states.
 *
 * Dependencies:
 *   shared.js
 *   tristate.css
 *
 * Syntax:
 *   In order ot activate TriState on a checkbox, add a "tristate" attribute
 *   containing the values for the three states, and specify the initial state
 *   of the checkbox with the "value" attribute.
 *
 *   If "value" is not among the values specified in "tristate", the checkbox
 *   will default to being unchecked.
 *
 *   Restrictions:
 *     - If specified, the "id" of the checkbox must be the same as the "name".
 *     - TriState creates a hidden field holding the current value of the
 *       tri-state checkbox. The name of this hidden field is the "name" of the
 *       checkbox followed by "_tristate". The script receiving the form submit
 *       must use the hidden field's value instead of the checkbox's.
 *
 * Example:
 *   <input type="checkbox" name="test" tristate="foo,bar,baz" value="bar">
 */

// initialization function called on window.onLoad event, see shared.js
_onLoad.push('triStateInit()');

// flag so other libraries know that TriState has been loaded up
if (document.getElementsByTagName && document.getElementById) {
    var TRISTATE = true;
}

function triStateInit()
{
    if (typeof(TRISTATE) != 'boolean') { return; }

    var objs = document.getElementsByTagName('input');

    var div, hidden, htmlFor, i, label, name, offset, values;

    var labels = document.getElementsByTagName("label");
    var labelsFor = new Array();
    for (i = 0; i < labels.length; i++) {
        htmlFor = labels[i].getAttribute('htmlFor');
        if (!htmlFor) {
            htmlFor = labels[i].getAttribute('for');
        }
        if (htmlFor) {
            labelsFor[htmlFor] = labels[i];
        }
    }

    for (i = 0; i < objs.length; i++) {
        if (objs[i].type == 'checkbox') {
            values = objs[i].getAttribute("tristate");
            if (values === null) { continue; }
            values = values.split(',');
            if (values.length != 3) { continue; }

            name = objs[i].name;
            objs[i].id = name;

            hidden = document.createElement('INPUT');
            hidden.setAttribute('type', 'hidden');
            hidden.setAttribute('name', name+'_tristate');
            hidden.setAttribute('id', name+'_tristate');
            hidden.setAttribute('value', objs[i].value);
            objs[i].form.appendChild(hidden);

            objs[i].value = ''; // just to avoid confusion

            offset = getOffset(objs[i]);
            div = document.createElement("DIV");
            div.className = 'triStateDiv';
            div.style.left = offset.left + "px";
            div.style.top = offset.top + "px";
            div.style.width = offset.width + "px";
            div.style.height = offset.height + "px";
            div.setAttribute('checkboxid', name);
            div.onclick = function(event) {
                triStateClick(this.getAttribute('checkboxid'), false);
            };
            document.body.appendChild(div);

            if ((label = labelsFor[name])) {
                label.setAttribute('checkboxid', name);
                label.onclick = function(e){
                    triStateClick(this.getAttribute('checkboxid'), false);
                    return false;
                };
            }

            triStateClick(name, true);
        }
    }
}

// Values for status: 0=unchecked, 1=checked, 2=mixed
function triStateClick(checkboxId, initialize)
{
    var checkbox = document.getElementById(checkboxId);
    var values = checkbox.getAttribute('tristate').split(',');
    var hidden = document.getElementById(checkboxId+'_tristate');
    var status;

    if (hidden.value == values[1]) {
        status = 2;
    } else if (hidden.value == values[2]) {
        status = 0;
    } else {
        status = 1;
    }

    if (initialize) {
        status--;
        if (status == -1) { status = 2; }
    }

    hidden.value = values[status];
    checkbox.checked = (status !== 0);
    checkbox.disabled = (status == 2);
}
