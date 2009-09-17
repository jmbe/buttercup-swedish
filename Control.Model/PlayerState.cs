using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Buttercup.Control.Common;
using Buttercup.Control.Common.IO;
using Buttercup.Control.Common.Net;
using Buttercup.Control.Model.Entities;


namespace Buttercup.Control.Model
{
	public class PlayerState
	{
		#region Fields (9)

		//The active SMIL document and reference (cached to determine whether the current phrase has changed).
		private SmilDocument _activeSmilDocument;
		private string _activeSmilReference;
		private bool _canResumePhrase;
		private PhraseConstructionKit _currentPhraseConstruction;
		private string _currentRequestedFileUri;
		private readonly ApplicationState _mainState;
		private static readonly Regex _smilRefExtractor;
		private static string _smilRefExtractorPattern = @"([^#\n\r\t]+)#(\w+)";
		private static readonly Regex _tagIdentifier;
		private static string _tagIdentifierPattern = @"<[^>]*>";

		#endregion Fields



		#region Constructors (2)

		public PlayerState(ApplicationState mainState)
		{
			_mainState = mainState; //Need to be able to access application-level information.
			_currentRequestedFileUri = String.Empty;

			//When the application loads, no book is loaded, therefore the player must be disabled.
			CurrentMode = PlayerMode.Disabled;
		}



		static PlayerState()
		{
			_smilRefExtractor = new Regex(_smilRefExtractorPattern, RegexOptions.None);
			_tagIdentifier = new Regex(_tagIdentifierPattern, RegexOptions.None);
		}

		#endregion Constructors



		#region Properties (5)

		/// <summary>
		/// Gets a value indicating whether the player can play content from the loaded book.
		/// </summary>
		/// <value>
		/// True if content can be played, otherwise false.
		/// </value>
		public bool CanPlay
		{
			get
			{
				return (CurrentMode == PlayerMode.Playing || CurrentMode == PlayerMode.Paused
					|| CurrentMode == PlayerMode.Initialised);
			}
		}

		/// <summary>
		/// Gets or sets the current mode of the player.
		/// </summary>
		/// <value>The current mode of the player.</value>
		public PlayerMode CurrentMode { get; set; }

		/// <summary>
		/// Gets a value indicating whether the player is currently playing a book.
		/// </summary>
		/// <value>
		/// True if the player is currently playing a book, otherwise false.
		/// </value>
		public bool IsPlaying
		{
			get { return CurrentMode == PlayerMode.Playing; }
		}

		/// <summary>
		/// Gets the navigator used to navigate the current book.
		/// </summary>
		/// <value>The navigator.</value>
		public BookNavigator Navigator { get; internal set; }

		/// <summary>
		/// Gets the method to use to present a newly-constructed phrase.
		/// </summary>
		public PresentPhraseMethod PresentPhrase { get; set; }

		#endregion Properties



		#region Delegates and Events (1)

		// Delegates (1) 

		public delegate void PresentPhraseMethod(Phrase currentPhrase, bool canResumePhrase);

		#endregion Delegates and Events



		#region Methods (8)

		/// <summary>
		/// Returns the number of pages in the current book loaded.
		/// </summary>
		/// <returns></returns>
		public int GetNumberOfPages()
		{
			if(_mainState != null)
			{
				return _mainState.CurrentBook.PageList.Pages.Count;
			}
			return -1;
		}



