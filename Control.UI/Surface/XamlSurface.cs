using System;
using System.IO;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Buttercup.Control.Model;
using Buttercup.Control.UI.Extensions;
using Buttercup.Control.UI.ViewModel;
using Control.UI;
using Control.UI.Gestures;


namespace Buttercup.Control.UI
{
	/// <summary>
	/// The XamlSurface implementation allows book fragments to be rendered in XAML. This
	/// implementation makes us of an XSL transformation to convert the book xml into XAML.
	/// 
	/// The XAML is then loaded in to the content of a ContentControl (for example a ScrollViewer)
	/// </summary>
	/// <remarks>
	/// XSL transformations are performed on the client, in javascript, as Silverlight has no
	/// native api to do this. To initiate the transformation, the javacript function, TransformFragmentToXaml,
	/// must be accessible to the web page that hosts the code that this class is running within.
	/// At the time of writing, this function was included in the file /scripts/buttercup.js, which,
	/// in turn, depends on, /scripts/xslt/js.
	/// </remarks>
	public class XamlSurface : Surface
	{
		#region Fields (5) 

		private ScaleTransform _contentTransform;
		private Brush ContrastBackground;
		private Brush ContrastForeground;
		private Brush ContrastHighlight;
		private Brush ContrastHighlightForeGround;

		#endregion Fields 



		#region Constructors (1) 

		/// <summary>
		/// Create an instance of the surface passing in the container that will host the
		/// XAML for the book.
		/// </summary>
		/// <param name="surfaceContainer"></param>
		public XamlSurface(ScrollViewer surfaceContainer)
		{
			SurfaceContainer = surfaceContainer;
			SurfaceContainer.SizeChanged += SurfaceContainer_SizeChanged;

			// Set the default scale.
			Scale = 1.0;

			// Register this object so that the javascript can call the XamlTransformationComplete
			// scriptable method when the transformation is complete.
			HtmlPage.RegisterScriptableObject("XamlSurface", this);
		}

		#endregion Constructors 



		#region Properties (7) 

		/// <summary>
		/// The BaseUri specifies the root path to the book contents. If set, the BaseUri is 
		/// prepended to image source properties.
		/// </summary>
		public Uri BaseUri { get; set; }

		/// <summary>
		/// A placeholder for the content of the SurfaceContainer. We are assuming this will 
		/// be a Panel.
		/// </summary>
		private Panel Content { get; set; }

		/// <summary>
		/// The currently selected element. We use a DependencyObject to cover both Runs (which
		/// are not UIElements) and TextBlock's and Images
		/// </summary>
		private DependencyObject CurrentElement { get; set; }

		MouseGestures MouseGesturesHandler { get; set; }

		/// <summary>
		/// Stores the ViewportHeight of the ScrollViewer when the Scale = 1.
		/// </summary>
		private double OriginalExtentHeight { get; set; }

		/// <summary>
		/// The control that hosts the book content. Most likely this will be a ScrollView.
		/// </summary>
		private ScrollViewer SurfaceContainer { get; set; }

		/// <summary>
		/// Private property to store the converted Xaml
		/// </summary>
		private XDocument Xaml { get; set; }

		#endregion Properties 



		#region Delegates and Events (4) 

		// Events (4) 

		public override event EventHandler<MouseGestureEventArgs> GestureRaised;

		/// <summary>
		/// This event is raised when an element within the surface has been selected (with
		/// a MouseLeftButtonDown event). The surface will highlight the item automatically.
		/// </summary>
		public override event EventHandler<BookElementSelectedEventArgs> ItemSelected;

		/// <summary>
		/// This event is raised as each Image is processed to allow users of this surface to
		/// programmatically set the source.
		/// </summary>
		public override event EventHandler PreImageRendered;

		/// <summary>
		/// This event is raised when the surface was been rendered. Not this happens
		/// asynchronous from the momement the Draw method has been called.
		/// </summary>
		public override event EventHandler RenderComplete;

        /// <summary>
        /// This event is raised if the surface can not render the book. This can happen if
        /// invalid or unparsable xml is supplied.
        /// </summary>
        public override event EventHandler DrawFailed;

		#endregion Delegates and Events 



		#region Methods (26) 

		// Public Methods (13) 

		/// <summary>
		/// Invoked on initialisation to ensure initially-displayed content (local mode only)
		/// is presented at the correct scale, and appropriate event handlers are connected.
		/// </summary>
		public void ArrangeInitialContent()
		{
			Content = SurfaceContainer.Content as Panel;

			if(Content != null)
			{
				Content.SizeChanged += Content_SizeChanged;
			}

			SurfaceContainer.InvalidateScrollInfo();
			SurfaceContainer.AdjustContentScale(Scale);
		}



