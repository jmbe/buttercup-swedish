using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Buttercup.Control.Model.Entities;
using System.Xml.Linq;

namespace Buttercup.Control.Model
{
	/// <summary>
	/// A lightweight entity containing the essential data needed to construct a Phrase.
	/// </summary>
	internal class PhraseConstructionKit
	{

		internal PhraseConstructionKit()
		{
			ConstructedPhrase = new Phrase();	
		}


		/// <summary>
		/// Gets or sets the phrase being constructed.
		/// </summary>
		/// <value>The phrase being constructed.</value>
		internal Phrase ConstructedPhrase
		{ get; private set; }


		/// <summary>
		/// Gets or sets the XML element for the audio information.
		/// </summary>
		/// <value>The audio element.</value>
		internal XElement AudioElement
		{ get; set; }


		internal string AudioFilePath
		{ get; set; }


		internal string SmilPath
		{ get; set; }


		internal string SmilReferenceID
		{ get; set; }

	
	}
}
