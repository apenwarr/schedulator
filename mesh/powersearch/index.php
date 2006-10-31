<?php
$words = implode(',', explode("\n", trim(file_get_contents('words'))));
?>

<html>
<head>
<title>Power Search Testing</title>
<script src="../shared.js" type="text/javascript"></script>
<script src="config.js" type="text/javascript"></script>
<link rel="stylesheet" href="powersearch.css" type="text/css">
<script src="powersearch.js" type="text/javascript"></script>
</head>

<body>
<form name="powerSearchForm">

<p>ip1: <input powersearch="<?=htmlspecialchars($words)?>" size="55" name="searchBox1">

<p>ip2: <input powersearch="<?=htmlspecialchars($words)?>" size="55" name="searchBox2">

<p>animal: <span><input size="30" name="animal"
    powersearch="cat,dog,ferret,turtle,fish"></span>

<p>test: <span><input size="30" name="test"
    powersearch="foo,bar,baz,a\, b\, and c,c:\\temp\\"></span>

<p>old box <input autocomplete="off" size="55" name="searchBoxOld">

</form>
</body>

</html>
