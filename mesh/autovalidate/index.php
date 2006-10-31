<?php

?>

<link rel="stylesheet" href="autovalidate.css" type="text/css">
<script src="../shared.js" type="text/javascript"></script>
<script src="autovalidate.js" type="text/javascript"></script>

<link rel="stylesheet" href="../powersearch/powersearch.css" type="text/css">
<script src="../powersearch/powersearch.js" type="text/javascript"></script>

<form name="autovalidateForm">

<p>
Minimum value: -2<br>
Maximum value: 102<br>
Default value: -1<br>
Step size: 5<br>
<span><input type="text" name="number1"
    autovalidate="integer" minvalue="-2" maxvalue="102"
    defvalue="-1" stepsize="5"
    size="5" value="ha!"></span>

<p>
Minimum value: 25<br>
Maximum value: none<br>
<span><input type="text" name="number2" autovalidate="integer" minvalue="25" size="5" value=""></span>

<p>
Ip address:<br>
<span><input type="text" name="ip" autovalidate="ipaddress" size="20" value="192"></span>

<p>
Domain name:<br>
<input type="text" name="host" size="30" value=""
    autovalidate="domainname" allowchars="*"
    powersearch="yahoo.com,michel.emond.com,net-itech.com,open.nit.ca,google.com">

<p>
IP Address and Domain name:<br>
<span><input type="text" name="host" size="30" value=")(*&?%$/&quot;!"
    autovalidate="ipdomainname" allowchars="*"></span>

</form>
