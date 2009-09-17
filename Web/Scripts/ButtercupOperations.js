
var buttercupPlayer = null;

IsIE = function() {
    return (document.recalc); // IE 5+
}

function buttercupPluginLoaded(sender, args) {
    buttercupPlayer = sender.getHost();
}

// Scrolls the content panel to the element with the given index.
ScrollToElement = function(elementID)
{
    var contentDiv = document.getElementById('ContentPanel');
    var elementToFocus = document.getElementById(elementID);

    if (contentDiv && elementToFocus)
    {
        var elementOffset = elementToFocus.offsetTop - elementToFocus.offsetHeight / 2;
        
        if (elementOffset)
        {
            if (elementOffset < 0) elementOffset = 0;
            contentDiv.scrollTop = elementOffset;
        }
    }
}

XamlTransformationComplete = function(param)
{
    try
    {
        buttercupPlayer.Content.XamlSurface.XamlTransformationComplete(param);
    } catch (ex)
    {
        alert('ERROR! ' + ex.message);
        alert(buttercupPlayer);
        alert(buttercupPlayer.Content);
        alert(buttercupPlayer.Content.XamlSurface);
    }
}

TransformFragmentToXaml = function(fragment, targetID) {
    new Transformation().setXml(fragment).setCallback(XamlTransformationComplete)
        .setXslt("/dtbook2xaml.xsl").transform(targetID);
}


AssignFragment = function(param)
{
    buttercupPlayer.Content.ButtercupSurface.DisplayBookCompleted();
}

// Performs an XSL transformation. This function is dependent on Johann Burkard's xslt.js file
// that contains cross browser support for xsl transformations.
// Arguemnts,
//      fragment:           full path OR string that represents the XML to be transformed.
//      targetID:           (optional) the id of the HTML element into which the resulting transformation should 
//                          be insered. If null, result is passed to the callback function.
TransformFragment = function(fragment, targetID)
{
    new Transformation().setXml(fragment).setCallback(AssignFragment)
        .setXslt("/dtbook2xhtml.xsl").transform(targetID);

    var contentDiv = document.getElementById(targetID);
    if (contentDiv)
        return contentDiv.innerHTML;
    else
        return '';
}


//Determines whether the browser supports the SAPI ActiveX control.
//IsSapiEnabled = function() {
    //return IsIE();
//}


