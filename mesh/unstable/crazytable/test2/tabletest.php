<script language="javascript">
<!--

function nodeClick(id)
{
    var row = document.getElementById('row'+id);
    var sub = row.nextSibling;

    sub.style.display = (sub.style.display == 'none' ? '' : 'none');
}

//-->
</script>


<body>

<?php

set_time_limit(60);

function getWord()
{
    $s = '';
    $l = rand(5, 10);
    for ($i = 0; $i < $l; $i++)
    {
        $s .= chr(97 + rand(0, 25));
    }
    return $s;
}

function getWords()
{
    $s = '';
    $l = rand(3, 5);
    for ($j = 0; $j < $l; $j++)
    {
        $s .= getWord() . ' ';
    }
    return $s;
}

function getRow($z, $hi, $ph)
{
    global $t;
    $t++;

    if ($ph) $ph .= '.';
    $ph .= $t;

    $c = 4; // column count
    $x = 4; // max depth limit

    $cw = round(100 / ($c+1)) . '%';
    $s = ($z == 0 ? 1 : rand(0, 5) > 0);


    $w = '<tr valign="top" id="row'.$t.'" path="'.$ph.'"><td width="'.$cw.'%">';

    for ($i = 0; $i < strlen($hi); $i++)
    {
        if ($i == (strlen($hi)-1))
        {
            $w .= (substr($hi, $i, 1) == '0' ? '&nbsp;&nbsp;+' : '&nbsp;&nbsp;&nbsp;\\');
        }
        else
        {
            $w .= (substr($hi, $i, 1) == '0' ? '&nbsp;&nbsp;|' : '&nbsp;&nbsp;&nbsp;');
        }
    }

    if ($z < $x && $s)
    {
        $w .= '<a href="javascript:nodeClick('.$t.');">'.$t.' '.getWord().'</a>';
    }
    else
    {
        $w .= $t.' '.getWord();
    }
    $w .= '&nbsp;</td>';

    for ($i = 0; $i < $c; $i++)
    {
        $w .= '<td style="padding:2px;" width="'.$cw.'">'.getWords().'</td>';
    }

    $w .= '</tr>';


    if ($z < $x && $s)
    {
        $r = '<tr><td width="99%" colspan="'.($c+1).'"><table width="100%" cellspacing="0" cellpadding="0" border="0">' . $w .
              '<tr><td width="99%" colspan="'.($c+1).'"><table width="100%" cellspacing="0" cellpadding="0" border="0">';

        $l = rand(1, 10);
        for ($i = 0; $i < $l; $i++)
        {
            $r .= getRow($z+1, $hi.($i==$l-1?'1':'0'), $ph);
        }

        $r .= '</table></td></tr></table></td></tr>';
    }
    else
    {
        $r = $w;
    }

    return $r;
}

$t = 0;

print '<table cellspacing="0" cellpadding="0" border="1">';
print getRow(0, '', '');
print '</table>';

?>

</body>