		public void GetCurrentPhraseAsync(PlayerMode previousState)
		{
			_currentPhraseConstruction = new PhraseConstructionKit();
			Phrase currentPhrase = _currentPhraseConstruction.ConstructedPhrase;
			currentPhrase.Text = GetSpeakableText();
			currentPhrase.ElementID = Navigator.CurrentElementID;

			Book currentBook = _mainState.CurrentBook;
			string bookPath = currentBook.FolderPath.FullName;

			//Get SMIL path from book XML
			string smilPath;
			string smilReferenceID;

			string currentSmilReference = GetCurrentSmilReference();

			_canResumePhrase = previousState == PlayerMode.Paused;

			//We've been given an element with no SMIL Reference, aka Unspeakable
			//Since the navigation methods all only navigate speakable elements, this
			//only results from clicking on an element on the surface
			if(currentSmilReference == null)
			{
				XAttribute hasAltText = Navigator.CurrentElement.Attribute("alt");
				if(hasAltText != null)
				{
					currentPhrase.Text = "Image. " + hasAltText.Value;
				}
				else
				{
					currentPhrase.Text = "......"; //Could be anything, but each character represents a fixed length of silence
					currentPhrase.Silent = true;
				}

				if(PresentPhrase != null)
				{
					//Cannot resume - just speak.
					PresentPhrase(currentPhrase, false);
				}

				return;
			}

			DeconstructSmilReference(currentSmilReference, out smilPath, out smilReferenceID);
			smilPath = _mainState.BookFileSystem.CombinePath(bookPath, smilPath);
			_currentPhraseConstruction.SmilPath = smilPath;
			_currentPhraseConstruction.SmilReferenceID = smilReferenceID;

			//Avoid repeat loading of previous SMIL document
			SmilDocument currentSmilDocument = _activeSmilDocument;
			if(currentSmilDocument == null || currentSmilDocument.SmilPath != smilPath)
			{
				//Need to coerce the expected file next to be received.
				_currentRequestedFileUri = smilPath; 

				//Get corresponding SMIL file from file system
				IFile smilFile = _mainState.BookFileSystem.GetFile(smilPath);

				//Exception thrown if file doesn't exist.
				//Stream smilStream = smilFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
				smilFile.OpenAsyncComplete += GetSmilFileComplete;
				smilFile.OpenAsync(_currentPhraseConstruction);
			}
			else
			{
				GetPhraseAudio(_currentPhraseConstruction);
			}
		}



		/// <summary>
		/// Constructs SmilDocument and sets it as active from the SMIL file contents.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GetSmilFileComplete(object sender, DownloadCompleteEventArgs e)
		{
			//Remove the event
			//This prevents a file from getting opened twice causing an exception when trying to read
			//the stream of the document since the file has already been opened and is currently stored 
			//in memory.
			((IFile)sender).OpenAsyncComplete -= GetSmilFileComplete;

			PhraseConstructionKit phraseConstruction = e.UserState as PhraseConstructionKit;

			XDocument smilXml;
			try
			{
				e.Result.Seek(0, SeekOrigin.Begin);
				smilXml = XDocument.Load(e.Result, LoadOptions.SetBaseUri);
			}
			catch(Exception ex)
			{
				Logger.Log("Stream length = " + e.Result.Length + "; Error: " + ex);
				throw;
			}

			SmilDocument currentSmilDocument = e.UserState as SmilDocument;

			//Load into SmilDocument object
			currentSmilDocument = new SmilDocument(phraseConstruction.SmilPath, smilXml);
			_activeSmilDocument = currentSmilDocument;

			//Bail out if this isn't the expected file or SMIL reference.
			if(phraseConstruction == null
					|| _currentRequestedFileUri != phraseConstruction.SmilPath
					|| _currentPhraseConstruction.SmilReferenceID != phraseConstruction.SmilReferenceID)
				return;

			GetPhraseAudio(phraseConstruction);
		}



