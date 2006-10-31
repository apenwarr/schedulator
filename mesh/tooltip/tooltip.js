/**
 * ToolTip
 *
 * Author:
 *   Michel Emond
 *   Net Integration Technologies
 *   www.net-itech.com
 *
 * Description:
 *   Allows to easily set a tooltip text to show up when the mouse cursor passes
 *   over some text. Can also be used to hide away (but keep available) chunks
 *   of text that would otherwise clutter the screen.
 *
 * Dependencies:
 *   shared.js
 *   tooltip.css
 *
 * Global objects:
 *   ToolTip   Constructor method
 *   toolTip   ToolTip object
 *
 * Syntax:
 *   In order to activate ToolTip on a chunk of text, surround the text with a
 *   span, having the following attributes:
 *
 *     tooltip:
 *       Indicates the label text that the mouse cursor has to pass over in
 *       order to trigger the tooltip to show up
 *
 *     color:
 *       Specifies the color of the label and the tooltip, can be any standard
 *       html color value (#C0FFC0, #07F, blue, pink, etc.) This attribute is
 *       optional and defaults to #FFC (pale yellow).
 *
 *   Restrictions:
 *     - Although html is allowed inside the span, it should be used as little
 *       as possible since lines automatically wraps around words at a maximum
 *       of 75 chars and html tags might end up broken in pieces.
 *     - ToolTip has been designed to work hand in hand with EditInPlace. Using
 *       DynamicStatus on top of EditInPlace with ToolTip is also possible as
 *       EditInPlace provides the connection between the two other libraries.
 *       Using ToolTip in other widgets combinations is not recommended as it
 *       could lead to unpredictable results.
 *
 * Example:
 *   <span tooltip="My tooltip" color="#CFC">Lorem ipsum dolor sit amet.</span>
 */

/*global Event, clearTimeout, document, setTimeout, trim, window, wordWrap */

function ToolTip()
{
    var self = this;
    this.isReady = true;

    if (!document.getElementById || !document.getElementsByTagName) {
        this.isReady = false;
        return;
    }

    var defaultColor = '#ffc';
    var isIE = document.all ? true : false;

    var netX, netY; // used when browser is not IE
    var activeTimeout;

    var text = '';
    var currentId = null;
    var color = '';
    var xpos = 0;
    var ypos = 0;

    function posX()
    {
        var tempX = document.body.scrollLeft + window.event.clientX;
        if (tempX < 0) { tempX = 0; }
        return tempX;
    }

    function posY()
    {
        var tempY = document.body.scrollTop + window.event.clientY;
        if (tempY < 0) { tempY = 0; }
        return tempY;
    }

    this.init = function ()
    {
        if (!isIE) {
            document.captureEvents(Event.MOUSEMOVE);
            document.onmousemove = self.mousePos;
        }

        var toolTipDiv = document.createElement('div');
        toolTipDiv.id = 'tooltipwidgetpopupdiv';
        toolTipDiv.style.position = 'absolute';
        toolTipDiv.style.visibility = 'hidden';
        document.body.appendChild(toolTipDiv);

        var spans = document.getElementsByTagName('span');
        for (var i = 0; i < spans.length; i++) {
            // spans combining editinplace and tooltip must be handled by
            // editinplace which will then call tooltip
            if (spans[i].getAttribute('editinplace') === null) {
                self.widgetInit(spans[i]);
            }
        }
    };

    this.mousePos = function (event)
    {
        netX = event.pageX;
        netY = event.pageY;
    };

    this.widgetInit = function (thisSpan, useNoStyle, overrideText)
    {
        if (thisSpan.getAttribute('tooltipinit') !== null) { return; }

        var label = thisSpan.getAttribute('tooltip');
        if (label === null) { return; }

        if (useNoStyle === undefined) { useNoStyle = false; }

        // avoid multiple initializations
        thisSpan.setAttribute('tooltipinit', 1);

        // tooltip may be disabled/enabled
        thisSpan.setAttribute('tooltipenabled', 1);

        var useText = ((overrideText === undefined) ?
            thisSpan.innerHTML : overrideText);
        thisSpan.setAttribute('tooltiptext', wordWrap(useText));

        label = trim(label);
        if (label === '') { label = 'ToolTip'; }
        thisSpan.innerHTML = label;

        var color = thisSpan.getAttribute('tooltipcolor');
        if (color === null) {
            color = defaultColor;
            thisSpan.setAttribute('tooltipcolor', color);
        }

        if (!useNoStyle) {
            thisSpan.style.backgroundColor = color;
            thisSpan.className = 'tooltip';
        }

        thisSpan.onmouseover = function (event)
        {
            if (this.getAttribute('tooltipenabled') == 1) {
                self.link(this);
                return true;
            }
        };
        thisSpan.onmouseout = function (event)
        {
            self.close();
        };
    };

    this.enable = function (spanObj)
    {
        if (spanObj.getAttribute('tooltipinit') !== null) {
            spanObj.setAttribute('tooltipenabled', 1);
        }
    };

    this.disable = function (spanObj)
    {
        spanObj.setAttribute('tooltipenabled', 0);
        self.close();
    };

    this.link = function (obj)
    {
        currentId = obj.id;
        text = '<div class="tooltip">' + obj.getAttribute('tooltiptext') +
               '</div>';
        color = obj.getAttribute('tooltipcolor');
        if (isIE) {
            xpos = posX();
            ypos = posY();
        } else {
            xpos = netX;
            ypos = netY;
        }
        activeTimeout = setTimeout('toolTip.show()'+'', 300);
    };

    this.show = function ()
    {
        if (xpos < 1) { xpos = 1; }
        if (ypos < 1) { ypos = 1; }

        var toolTipDiv;
        if (isIE) {
            toolTipDiv = document.all.tooltipwidgetpopupdiv;
        } else {
            toolTipDiv = document.getElementById('tooltipwidgetpopupdiv');
        }

        toolTipDiv.style.visibility = 'visible';
        toolTipDiv.style.backgroundColor = color;
        toolTipDiv.style.left = xpos + 'px';
        toolTipDiv.style.top = ypos + 'px';
        toolTipDiv.innerHTML = text;
    };

    this.close = function (requestId)
    {
        if (requestId && requestId != currentId) {
            return;
        }
        currentId = null;

        var toolTipDiv;
        if (isIE) {
            toolTipDiv = document.all.tooltipwidgetpopupdiv;
        } else {
            toolTipDiv = document.getElementById('tooltipwidgetpopupdiv');
        }
        toolTipDiv.style.visibility = 'hidden';
        toolTipDiv.innerHTML = '';

        clearTimeout(activeTimeout);
    };
}

var toolTip = new ToolTip();

if (toolTip.isReady) {
    // initialization function called on window.onLoad event, see shared.js
    _onLoad.push('toolTip.init()');
}
