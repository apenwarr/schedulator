<?php

if ($_GET) {
    print '<pre>';
    print_r($_GET);
    print '</pre>';
}

?>

<link rel="stylesheet" href="tristate.css" type="text/css">
<script src="../shared.js" type="text/javascript"></script>
<script src="tristate.js" type="text/javascript"></script>

<form name="fooform">


<input id="test" type="checkbox" name="test" tristate="foo,bar,baz" value="foo" checked>
<label for="test">test</label>

<input type="submit">

</form>
