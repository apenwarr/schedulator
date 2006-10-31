<?php

function get_data()
{
    if (!file_exists('outdated')) return;
    return trim(file_get_contents('outdated'));
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


header('Expires: Thu, 01 Dec 1994 16:00:00 GMT');

$data = get_data();

$new_data = 0;
while (!$new_data) {
    sleep(1);
    $update = get_data();
    if ($update != $data) {
        $new_data = 1;
    }
}

print array_to_js(array(2 => 1, 8 => 1));

?>
