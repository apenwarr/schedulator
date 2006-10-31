<?php

function get_random_str()
{
    $s = '';
    for ($j = 0; $j < 5; $j++)
    {
        $s .= chr(97 + rand(0, 25));
    }
    return $s;
}


$max = 1000;
$cc = 10;

$a = array();

$n = array();
$n['id'] = 0;
$n['name'] = 'root';
$n['cols'] = array();
$n['nodes'] = array();
$n['nodescount'] = 0;

for ($i = 0; $i < $cc; $i++)
{
    array_push($n['cols'], "Column " . chr($i+65));
}

array_push($a, $n);


for ($i = 1; $i < $max; $i++)
{
    $n = array();
    $n['id'] = $i;
    $n['name'] = get_random_str() . " node $i";

    $n['cols'] = array();
    for ($j = 0; $j < $cc; $j++)
    {
        array_push($n['cols'], get_random_str() . " $i " . chr($j+97));
    }

    $n['nodes'] = array();
    $n['nodescount'] = 0;

    $parent = rand(0, $i-1);
    array_push($a[$parent]['nodes'], $i);
    $a[$parent]['nodescount']++;

    if ($i > 10 && rand(0, 20) == 0)
    {
        $parent = rand(0, $i-1);
        if (array_search($i, $a[$parent]['nodes']) === false)
        {
            array_push($a[$parent]['nodes'], $i);
            $a[$parent]['nodescount']++;
        }
    }

    array_push($a, $n);
}

$h = fopen('db', 'w');
fwrite($h, serialize($a));

?>
