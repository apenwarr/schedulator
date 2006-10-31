<?php

function sort_column($a, $b)
{
    global $sort_col, $sort_order;

    if ($sort_col == -1)
    {
        return strcasecmp($a['name'], $b['name']) * $sort_order;
    }
    else
    {
        return strcasecmp($a['cols'][$sort_col], $b['cols'][$sort_col]) * $sort_order;
    }
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

        $is_array = 1;
        foreach ($o as $k => $v)
        {
            if (!is_numeric($k))
            {
                $is_array = 0;
                break;
            }
        }

        foreach ($o as $k => $v) {
            if (!$is_array)
            {
                $key = '"'.escape_double_quote($k).'":';
            }
            $r[] = $key . array_to_js($v);
        }

        if ($is_array)
        {
            return '[' . implode(',', $r) . ']';
        }
        else
        {
            return '{' . implode(',', $r) . '}';
        }
    } else {
        return '"'.escape_double_quote($o).'"';
    }
}


$a = unserialize(file_get_contents('db'));

if (isset($_GET['id'])) {

    $v = array();

    if ($_GET['id'] == 0)
    {
        array_push($v, array_to_js($a[0]));
    }

    $nodes = $a[$_GET['id']]['nodes'];
    for ($i = 0; $i < count($nodes); $i++)
    {
        array_push($v, array_to_js($a[$nodes[$i]]));
    }

    print '{"request":"getsubnodes","id":"' . $_GET['id'] . '",' .
          '"rowid":"' . $_GET['rowid'] . '","nodes":[' . implode(',', $v) . ']}';

} else if (isset($_GET['page'])) {

    $sort_col = $_GET['sortcol'];
    if ($sort_col < -1 || $sort_col > count($a[0]['cols'])-1) { $sort_col = -1; }

    $sort_order = $_GET['sortorder'];
    if ($sort_order != -1) { $sort_order = 1; }

    unset($a[0]);
    usort($a, 'sort_column');

    $page_length = 20;

    $max_page = ceil(count($a) / $page_length);
    $page = min($_GET['page'], $max_page);
    $index = ($page - 1) * $page_length;

    $v = array();
    for ($i = $index; $i < $index+$page_length; $i++)
    {
        array_push($v, array_to_js($a[$i]));
    }

    print '{"page":"' . $page . '",' .
          '"sortcol":"' . $sort_col . '",' .
          '"sortorder":"' . $sort_order . '",' .
          '"pagelength":"' . $page_length . '",' .
          '"totalcount":"' . count($a) . '",' .
          '"nodes":[' . implode(',', $v) . ']}';
}

?>
