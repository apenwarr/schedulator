
<script src="../shared.js" type="text/javascript"></script>
<script src="../json.js" type="text/javascript"></script>

<script src="config.js" type="text/javascript"></script>

<link rel="stylesheet" href="crazytable.css" type="text/css">
<script src="crazytable.js" type="text/javascript"></script>

<link rel="stylesheet" href="../editinplace/editinplace.css" type="text/css">
<script src="../editinplace/editinplace.js" type="text/javascript"></script>


<script src="uniconf.js" type="text/javascript"></script>

<p>
<form name="searchForm" action="index.php" method="get">

<?php
$browse = 'Browse';
$search = 'Search';
$search_value = '';
$span_search = '';
if (isset($_GET['search']) && $_GET['search'] != '') {
    $browse = '<a href="index.php">'.$browse.'</a>';
    $search = "<b>$search</b>";
    $search_value = $_GET['search'];
    if (get_magic_quotes_gpc()) {
        $search_value = stripslashes($search_value);
    }
} else {
    $browse = "<b>$browse</b>";
}
?>
<?=$browse?> | <?=$search?>&nbsp;
<input type="text" name="search" size="20" value="<?=htmlspecialchars($search_value)?>">
<input type="submit" value="Go">
</form>

<p>

<form>

<span crazytable="uniconf" searchstring="<?=htmlspecialchars($search_value)?>"></span>

<p>
<table cellspacing="0" cellpadding="0" border="0">

<tr>
<td colspan="2"><p><a href="javascript:;" onClick="uniconfUncheckAll()">Uncheck All</a><br><br></td>
</tr>

<tr>
<td colspan="2">
    <table width="100%" cellspacing="0" cellpadding="0" border="0">
    <tr>
    <td><p><input type="button" value="Delete" onClick="uniconfDelete()"><br><br></td>
    <td align="right"><p><input type="button" value="Save" onClick="uniconfSave()"><br><br></td>
    </tr>
    </table>
</td>
</tr>

<tr>
<td>
    <p><hr size="1" noshade>

    <p><b>Set UniConf Key</b>

    <p>
    <table cellspacing="3" cellpadding="0" border="0">
    <tr><td>Key: </td><td><input type="text" name="setkey" size="40" value=""></td></tr>
    <tr><td>Value: </td><td><input type="text" name="setvalue" size="40" value=""></td></tr>
    </table>

    <p><input type="button" value="Set" onClick="uniconfFormSet(this.form)">
</td>
</tr>
</table>

</form>
