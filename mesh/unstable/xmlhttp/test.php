<?php

$a = $_GET['a'];
$b = $_GET['b'];

if (!is_numeric($a) || !is_numeric($b)) {
    $a = '';
    $b = '';
    $total = '';
} else {
   $total = $a + $b;
}

$acc = $_SERVER['HTTP_ACCEPT'];

if (strpos($acc, 'message/x-jl-formresult') !== false) {
    print $total;
} else {

?>

<script src="xmlhttp.js" type="text/javascript"></script>
<script language="javascript">
<!--
function calc()
{
    frm=document.forms[0];
    url="test.php?a="+frm.elements['a'].value+"&b="+frm.elements['b'].value;
    xmlhttp.open("GET",url,true);
    xmlhttp.onreadystatechange=function() {
        if (xmlhttp.readyState==4) {
            frm.elements['total'].value=xmlhttp.responseText;
        }
    }
    xmlhttp.setRequestHeader('Accept','message/x-jl-formresult');
    xmlhttp.send(null);
    return false;
}
//-->
</script>

<form action="test.php" method="get" onsubmit="return calc();">
<input type=text name=a value="<?=$a?>"> + <input type=text name=b value="<?=$b?>">
 = <input type=text name=total value="<?=$total?>">
<input type=submit value="Calculate">
</form>

<?php

}

?>
