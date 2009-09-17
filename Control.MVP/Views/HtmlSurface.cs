using System;
using System.Net;
using Buttercup.Control.Model;
using System.Windows.Browser;
using System.Xml.Linq;

namespace Control.MVP.Views
{
	public class HtmlSurface : Surface
	{
		private int _currentHighlightIndex = -1;



		public HtmlSurface(XDocument bookFragment)
			: base(bookFragment)
		{
		}



		/// <summary>
		/// Renders the HTML of the book fragment to the display.
		/// </summary>
		public override void Draw()
		{
			HtmlElement og = HtmlPage.Document.GetElementById("Content");
			try
			{
				HtmlPage.Window.CreateInstance("RenderContentHtml", GetHtml());
			}
			catch(Exception)
			{
			}
		}



		/// <summary>
		/// Highlights the element with the given index.
		/// </summary>
		/// <param name="elementIndex">The index of the element to highlight.</param>
		public override void Highlight(int elementIndex)
		{
			try
			{
				HtmlPage.Window.CreateInstance("HighlightElement", elementIndex, _currentHighlightIndex);
			}
			catch(Exception)
			{
			}
			finally
			{
				_currentHighlightIndex = elementIndex;
			}
		}



		/// <summary>
		/// Sets the contrast scheme to the given value.
		/// </summary>
		/// <param name="contrastScheme">The contrast scheme to use.</param>
		public override void SetContrast(ContrastLevel contrastScheme)
		{
			try
			{
				HtmlPage.Window.CreateInstance("SetContrastClass", contrastScheme.ToString());
			}
			catch(Exception)
			{
			}
		}



		/// <summary>
		/// Focuses on the element with the given index, e.g. scrolls the container so that the
		/// element is visible.
		/// </summary>
		/// <param name="elementIndex">The index of the element to focus on.</param>
		public override void SetFocus(int elementIndex)
		{
			try
			{
				HtmlPage.Window.CreateInstance("ScrollToElement", elementIndex);
			}
			catch(Exception)
			{
			}
		}



		/// <summary>
		/// Sets the font size for the text content.
		/// </summary>
		/// <param name="fontSize">The font size scaling value.</param>
		/// <remarks>The font size is scaled so that a value of 8 corresponds to 100%.</remarks>
		public override void SetFontSize(int fontSize)
		{
			int percentage = 100;

			if(fontSize > 0 && fontSize <= 32)
				percentage = fontSize * 100 / 8;

			try
			{
				HtmlPage.Window.CreateInstance("SetFontSize", percentage);
			}
			catch(Exception)
			{
			}
		}



		/// <summary>
		/// Gets the HTML of the underlying book fragment being rendered.
		/// </summary>
		/// <returns></returns>
		public string GetHtml()
		{
			string html = String.Empty;
			try
			{
				html = HtmlPage.Window.CreateInstance("TransformFragment", _bookFragment).ToString();
			}
			catch(Exception)
			{
			}

			return html;
		}


	}
}
