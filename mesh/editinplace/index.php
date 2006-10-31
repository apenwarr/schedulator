<?php

if ($_POST) {
    print '<pre>';
    print_r($_POST);
    print '</pre>';
}

?>

<script src="../shared.js" type="text/javascript"></script>

<script src="config.js" type="text/javascript"></script>

<link rel="stylesheet" href="../tooltip/tooltip.css" type="text/css">
<script src="../tooltip/tooltip.js" type="text/javascript"></script>

<link rel="stylesheet" href="editinplace.css" type="text/css">
<script src="editinplace.js" type="text/javascript"></script>

<link rel="stylesheet" href="../autovalidate/autovalidate.css" type="text/css">
<script src="../autovalidate/autovalidate.js" type="text/javascript"></script>

<link rel="stylesheet" href="../powersearch/powersearch.css" type="text/css">
<script src="../powersearch/powersearch.js" type="text/javascript"></script>


<script type="text/javascript">
<!--
function bar()
{
    editInPlace.updateInitValue('usertype', 'team');
}
//-->
</script>


<form action="index.php" name="editinplaceForm" method="post">

<p>
<span id="usertype" editinplace="radio"
    values="user:User,admin:Admin,team:Team"
    imagemap="useradminteam">user</span>
<input type="button" value="Bar" onClick="bar();">

<p>
<textarea name="textarea1" cols="100" rows="20" wrap="virtual"
    editinplace="textarea"
    tooltip="&lt;img src=&quot;edit.gif&quot;&gt;" tooltipcolor="#ffc"
    >Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. &lt;b&gt;i am bold!</b><br>
<br>
Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi. Lorem ipsum dolor sit amet, consectetuer adipiscing elit, sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat.

Ut wisi enim ad minim veniam, quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo consequat. Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi.</textarea>

<input type="button" value="foo" onClick="editInPlace.updateInitValue('textarea1', 'foo\n\nbar <b>bold</b>')">

<p>
<textarea name="textarea2" cols="80" rows="10" wrap="virtual"
    editinplace="textarea"
    tooltip="bar" tooltipcolor="#fcc"
    >foo</textarea>

<p>
<span id="textfield" editinplace="text" size="50">text <img> &lt;foo&gt; content</span>

<p>
<span id="myradios" editinplace="radio"
    values="1:One,2:Two,3:Three">two</span>

<p>
<span id="mycheckboxes[]" editinplace="checkbox"
    values="FF0000:Red,00FF00:Green,0000FF:Blue">Green ,blue</span>
<br>
<span id="mycheckboxes2[]" editinplace="checkbox"
    values="foo:foo,bar:bar,baz:baz">foo, baz</span>

<p>
<span id="myselect" editinplace="select"
    values="cake:Cake,icecream:Ice cream cone,pie:Pie,sorbet:Sorbet &lt;b&gt;bold&lt;/b&gt;">pie</span>

<p>
Minimum value: -10<br>
Maximum value: 10<br>
<span id="integer" editinplace="text" size="5" stepsize="3"
    autovalidate="integer" minvalue="-10" maxvalue="10">foo</span>

<p>
Ip address:<br>
<span id="ipaddress" editinplace="text" size="20"
    autovalidate="ipaddress">42</span>

<p>
Domain name:<br>
<span id="hostname" editinplace="text" size="30"
    autovalidate="domainname" allowchars="*"
    powersearch="yahoo.com,michel.emond.com,net-itech.com,open.nit.ca,google.com"
    >localhost.*</span>

<p>
<input type="submit">

</form>