		public override string CurrentHighlightableElementID()
		{
			object element = CurrentElement;
			if(element is Run)
			{
				return GetParentOfRun(element as Run).Name;
			}

			if(element == null)
			{
				return null;
			}

			return (element as FrameworkElement).Name;
		}



		/// <summary>
		/// Initiates drawing the book fragment to the surface. This method runs asynchronously -
		/// users should subscribe to the TransformationComplete event to determine when the
		/// surface has been fully rendered.
		/// </summary>
		public override void Draw()
		{
			if(Content != null)
			{
				Content.SizeChanged -= Content_SizeChanged;
			}

			// Calls the TransformFragmentToXaml function (in Buttercup.js) to transform the
			// book fragment.
			HtmlPage.Window.Invoke("TransformFragmentToXaml", BookFragment.ToString(), null);
		}



		/// <summary>
		/// Checks whether a certain element exists on the display surface.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public override bool ElementExists(string id)
		{
			return (GetElement(id) != null);
		}



		public override bool ElementHighlightable(string id)
		{
			object element = GetElement(id);
			return !(element is Run);
		}



		/// <summary>
		/// Given a navigatable element, returns the hightable surface element that will contain that element
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public override string GetHighlightableElementOfElement(string id)
		{
			object element = GetElement(id);
			if(element is Run)
			{
				return GetParentOfRun(element as Run).Name;
			}

			if(element == null)
			{
				return null;
			}

			return (element as FrameworkElement).Name;
		}



		/// <summary>
		/// Highlight a specific element based on its id. The id is the same as the that on 
		/// the original BookFragment XML.
		/// </summary>
		/// <param name="id"></param>
		public override void Highlight(string id)
		{
			DependencyObject newElement = GetElement(id) as DependencyObject;
			Highlight(newElement);
		}



		/// <summary>
		/// Refreshes the layout of the surface when images' layouts are updated.
		/// This is invoked for server-hosted books where image URLs must be resolved.
		/// </summary>
		/// <param name="sender">The sender (Image).</param>
		public void RefreshLayout(object sender, SizeChangedEventArgs e)
		{
			Image targetImage = sender as Image;

			if(targetImage != null)
			{
				targetImage.SizeChanged -= RefreshLayout;
			}

			SetScale(Scale);
		}



		/// <summary>
		/// Set the colour combinations for the surface.
		/// </summary>
		/// <param name="level"></param>
		public override void SetContrast(ContrastLevel level)
		{
			//By the time this is called, the DisplayAttributes have been updated to use the new colours.
			ContrastBackground = DisplayAttributes.CommonInstance.DefaultBackground;
			ContrastForeground = DisplayAttributes.CommonInstance.DefaultForeground;
			ContrastHighlight = DisplayAttributes.CommonInstance.HighlightBackground;
			ContrastHighlightForeGround = DisplayAttributes.CommonInstance.HighlightForeground;

			SurfaceContainer.Foreground = ContrastForeground;
			SurfaceContainer.Background = ContrastBackground;
			Highlight(CurrentElement);
		}



		/// <summary>
		/// Set the focus of the surface to the currently highlighted element.
		/// </summary>
		/// <remarks>
		/// This method could be made redundant if it was assumed that the Highlight method
		/// also set the focus.
		/// </remarks>
		public override void SetFocus()
		{
			if(CurrentElement != null)
			{
				SetFocus(CurrentElement);
			}
		}



		/// <summary>
		/// Set's the source of the image specified by the id. This is useful when the image
		/// source can not be set by a url. For example, if the images are stored in Isolated
		/// Storage.
		/// </summary>
		/// Thrown when the id does not correspond to an image.
		/// </exception>
		/// <param name="id"></param>
		/// <param name="source"></param>
		public override void SetImageSource(string id, Stream source)
		{
			Image image = GetElement(id) as Image;
			if(image != null)
			{
				BitmapImage imageSource = new BitmapImage();
				imageSource.SetSource(source);
				image.Source = imageSource;
			}
			else
			{
				throw new ArgumentException(String.Format("Element, {0}, is not an Image"));
			}
		}



		/// <summary>
		/// Set the scale of the surface content. 1 = normal.
		/// </summary>
		/// <param name="scale"></param>
		public override void SetScale(double scale)
		{
			if(Scale > 0)
			{
				Scale = scale;
				SurfaceContainer.AdjustContentScale(Scale);
			}
		}



