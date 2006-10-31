<?php

print '<pre>';
print_r($_GET);
print '</pre>';

?>

<script src="listswap.js" type="text/javascript"></script>

<form name="generalform">

<table>
<tr>
<td>
    <b><font color="white">Available Teams</font></b>
</td>

<td></td>

<td>
    <b><font color="white">Member of Teams</font></b>
</td>
</tr>

<tr>
<td>
    <select multiple name="team_users_out" size="20" style="width:200px">
        <option value="autoinstall">autoinstall (Team)</option>
        <option value="log">log (Team)</option>
        <option value="mailarc">mailarc (Team)</option>
    </select>
</td>

<td align="center">
    <input type="button" name="add" value="Join &gt;&gt;" style="width:90"
        onclick="listSwap.add('generalform', 'team_users');">
    <p>
    <input type="button" name="del" value="&lt;&lt; Leave" style="width:90"
        onclick="listSwap.remove('generalform', 'team_users');">
</td>

<td>
    <select multiple name="team_users_in" size="20" style="width:200px">
        <option value="exchangeit-admin">exchangeit-admin</option>
        <option value="webmaster">webmaster</option>
    </select>
    <input type="hidden" name="team_users" value="exchangeit-admin,webmaster">
</td>
</tr>
</table>

<input type="submit">

</form>
