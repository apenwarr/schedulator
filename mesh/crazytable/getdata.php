<?php

/*
output format:

{
    "source":"db",
    "mode":"thread",
    "rowpath":"0",
    "expandednodes":["6","10"],
    "nodes":[
        {"id":"0","name":"root","values":["Column 1","Column 2","Column 3"],"nodes":["1","2","6","11","12"],"nodescount":"5"},
        {"id":"1","name":"A**","values":["a1","a2","a3"],"nodes":[],"nodescount":"0"},
        {"id":"2","name":"B**","values":["b1","b2","b3"],"nodes":[],"nodescount":"3"},
        {"id":"6","name":"F**","values":["f1","f2","f3"],"nodes":[],"nodescount":"3"},
        {"id":"11","name":"K**","values":["k1","k2","k3"],"nodes":[],"nodescount":"0"},
        {"id":"12","name":"L**","values":["l1","l2","l3"],"nodes":[],"nodescount":"0"},
        {"id":"6","name":"F**","values":["f1","f2","f3"],"nodes":["7","8","10"],"nodescount":"3"},
        {"id":"7","name":"G**","values":["g1","g2","g3"],"nodes":[],"nodescount":"0"},
        {"id":"8","name":"H**","values":["h1","h2","h3"],"nodes":[],"nodescount":"2"},
        {"id":"10","name":"J**","values":["j1","j2","j3"],"nodes":[],"nodescount":"1"},
        {"id":"10","name":"J**","values":["j1","j2","j3"],"nodes":["2"],"nodescount":"1"},
        {"id":"2","name":"B**","values":["b1","b2","b3"],"nodes":[],"nodescount":"3"}
    ]
}

*/

if ($_GET['id'] == 0) {
    $expanded_nodes = array();
    for ($i = 0; $i < 1000; $i++) {
        $expanded_nodes[] = $i;
    }

    $expanded_nodes = array(1, 17, 20, 28, 38, 496, 647);
    $expanded_nodes = array(6, 10);
    $expanded_nodes = array('11~p', '12~p', '13~p');
} else {
    $expanded_nodes = array();
}


function setNode(&$nodes, $id)
{
    if (!array_key_exists($id, $nodes)) {
        $nodes[$id] = array();
        $nodes[$id]['id'] = $id;
        $nodes[$id]['name'] = '';
        $nodes[$id]['values'] = array();
        $nodes[$id]['nodes'] = array();
        $nodes[$id]['nodescount'] = 0;
    }
}

function get_all_data($source)
{
    if (!file_exists($source)) return;

    $db = file_get_contents($source);

    $nodes = array();

    foreach (explode("\n", $db) as $line) {
        if (trim($line) == '') continue;

        list($id, $name, $parents, $values) = explode(':', $line, 4);

        setNode($nodes, $id);

        $nodes[$id]['name'] = $name;
        $nodes[$id]['values'] = explode(':', $values);

        foreach ($parents as $parent) {
            setNode($nodes, $parent);
        }
    }
}


function get_data($source, $get_id, $auto_expand = false)
{
    if (!file_exists($source)) return;

    $db = file_get_contents($source);

    $parent_node = array();
    $sub_nodes = array();
    $nodes_count = array();

    $found_get_id = false;

    foreach (explode("\n", $db) as $line) {
        if (trim($line) == '') continue;

        list($id, $name, $parents, $values) = explode(':', $line, 4);
        if ($id != '' && $id == $get_id) {
            $parent_node['id'] = $id;
            $parent_node['name'] = $name;
            $parent_node['values'] = explode(':', $values);
            $parent_node['nodes'] = array();
            $parent_node['nodescount'] = 0;
            $found_get_id = true;
        }

        foreach (split(',', $parents) as $parent) {
            if ($parent != $id) {
                if (array_key_exists($parent, $nodes_count)) {
                    $nodes_count[$parent]++;
                } else {
                    $nodes_count[$parent] = 1;
                }
            }
        }
    }

    if ($found_get_id) {
        if (array_key_exists($get_id, $nodes_count)) {
            $parent_node['nodescount'] = $nodes_count[$get_id];
        }

        foreach (explode("\n", $db) as $line) {
            if (!$line) { continue; }
            list($id, $name, $parents, $values) = explode(':', $line, 4);
            foreach (split(',', $parents) as $parent) {
                if ($parent != $id && $parent == $get_id) {
                    $node = array();
                    $node['id'] = $id;
                    $node['name'] = $name;
                    $node['values'] = explode(':', $values);
                    $node['nodes'] = array();
                    $node['nodescount'] = 0;

                    if (array_key_exists($id, $nodes_count)) {
                        $node['nodescount'] = $nodes_count[$id];
                    }

                    array_push($sub_nodes, $node);

                    array_push($parent_node['nodes'], $id);
                }
            }
        }

        array_unshift($sub_nodes, $parent_node);

        if (!$auto_expand) {
            global $expanded_nodes;
            foreach ($expanded_nodes as $id) {
                foreach (get_data($source, $id, true) as $node) {
                    array_push($sub_nodes, $node);
                }
            }
        }

        return $sub_nodes;
    }

    return array();
}


function escape_double_quote($text)
{
    $text = str_replace('\\', '\\\\', $text);
    return str_replace('"', '\\"', $text);
}

function array_to_js($o)
{
    if (is_array($o)) {
        $is_array = true;
        foreach ($o as $k => $v) {
            if (!is_numeric($k)) {
                $is_array = false;
            }
        }

        $r = array();
        foreach ($o as $k => $v) {
            if ($is_array) {
                $r[] = array_to_js($v);
            } else {
                $r[] = '"'.escape_double_quote($k).'":'.array_to_js($v);
            }
        }

        if ($is_array) {
            return '[' . implode(',', $r) . ']';
        } else {
            return '{' . implode(',', $r) . '}';
        }
    } else {
        return '"'.escape_double_quote($o).'"';
    }
}

function doublequote($s)
{
    return '"'.$s.'"';
}


header('Expires: Thu, 01 Dec 1994 16:00:00 GMT');

set_time_limit(0);

sleep(0);

if (isset($_GET['id'])) {

    $get_id = $_GET['id'];
    if (!$get_id) { $get_id = 0; }

    $row_path = isset($_GET['rowpath']) ? $_GET['rowpath'] : $get_id;

    if ($get_id == -1) {

/*
        $queue = array(0);
        $return_array = array();

        $id = array_shift($queue);
        while (isset($id)) {
            $data = get_data($_GET['source'], $id, $row_path);
            array_push($return_array, array_to_js($data));

            foreach ($data['nodes'] as $key => $value) {
                array_push($queue, $key);
            }

            $id = array_shift($queue);
        }

        print '['.implode(',', $return_array).']';
*/

    } else {

        print '{"source":"'.$_GET['source'].'",'.
              #'"mode":"thread",'.
              '"rowpath":"'.$row_path.'",'.
              '"expandednodes":['.implode(',', array_map('doublequote', $expanded_nodes)).'],'.
              '"nodes":'.array_to_js(get_data($_GET['source'], $get_id)).'}';

    }

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
    for ($i = $index; $i < $index+$page_length; $i++) {
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
