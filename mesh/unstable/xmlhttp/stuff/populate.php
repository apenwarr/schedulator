<?php

$teams = array('', 'exchangeit-admin', 'webmaster', 'exchangeit-admin, webmaster');

$vowels = array('a','e','i','o','u','y');
$consts = array();
for ($i = ord('a'); $i <= ord('z'); $i++) {
    if (!in_array(chr($i), $vowels)) {
        $consts[] = chr($i);
    }
}

$users = array();

for ($i = 0; $i < 500; $i++) {

    $user = array();

    $r = '';
    for ($j = 0;  $j < 4; $j++) {
        $r .= $consts[rand(0, count($consts)-1)];
        $r .= $vowels[rand(0, count($vowels)-1)];
    }
    $user['uname'] = $r;

    $r .= ' ';
    for ($j = 0;  $j < 4; $j++) {
        $r .= $consts[rand(0, count($consts)-1)];
        $r .= $vowels[rand(0, count($vowels)-1)];
    }
    $user['name'] = ucwords($r);

    $user['teams'] = $teams[rand(0, 3)];
    $user['pptp'] = rand(0, 1);
    $user['ftp'] = rand(0, 1);
    $user['eit'] = rand(0, 1);
    $user['emails'] = $emails = rand(0, 10);
    $user['diskspace'] = (rand(0, 500) / 10);

    $users[] = $user;
}

$h = fopen('users', 'w');
fwrite($h, serialize($users));
fclose($h);

?>

foo
