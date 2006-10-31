/**
 * EditInPlace
 *
 * Author:
 *   Michel Emond
 *   Net Integration Technologies
 *   www.net-itech.com
 *
 * Description:
 *   Dynamically creates form widgets allowing in-place and instant edit
 *   capabilities.
 *
 * Dependencies:
 *   shared.js
 *   editinplace.css
 *   autovalidate.js (in some cases)
 *   powersearch.js (in some cases)
 *   tooltip.js (in some cases)
 *
 * Global objects:
 *   EditInPlace   Constructor method
 *   editInPlace   EditInPlace object
 *
 * Syntax:
 *   In order to activate EditInPlace on a text that may be edited, surround the
 *   text with a span, having the following attributes:
 *
 *     editinplace:
 *       Indicates the type of form widget to use. Valid values are: "text",
 *       "radio", "checkbox", and "select".
 *
 *       When using the "checkbox" widget type, a special hidden field named
 *       "editinplace_checkboxes" is created to help keeping track of the
 *       unchecked checkboxes. The real name of this hidden field actually is
 *       "editinplace_checkboxes[]", the brackets being there to turn this field
 *       into an array. When a checkbox widget is set to "edit mode", its name
 *       is added to this array, and when the "edit mode" is cancelled, its name
 *       is removed from the array.
 *
 *     id:
 *       When created, the form widget will be named with this value. This value
 *       is also used to identify the EditInPlace component.
 *
 *     size:
 *       When the value of editinplace is "text", this attribute is given to the
 *       text field widget. This attribute is optional.
 *
 *     values:
 *       When the value of editinplace is either "radio", "checkbox", or
 *       "select", this attribute provides the values to populate the widgets.
 *       See examples below.
 *
 *     imagemap:
 *       When editinplace is set to either "radio" or "checkbox, use this
 *       attribute to assign an imagemap to the widget. This attribute is
 *       optional and requires the imagemap to be defined in a custom
 *       configuration. See example below and shared.js for information about
 *       custom configuration.
 *
 *   Textarea:
 *     In order to define a textarea EditInPlace widget, simply define a
 *     textarea as usual. Along with the "name", "cols", "rows", and "wrap"
 *     attributes, set the "editinplace" attribute to "textarea". The "name"
 *     attribute must be unique across the "id" attribute of the other
 *     EditInPlace widgets.
 *
 *     Additionaly, the "textarea" type may be combined with ToolTip. See
 *     example below.
 *
 *   Restrictions:
 *     - Html is not enabled with EditInPlace. It is STRONGLY recommended to
 *       replace the html special characters ('<', '>', '"', and '&') by their
 *       respective entities. This goes for any text inside of a span or a
 *       textarea. This is a standard restriction of html. Most browsers don't
 *       really care but the results could vary depending on each browser's html
 *       rendering implementation. Following this precaution will avoid
 *       surprises when using EditInPlace.
 *     - The keys and values described in the 'values' property must not contain
 *       commas ',' or colons ':'. See examples below.
 *
 *   Widgets combination
 *     In order to combine EditInPlace with AutoValidation and PowerSearch, the
 *     same attributes must be used with the span, instead of the input tag. See
 *     examples below. One example shows how to combine EditInPlace,
 *     AutoValidation and PowerSearch in the same widget.
 *
 * Custom configuration
 *   When using the imgBasePath setting to use custom images from a different
 *   location, three additional custom configuration parameters are available
 *   for EditInPlace: cancelImgName, cancelImgWidgh, and cancelImgHeight, their
 *   respective default values being 'cancel.gif', 16, and 16.
 *
 *   Setting them in a custom configuration must be done like this:
 *     widgetsConfig.editInPlace.cancelImgName = '<image name>';
 *     widgetsConfig.editInPlace.cancelImgWidth = <image width>;
 *     widgetsConfig.editInPlace.cancelImgHeight = <image height>;
 *
 *   See the description of the imagemap attribute above for information about
 *   the imagemap custom configuration.
 *
 *   See shared.js for information about imgBasePath and custom configuration.
 *
 * Examples, basic usage:
 *   <span id="textfield" editinplace="text" size="50">text content</span>
 *
 *   <span id="radios" editinplace="radio"
 *       values="1:Choice number one,2:Choice number two,3:Choice number three">
 *       Choice number two</span>
 *
 *   <span id="checkboxes" editinplace="checkbox"
 *       values="FF0000:Red,00FF00:Green,0000FF:Blue">Green, Blue</span>
 *
 *   <textarea name="textarea" cols="50" rows="10" wrap="virtual"
 *       editinplace="textarea"
 *       tooltip="ToolTip Label" tooltipcolor="#ffc"
 *       >Lorem ipsum dolor sit amet.</textarea>
 *
 * Examples, combining with AutoValidation and PowerSearch
 *   <span id="integer" editinplace="text" size="5"
 *       autovalidate="integer" minvalue="-10" maxvalue="10">foo</span>
 *
 *   <span id="ipaddress" editinplace="text" size="20"
 *       autovalidate="ipaddress">42</span>
 *
 *   <span id="hostname" editinplace="text" size="30"
 *       autovalidate="domainname" allowchars="*"
 *       powersearch="net-itech.com,open.nit.ca,google.com"
 *       >localhost.*</span>
 *
 * Example, combining the "textarea" type with ToolTip
 *   <span id="textarea2" editinplace="textarea" cols="50" rows="10"
 *       tooltip="&lt;img src=&quot;edit.png&quot;&gt;" tooltipcolor="#ffc"
 *       ><textarea>Lorem ipsum dolor sit amet.</textarea></span>
 *
 * Example, using an image map
 *   In the custom configuration:
 *     widgetsConfig.editInPlace.imageMap['useradminteam'] = {
 *         'user':'../mycustomimages/icon_user.gif',
 *         'admin':'../mycustomimages/icon_admin.gif',
 *         'team':'../mycustomimages/icon_team.gif'
 *     };
 *
 *   In the html:
 *     <span id="usertype" editinplace="radio"
 *       values="user:User,admin:Admin,team:Team"
 *       imagemap="useradminteam">User</span>
 */

