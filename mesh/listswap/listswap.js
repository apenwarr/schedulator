/**
 * ListSwap
 *
 * Author:
 *   Michel Emond
 *   Net Integration Technologies
 *   www.net-itech.com
 *
 * Description:
 *   Generalized helper functions to move elements back and forth between two
 *   lists.
 *
 * Dependencies:
 *   None
 *
 * Global objects:
 *   ListSwap   Constructor method
 *   listSwap   ListSwap object
 *
 * How to use:
 *   The following html elements must be present:
 *     - One list holding the unused values, named "FIELD_NAME_out".
 *     - One list holding the used values, named "FIELD_NAME_in".
 *     - Two buttons, calling respectively listSwap.add and listSwap.remove (see
 *       below for the syntax and an example).
 *     - A hidden field, named "FIELD_NAME". This field is a comma separated
 *       list of the values contained in FIELD_NAME_in. This technique is to
 *       avoid having to select the elements in FIELD_NAME_in before submitting
 *       the form. It is important to fill this field with the same values as
 *       FIELD_NAME_in when generating the html.
 *
 *   Important: FIELD_NAME has to be a valid form element name and the same
 *              value for the two lists and the hidden field.
 *
 * Syntax:
 *   The listSwap.add and listSwap.remove methods are called with two
 *   parameters: the form name, followed by the value of FIELD_NAME.
 *
 * Example:
 *   <form name="myForm">
 *
 *   <select multiple name="myList_out" size="20" style="width:200px">
 *     <option value="foo">foo</option>
 *     <option value="bar">bar</option>
 *   </select>
 *
 *   <input type="button" name="add" value="Add &gt;&gt;"
 *     onclick="listSwap.add('myForm', 'myList');">
 *
 *   <input type="button" name="del" value="&lt;&lt; Remove"
 *     onclick="listSwap.remove('myForm', 'myList');">
 *
 *   <select multiple name="myList_in" size="20" style="width:200px">
 *     <option value="baz">baz</option>
 *   </select>
 *
 *   <input type="hidden" name="myList" value="baz">
 *
 *   </form>
 */

/*global Option, document */

function ListSwap()
{
    function optionCompare(a, b)
    {
        if (a.text < b.text) {
            return -1;
        } else if (a.text > b.text) {
            return 1;
        } else {
            return 0;
        }
    }

    function sortList(list)
    {
        var temp = [list.length];
        var i;

        for (i = 0; i < list.length; i++) {
            temp[i] = new Option(list.options[i].text, list.options[i].value);
        }

        temp.sort(optionCompare);

        for (i = 0; i < list.length; i++) {
            list.options[i] = temp[i];
        }
    }

    function listMove(from, to, inList, save)
    {
        var emptyListString = '(List Empty)';

        var fromSelected = -1;

        for (var i = from.length-1; i >= 0; i--) {
            if (from.options[i].selected &&
                from.options[i].value !== '') {
                if (to.length == 1 && to.options[0].value === '') {
                    to.options[0] = null;
                }
                to.options[to.length] = new Option(from.options[i].text,
                                                   from.options[i].value);
                from.options[i] = null;
                fromSelected = i;
            }
        }

        if (from.length === 0) {
            from.options[0] = new Option(emptyListString, '');
        } else if (fromSelected > -1) {
            from.selectedIndex = (fromSelected > from.length-1) ?
                from.length-1 : fromSelected;
        }

        if (to.length === 0) {
            to.options[0] = new Option(emptyListString, '');
        }

        sortList(to);

        var saveValues = [];
        for (i = 0; i < inList.length; i++) {
            saveValues.push(inList[i].value);
        }
        save.value = saveValues.join(',');
    }

    this.add = function (formName, fieldName)
    {
        var form = document.forms[formName];
        var from = form[fieldName+'_out'];
        var to = form[fieldName+'_in'];
        var save = form[fieldName];

        if (!from || !to || !save) { return; }

        listMove(from, to, to, save);
    };

    this.remove = function (formName, fieldName)
    {
        var form = document.forms[formName];
        var from = form[fieldName+'_in'];
        var to = form[fieldName+'_out'];
        var save = form[fieldName];

        if (!from || !to || !save) { return; }

        listMove(from, to, from, save);
    };
}

var listSwap = new ListSwap();
