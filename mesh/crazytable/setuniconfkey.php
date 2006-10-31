<?php

$key = $_GET['key'];
$value = $_GET['value'];

if (get_magic_quotes_gpc()) {
  if (isset($key)) { $key = stripslashes($key); }
  if (isset($value)) { $value = stripslashes($value); }
}

$url = 'http://kid/~root/uniset.php?key=' . rawurlencode($key);

if (isset($value)) {
    $url .= '&value=' . rawurlencode($value);
}

$handle = fopen($url, 'r');
$contents = '';
while (!feof($handle)) {
  $contents .= fread($handle, 8192);
}
fclose($handle);

print $contents;

?>
