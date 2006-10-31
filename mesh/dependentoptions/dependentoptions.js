/**
 * DependentOptions
 *
 * Author:
 *   Michel Emond
 *   Net Integration Technologies
 *   www.net-itech.com
 *
 * Description:
 *   Allows to set dependancies between form fields. Form fields will become
 *   enabled or disabled based on conditions.
 *
 * Dependencies:
 *   shared.js
 *
 * Global objects:
 *   DependentOptions   Constructor method
 *   dependentOptions   DependentOptions object
 *
 * Syntax:
 *   In order to activate DependentOptions on a form field, add a "dependon"
 *   attribute containing the NAME of the field it depends on, and specify the
 *   dependency condition with EITHER the "enableif" or the "disableif"
 *   attribute.
 *
 *   The value for "enableif" and "disableif" is either a value, or a comma
 *   separated list a values; in the latter case, the OR logic is used. Values
 *   are tested against the current form field value, for text field, and
 *   against whether or not a value is selected, for radio buttons, checkboxes,
 *   and select lists. Evaluation of values is case insensitive, and spaces are
 *   trimmed from the beginning and the end before evaluating.
 *
 *   Restrictions:
 *     - DependentOptions works with multiple forms on a page, however
 *       dependancies cannot be set across different forms.
 *     - Forms MUST have a UNIQUE NAME.
 *     - The onClick, onKeyUp, and onBlur events of the form fields are
 *       overwritten.
 *
 * Behavior:
 *   Form fields automatically get disabled if the form field they depend on
 *   becomes disabled.
 *
 *   When updating a form field, the dependency update cascades through its
 *   sub-dependency hierarchy.
 *
 *   Dependancies on non-existant fields will fail silently.
 *
 *   Labels attached to a form field get disabled along with it.
 *
 *   There is no need to manually set the disable property in the html. Form
 *   fields are disabled appropriately during the initialization.
 *
 * Example:
 *   <p><input id="useitem" type="checkbox" name="useitem" value="1">
 *   <label for="useitem">Use Item</label>
 *   <p>Item:
 *   <select dependon="useitem" enableif="1" name="item">
 *       <option value="cat">Cat
 *       <option value="dog">Dog
 *       <option value="table">Table
 *   </select>
 *   <p>Is this your favourite animal?
 *   <input dependon="item" disableif="table" id="favoriteyes" type="radio"
 *       name="isfavorite" value="yes">
 *   <label for="favoriteyes">Yes</label>
 *   <input dependon="item" disableif="table" id="favoriteno" type="radio"
 *       name="isfavorite" value="no">
 *   <label for="favoriteno">No</label>
 */

/*global addEventHandler, document, trim */

