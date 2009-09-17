
var buttercupVoice = null;
var buttercupAppVoice = null;
var isButtercupVoicePaused = false;

try {
    buttercupVoice = new ActiveXObject("Sapi.SpVoice");
    buttercupAppVoice = new ActiveXObject("Sapi.SpVoice");
}
catch (ex) {
    //Could not load SAPI or SAPI not available
}

//Determines whether SAPI is able to be used.
IsSapiEnabled = function() {
    if (buttercupVoice) {
        return true;
    }

    return false;
}

if (IsSapiEnabled())
{
    //SAPI is Enabled, so load the rest of the SAPI functions
    document.write("<script src='Scripts/SAPI.js'><\/script>");
}