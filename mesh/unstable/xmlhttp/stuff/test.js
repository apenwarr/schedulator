/*
var myJSONObject = {"bindings": [
        {"ircEvent": "PRIVMSG", "method": "newURI", "regex": "^http://.*"},
        {"ircEvent": "PRIVMSG", "method": "deleteURI", "regex": "^delete.*"},
        {"ircEvent": "PRIVMSG", "method": "randomURI", "regex": "^random.*"}
    ]
};

//alert(myJSONObject.bindings[0].method);

var foo = eval(myJSONObject);

alert(foo.bindings[0].method);
*/


var s = '["1": "foo","total":"500"]';

n = eval('('+s+')');

alert(n);
