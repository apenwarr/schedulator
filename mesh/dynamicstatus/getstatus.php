<?php

set_time_limit(10);

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

function get_data($keys)
{
    if (!file_exists('db')) return;

    $db = file_get_contents('db');

    $data = array();

    foreach(explode("\n", $db) as $line) {
        if ($line) {
            preg_match('/(.*?):(.*)/', $line, $matches);

            if (in_array($matches[1], $keys)) {
                $data[$matches[1]] = $matches[2];
            }
        }
    }

    return $data;
}

header('Expires: Thu, 01 Dec 1994 16:00:00 GMT');

$keys = explode(',', $_GET['keys']);

$data = get_data($keys);

if (isset($_GET['first'])) {
    print array_to_js($data);
    exit;
}

$new_data = array();
while (!$new_data) {
    sleep(1);
    $update = get_data($keys);
    foreach ($update as $key => $value) {
        if (!array_key_exists($key, $data) || $value != $data[$key]) {
            $new_data[$key] = $value;
        }
    }
}

print array_to_js($new_data);

?>