		/// <summary>
		/// Asynchronously loads the audio for the current phrase.
		/// </summary>
		/// <param name="smilReferenceID"></param>
		private void GetPhraseAudio(PhraseConstructionKit phraseConstruction)
		{
			//Determine whether current phrase has changed (to determine whether the 'new' phrase
			// should be started or resumed).
			string currentSmilReference = String.Format("{0}#{1}", phraseConstruction.SmilPath,
					phraseConstruction.SmilReferenceID);
			_canResumePhrase = _canResumePhrase && ( currentSmilReference == _activeSmilReference );
			_activeSmilReference = currentSmilReference;
	
			//Extract SMIL object to get media file path, start & stop times.
			XElement audioElement = _activeSmilDocument.GetAudioElementBySmilID(phraseConstruction.SmilReferenceID);
			phraseConstruction.AudioElement = audioElement;
			if(audioElement != null)
			{
				XAttribute audioSrc = audioElement.Attribute("src");
				if(audioSrc != null)
				{
					string audioFilePath = audioSrc.Value;
					audioFilePath = _mainState.BookFileSystem.CombinePath
							(_mainState.CurrentBook.FolderPath.FullName, audioFilePath);

					Logger.Log("OpenAsync Audio File {0}", audioFilePath);

					//Need to coerce the expected file next to be received.
					_currentRequestedFileUri = audioFilePath;
					phraseConstruction.AudioFilePath = audioFilePath;
					
					IFile currentFile = _mainState.BookFileSystem.GetFile(audioFilePath);
					currentFile.OpenAsyncComplete += GetPhraseAudioComplete;
					currentFile.OpenAsync(phraseConstruction);
				}
			}
			else
			{
				if(PresentPhrase != null)
				{
					PresentPhrase(phraseConstruction.ConstructedPhrase, _canResumePhrase);
				}
			}
		}



		/// <summary>
		/// Constructs the audio clip for the current phrase to be played by the player,
		/// when it is finished the phrase is delegated to the playerpresenter for playback.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GetPhraseAudioComplete(object sender, DownloadCompleteEventArgs e)
		{
			//Remove the event
			//This prevents a file from getting opened twice causing an exception when trying to read
			//the stream of the document since the file has already been opened and is currently stored 
			//in memory.
			( (IFile)sender ).OpenAsyncComplete -= GetPhraseAudioComplete;

			PhraseConstructionKit phraseConstruction = e.UserState as PhraseConstructionKit;

			//Bail out if this isn't the expected file or SMIL reference.
			if(phraseConstruction == null
					|| _currentRequestedFileUri != phraseConstruction.AudioFilePath
					|| _currentPhraseConstruction.SmilReferenceID != phraseConstruction.SmilReferenceID )
				return;

			XElement audioElement = phraseConstruction.AudioElement;
			string audioClipStart = audioElement.Attribute("clipBegin").Value;
			string audioClipEnd = audioElement.Attribute("clipEnd").Value;

			Logger.Log("CompletedAsync Audio File {0}", audioElement.Attribute("src").Value);

			Phrase currentPhrase = phraseConstruction.ConstructedPhrase;
			currentPhrase.Audio = new Audio
			{
				SourceStream = e.Result,
				ClipStart = ValueConversionHelper.GetConvertedTimeSpan(audioClipStart),
				ClipEnd = ValueConversionHelper.GetConvertedTimeSpan(audioClipEnd)
			};

			if(PresentPhrase != null)
			{
				PresentPhrase(currentPhrase, _canResumePhrase);
			}
		}


		private void DeconstructSmilReference(string rawSmilReference, out string smilPath, out string smilReferenceID)
		{
			//The current element is guaranteed to have the SMIL reference attribute, since it is speakable.
			Match referenceMatch = _smilRefExtractor.Match(rawSmilReference);

			smilPath = referenceMatch.Groups[1].Value;
			smilReferenceID = referenceMatch.Groups[2].Value;
		}



		/// <summary>
		/// Retrieves the Smil Reference of the current element
		/// </summary>
		/// <returns></returns>
		private string GetCurrentSmilReference()
		{
			XElement currentElement = Navigator.CurrentElement;

			if(currentElement == null)
			{
				return String.Empty;
			}

			//The current element is guaranteed to have the SMIL reference attribute, since it is speakable.
			XAttribute smilRef = currentElement.Attribute("smilref");
			string rawReference = null;
			if(smilRef != null)
			{
				rawReference = smilRef.Value;
			}

			return rawReference;
		}



		private string GetSpeakableText()
		{
			XElement currentElement = Navigator.CurrentElement;

			if(currentElement == null)
			{
				return String.Empty;
			}

			string cleanedText = _tagIdentifier.Replace(currentElement.Value, String.Empty);
			return cleanedText;
		}

		#endregion Methods

	
	}
}