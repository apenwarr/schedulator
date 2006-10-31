<?php

print '<pre>';
print_r($_GET);
print '</pre>';

?>

<script src="../shared.js" type="text/javascript"></script>
<script src="../json.js" type="text/javascript"></script>

<script src="config.js" type="text/javascript"></script>

<link rel="stylesheet" href="../editinplace/editinplace.css" type="text/css">
<script src="../editinplace/editinplace.js" type="text/javascript"></script>

<link rel="stylesheet" href="dynamicstatus.css" type="text/css">
<script src="dynamicstatus.js" type="text/javascript"></script>

<link rel="stylesheet" href="../autovalidate/autovalidate.css" type="text/css">
<script src="../autovalidate/autovalidate.js" type="text/javascript"></script>

<link rel="stylesheet" href="../powersearch/powersearch.css" type="text/css">
<script src="../powersearch/powersearch.js" type="text/javascript"></script>

<link rel="stylesheet" href="../tooltip/tooltip.css" type="text/css">
<script src="../tooltip/tooltip.js" type="text/javascript"></script>

<form name="statusForm">

<p>
<input type="button" value="Pop window" onClick="window.open('index.php', 'dynamicwindow', 'width=400,height=400');">

<h1>Realtime Widgets</h1>

<p>
<textarea name="textarea2" cols="10" rows="4" wrap="virtual"
    editinplace="textarea"
    dynkey="field11">textarea test a</textarea>
<br>
<textarea name="textarea3" cols="10" rows="4" wrap="virtual"
    editinplace="textarea"
    tooltip="foo a"
    dynkey="field12">textarea test b</textarea>
<br>
<textarea name="textarea4" cols="10" rows="4" wrap="virtual"
    editinplace="textarea"
    tooltip="foo b" tooltipcolor="#ffc"
    dynkey="field13">textarea test c</textarea>

<p>
<span id="textfield2" editinplace="text" size="50"
    dynkey="field10"
    autovalidate="domainname" allowchars="*"
    powersearch="nitix.com,google.ca"
    >text content</span>

<p>
<input type="checkbox" name="checkbox1" dynkey="field6" value="foo"> foo
<input type="checkbox" name="checkbox1" dynkey="field6" value="foo,bar" checked> foo,bar
<input type="checkbox" name="checkbox2" dynkey="field9" value="baz"> baz
<br>

<input type="radio" name="radio1" dynkey="field7" value="foo" checked> foo
<input type="radio" name="radio1" dynkey="field7" value="bar"> bar
<input type="radio" name="radio2" dynkey="field8" value="baz"> baz
<br>

<input type="text" name="textfield1" dynkey="field1"
    powersearch="nitix.com,google.ca"
    >
<br>

<textarea name="textarea1" dynkey="field2"></textarea>
<br>

<select name="select1" dynkey="field3">
<option value="foo">foo
<option value="bar">bar
<option value="baz">baz
</select>
<br>

<select name="select2" dynkey="field4" size="4">
<option value="foo">foo
<option value="bar">bar
<option value="baz">baz
<option value="qux">qux
<option value="quux">quux
<option value="corge">corge
<option value="grault">grault
<option value="garply">garply
<option value="waldo">waldo
<option value="fred">fred
<option value="plugh">plugh
<option value="xyzzy">xyzzy
<option value="thud">thud
</select>
<br>

<select name="select3" dynkey="field5" size="4" multiple>
<option value="ye!\">ye!\
<option value="foo">foo
<option value="bar">bar
<option value="foo/bar">foo/bar
<option value="foo,bar">foo,bar
<option value="foo\bar">foo\bar
<option value="baz">baz
<option value="qux">qux
<option value="quux">quux
<option value="corge">corge
<option value="grault">grault
<option value="garply">garply
<option value="waldo">waldo
<option value="fred">fred
<option value="plugh">plugh
<option value="xyzzy">xyzzy
<option value="thud">thud
</select>
<br>

<p>
<span id="gauge1" dyntype="gauge">a</span><br>
<span id="gauge2" dyntype="gauge">b</span><br>
<span id="gauge3" dyntype="gauge">c</span>

<p>
<span id="light1" dyntype="light">d</span><br>
<span id="light2" dyntype="light">e</span><br>
<span id="light3" dyntype="light">f</span>

<p>
<span id="text1" dyntype="text">foo</span>

<p>
<span id="text2" dyntype="text">bar</span>

<p>
<span id="bar1" dyntype="progressbar">boo</span>
<span id="bar2" dyntype="progressbar">blah</span>

<p>
<input type="submit">

</form>