function DependentOptions()
{
    var self = this;
    this.isReady = true;

    if (!document.getElementsByTagName) {
        this.isReady = false;
        return;
    }

    var dependentsMap = {};
    var dependencyMap = {}; // used in initialization to avoid cycles
    var labelsObj = {};

    function widgetInit(widget)
    {
        var dependOn = widget.getAttribute('dependon');
        var enableIf = widget.getAttribute('enableif');
        var disableIf = widget.getAttribute('disableif');

        var formName = widget.form.name;
        if (!formName) { return; }

        if (!dependencyMap[formName]) {
            dependencyMap[formName] = {};
            dependentsMap[formName] = {};
        }

        if (dependOn && (enableIf !== undefined || disableIf !== undefined ) &&
            document.forms[formName] && document.forms[formName][dependOn] &&
            !dependencyMap[formName][widget.name] &&
            !dependsOn(formName, dependOn, widget.name)) {
            // fail silently on unsupported widget types
            var dependObj = document.forms[formName][dependOn];
            var objType;
            if ((dependObj.length !== undefined) &&
                (dependObj.options === undefined)) {
                // radio or checkbox with many values
                if (dependObj[0]) { objType = dependObj[0].type; }
            } else {
                // any other widget types
                objType = dependObj.type;
            }
            if (objType != 'checkbox' && objType != 'radio' &&
                objType != 'select-one' && objType != 'select-multiple' &&
                objType != 'text' && objType != 'textarea') {
                return;
            }

            // build the dependency maps
            dependencyMap[formName][widget.name] = dependOn;
            if (!dependentsMap[formName][dependOn]) {
                dependentsMap[formName][dependOn] = [];
            }
            var temp = {};
            temp.dependentName = widget.name;
            temp.enableIf = enableIf;
            temp.disableIf = disableIf;
            dependentsMap[formName][dependOn].push(temp);

            // sets the proper event on the widget
            if ((dependObj.length !== undefined) &&
                (dependObj.options === undefined)) {
                // radio or checkbox with many values
                for (var i = 0; i < dependObj.length; i++) {
                    dependObj[i].onclick = function (event) {
                        checkDependency(this);
                    };
                }
            } else {
                if (dependObj.type == 'text' || dependObj.type == 'textarea') {
                    // text
                    addEventHandler(dependObj, 'onkeyup', checkDependency);
                    addEventHandler(dependObj, 'onblur', checkDependency);

                } else {
                    // select, radio or checkbox with single value
                    dependObj.onclick = function (event) {
                        checkDependency(this);
                    };
                }
            }
        }
    }

    /**
    * Returns whether or not a widget has a specific value or one value among a
    * list of values. Lists of values are comma separated. "Having a value"
    * means "is the value" for text widgets, and "the value is selected" for
    * every other widget types.
    *
    * The return value of this function is also based on the enabled/disabled
    * state of the widget. If the widget is disabled, False will be returned in
    * an "enableIf" context, and True in a "disableIf" context.
    *
    * In the case of radio buttons and checkboxes, the widget is considered
    * disabled only if every form elements of the same name are disabled.
    */
    function hasValue(obj, value, logicIsEnableIf)
    {
        var i, j;
        var values = value.split(',');
        var elem = document.forms[obj.form.name][obj.name];
        if (obj.type == 'checkbox' || obj.type == 'radio') {
            var disabledCount = 0;
            for (j = 0; j < elem.length; j++) {
                if (elem[j].disabled) {
                    disabledCount++;
                }
            }
            if (disabledCount == elem.length) {
                return !logicIsEnableIf;
            }
            for (i = 0; i < values.length; i++) {
                if (elem.length) {
                    for (j = 0; j < elem.length; j++) {
                        if (elem[j].value == values[i] && elem[j].checked &&
                            !elem[j].disabled) {
                            return true;
                        }
                    }
                } else {
                    if (elem.value == values[i] && elem.checked &&
                        !elem.disabled) {
                        return true;
                    }
                }
            }
            return false;
        } else if (obj.type == 'select-one' || obj.type == 'select-multiple') {
            if (obj.disabled) {
                return !logicIsEnableIf;
            }
            for (i = 0; i < values.length; i++) {
                for (j = 0; j < obj.options.length; j++) {
                    if (obj.options[j].value == values[i] &&
                        obj.options[j].selected) {
                        return true;
                    }
                }
            }
            return false;
        } else if (obj.type == 'text' || obj.type == 'textarea') {
            if (obj.disabled) {
                return !logicIsEnableIf;
            }
            var textValue = trim(obj.value).toLowerCase();
            for (i = 0; i < values.length; i++) {
                if (textValue == trim(values[i]).toLowerCase()) {
                    return true;
                }
            }
            return false;
        } else {
            // unsupported widget type
            return false;
        }
    }

    /**
    * Traverse the dependency map and finds whether or not an obj has a subject
    * in its parents. This is used during initialization to avoid cycles in the
    * dependency relations.
    */
    function dependsOn(formName, objName, subjectName)
    {
        var parent = objName;
        while (parent !== null && parent !== subjectName) {
            parent = dependencyMap[formName][parent] ?
                dependencyMap[formName][parent] : null;
        }
        return parent == subjectName;
    }

    /**
    * Update the status of every dependent widgets and cascades through the
    * sub-dependency hierarchy.
    */
    function checkDependency(obj)
    {
        var formName = obj.form.name;

        var dependents = dependentsMap[formName][obj.name];
        if (!dependents) { return; }

        var dependent, disableDependent, i, j, k, labelColor;
        for (i = 0; i < dependents.length; i++) {
            dependent = document.forms[formName][dependents[i].dependentName];

            if (dependents[i].enableIf !== null) {
                disableDependent = !hasValue(obj, dependents[i].enableIf, true);
            } else {
                disableDependent = hasValue(obj, dependents[i].disableIf,false);
            }
            labelColor = disableDependent ? 'GrayText' : 'WindowText';

            if ((dependent.length !== undefined) &&
                (dependent.options === undefined)) {
                // radio or checkbox with many values
                for (j = 0; j < dependent.length; j++) {
                    dependent[j].disabled = disableDependent;
                    if (dependent[j].id && labelsObj[dependent[j].id]) {
                        for (k=0; k<labelsObj[dependent[j].id].length; k++) {
                            labelsObj[dependent[j].id][k].style.color =
                                labelColor;
                        }
                    }
                }
                if (dependent[0]) {
                    checkDependency(dependent[0]);
                }
            } else {
                // text, textarea, select, radio or checkbox with single value
                dependent.disabled = disableDependent;
                if (dependent.id && labelsObj[dependent.id]) {
                    for (k = 0; k < labelsObj[dependent.id].length; k++) {
                        labelsObj[dependent.id][k].style.color = labelColor;
                    }
                }
                checkDependency(dependent);
            }
        }
    }

    this.init = function ()
    {
        var fieldName, formName, i, labelFor, obj, topWidgets, widgets;

        // locate every label, they get enabled/disabled along with their
        // respective widgets
        var labels = document.getElementsByTagName("label");
        for (i = 0; i < labels.length; i++) {
            labelFor = labels[i].getAttribute('htmlFor');
            if (!labelFor) {
                labelFor = labels[i].getAttribute('for');
            }
            if (labelFor) {
                if (!labelsObj[labelFor]) {
                    labelsObj[labelFor] = [];
                }
                labelsObj[labelFor].push(labels[i]);
            }
        }

        // initialize the widgets
        widgets = document.getElementsByTagName("input");
        for (i = 0; i < widgets.length; i++) {
            widgetInit(widgets[i]);
        }
        widgets = document.getElementsByTagName("select");
        for (i = 0; i < widgets.length; i++) {
            widgetInit(widgets[i]);
        }
        widgets = document.getElementsByTagName("textarea");
        for (i = 0; i < widgets.length; i++) {
            widgetInit(widgets[i]);
        }

        // locate every top widgets in the dependency hierarchy and triggers an
        // initial dependency check
        for (formName in dependencyMap) {
            topWidgets = {};
            for (fieldName in dependencyMap[formName]) {
                if (!dependencyMap[formName][dependencyMap[formName][fieldName]]) {
                    topWidgets[dependencyMap[formName][fieldName]] = 1;
                }
            }
            for (fieldName in topWidgets) {
                obj = document.forms[formName][fieldName];
                if ((obj.length !== undefined) && (obj.options === undefined)) {
                    // radio or checkbox with many values
                    if (obj[0]) { checkDependency(obj[0]); }
                } else {
                    // any other widget types
                    checkDependency(obj);
                }
            }
        }
    };
}

var dependentOptions = new DependentOptions();

if (dependentOptions.isReady) {
    // initialization function called on window.onLoad event, see shared.js
    _onLoad.push('dependentOptions.init()');
}
