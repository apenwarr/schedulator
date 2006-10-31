<?php

print '<pre>';
print_r($_GET);
print '</pre>';

?>

<script src="../shared.js" type="text/javascript"></script>

<link rel="stylesheet" href="tablesort.css" type="text/css">
<script src="tablesort.js" type="text/javascript"></script>


<form>

<table id="mytable" tablesort="1" border="1">

<?php

$first = 1;
$c = array();
for ($i = 0; $i < 8; $i++) {
    print '<tr>';
    for ($j = 0; $j < 5; $j++) {
        if ($first) {
            print '<th>column'.$j.'</th>';
        } else {
            print '<td><input type=checkbox value="i'.$i.'j'.$j.'" name="foo[]">';
            if (!isset($c[$j])) {
                $c[$j] = rand(0, 1);
            }
            if ($c[$j]) {
                for ($k = 0; $k < 10; $k++) {
                    print chr(rand(ord('a'), ord('z')));
                }
            } else {
                print (($i == $j) ? '-' : rand(100,999));
            }
            print '</td>';
        }
    }
    print '</tr>'."\n";
    $first = 0;
}

?>

</table>

<input type=submit>

</form>
