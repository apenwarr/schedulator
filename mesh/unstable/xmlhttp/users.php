<?php

function colsort($a, $b)
{
    global $sortcol;
    return strnatcasecmp($a[$sortcol], $b[$sortcol]);
}

function escape_double_quote($text)
{
    $text = str_replace('\\', '\\\\', $text);
    return str_replace('"', '\\"', $text);
}

function array_to_js($o)
{
    if (is_array($o)) {
        $r = array();
        foreach ($o as $k => $v) {
            $r[] = '"'.escape_double_quote($k).'":'.array_to_js($v).'';
        }
        return '{' . implode(',', $r) . '}';
    } else {
        return '"'.escape_double_quote($o).'"';
    }
}

function getpage($page, $length)
{
    global $users, $cols;

    $start = ($page - 1) * $length;
    $pageusers = array();
    for ($i = $start; $i < ($start + $length); $i++) {
        $user = array();
        for ($j = 0; $j < count($cols); $j++) {
            $user[$j] = $users[$i][$cols[$j]];
        }
        $pageusers[] = $user;
    }

    return array_to_js(array('page' => $pageusers, 'total' => count($users)));
}

sleep(5);

$users = unserialize(file_get_contents('users'));

$users[0]['name'] = 'John "Hat" O\'Neil';
$users[0]['uname'] = 'aaaaaaa';

$cols = array('uname', 'name', 'teams', 'pptp', 'ftp', 'eit', 'emails', 'diskspace');
$sortcol = $_GET['c'];
if (!$sortcol) { $sortcol = 0; }
if (!array_key_exists($sortcol, $cols)) {
    $sortcol = 0;
}
$sortcol = $cols[$sortcol];
usort($users, 'colsort');

$page = $_GET['p'];
if (!$page) { $page = 1; }

$length = $_GET['l'];
if (!$length) { $length = 20; }

print getpage($page, $length);

?>
