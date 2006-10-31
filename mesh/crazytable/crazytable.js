/**
 * CrazyTable
 *
 * Author:
 *   Michel Emond
 *   Net Integration Technologies
 *   www.net-itech.com
 *
 * Description:
 *
 * Dependencies:
 *   shared.js
 *   json.js
 *
 * Syntax:
 *
 *   Restrictions:
 *     - The root node id must be 0 (zero).
 *     - It is assumed that the server will provide valid data with
 *       parent/child relationships that won't lead into cycles.
 *     - The server must return an Expires header to work around the browser's
 *       cache.
 *
 * Custom configuration
 *   CrazyTable has a large number of parameters available in order to change
 *   its appearance and behavior. The complete list along with an explanation is
 *   located in shared.js.
 *
 *   Setting them in a custom configuration must be done like this:
 *     widgetsConfig.crazyTable.<parameter name> = <value>;
 *
 *   See shared.js for information about custom configuration.
 *
 * Backend script:
 *   CrazyTable relies on a backend script providing the data. The value of
 *   dataScript must absolutely be set, in a custom configuration, to the
 *   location and name of this script.
 *
 *   Setting dataScript in a custom configuration must be done like this:
 *     widgetsConfig.crazyTable.dataScript = '<script name>';
 *
 *   See shared.js for information about custom configuration.
 *
 * Examples:
 *   <span crazytable="mysource"></span>
 */

/*global clearDomObj, document, editInPlace escapeQuotes, getRequestObject,
         getXmlHttp, isArray, isDefined, isFloating, isObject, JSON, navigator,
         setTimeout, toInteger, widgetsConfig */
//uni demo: escapeQuotes, setTimeout, editInPlace