/*global autoValidate, document, fromHtmlEntities, isDefined, navigator, Option,
         powerSearch, regExp, toHtmlEntities, toolTip, trim, widgetsConfig */

function EditInPlace()
{
    var self = this;
    this.isReady = true;
    this.editModeCount = 0;

    if (!document.getElementById || !document.getElementsByTagName) {
        this.isReady = false;
        return;
    }

    // This variable is set to be used by other libraries to get a list of the
    // valid EditInPlace spans. Used by DynamicStatus.
    this.spans = {};

    function splitValuesList(vals)
    {
        if (!vals) { return; }
        vals = vals.split(',');

        var temp, value;
        var values = [];
        for (var i = 0; i < vals.length; i++) {
            temp = vals[i].split(':');
            if (temp[0] && temp[1]) {
                value = {};
                value.value = temp[0];
                value.text = temp[1];
                values.push(value);
            }
        }

        return values;
    }

    // valid field types for span, textarea is treated independently
    function isValidFieldType(fieldType)
    {
        var validTypes = {'text':1, 'radio':1, 'checkbox':1, 'select':1};
        return validTypes[fieldType.toLowerCase()] !== undefined;
    }

    // Puts two tables around an object, the first to delemit the clickable
    // zone, and the second to define the editinplace style.
    function tableIfy(objId)
    {
        var obj = document.getElementById(objId);

        var table1 = document.createElement('TABLE');
        table1.style.display = 'inline';
        table1.cellSpacing = 0;
        table1.cellPadding = 0;
        table1.border = 0;
        var tbody1 = document.createElement('TBODY');
        var row1 = document.createElement('TR');
        var cell1 = document.createElement('TD');

        var table2 = document.createElement('TABLE');
        table2.className = 'editinplace';
        table2.cellSpacing = 0;
        table2.cellPadding = 0;
        table2.border = 0;
        var tbody2 = document.createElement('TBODY');
        var row2 = document.createElement('TR');
        var cell2 = document.createElement('TD');
        //cell2.id = obj.id+'subcell'; // fixme, remove this once proven useless
        cell2.innerHTML = obj.innerHTML;

        row2.appendChild(cell2);
        tbody2.appendChild(row2);
        table2.appendChild(tbody2);

        cell1.appendChild(table2);
        row1.appendChild(cell1);
        tbody1.appendChild(row1);
        table1.appendChild(tbody1);

        obj.innerHTML = '';
        obj.appendChild(table1);
    }

    this.widgetInit = function (obj)
    {
        var attr, attributes, fieldType, imageMap, imgSrc, innerText, j;
        var parentNode, removeTa, temp, thisInnerText, thisSpan, thisTa;
        var useImages, values;

        var tagName = obj.tagName.toLowerCase();

        if (tagName == 'span') {
            thisSpan = obj;
            fieldType = thisSpan.getAttribute('editinplace');
            if (thisSpan.id !== '' && fieldType !== null &&
                isValidFieldType(fieldType)) {
                thisSpan.onclick = function (event) {
                    self.showEdit(this.id);
                };
                self.spans[thisSpan.id] = thisSpan.id;

                // this maneuver is useful to normalize html entities and
                // special chars
                thisInnerText =
                    toHtmlEntities(fromHtmlEntities(thisSpan.innerHTML));

                // The inittext attribute is used by radio and checkbox to find
                // which values to select. It is useful to preserve the initial
                // content of the span in a distinct attribute here since an
                // imagemap may overwrite its innerHTML property.
                thisSpan.setAttribute('initText', trim(thisInnerText));

                // image map support
                if ((imageMap = thisSpan.getAttribute('imagemap'))) {
                    fieldType = fieldType.toLowerCase();
                    if (!widgetsConfig.editInPlace.imageMap[imageMap] ||
                        (fieldType != 'radio' && fieldType != 'checkbox')) {
                        thisSpan.removeAttribute('imagemap');
                        imageMap = undefined;
                    } else {
                        imageMap = widgetsConfig.editInPlace.imageMap[imageMap];
                    }
                    if (imageMap) {
                        values = splitValuesList(
                            thisSpan.getAttribute('values'));
                        if (!values) { values = []; }
                        innerText = trim(thisInnerText);
                        useImages = document.createElement('span');
                        // go backwards so selected item with radio is the same
                        // as when there is no imagemap
                        for (j = values.length-1; j >= 0; j--) {
                            regExp = new RegExp('(^|, *)'+toHtmlEntities(
                                values[j].text)+'($| *,)', 'i');
                            if (!(!(innerText.match(regExp)))) {
                                if ((imgSrc = imageMap[values[j].value])) {
                                    temp = document.createElement('img');
                                    temp.src = imgSrc;
                                    temp.alt = values[j].text;
                                    temp.title = values[j].text;
                                } else {
                                    temp = document.createTextNode(
                                        '['+values[j].text+']');
                                }
                                useImages.appendChild(temp);
                                if (fieldType == 'radio') { break; }
                            }
                        }
                        thisInnerText = useImages.innerHTML;
                    }
                }

                thisSpan.innerHTML = thisInnerText;
                thisSpan.setAttribute('initInnerHTML', thisSpan.innerHTML);
                tableIfy(thisSpan.id);
            }

        } else if (tagName == 'textarea') {
            thisTa = obj;
            removeTa = false;
            fieldType = thisTa.getAttribute('editinplace');
            if (thisTa.name !== '' && fieldType &&
                fieldType.toLowerCase() == 'textarea') {
                // create the editinplace span
                thisSpan = document.createElement('span');
                thisSpan.id = thisTa.name;
                thisSpan.setAttribute('editinplace', 'textarea');
                thisSpan.innerHTML =
                    toHtmlEntities(thisTa.value).replace(/\n/g, '<br>');

                // copy attributes from textarea to span
                attributes = ['cols', 'rows', 'wrap', 'tooltip',
                              'tooltipcolor', 'dynkey'];
                for (j = 0; j < attributes.length; j++) {
                    if ((attr = thisTa.getAttribute(attributes[j]))) {
                        thisSpan.setAttribute(attributes[j], attr);
                    }
                }

                // replace the textarea by the span
                parentNode = thisTa.parentNode;
                if (parentNode) {
                    parentNode.insertBefore(thisSpan, thisTa);
                    removeTa = true;
                }

                // initialize editinplace
                thisSpan.onclick = function (event) {
                    self.showEdit(this.id);
                };
                self.spans[thisSpan.id] = thisSpan.id;
                thisSpan.setAttribute('initInnerHTML',
                                      toHtmlEntities(thisTa.value));
                // tooltip integration
                if (thisSpan.getAttribute('tooltip') !== null &&
                    isDefined('toolTip') && toolTip.isReady) {
                    toolTip.widgetInit(thisSpan, true,
                                       toHtmlEntities(thisTa.value));
                }
                tableIfy(thisSpan.id);
            }
            return removeTa;
        }
    };

    this.init = function ()
    {
        var i, removeTa, thisTa;

        var spans = document.getElementsByTagName("span");
        for (i = 0; i < spans.length; i++) {
            self.widgetInit(spans[i]);
        }

        var textareas = document.getElementsByTagName('textarea');
        var taToRemove = [];
        for (i = 0; i < textareas.length; i++) {
            removeTa = self.widgetInit(textareas[i]);
            if (removeTa){
                taToRemove.push(textareas[i]); // remove the textarea later
            }
        }
        // remove the textareas
        while (taToRemove.length > 0) {
            thisTa = taToRemove.pop();
            thisTa.parentNode.removeChild(thisTa);
        }
    };

    this.showView = function (spanId)
    {
        var span = document.getElementById(spanId);
        var fieldType = span.getAttribute('editinplace').toLowerCase();

        self.editModeCount--;

        if (fieldType == 'textarea' && span.getAttribute('tooltipinit')) {
            toolTip.enable(span);
        }

        document.getElementById('view'+spanId).style.display = '';

        var spanEdit = document.getElementById('edit'+spanId);
        span.removeChild(spanEdit);
    };

    this.showEdit = function (spanId)
    {
        if (document.getElementById('edit'+spanId)) {
            return;
        }

        self.editModeCount++;

        var span = document.getElementById(spanId);
        var fieldType = span.getAttribute('editinplace').toLowerCase();

        // when textarea uses tooltip, tolltip gets disabled in edit mode
        if (fieldType == 'textarea' && span.getAttribute('tooltipinit')) {
            toolTip.disable(span);
        }

        var innerHTML = span.getAttribute('initInnerHTML');
        var innerText = trim(innerHTML);

        // edit span
        var spanEdit = document.createElement('span');
        spanEdit.setAttribute("id", 'edit'+spanId);
        spanEdit.style.display = '';

        var editWidget, i, powerSearchInit, primaryFormField, temp;

        if (fieldType == 'text') {
            editWidget = document.createElement('input');
            editWidget.id = 'widget'+spanId;
            editWidget.name = spanId;
            editWidget.type = 'text';
            editWidget.value = fromHtmlEntities(innerText);
            var size;
            if ((size = span.getAttribute('size'))) {
                editWidget.setAttribute('size', size);
            }

            primaryFormField = editWidget;

            if (isDefined('powerSearch') && powerSearch.isReady) {
                var powerSearchAttr = span.getAttribute('powersearch');
                if (powerSearchAttr) {
                    editWidget.setAttribute('powersearch', powerSearchAttr);
                    // powersearch widget initialized later, see below
                    powerSearchInit = editWidget;
                }
            }

            if (isDefined('autoValidate') && autoValidate.isReady) {
                var autoValidateType = span.getAttribute('autovalidate');
                if (autoValidateType) {
                    editWidget.setAttribute('autovalidate', autoValidateType);
                    var minValue, maxValue, stepSize, allowChars;
                    if ((minValue = span.getAttribute('minvalue'))) {
                        editWidget.setAttribute('minvalue', minValue);
                    }
                    if ((maxValue = span.getAttribute('maxvalue'))) {
                        editWidget.setAttribute('maxvalue', maxValue);
                    }
                    if ((stepSize = span.getAttribute('stepsize'))) {
                        editWidget.setAttribute('stepsize', stepSize);
                    }
                    if ((allowChars = span.getAttribute('allowchars'))) {
                        editWidget.setAttribute('allowchars', allowChars);
                    }
                    var autovalidateSpan = document.createElement("SPAN");
                    autovalidateSpan.appendChild(editWidget);
                    autoValidate.widgetInit(editWidget);
                    editWidget = autovalidateSpan;
                }
            }

            spanEdit.appendChild(editWidget);

        } else if (fieldType == 'textarea') {
            editWidget = document.createElement('textarea');
            editWidget.id = 'widget'+spanId;
            editWidget.name = spanId;
            editWidget.value = fromHtmlEntities(innerText);

            // copy attributes from span to textarea
            var attr;
            var attributes = ['cols', 'rows', 'wrap'];
            for (i = 0; i < attributes.length; i++) {
                if ((attr = span.getAttribute(attributes[i]))) {
                    editWidget.setAttribute(attributes[i], attr);
                }
            }

            // special case for textarea
            innerHTML = innerHTML.replace(/\n/g, '<br>');

            primaryFormField = editWidget;

            spanEdit.appendChild(editWidget);

        } else {
            var values = splitValuesList(span.getAttribute('values'));
            if (!values) { return; }

            if (fieldType == 'radio' || fieldType == 'checkbox') {
                if (fieldType == 'checkbox') {
                    // add a special hidden field for checkboxes
                    var widgetName = spanId;
                    if (widgetName.substr(widgetName.length-2) == '[]') {
                        // remove the array notation
                        widgetName = widgetName.substr(0, widgetName.length-2);
                    }
                    editWidget = document.createElement('input');
                    editWidget.name = 'editinplace_checkboxes[]';
                    editWidget.type = 'hidden';
                    editWidget.value = widgetName;
                    spanEdit.appendChild(editWidget);
                }

                var isIE = !(navigator &&
                    navigator.userAgent.toLowerCase().indexOf("msie") == -1);
                var initText = trim(span.getAttribute('initText'));
                var isSelected, regExp;
                for (i = 0; i < values.length; i++) {
                    regExp = new RegExp(
                        '(^|, *)'+toHtmlEntities(values[i].text)+'($| *,)',
                        'i');
                    isSelected = !(!(initText.match(regExp)));
                    if (isIE) {
                        // IE is silly
                        editWidget = document.createElement('<INPUT '+
                            (isSelected ? 'checked="checked"' : '')+
                            ' id="widget'+spanId+i+'" name="'+spanId+
                            '" />');
                    } else {
                        editWidget = document.createElement("INPUT");
                        editWidget.name = spanId;
                        editWidget.id = 'widget'+spanId+i;
                        editWidget.checked = isSelected;
                    }
                    editWidget.type = fieldType;
                    editWidget.value = values[i].value;

                    if (!primaryFormField &&
                        ((fieldType == 'radio' && isSelected) ||
                         fieldType == 'checkbox')) {
                        primaryFormField = editWidget;
                    }

                    spanEdit.appendChild(editWidget);

                    // radio or checkbox label
                    if (isIE) {
                        // IE is silly
                        editWidget = document.createElement('<LABEL '+
                            'for="widget'+spanId+i+'"></LABEL>');
                    } else {
                        editWidget = document.createElement("LABEL");
                        editWidget.setAttribute('for', 'widget'+spanId+i);
                    }
                    temp = document.createTextNode(values[i].text);
                    editWidget.appendChild(temp);
                    spanEdit.appendChild(editWidget);
                }

            } else if (fieldType == 'select') {
                editWidget = document.createElement("SELECT");
                editWidget.name = spanId;
                editWidget.id = 'widget'+spanId;
                for (i = 0; i < values.length; i++) {
                    editWidget.options[i] =
                        new Option(values[i].text, values[i].value);
                    if (trim(innerText.toLowerCase()) ==
                        toHtmlEntities(trim(values[i].text.toLowerCase()))) {
                        editWidget.selectedIndex = i;
                    }
                }

                primaryFormField = editWidget;

                spanEdit.appendChild(editWidget);

            }

            span.setAttribute('valuescount', values.length);
        }

        var cancelLink = document.createElement("A");
        cancelLink.setAttribute("href",
            "javascript:editInPlace.showView('"+spanId+"');");
        cancelLink.setAttribute("alt", 'Cancel');
        cancelLink.setAttribute("title", 'Cancel');
        cancelLink.setAttribute("onmouseout", "window.status='';");
        cancelLink.setAttribute("onmouseover",
            "window.status='Cancel';return true;");

        var cancelImg = document.createElement("IMG");
        cancelImg.setAttribute("src",
            widgetsConfig.imgBasePath+'editinplace/' +
            widgetsConfig.editInPlace.cancelImgName);
        cancelImg.setAttribute("hspace", "5");
        cancelImg.setAttribute("width",
            widgetsConfig.editInPlace.cancelImgWidth);
        cancelImg.setAttribute("height",
            widgetsConfig.editInPlace.cancelImgHeight);
        cancelImg.setAttribute("border", "0");

        cancelLink.appendChild(cancelImg);

        if (widgetsConfig.editInPlace.doCancelLink)
	    spanEdit.appendChild(cancelLink);

        if (span.getAttribute('isinitialized')) {
            document.getElementById('view'+spanId).style.display = 'none';
        } else {
            // view span
            var spanView = document.createElement("SPAN");
            spanView.setAttribute("id", 'view'+spanId);
            spanView.style.display = 'none';
            if (fieldType == 'textarea' && span.getAttribute('tooltipinit')) {
                spanView.innerHTML = trim(span.getAttribute('tooltip'));
            } else {
                spanView.innerHTML = innerHTML;
            }

            // initialize span
            span.innerHTML = '';
            span.className = '';
            span.appendChild(spanView);

            // apply styles once the span is set on the page
            tableIfy(spanView.id);

            span.setAttribute('isinitialized', '1');
        }

        span.appendChild(spanEdit);

        // powersearch widget initialized once the span is set on the page
        if (powerSearchInit) {
            powerSearch.widgetInit(powerSearchInit);
        }

        // sets the form field's initial state
        if (primaryFormField) {
            primaryFormField.focus();
            if (fieldType == 'text') {
                primaryFormField.select();
            } else if (fieldType == 'textarea') {
                // IE is silly, needs this to focus and set the cursor
                if (primaryFormField.createTextRange) {
                    temp = primaryFormField.createTextRange();
                    temp.moveStart("character", primaryFormField.value.length);
                    temp.select();
                }
            }
        }
    };

    /**
    * Public function allowing other libraries to update the initial value of
    * the EditInPlace widget.
    *
    * This function is used by DynamicStatus.
    *
    * The newValue parameter must be either text for text fields, or the value
    * of the key for radio, checkbox or select. The 'key' stands for the value
    * that would be sent on form submit, not the text showing in a drop list for
    * instance.
    *
    * For checkbox, multiple values must be provided as a comma separated list.
    * Since the syntax of EditInPlace for providing the list of values excludes
    * commas, there is no need to escape special characters.
    *
    * The form field will not get updated while the widget is in 'edit' mode.
    * This is a normal behavior.
    */
    this.updateInitValue = function (spanId, newValue)
    {
        var span, fieldType;

        if (!(span = document.getElementById(spanId))) { return; }
        if (!(fieldType = span.getAttribute('editinplace'))) { return; }
        fieldType = fieldType.toLowerCase();

        var spanView = document.getElementById('view'+spanId);

        if (fieldType == 'text' || fieldType == 'textarea') {
            var spanInnerHTML = toHtmlEntities(newValue);
            if (fieldType == 'textarea') {
                spanInnerHTML = spanInnerHTML.replace(/\n/g, '<br>');
            }
            span.setAttribute('initInnerHTML', toHtmlEntities(newValue));
            if (fieldType == 'textarea' && span.getAttribute('tooltipinit')) {
                toolTip.close(span.id);
                span.setAttribute('tooltiptext', spanInnerHTML);
            } else if (span.getAttribute('isinitialized')) {
                spanView.innerHTML = spanInnerHTML;
                tableIfy(spanView.id);
            } else {
                span.innerHTML = spanInnerHTML;
                tableIfy(span.id);
            }

        } else {
            var i, imageMap, imgSrc, initText, newText, useImages;

            var useValue = {};
            var newValues = newValue.split(',');
            for (i = 0; i < newValues.length; i++) {
                useValue[newValues[i]] = 1;
            }

            var values = splitValuesList(span.getAttribute('values'));

            var temp = [];
            for (i in values) {
                if (useValue[values[i].value]) {
                    temp.push(values[i].text);
                }
            }
            newText = toHtmlEntities(temp.join(', '));
            initText = trim(newText);

            // image map support
            if ((imageMap = span.getAttribute('imagemap')) &&
                (fieldType == 'radio' || fieldType == 'checkbox')) {
                imageMap = widgetsConfig.editInPlace.imageMap[imageMap];
                useImages = document.createElement('span');
                for (i = values.length-1; i >= 0; i--) {
                    if (useValue[values[i].value]) {
                        if ((imgSrc = imageMap[values[i].value])) {
                            temp = document.createElement('img');
                            temp.src = imgSrc;
                            temp.alt = values[i].text;
                            temp.title = values[i].text;
                        } else {
                            temp = document.createTextNode(
                                '['+values[i].text+']');
                        }
                        useImages.appendChild(temp);
                        if (fieldType == 'radio') { break; }
                    }
                }
                newText = useImages.innerHTML;
            }

            if (span.getAttribute('isinitialized')) {
                spanView.innerHTML = newText;
                span.setAttribute('initInnerHTML', spanView.innerHTML);
                span.setAttribute('initText', initText);
                tableIfy(spanView.id);
            } else {
                span.innerHTML = newText;
                span.setAttribute('initInnerHTML', span.innerHTML);
                span.setAttribute('initText', initText);
                tableIfy(span.id);
            }
        }
    };
}

var editInPlace = new EditInPlace();

if (editInPlace.isReady) {
    // initialization function called on window.onLoad event, see shared.js
    _onLoad.push('editInPlace.init()');
}
