using System.Collections.Generic;
using System.Xml.Linq;

namespace Buttercup.Control.Model.Entities
{
    public class SmilDocument
    {
		#region Fields (3) 

        private static readonly XNamespace _smilNamespace;
        private readonly string _smilPath;
        private readonly IEnumerable<XElement> _speakableElements;

		#endregion Fields 

		#region Constructors (2) 

        public SmilDocument(string smilPath, XDocument xml)
        {
            _smilPath = smilPath;
            XElement smilRootElement = xml.Element(_smilNamespace + "smil").Element(_smilNamespace + "body");
            _speakableElements = smilRootElement.Descendants(_smilNamespace + "par");
        }

        static SmilDocument()
        {
            _smilNamespace = "http://www.w3.org/2001/SMIL20/";
        }

		#endregion Constructors 

		#region Properties (1) 

        /// <summary>
        /// Gets the path of the SMIL document.
        /// </summary>
        /// <value>The path of the SMIL document.</value>
        public string SmilPath
        {
            get { return _smilPath; }
        }

		#endregion Properties 

		#region Methods (1) 

		// Public Methods (1) 

        /// <summary>
        /// Gets the XML element containing the audio clip information by SMIL ID.
        /// </summary>
        /// <param name="id">The reference ID in the SMIL document.</param>
        /// <returns>the audio element if it exists, otherwise null.</returns>
        public XElement GetAudioElementBySmilID(string id)
        {
            XElement audioElement = null;

            List<XElement> filteredElements = _speakableElements.FilterByAttribute("id", id);
            if (filteredElements.Count > 0)
            {
                XElement currentElement = filteredElements[0];
                audioElement = currentElement.Element(_smilNamespace + "audio");
            }

            return audioElement;
        }

		#endregion Methods 
    }
}