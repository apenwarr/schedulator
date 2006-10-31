_onLoad.push('displayUserPage(1, 0)');

var _pagesCache = new Array();
var _usersCount; // fixme implement cache flush when user count changes
var _displayPage;
var _displaySort;

function requestUserPage(page, colsort)
{
    var xmlhttp=false;
    /*@cc_on @*/
    /*@if (@_jscript_version >= 5)
    // JScript gives us Conditional compilation, we can cope with old IE versions.
    // and security blocked creation of the objects.
    try {
        xmlhttp = new ActiveXObject("Msxml2.XMLHTTP");
    } catch (e) {
        try {
            xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
        } catch (E) {
            xmlhttp = false;
        }
    }
    @end @*/
    if (!xmlhttp && typeof XMLHttpRequest!='undefined') {
    xmlhttp = new XMLHttpRequest();
    }

//alert('http request page: ' + page);
document.forms.requestsForm.requests.value += 'rq'+page + ' ';


    var url = "users.php?p=" + page + "&c=" + colsort;
    xmlhttp.open("GET", url, true);
    xmlhttp.onreadystatechange = function() {
        if (xmlhttp.readyState == 4) {
            updateUsersCache(page, colsort, xmlhttp.responseText);
document.forms.requestsForm.requests.value += 'ok'+page + ' ';
        }
    }
    //xmlhttp.setRequestHeader('Accept','message/webconfig-usersinfo');
    xmlhttp.send(null);
}

function updateUsersCache(page, colsort, data)
{
    data = eval('('+data+')');
    _pagesCache[page] = new Array();

    for (index in data['page']) {
        _pagesCache[page][index] = data['page'][index];
    }

    if (page == _displayPage) {
        updateUserPage(page);
    }
}

function displayUserPage(page, colsort)
{
    _displayPage = page;
    _displaySort = colsort; // fixme

    if (_pagesCache[page]) {
        updateUserPage(page);
    } else {
        requestUserPage(page, 0);
    }

    if (page > 1 && !_pagesCache[page-1]) {
        requestUserPage(page-1, 0);
    }

    // fixme check for maximum page number

    if (!_pagesCache[page+1]) {
        requestUserPage(page+1, 0);
    }

}

function updateUserPage(page)
{
    var span = document.getElementById('userslist');

    span.innerHTML = 'page: '+page+'<br><br>';
    for (var i = 0; i < _pagesCache[page].length; i++) {
        span.innerHTML += _pagesCache[page][i][0]+', '+_pagesCache[page][i][1]+' ' + '<br>';
    }

    span.innerHTML += '<a href="javascript:displayUserPage('+(page-1)+', 0)">&lt; prev</a>&nbsp;';
    span.innerHTML += '<a href="javascript:displayUserPage('+(page+1)+', 0)">next &gt;</a>';
}