function CrazyTable()
{
    var self = this;
    this.isReady = true;

    // delimiter must be only ONE char
    // must not be on of the special regex char: \|()[{^$*+?.
    this.delimiter = ':';

    //uni demo
    // delay in milliseconds for dynamic update
    this.dyndelay = 5000;

    if (!document.getElementById || !document.getElementsByTagName ||
        !getXmlHttp()) {
        this.isReady = false;
        return;
    }

    var ctCache = {};
    /*
    ctCache{souce} CacheData

    CacheData
        span  HTML Span
        data  {nodeId} Node
        rows  {rowpath} HTML Row

    Node
        id          Number
        name        String
        values      [] String
        nodes       [] nodeId
        nodescount  Number
        rows        {rowpath} RowInfo

    RowInfo
        isexpanded  Boolean
    */

    function sendHierarchyDataRequest(source, getId, rowPath)
    {
        if (rowPath === undefined) { rowPath = getId; }
        var requestObj = getRequestObject();
        var url = widgetsConfig.crazyTable.dataScript + '?source=' + source +
                  '&id=' + escape(getId) + '&rowpath=' + escape(rowPath);
        //uni demo
        if (ctCache[source].searchString !== '') {
            url += '&search=' + escape(ctCache[source].searchString);
        }
        //uni demo: end
        requestObj.send(url, self.updateHierarchyData);
    }

    function setNode(source, nodeData)
    {
        if (!ctCache[source].data[nodeData.id]) {
            ctCache[source].data[nodeData.id] = {};
        }

        var node = ctCache[source].data[nodeData.id];

        if (node.nodes && node.nodes.length > 0) {
            return;
        }

        var i;

        node.id = nodeData.id;
        node.name = nodeData.name;
        node.values = [];
        //uni demo
        /*
        if (!nodeData.values) {
            document.body.appendChild(document.createTextNode(dump(nodeData)+' does not have values '));
        }
        */
        //uni demo: end
        for (i = 0; i < nodeData.values.length; i++) {
            node.values[i] = nodeData.values[i];
        }
        node.nodes = [];
        for (i = 0; i < nodeData.nodes.length; i++) {
            node.nodes.push(nodeData.nodes[i]);
        }
        node.nodescount = nodeData.nodescount;
    }

    function escapeDelimiter(s)
    {
        var d = self.delimiter;
        return s.replace(d, d+d);
    }

    function splitRowpath(s)
    {
        var d = self.delimiter;
        var a = [];
        var t = '';
        var i = 0;
        var p;
        while (i < s.length)
        {
            p = s.indexOf(d, i);
            if (p == -1) { p = s.length; }
            if (s.indexOf(d, p+1) == p+1) {
                t += s.substring(i, p+1);
                i = p+2;
            } else {
                t += s.substring(i, p);
                a.push(t);
                t = '';
                i = p+1;
            }
        }
        return a;
    }

    // used to prevent hierarchy recursion
    function isRecursing(rowpath, nodeId)
    {
        var d = self.delimiter;
        nodeId = escapeDelimiter(nodeId);
        var re = new RegExp('(^|([^'+d+']'+d+'))'+nodeId+d+'[^'+d+']');
        return rowpath.search(re) > -1;
    }

    function setRow(source, nodeData, rowpath, expandedNodesHash)
    {
        var node = ctCache[source].data[nodeData.id];
        if (node === undefined) { return; }

        if (node.rows === undefined) {
            node.rows = {};
        }
        if (node.rows[rowpath] === undefined) {
            node.rows[rowpath] = {};
        }

        if (node.rows[rowpath].isexpanded === undefined) {
            var maxLevel = widgetsConfig.crazyTable.hierarchyAutoExpandMaxLevel;
            node.rows[rowpath].isexpanded =
                Boolean(expandedNodesHash[node.id] &&
                        (maxLevel == -1 ||
                         splitRowpath(rowpath).length <= (maxLevel+1)));
            if (node.rows[rowpath].isexpanded) {
                nodeData.nodes = node.nodes;
                nodeData.nodescount = node.nodescount;
            }
        }

        if (isRecursing(rowpath, node.id)) { return; }

        var tempData;
        for (var i = 0; i < nodeData.nodes.length; i++) {
            tempData = {'id':nodeData.nodes[i],'nodes':[],'nodescount':0};
            setRow(source, tempData,
                   rowpath+self.delimiter+escapeDelimiter(nodeData.nodes[i]),
                   expandedNodesHash);
        }
    }

    function getImage(name, id)
    {
        var imgId = '';
        if (id !== undefined) {
            imgId = 'id="hl' + id + '"';
        }
        return '<img src="' + widgetsConfig.imgBasePath + 'crazytable/images/' +
               name + '" ' + imgId + ' alt="" title="" width="15" ' +
               'height="18" border="0">';
    }

    function getImageName(isLastImg, isLastItem, isExpandable, isExpanded)
    {
        var name;
        if (isLastImg) {
            name = (isLastItem ? 'hl2' : 'hl3') +
                   (isExpandable ? (isExpanded ? 'm' : 'p') : '');
        } else {
            name = (isLastItem ? 'hl0' : 'hl1');
        }
        return name + '.png';
    }

    // From a rowpath, returns whether or not a node is the last node of its
    // parent. The parameter i is a rowpath index (zero based) to specify the
    // subject node. If omitted, the subject node will be the last one in the
    // rowpath.
    //
    // Examples:
    //   isLastNode('0/1/4/12', 2)
    //     checks if node 4 (index 2) is the last subnode of node 1
    //   isLastNode('0/1/4/12')
    //     checks if node 12 (the last one) is the last subnode of node 4
    function isLastNode(source, rowpath, i)
    {
        var rowpathNodes = splitRowpath(rowpath);

        if (!i) { i = rowpathNodes.length-1; }

        var nodes = ctCache[source].data;
        var pn = nodes[rowpathNodes[i-1]];

        return rowpathNodes[i] == pn.nodes[pn.nodes.length-1];
    }

    function getHierarchyLines(source, rowpath, isExpandable)
    {
        var imgId, isLastImg, isLastItem, s;

        var r = '';
        var rowpathNodes = splitRowpath(rowpath);

        for (var i = 1; i < rowpathNodes.length; i++) {
            isLastImg = (i == (rowpathNodes.length-1));
            isLastItem = isLastNode(source, rowpath, i);
            imgId = (isLastImg && isExpandable) ? rowpath : undefined;

            s = getImageName(isLastImg, isLastItem, isExpandable, false);
            s = getImage(s, imgId);
            if (isLastImg) {
                if (isExpandable) {
                    s = '<a href="javascript:;"' +
                        'onmouseover="window.status=\'Expand/Collapse ' +
                        'this node\';return true;" ' +
                        'onmouseout="window.status=\'\';" ' +
                        'onClick="crazyTable.rowClick(\''+source+'\',' +
                        '\''+rowpath+'\');return false;">' + s + '</a>';
                }
                s += '&nbsp;';
            }

            r += s;
        }

        return r;
    }

    function createRow(source, rowpath, isHeader)
    {
        var nodeId = splitRowpath(rowpath).pop();
        var node = ctCache[source].data[nodeId];
        if (isHeader === undefined) { isHeader = false; }

        var row = document.createElement('TR');
        row.setAttribute('id', rowpath);

        // IE is silly, the :hover selector is not supported
        if (!isHeader) {
            row.onmouseover = function () {
                this.className += 'hover';
            };
            row.onmouseout = function () {
                this.className =
                    this.className.substr(0, this.className.length-5);
            };
        }

        var cell;
        var cellTag = (isHeader ? 'TH' : 'TD');

        var isIE = !(navigator &&
                     navigator.userAgent.toLowerCase().indexOf("msie") == -1);

        if (isIE) {
            // IE is silly
            cell = document.createElement('<' + cellTag + ' nowrap="nowrap">');
        } else {
            cell = document.createElement(cellTag);
            cell.setAttribute('nowrap', 'nowrap');
        }

        if (isHeader) {
            cell.innerHTML = node.name;
            // fixme: add this back when implementing sorting
            //'<img src="' + widgetsConfig.imgBasePath +
            //'crazytable/images/thread.gif"> ';
        } else {
            var isExpandable = node.nodescount > 0;
            if (isRecursing(rowpath, nodeId)) {
                isExpandable = false;
            }
            cell.innerHTML = getHierarchyLines(source, rowpath, isExpandable) +
                             //'<font color="red">'+nodeId+'</font> '+ //uni demo
                             (isExpandable ?
                              '<a href="javascript: ;" ' +
                              'onmouseover="window.status=\'Expand/Collapse ' +
                              'this node\';return true;" ' +
                              'onmouseout="window.status=\'\';" ' +
                              'onClick="crazyTable.rowClick(\'' + source +
                              '\',\'' + rowpath + '\');return false;">' : '') +
                             node.name + (isExpandable ? '</a>' : '') +
                             '&nbsp;';

            //uni demo
            var cb = document.createElement('input');
            cb.type = 'checkbox';
            cb.id = 'ma' + rowpath;
            cb.onclick = function (event) {
                ctCache[source].maChecked[rowpath] = this.checked;
            };
            if (!ctCache[source].maChecked[rowpath]) {
                ctCache[source].maChecked[rowpath] = false;
            }
            cb.checked = ctCache[source].maChecked[rowpath];
            cell.insertBefore(cb, cell.childNodes[0]);
            //uni demo:end
        }
        row.appendChild(cell);

        //uni demo
        var span, spanContent;

        for (var i = 0; i < node.values.length; i++) {
            if (isIE) {
                // IE is silly
                cell = document.createElement('<' + cellTag +
                    (isHeader ? ' nowrap="nowrap"' : '') + '>');
            } else {
                cell = document.createElement(cellTag);
                if (isHeader) {
                    cell.setAttribute('nowrap', 'nowrap');
                }
            }
            cell.setAttribute('valign', 'top');
            //uni demo
            //cell.innerHTML = node.values[i];
            if (!isHeader && node.values[i] !== '' &&
                isDefined('editInPlace') && editInPlace.isReady) {
                span = document.createElement('span');
                span.id = 'editinplace_'+rowpath;
                span.setAttribute('editinplace', 'text');
                span.setAttribute('size', 20);
                spanContent = document.createTextNode(node.values[i]);
                span.appendChild(spanContent);
                cell.appendChild(span);
                // editInPlace widgets cannot be initialized here since they
                // haven't been added to the document yet
            } else {
                cell.innerHTML = node.values[i];
            }
            //uni demo: end
            row.appendChild(cell);
        }

        return row;
    }

    function setRowsColor(row)
    {
        if (!row) { return; } //uni demo

        var classNames = ['crazytableevenrow', 'crazytableoddrow'];
        var curIndex = ((row.className == classNames[0] ||
                         row.className == classNames[0]+'hover') ? 1 : 0);

        while ((row = row.nextSibling)) {
            row.className = classNames[curIndex];
            curIndex = 1 - curIndex;
        }
    }

    function setNotice(source, s)
    {
        // update initialization notice
        if (ctCache && ctCache[source] && ctCache[source].span) {
            var spanChildren = ctCache[source].span.childNodes;
            if (spanChildren.length > 1) {
                spanChildren[1].nodeValue = s;
            }
        }
    }

    function setNoticeFailed(source)
    {
        setNotice(source, 'Initialization failed');
    }

    function expandHierarchyRow(source, rowpath)
    {
        var curId, curRowpath, i, isLastItem, nextRow, node, nodeImg, row;
        var tempRowId, tempRowpath;

        var isNewRow; //uni demo

        var curCache = ctCache[source];
        var table = curCache.span.childNodes[0];
        var tbody = table.childNodes[0];

        if (table.rows.length === 0) {
            // aborts if table is empty and root node doesn't exists
            if (!curCache.data[rowpath]) {
                setNoticeFailed(source);
                return;
            }

            row = createRow(source, rowpath, true);
            row.className = 'crazytableheaderrow';
            tbody.appendChild(row);
            curCache.rows[rowpath] = row;

            // removes initialization notice
            curCache.span.removeChild(curCache.span.childNodes[1]);
        }

        var expandQueue = [];
        expandQueue.push(rowpath);

        while ((curRowpath = expandQueue.shift())) {
            curId = splitRowpath(curRowpath).pop();
            node = curCache.data[curId];

            if (isRecursing(curRowpath, curId)) { continue; }

            row = curCache.rows[curRowpath];

            if (!row) { continue; } //uni demo

            // special case, abort expand if node is not showing
            if (isFloating(row)) { continue; }
            nextRow = row.nextSibling;

            for (i = 0; i < node.nodes.length; i++) {
                tempRowId = node.nodes[i];
                tempRowpath = curRowpath + self.delimiter +
                              escapeDelimiter(tempRowId);

                // skips if node doesn't exist, or hasn't already been provided
                if (!curCache.data[tempRowId]) { continue; }

                isNewRow = false; //uni demo
                if (!(row = curCache.rows[tempRowpath])) {
                    row = createRow(source, tempRowpath, false);
                    curCache.rows[tempRowpath] = row;
                    isNewRow = true; //uni demo
                }

                if (isFloating(row)) {
                    if (nextRow) {
                        tbody.insertBefore(row, nextRow);
                    } else {
                        tbody.appendChild(row);
                    }
                    //uni demo
                    if (isNewRow) {
                        rowInitWidgets(row);
                    }
                    //uni demo: end
                }

                if (curCache.data[tempRowId].rows[tempRowpath].isexpanded) {
                    expandQueue.push(tempRowpath);
                }
            }

            node.rows[curRowpath].isexpanded = true;

            // if any row other than the root
            if (splitRowpath(curRowpath).length > 1) {
                nodeImg = document.getElementById('hl' + curRowpath);
                if (nodeImg) {
                    isLastItem = isLastNode(source, curRowpath);
                    nodeImg.src = widgetsConfig.imgBasePath +
                                  'crazytable/images/' +
                                  getImageName(true, isLastItem, true, true);
                }
            }
        }

        setRowsColor(curCache.rows[rowpath]);
    }

    function collapseHierarchyRow(source, rowpath)
    {
        var curNode, curNodeId, curRowpathLength, curRowpathParts, imgSet;
        var isLastItem, nextRow, nodeImg;

        var curCache = ctCache[source];
        var table = curCache.span.childNodes[0];
        var tbody = table.childNodes[0];

        var rowpathParts = splitRowpath(rowpath);
        var rowpathLength = rowpathParts.length;
        var nodeId = rowpathParts.pop();
        var node = curCache.data[nodeId];

        var row = curCache.rows[rowpath].nextSibling;
        if (!row) { return; }
        curRowpathParts = splitRowpath(row.id);
        curRowpathLength = curRowpathParts.length;
        curNodeId = curRowpathParts.pop();
        curNode = curCache.data[curNodeId];

        var widgetId; //uni demo
        while (row && curRowpathLength > rowpathLength) {
            nodeImg = document.getElementById('hl' + row.id);
            isLastItem = isLastNode(source, row.id);
            imgSet = false;
            if (!widgetsConfig.crazyTable.hierarchyKeepExpandState &&
                curNode.nodescount > 0) {
                curNode.rows[row.id].isexpanded = false;
                if (nodeImg) {
                    nodeImg.src = widgetsConfig.imgBasePath +
                                  'crazytable/images/' +
                                  getImageName(true, isLastItem, true, false);
                    imgSet = true;
                }
            }

            // removes hourglass before hiding row
            if (!imgSet && nodeImg &&
                nodeImg.src.substr(-13) == 'hourglass.gif') {
                nodeImg.src = widgetsConfig.imgBasePath + 'crazytable/images/' +
                    getImageName(true, isLastItem, true, false);
            }

            //uni demo
            if (isDefined('editInPlace') && editInPlace.isReady) {
                widgetId = 'editinplace_'+row.id;
                if (document.getElementById('edit'+widgetId)) {
                    editInPlace.showView(widgetId);
                }
            }
            //uni demo:end

            nextRow = row.nextSibling;
            tbody.removeChild(row);
            row = nextRow;
            if (row) {
                curRowpathParts = splitRowpath(row.id);
                curRowpathLength = curRowpathParts.length;
                curNodeId = curRowpathParts.pop();
                curNode = curCache.data[curNodeId];
            }
        }

        node.rows[rowpath].isexpanded = false;

        // if any row other than the root
        if (rowpathLength > 1) {
            nodeImg = document.getElementById('hl' + rowpath);
            if (nodeImg) {
                isLastItem = isLastNode(source, rowpath);
                nodeImg.src = widgetsConfig.imgBasePath + 'crazytable/images/' +
                              getImageName(true, isLastItem, true, false);
            }
        }

        setRowsColor(curCache.rows[rowpath]);
    }

    this.init = function ()
    {
        var source, tbody, table, thisSpan;
        var searchString; //uni demo

        if (widgetsConfig.crazyTable.dataScript === '') { return; }

        var spans = document.getElementsByTagName("span");

        if (spans.length < 1) { return; }
        for (var i = 0; i < spans.length; i++) {
            thisSpan = spans[i];
            source = thisSpan.getAttribute('crazytable');
            if (source !== null) {
                thisSpan.setAttribute('id', 'crazytable' + source);
                table = document.createElement('TABLE');
                tbody = document.createElement('TBODY');
                table.appendChild(tbody);
                table.cellSpacing = 2;
                table.cellPadding = 1;
                table.border = 0;
                thisSpan = clearDomObj(thisSpan);
                thisSpan.appendChild(table);

                thisSpan.appendChild(
                    document.createTextNode('Initializing...'));

                ctCache[source] = {};
                ctCache[source].span = thisSpan;
                ctCache[source].data = {};
                ctCache[source].rows = {};

                //uni demo, this line and all other "locked" related lines
                ctCache[source].locked = false;

                //uni demo
                // "ma" stands for Multiple Action, i.e. checkboxes beside each nodes
                ctCache[source].maChecked = {};
                ctCache[source].searchString = '';
                if ((searchString = thisSpan.getAttribute('searchstring')) &&
                    searchString !== '') {
                    ctCache[source].searchString = searchString;
                }

                sendHierarchyDataRequest(source, 0);

                //uni demo
                setTimeout("crazyTable.sendDynamicRequest('" +
                    escapeQuotes(source) + "')", self.dyndelay); // 10 seconds
            }
        }
    };

    this.rowClick = function (source, rowpath)
    {
        if (!ctCache[source]) { return; }

        var nodeId = splitRowpath(rowpath).pop();
        var node = ctCache[source].data[nodeId];

        if (node.rows[rowpath].isexpanded) {
            while (ctCache[source].locked){}
            ctCache[source].locked = true;
            collapseHierarchyRow(source, rowpath);
            ctCache[source].locked = false;
        } else if (node.nodescount > node.nodes.length) {
            var nodeImg = document.getElementById('hl' + rowpath);
            if (nodeImg) {
                nodeImg.src = widgetsConfig.imgBasePath +
                              'crazytable/images/hourglass.gif';
            }
            sendHierarchyDataRequest(source, nodeId, rowpath);
        } else {
            while (ctCache[source].locked){}
            ctCache[source].locked = true;
            var tempData = {'id':node.id, 'nodes':node.nodes,
                            'nodescount':node.nodescount};
            setRow(source, tempData, rowpath, {});
            expandHierarchyRow(source, rowpath);
            ctCache[source].locked = false;
        }
    };

    this.updateHierarchyData = function (returnedData)
    {
        var i;

        var data = JSON.parse(returnedData);

        // data validation
        if (!isObject(data) || !data.source) { return; }
        if (!ctCache[data.source] || !data.expandednodes ||
            !data.nodes || !isArray(data.nodes) || !data.rowpath) {
            setNoticeFailed(data.source);
            return;
        }

        // nodes validation
        for (i = data.nodes.length-1; i >= 0; i--) {
            if (!data.nodes[i].id) { delete data.nodes[i]; }
            if (!data.nodes[i].name) { data.nodes[i].name = 'Unnamed'; }
            if (!data.nodes[i].values || !isArray(data.nodes[i].values)) {
                data.nodes[i].values = [];
            }
            if (!data.nodes[i].nodes || !isArray(data.nodes[i].nodes)) {
                data.nodes[i].nodes = [];
            }
            if (!data.nodes[i].nodescount ||
                toInteger(data.nodes[i].nodescount) === null) {
                data.nodes[i].nodescount = data.nodes[i].nodes.length;
            }
        }

        while (ctCache[data.source].locked){}
        ctCache[data.source].locked = true;

        for (i = 0; i < data.nodes.length; i++) {
            setNode(data.source, data.nodes[i]);
        }

        var expandedNodesHash = {};
        var node, nodeId;
        for (i = 0; i < data.expandednodes.length; i++) {
            nodeId = data.expandednodes[i];
            node = ctCache[data.source].data[nodeId];
            if (node && node.nodes.length > 0) {
                expandedNodesHash[nodeId] = true;
            }
        }

        if (data.nodes.length > 0) {
            setRow(data.source, data.nodes[0], data.rowpath,
                   expandedNodesHash);
            expandHierarchyRow(data.source, data.rowpath);
        }

        ctCache[data.source].locked = false;
    };


    // The following methods were added for the UniConf browser prototype, and
    // are intentionnaly not well designed. They MUST be revised eventually.

    function resetRows(source, nodeData)
    {
        var rowpathParts;

        var d = self.delimiter;
        var nodeId = escapeDelimiter(nodeData.id);
        var regExp = new RegExp('(^|'+d+')'+nodeId+'$');
        var parents = [];
        for (var rowpath in ctCache[source].rows) {
            if (rowpath.search(regExp) != -1) {
                var tempData = {'id':nodeData.id, 'nodes':nodeData.nodes,
                                'nodescount':nodeData.nodescount};
                setRow(source, tempData, rowpath, {});

                rowpathParts = splitRowpath(rowpath);
                rowpathParts.pop();
                parents.push(rowpathParts.pop());
            }
        }
        return parents;
    }

    function dynUpdateNode(source, nodeData)
    {
        var node = ctCache[source].data[nodeData.id];
        if (!node) { return; }

        node.name = nodeData.name;
        node.values = nodeData.values;

        if (node.nodes.length > 0 && nodeData.nodes.length === 0 &&
            nodeData.nodescount > 0) {
            nodeData.nodes = node.nodes;
            nodeData.nodescount = node.nodescount;
        } else {
            node.nodes = nodeData.nodes;
            node.nodescount = nodeData.nodescount;
        }

        var redrawNodes = resetRows(source, nodeData);

        return redrawNodes;
    }

    function doRedrawNodes(source, redrawNodes)
    {
        var i, node, nodeId, regExp1, regExp2, rowpath;

        var redrawRowpath = [];
        var deleteRowpath = [];
        var d = self.delimiter;
        for (i = 0; i < redrawNodes.length; i++) {
            node = ctCache[source].data[redrawNodes[i]];
            nodeId = escapeDelimiter(redrawNodes[i]);
            regExp1 = new RegExp('(^|'+d+')'+nodeId+'$');
            regExp2 = new RegExp('(^|'+d+')'+nodeId+d);
            for (rowpath in ctCache[source].rows) {
                if (rowpath.search(regExp1) != -1) {
                    if (node.rows[rowpath].isexpanded) {
                        redrawRowpath.push(rowpath);
                    }
                    var tempData = {'id':node.id, 'nodes':node.nodes,
                                    'nodescount':node.nodescount};
                    setRow(source, tempData, rowpath, {});
                } else if (rowpath.search(regExp2) != -1) {
                    deleteRowpath.push(rowpath);
                }
            }
        }

        for (i = 0; i < redrawRowpath.length; i++) {
            collapseHierarchyRow(source, redrawRowpath[i]);
        }
        for (i = 0; i < deleteRowpath.length; i++) {
            delete ctCache[source].rows[deleteRowpath[i]];
        }
        for (i = 0; i < redrawRowpath.length; i++) {
            expandHierarchyRow(source, redrawRowpath[i]);
        }
    }

    function dynamicUpdate(data)
    {
        while (ctCache[data.source].locked){}
        ctCache[data.source].locked = true;

        var i, j, node, temp;

        var nodesHash = {};
        for (i = 0; i < data.nodes.length; i++) {
            node = data.nodes[i];
            if (ctCache[data.source].data[node.id]) {
                temp = dynUpdateNode(data.source, node);
                for (j = 0; j < temp.length; j++) {
                    if (temp[j] === undefined) { temp[j] = '0'; }
                    nodesHash[temp[j]] = 1;
                }
            } else {
                setNode(data.source, node);
            }
        }

        var nodes = [];
        for (i in nodesHash) {
            nodes.push(i);
        }

        doRedrawNodes(data.source, nodes);

        ctCache[data.source].locked = false;
    }

    this.dynamicRequestUpdate = function(returnedData)
    {
        if (returnedData === '') { return; }

        var data = JSON.parse(returnedData);
        if (!isObject(data) || !data.source) { return; }

        // do not update nodes while in edit mode
        if (isDefined('editInPlace') && editInPlace.isReady &&
            editInPlace.editModeCount > 0) {
            return;
        }

        dynamicUpdate(data);
    };

    this.sendDynamicRequest = function (source)
    {
        var node, nodeId;

        var requestObj = getRequestObject();

        var nodes = [];
        for (var rowpath in ctCache[source].rows) {
            nodeId = splitRowpath(rowpath).pop();
            node = ctCache[source].data[nodeId];
            if (node && node.nodescount > 0 && node.nodes.length > 0) {
                nodes.push(escape(nodeId));
            }
        }

        var url = widgetsConfig.crazyTable.dataScript +
                  '?source=' + escape(source) +
                  '&nodes=' + nodes.join(',');
        //uni demo
        if (ctCache[source].searchString !== '') {
            url += '&search=' + escape(ctCache[source].searchString);
        }
        //uni demo: end

        var completeHandler = function () {
            setTimeout("crazyTable.sendDynamicRequest('" + escapeQuotes(source) +
                "')", self.dyndelay);
        };

        requestObj.send(url, self.dynamicRequestUpdate,
                        completeHandler, completeHandler);
    };

    this.getCheckedNodesIds = function (source)
    {
        var a = [];
        for (var rowpath in ctCache[source].rows) {
            if (isFloating(ctCache[source].rows[rowpath])) { continue; }
            if (ctCache[source].maChecked[rowpath]) {
                a.push(splitRowpath(rowpath).pop());
            }
        }
        return a;
    };

    this.clearCheckedNodesIds = function (source)
    {
        var cb;
        for (var rowpath in ctCache[source].maChecked) {
            cb = document.getElementById('ma'+rowpath);
            if (cb) {
                cb.checked = false;
            }
        }
        ctCache[source].maChecked = {};
    };

    function rowInitWidgets(row)
    {
        var i, spans;
        if (isDefined('editInPlace') && editInPlace.isReady) {
            spans = row.getElementsByTagName('span');
            for (i = 0; i < spans.length; i++) {
                editInPlace.widgetInit(spans[i]);
            }
        }
    }

    this.getEditInPlaceValues = function (source)
    {
        var inputs = ctCache[source].span.getElementsByTagName('input');
        var a = [];
        var temp, thisInput;
        for (var i = 0; i < inputs.length; i++) {
            thisInput = inputs[i];
            if (thisInput.name.substr(0, 12) == 'editinplace_') {
                temp = {};
                temp.key = splitRowpath(thisInput.name).pop();
                temp.value = thisInput.value;
                a.push(temp);
            }
        }
        return a;
    };

    this.cancelEditInPlaceWidgets = function (source)
    {
        var thisInput;
        var inputs = ctCache[source].span.getElementsByTagName('input');
        for (var i = 0; i < inputs.length; i++) {
            thisInput = inputs[i];
            if (thisInput.name.substr(0, 12) == 'editinplace_') {
                editInPlace.showView(thisInput.name);
            }
        }
    };


}

var crazyTable = new CrazyTable();

if (crazyTable.isReady) {
    // initialization function called on window.onLoad event, see shared.js
    _onLoad.push('crazyTable.init()');
}