		/// <summary>
		/// This method is called by javascript once the XSL transformation has 
		/// completed.
		/// </summary>
		/// <param name="param">
		/// The XAML resulting from the transformation.
		/// </param>
		[ScriptableMember]
		public void XamlTransformationComplete(object param)
		{
            if (param.ToString() == "")
            {
                // This will be the result if the xslt transformation fails (in javascript)
                // The javascript will have displayed the error.
                if (DrawFailed != null)
                {
                    DrawFailed(this, new EventArgs());
                }
            }
            else
            {
                Xaml = XDocument.Parse(param.ToString());

                // Load the XAML into a Panel instance. Note that this assumes the
                // XAML has any Panel as its root node.
                Content = (Panel)XamlReader.Load(param.ToString());

                MouseGesturesHandler = new MouseGestures(SurfaceContainer);
                MouseGesturesHandler.GestureRaised += MouseGesturesHandler_GestureRaised;

                // Attach a single event handler (ItemSelectedInternal) to the MouseLeftButtonDown 
                // event of all selectable elements (TextBlocks and Images)
                PostProcessContent(Content);

                // Set the Content of the SurfaceContainer
                //SurfaceContainer.Loaded += _surfaceContainer_Loaded;
                Content.Loaded += _surfaceContainer_Loaded;
                SurfaceContainer.Content = Content;
            }
		}



		// Private Methods (13) 

		/// <summary>
		/// When the SurfaceContainer has fully loaded we can raise the RenderComplete
		/// event to advise users of this surface that the rendering has fully completed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _surfaceContainer_Loaded(object sender, RoutedEventArgs e)
		{
			//Unwire the event to prevent multiple invocations.
			//SurfaceContainer.Loaded -= _surfaceContainer_Loaded;
            Content.Loaded -= _surfaceContainer_Loaded;
			if(Content != null)
			{
				Content.SizeChanged += Content_SizeChanged;
			}

			SurfaceContainer.InvalidateScrollInfo();
			SurfaceContainer.AdjustContentScale(Scale);

			// Let subscribers know that the transformation is complete and the surface is fully
			// rendered.
			if(RenderComplete != null)
			{
				RenderComplete(this, new EventArgs());
			}
		}



		/// <summary>
		/// Raised when the content itself (as opposed to the containing Panel) resizes.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
		void Content_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if(CurrentElement != null)
			{
				SetFocus(CurrentElement);
			}
		}



		/// <summary>
		/// Helper method to return the element based on its id. 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		private object GetElement(string id)
		{
			if(id != null && Content != null)
			{
				return Content.FindName(id);
			}
			else
			{
				return null;
			}
		}



		/// <summary>
		/// Background's are set by identifying the first Border that is an ancestor of the
		/// supplied element.
		/// </summary>
		/// <param name="element">
		/// A DependencyObject that typically represents either a FrameworkElement or an
		/// Inline element like a Run.
		/// </param>
		/// <returns></returns>
		private Border GetElementBorder(DependencyObject element)
		{
			Border border = null;

			// If the element to highlight is a Run then we need to get it's parent TextBlock
			// Run's do not have an idenifiable Parent.
			if(element is Run)
			{
				TextBlock textBlock = GetParentOfRun(element as Run);
				if(textBlock != null)
				{
					border = textBlock.GetParentOfType<Border>();
				}
			}
			else if(element is FrameworkElement)
			{
				border = ((FrameworkElement)element).GetParentOfType<Border>();
			}

			return border;
		}



		private Run GetFirstRunOfTextBlock(TextBlock textBlock)
		{
			Run run = null;
			foreach(Inline inline in textBlock.Inlines)
			{
				if(inline is Run)
				{
					run = inline as Run;
					break;
				}
			}
			return run;
		}



		private TextBlock GetParentOfRun(Run run)
		{
			TextBlock textBlock = null;
			XNamespace xmlns = "http://schemas.microsoft.com/client/2007";
			XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";

			var q = Xaml.Descendants(xmlns + "Run").FilterByAttribute(x + "Name", run.Name);
			if(q.Count > 0)
			{
				string parentId = q[0].Parent.Attribute(x + "Name").Value;
				textBlock = GetElement(parentId) as TextBlock;
			}
			return textBlock;
		}



		private bool HasDerivedId(FrameworkElement element)
		{
			return element.Name.StartsWith("_");
		}



