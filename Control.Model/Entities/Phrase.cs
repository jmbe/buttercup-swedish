using System;

namespace Buttercup.Control.Model.Entities
{
    /// <summary>
    /// Represents a general set of words that can be spoken. This could 
    /// be a sentence, title, description...
    /// </summary>
    public class Phrase : ISpeakable
    {
		#region Properties (2) 

        public Audio Audio { get; set; }

        public string Text { get; set; }

        public Boolean Silent { get; set; }

        public string ElementID { get; set; }

		#endregion Properties 
    }
}