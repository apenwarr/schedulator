<?php

function new_node($key, $value)
{
    $node = array();
    $node['id'] = $key;
    $node['name'] = array_pop(split('/', $key));
    $node['values'] = array($value);
    $node['nodes'] = array();
    $node['nodescount'] = 0;
    return $node;
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

function split_line_parts($line)
{
    $parts = preg_split('/(}?) = (\{?)/', $line, -1, PREG_SPLIT_DELIM_CAPTURE);

    $key = '/'.$parts[0];
    if ($parts[1]) { $key = substr($key, 1); }

    $value = $parts[3];
    if ($parts[2]) { $value = substr($value, 0, strlen($value)-1); }

    return array($key, $value);
}

function get_result($get_id)
{
    global $uniconf;

    if ($get_id === 0) {
        $get_key = '';
    } else {
        $get_key = $get_id;
    }

    $preg_parent = preg_quote($get_key, '/');
    $sub_nodes = array();
    $preg_sub_node = '';

    $parent_node = array();
    if ($get_id === 0) {
        $parent_node['id'] = '0';
        $parent_node['name'] = '&nbsp;Key&nbsp;';
        $parent_node['values'] = array('&nbsp;Value&nbsp;');
        $parent_node['nodes'] = array();
        $parent_node['nodescount'] = 0;
        $preg_parent = '';
    }

    foreach ($uniconf as $line) {
        if ($line == '') { continue; }
        list($key, $value) = split_line_parts($line);

        if ($get_id !== 0 && $parent_node && strpos($key, $get_key) !== 0) {
            break;
        } else if ($key == $get_key) {
            $parent_node = new_node($key, $value);
        } else if ($parent_node) {
            if (preg_match('/^'.$preg_parent.'\/[^\/]+$/', $key)) {
                array_push($parent_node['nodes'], $key);
                $parent_node['nodescount']++;
                $sub_nodes[$key] = new_node($key, $value);
                $preg_sub_node = preg_quote($key, '/');
            } else if (preg_match('/^'.$preg_sub_node.'\/[^\/]+$/', $key)) {
                $temp = split('/', $key);
                array_pop($temp);
                $temp = join('/', $temp);
                $sub_nodes[$temp]['nodescount']++;
            }
        }
    }

    array_unshift($sub_nodes, $parent_node);

    $result = array();
    foreach ($sub_nodes as $node) {
        if ($node) {
            array_push($result, $node);
        }
    }

    return $result;
}

function get_search_result($get_id)
{
    $r = get_result($get_id);
    $result = array();
    if (count($r) > 0) {
        $result[$r[0]['id']] = $r[0];

        for ($i = 1; $i < count($r); $i++) {
            if ($r[$i]['nodescount'] > 0) {
                $s = get_search_result($r[$i]['id']);
                foreach ($s as $key => $value) {
                    $result[$key] = $value;
                }
            } else {
                $result[$r[$i]['id']] = $r[$i];
            }
        }
    }
    return $result;
}


$handle = fopen("http://kid/~root/uniread.php", "rb");
$contents = '';
while (!feof($handle)) {
  $contents .= fread($handle, 8192);
}
fclose($handle);
$uniconf = split("\n", $contents);
$expandednodes = '[]';


// search filter
$uniconf_search = array();
if (isset($_GET['search'])) {
    $search_string = strtolower($_GET['search']);
    if (get_magic_quotes_gpc()) {
        $search_string = stripslashes($search_string);
    }

    foreach ($uniconf as $line) {
        list($key, $value) = split_line_parts($line);
        if (strpos(strtolower($key), $search_string) !== false ||
            strpos(strtolower($value), $search_string) !== false) {
            $parts = split('/', $key);
            $a = array();
            while (count($parts) > 1) {
                array_push($a, array_shift($parts));
                $e = join('/', $a);
                if (!$e) { continue; }
                if (!$uniconf_search[$e]) {
                    $uniconf_search[$e] = '{}';
                }
            }
            $uniconf_search[$key] = $value;
        }
    }

    $uniconf = array();
    $expandednodes = array('0');
    foreach ($uniconf_search as $key => $value) {
        array_push($uniconf, substr($key, 1) . ' = ' . $value);
        array_push($expandednodes, $key);
    }
    $expandednodes = array_to_js($expandednodes);
}


if (isset($_GET['id'])) {

    $get_id = $_GET['id'];
    if (!$get_id) {
        $get_id = 0;
    }
    $row_path = isset($_GET['rowpath']) ? $_GET['rowpath'] : $get_id;

    if (isset($_GET['search'])) {
        $search_result = get_search_result($get_id);
        $result = array();
        foreach ($search_result as $n) {
            array_push($result, $n);
        }
    } else {
        $result = get_result($get_id);
    }

    print '{"source":"'.$_GET['source'].'",'.
          '"rowpath":"'.$row_path.'",'.
          '"expandednodes":'.$expandednodes.','.
          '"nodes":'.array_to_js($result).'}';

} else if (isset($_GET['nodes'])) {

    $result = array();

    $nodes = split(',', $_GET['nodes']);
    foreach ($nodes as $node) {
        $node = rawurldecode($node);
        if ($node === '0') { $node = 0; }
        $temp = get_result($node);
        $result = array_merge($result, $temp);
    }

    print '{"source":"'.$_GET['source'].'",'.
          '"nodes":'.array_to_js($result).'}';

}

?>
