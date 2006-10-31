<?php

$max = 1000;
$cc = 10;

$h = fopen('db2', 'w');

$c = '';
for ($i = 0; $i < $cc; $i++)
{
    $c .= ":Column " . chr($i+65);
}

fwrite($h, "0:root:0$c\n");

for ($i = 1; $i < $max; $i++)
{
    $name = "Node $i";
    $parent = rand(0, $i-1);

    $c = '';
    for ($j = 0; $j < $cc; $j++)
    {
        $c .= ":$i" . chr($j+97);
    }

    fwrite($h, "$i:$name:$parent$c\n");
}

?>
