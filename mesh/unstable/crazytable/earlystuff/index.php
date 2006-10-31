<?php

function render_table($data, $sortable = true, $editable = true)
{
    $id = rand();

    $sort_stuff = $sortable ? 'sortable="1" editinplace="1,2,4,5,6" id="'.$id.'"' : '';

    print '<p>User search: <input powersearch="'.$id.',1,2" size="55" name="userSearch'.$id.'">'."\n\n";

    print '<p><table '.$sort_stuff.' border="1">' . "\n";

    if (count($data)) {
        print '<tr><th align="left" colspan="'.count($data[0]).'"><img src="niti-logo.png" width=20 height=20>Dummy Header</th></tr>';
    }

    $first = 1;
    foreach ($data as $row) {
        $celltag = $first ? 'th' : 'td';
        print '<tr>';
        foreach ($row as $cell) {
            if ($cell == '') { $cell = ' &nbsp;'; }
            print "<$celltag>$cell</$celltag>";
        }
        print '</tr>' . "\n";
        $first = 0;
    }
    print '</table>' . "\n";
}


$keys = array('local', 'userid', 'fullname', 'teams', 'pptp', 'ftp',
              'exchangeit', 'emails', 'diskspace', 'action');

$headers = array('Local', 'User ID', 'Full Name', 'Teams', 'PPTP / Dial-In',
                 'FTP', 'ExchangeIt!', 'Emails', 'Disk Space Used', 'Action');

$names = array('Andrew MacPherson', 'Avery Pennarun', 'Allison Marles', 'Christine Prime', 'Dave Coombs', 'Dave Carney', 'Damian Gryski', 'Hubert Figuière', 'Aaron Cameron', 'Jeff Klink', 'Mark Côté', 'Michel Émond', 'Nikhil Varma', 'Peter McCurdy', 'Patrick Patterson', 'Pierre Phaneuf', 'Peter Zion', 'Simon Law', 'William Lachance');

$unames = array('andrew', 'apenwarr', 'apm', 'cprime', 'dcoombs', 'dfcarney', 'dmg', 'hub', 'jimmy', 'jklink', 'mcote', 'mich', 'nikhil', 'pmccurdy', 'ppatters', 'pphaneuf', 'pzion', 'sfllaw', 'wlach');

$vowels = array('a','e','i','o','u','y');
$consts = array();
for ($i = ord('a'); $i <= ord('z'); $i++) {
    if (!in_array(chr($i), $vowels)) {
        $consts[] = chr($i);
    }
}

for ($i = 0; $i < 500; $i++) {
    $r = '';
    for ($j = 0;  $j < 4; $j++) {
        $r .= $consts[rand(0, count($consts)-1)];
        $r .= $vowels[rand(0, count($vowels)-1)];
    }
    $unames[] = $r;

    $r .= ' ';
    for ($j = 0;  $j < 4; $j++) {
        $r .= $consts[rand(0, count($consts)-1)];
        $r .= $vowels[rand(0, count($vowels)-1)];
    }
    $names[] = ucwords($r);
}

$teams = array('', 'exchangeit-admin', 'webmaster', 'exchangeit-admin, webmaster');

$data = array();
$row = array();
for ($j = 0; $j < 10; $j++) {
    $row[$keys[$j]] = $headers[$j];
}
$data[0] = $row;

for ($i = 0; $i < count($unames); $i++) {
    $row = array();
    $row[0] = '*';
    $row[1] = $unames[$i];
    $row[2] = $names[$i];
    $row[3] = $teams[rand(0, 3)];
    $row[4] = rand(0, 1) ? '*' : '';
    $row[5] = rand(0, 1) ? '*' : '';
    $row[6] = rand(0, 1) ? '*' : '';
    $row[7] = ($emails = rand(0, 10)) == 0 ? '-' : $emails;
    $row[8] = (rand(0, 500) / 10) . ' KB';
    $row[9] = '[...] [X]';
    $data[$i+1] = $row;
}

?>

<html>
<head>
<title>JS Hacking</title>
<link rel="stylesheet" href="sorttable.css" type="text/css">
<link rel="stylesheet" href="powersearch/powersearch.css" type="text/css">
<script src="shared.js" type="text/javascript"></script>
<script src="sorttable.js" type="text/javascript"></script>
<script src="powersearch/powersearch.js" type="text/javascript"></script>
<script src="editinplace.js" type="text/javascript"></script>
</head>

<body>

<form name="foo">

<input type="button" value="Enable Edit in place" onClick="editInPlaceInit(); this.style.display='none';">

<?php

render_table($data);

?>

</form>

</body>
</html>

