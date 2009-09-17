NotifyAppSpeechCompleted = function()
{
    //try/catch is just to avoid javascript error when silverlight/activex objects load
    //out of intended order (i.e. for slower machines)...
    //TODO handle this better than a blind try/catch
    try
    {
        buttercupPlayer.Content.ButtercupAppVoice.PlayAppSpeechCompleted();
    } catch (ex) {
    
    }
}

//Raised when the SAPI ActiveX control has finished speaking the current phrase.
NotifySpeechCompleted = function()
{
    //try/catch is just to avoid javascript error when silverlight/activex objects load
    //out of intended order (i.e. for slower machines)...
    //TODO handle this better than a blind try/catch
    try
    {
        buttercupPlayer.Content.ButtercupVoice.PlaySpeechCompleted();
    } catch (ex) {
    
    }
}


function buttercupPluginLoaded(sender, args) 
{
    buttercupPlayer = sender.getHost();
	
    try 
    {

	    if(buttercupVoice)
	    {
		    function buttercupVoice::EndStream() 
		    {
			    // If the InputWordLength is zero it means we have actually finished playing
			    // the "Stop" phrase. In this case, we don't want to raise a notification 
			    // event.
			    if (buttercupVoice.Status.InputWordLength > 0)
			    {
				    NotifySpeechCompleted();
			    }
		    }

		    function buttercupAppVoice::EndStream() 
		    {
			    // If the InputWordLength is zero it means we have actually finished playing
			    // the "Stop" phrase. In this case, we don't want to raise a notification 
			    // event.
			    if (buttercupAppVoice.Status.InputWordLength > 0)
			    {
				    NotifyAppSpeechCompleted();
			    }
		    }
	    }
    }
    catch (ex)
    {
	    alert('Failed to define EndStream event on SAPI voice ActiveX object.'); 
    }
}

//Set the volume for SAPI ActiveX control
SetVolume = function(volume)
{
    if (buttercupVoice && buttercupAppVoice)
    {
	    buttercupVoice.Volume = volume * 100; //volume for SAPI is between 0 and 100
	    buttercupAppVoice.Volume = volume * 100; //volume for SAPI is between 0 and 100
    }
}

//Set the speaking rate of the SAPI ActiveX control
SetRate = function(rate)
{
    if (buttercupVoice && buttercupAppVoice)
    {
	    buttercupVoice.Rate = rate; //rate is an integer between -10 and 10
	    buttercupAppVoice.Rate = rate; //rate is an integer between -10 and 10
    }
}

//Starts playing the given text with SAPI via ActiveX
PlaySpeech = function(text)
{            
    // If we have asked to Play then Stop the existing talking and, in case, we
    // are in a Paused state call Resume.
    StopSpeech();
    ResumeSpeech();
	
    //SVSFlagsAsync (1) | SVSFIsNotXML (16) 
    var SVSFlags = 17;
    if (buttercupVoice) {
	    buttercupVoice.Speak(text, SVSFlags);
    }
}

//Starts playing the given text with SAPI via ActiveX
PlayHighPrioritySpeech = function(text)
{            
    //Stop application voice if it is playing.
    if (buttercupAppVoice) {
	    buttercupAppVoice.Speak("", 3); //Apparently, this is the way to stop SAPI.

	    //SVSFlagsAsync (1) | SVSFPurgeBeforeSpeak (2) | SVSFIsNotXML (16) 
	    var SVSFlags = 19;
	    buttercupAppVoice.Speak(text, SVSFlags);
    }
}

//Stops the speech in SAPI via ActiveX
StopHighPrioritySpeech = function()
{
    //SVSFPurgeBeforeSpeak (2) 
    if (buttercupAppVoice) {
	    buttercupAppVoice.Speak("", 3); //Apparently, this is the way to stop SAPI.
    }
}

//Pauses the speech in SAPI via ActiveX

PauseSpeech = function()
{
    if(!isButtercupVoicePaused)
    {
	    if (buttercupVoice) {
		    buttercupVoice.Pause(); 
	    }
	    isButtercupVoicePaused = true;
    }
	
}


//Stops the speech in SAPI via ActiveX
StopSpeech = function()
{
    //SVSFPurgeBeforeSpeak (2) 
    if (buttercupVoice) {
	    buttercupVoice.Speak("", 3); //Apparently, this is the way to stop SAPI.
    }
}


//Resumes playback of the speech in SAPI via ActiveX
ResumeSpeech = function()
{
    if(isButtercupVoicePaused)
    {
	    if (buttercupVoice) {
		    buttercupVoice.Resume();
		    isButtercupVoicePaused = false;
	    }
    }
}