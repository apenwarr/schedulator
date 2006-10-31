function uniconfSet(key, value)
{
    var url = 'setuniconfkey.php?key=' + escape(key);
    if (value !== undefined) {
        url += '&value='+escape(value);
    }
    var requestObj = getRequestObject();
    requestObj.send(url, function(s){});
}

function uniconfUncheckAll()
{
    crazyTable.clearCheckedNodesIds('uniconf');
}

function uniconfDelete()
{
    var nodes = crazyTable.getCheckedNodesIds('uniconf');
    var requestObj, url;
    for (var i = 0; i < nodes.length; i++) {
        uniconfSet(nodes[i]);
    }
    uniconfUncheckAll();
}

function uniconfFormSet(form)
{
    uniconfSet(form.setkey.value, form.setvalue.value);
    form.setkey.value = '';
    form.setvalue.value = '';
}

function uniconfSave()
{
    changes = crazyTable.getEditInPlaceValues('uniconf');
    for (var i = 0; i < changes.length; i++) {
        uniconfSet(changes[i].key, changes[i].value);
    }
    crazyTable.cancelEditInPlaceWidgets('uniconf');
}
