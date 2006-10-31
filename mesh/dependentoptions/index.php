
<script src="../shared.js" type="text/javascript"></script>
<script src="dependentoptions.js" type="text/javascript"></script>

<link rel="stylesheet" href="../autovalidate/autovalidate.css" type="text/css">
<script src="../autovalidate/autovalidate.js" type="text/javascript"></script>

<link rel="stylesheet" href="../powersearch/powersearch.css" type="text/css">
<script src="../powersearch/powersearch.js" type="text/javascript"></script>


<form name="dependForm">

<input id="useoptions1" type="checkbox" name="useoptions" value="1" checked>
<label for="useoptions1">Use options 1</label>
<input id="useoptions2" type="checkbox" name="useoptions" value="2">
<label for="useoptions2">Use options 2</label>

<dl><dd>

    <p>
    <input dependon="useoptions" enableif="1" id="usecolor" type="checkbox" name="usecolor" value="1">
    <label for="usecolor">Say your favorite color!</label>

    <dl><dd>

        <p>
        <input dependon="usecolor" enableif="1" id="colorred" type="radio" name="color" value="red">
        <label for="colorred">Red</label>

        <input dependon="usecolor" enableif="1" id="colorgreen" type="radio" name="color" value="green">
        <label for="colorgreen">Green</label>

        <input dependon="usecolor" enableif="1" id="colorblue" type="radio" name="color" value="blue">
        <label for="colorblue">Blue</label>

        <input dependon="usecolor" enableif="1" id="colorother" type="radio" name="color" value="other">
        <label for="colorother">Other</label>

        <input dependon="color" enableif="other" type="text" name="colorothertext" value="">

            <input dependon="colorothertext" enableif="red,green,blue"
                id="differentshade" type="checkbox" name="differentshade" value="1">
            <label for="differentshade">Different shade</label>

    </dd></dl>


<p>
<label for="ip">IP Address</label>
<span><input type="text" name="ip" size="20"
    dependon="useoptions" enableif="1"
    autovalidate="ipaddress"
></span>
<span><input type="text" name="ip2" size="20"
    dependon="ip" enableif="127.0.0.1,0.0.0.0"
    autovalidate="domainname" allowchars="*()"
></span>


    <p>
    <label for="zitem">Item #1</label>

    <select id="zitem" dependon="useoptions" enableif="999" name="zitem">
    <option value="cat">Cat
    <option value="dog">Dog
    <option value="table">Table
    </select>

<input type="text" name="item" size="5"
    dependon="useoptions" enableif="1"
    powersearch="cat,dog,table">

    <dl><dd>
        <p>
        <label for="favoriteyes">Is this your favourite animal?</label>
        <input dependon="item" disableif="table" id="favoriteyes" type="radio" name="isfavorite" value="yes">
        <label for="favoriteyes">Yes</label>
        <input dependon="item" disableif="table" id="favoriteno" type="radio" name="isfavorite" value="no">
        <label for="favoriteno">No</label>
        <br><label for="favoriteyes">Is disabled if item1 is table</label>
    </dd></dl>

    <p>
    <label for="itemcloud">Item #2</label>

    <input dependon="useoptions" enableif="1" id="itemcloud" type="radio" name="item2" value="cloud">
    <label for="itemcloud">Cloud</label>

    <input dependon="useoptions" enableif="1" id="itemtable" type="radio" name="item2" value="table">
    <label for="itemtable">Table</label>

    <input dependon="useoptions" enableif="1" id="itemcouch" type="radio" name="item2" value="couch">
    <label for="itemcouch">Couch</label>


    <dl><dd>

        <p>
        <label for="livinroomyes">Is this in your living room?</label>
        <input dependon="item2" disableif="cloud" id="livinroomyes" type="radio" name="livingroom" value="yes">
        <label for="livinroomyes">Yes</label>
        <input dependon="item2" disableif="cloud" id="livingroomno" type="radio" name="livingroom" value="no">
        <label for="livingroomno">No</label>
        <br><label for="livinroomyes">Is disabled if item2 is cloud</label>

        <p>
        <label for="describelivingroom">Describe the living room:</label><br>
        <textarea dependon="livingroom" enableif="yes" id="describelivingroom"
            name="describelivingroom" cols="40" rows="4" wrap="virtual"></textarea>

        <br>
        <input dependon="describelivingroom" disableif=""
            id="foo1" type="checkbox" name="foo1" value="1">
        <label for="foo1">Proofread done</label>
        <input dependon="describelivingroom" enableif=""
            id="foo2" type="checkbox" name="foo2" value="1">
        <label for="foo2">Provide by regular mail</label>

    </dd></dl>

</dd></dl>

</form>