		/// <summary>
		/// Private helper method to highlight a specific element.
		/// </summary>
		/// <param name="element"></param>
		private void Highlight(DependencyObject element)
		{
			// Undo the CurrentElement's Highlight
			if(CurrentElement != null)
			{
				Border currentElementBorder = GetElementBorder(CurrentElement);
				if(currentElementBorder != null)
				{
					currentElementBorder.ClearValue(Border.BackgroundProperty);
					if(CurrentElement is TextBlock)
					{
						(CurrentElement).ClearValue(TextBlock.ForegroundProperty);
					}
					if(CurrentElement is Run)
					{
						Run currentRun = (Run)CurrentElement;
						currentRun.ClearValue(Inline.FontWeightProperty);

						TextBlock parentTextBlock = GetParentOfRun(currentRun);
						if(parentTextBlock != null)
						{
							parentTextBlock.ClearValue(TextBlock.ForegroundProperty);
						}
					}
				}
			}

			// Set the new element's Highlight
			Border newElementBorder = GetElementBorder(element);
			if(newElementBorder != null)
			{
				newElementBorder.Background = ContrastHighlight;
				if(element is TextBlock)
				{
					((TextBlock)element).Foreground = ContrastHighlightForeGround;
				}
				if(element is Run)
				{
					Run highlightedRun = (Run)element;
					//highlightedRun.FontWeight = FontWeights.Bold;

					TextBlock parentTextBlock = GetParentOfRun(highlightedRun);
					if(parentTextBlock != null)
					{
						parentTextBlock.Foreground = ContrastHighlightForeGround;
					}
				}
			}

			SetFocus(element);
		}



		/// <summary>
		/// This event handler is raised when an element (for example, TextBlock or Image)
		/// is selected from within the surface. 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ItemSelectedInternal(object sender, MouseButtonEventArgs e)
		{
			DependencyObject element = sender as DependencyObject;
			if(element != null)
			{
				if(ItemSelected != null)
				{
					if(element is FrameworkElement && MouseGesturesHandler.State != GestureState.InProgress)
					{
						Highlight(element);

						string selectableName = null;

						if(element is TextBlock && HasDerivedId((FrameworkElement)element))
						{
							Run run = GetFirstRunOfTextBlock(element as TextBlock);
							selectableName = run.Name;
						}
						else
						{
							selectableName = ((FrameworkElement)element).Name;
						}
						ItemSelected(sender, new BookElementSelectedEventArgs(selectableName));
					}
				}
			}
		}



		private void MouseGesturesHandler_GestureRaised(object sender, MouseGestureEventArgs e)
		{
			// We pass on all gestures to the XAMLSurface event
			if(GestureRaised != null)
			{
				GestureRaised(this, e);
			}
		}



		/// <summary>
		/// Event handlers for the TextBlocks and Images are added programmatically since they
		/// can't be defined on the XAML itself.
		/// </summary>
		/// <param name="content"></param>
		private void PostProcessContent(object content)
		{
			// We aren't interested in borders so simply look at the child instead.
			if(content is Border)
			{
				content = ((Border)content).Child;
			}

			// If processing an image raise the PreImageRendered event to allow users to
			// set the source programmatically - if required.
			if(content is Image)
			{
				Image image = content as Image;
				if(PreImageRendered != null)
				{
					PreImageRendered(image, new EventArgs());
				}
			}

			// Attach a MouseLeftButtonDown event to TextBlocks and Images only
			if(content is TextBlock || content is Image)
			{
				UIElement element = (UIElement)content;
				element.MouseLeftButtonUp += ItemSelectedInternal;
			}

			// Recursively attach handlers to child elements of panels (e.g. StackPanels).
			if(content is Panel)
			{
				Panel control = (Panel)content;
				foreach(UIElement element in control.Children)
					PostProcessContent(element);
			}
		}



		/// <summary>
		/// Private helper method to set the focus to a specific element.
		/// </summary>
		/// <param name="element"></param>
		private void SetFocus(DependencyObject element)
		{
			CurrentElement = element;

			// Run elements can't be scrolled to directly so we find their parent through
			// a custom ParentTagId property.
			if(element is Run)
			{
				element = GetParentOfRun(element as Run);
			}

			if(element is FrameworkElement)
			{
				((FrameworkElement)element).ScrollIntoView(SurfaceContainer, Scale);
			}
		}



		/// <summary>
		/// Raised when the parent (ScrollViewer) is resized. This is a workaround to fix a Measure issue with the
		/// ScrollViewer in Silverlight 2.0 RTM.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e"></param>
		private void SurfaceContainer_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if(Content == null)
			{
				return;
			}

			Content.Width = (SurfaceContainer.ViewportWidth - 1.0) / Scale;

			if(CurrentElement != null)
			{
				SetFocus(CurrentElement);
			}
		}

		#endregion Methods 
	}
}