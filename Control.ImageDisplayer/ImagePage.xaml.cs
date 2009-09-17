using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using Buttercup.Control.Common.IO;
using Buttercup.Control.Common;
using System.IO;
using System.ComponentModel;
using System.Windows.Browser;

namespace Control.ImageDisplayer
{
    public partial class ImagePage : UserControl
    {
        private BackgroundWorker _worker;
		private Stream _imageDataStream;
		private BitmapImage _image;

        public ImagePage()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(ImagePage_Loaded);

            //_worker = new BackgroundWorker();
            //_worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            //_worker.ProgressChanged += new ProgressChangedEventHandler(_worker_ProgressChanged);
            //_worker.WorkerReportsProgress = true;
            //_worker.RunWorkerAsync();
            
        }

        public string ImageUrl { get; set; }

		public string ElementID { get; set; }


        void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BitmapImage image = new BitmapImage();
            image.SetSource(e.UserState as Stream);
            mainImage.Source = image;
        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
			//Not Used.
            /*IFileSystem fileSystem = FileSystemFactory.FileSystem;
            _imageDataStream = fileSystem.OpenFile(ImageUrl, FileMode.Open, FileAccess.Read, FileShare.Read);

            _worker.ReportProgress(100, _imageDataStream);*/
        }

        void ImagePage_Loaded(object sender, RoutedEventArgs e)
        {
			IFileSystem fileSystem = FileSystemFactory.FileSystem;
			_imageDataStream = fileSystem.OpenFile(ImageUrl, FileMode.Open, FileAccess.Read, FileShare.Read);
			
			_image = new BitmapImage();
            _image.SetSource(_imageDataStream);
            mainImage.Source = _image;
			ResizeImage();
        }


		void ResizeImage()
		{
			HtmlElement hostElement = HtmlPage.Document.GetElementById(ElementID);

			if(hostElement != null)
			{
				mainImage.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
				Size imageSize = mainImage.DesiredSize;
				hostElement.SetStyleAttribute("width", String.Format("{0}px", (int)imageSize.Width));
				hostElement.SetStyleAttribute("height", String.Format("{0}px", (int)imageSize.Height));
			}
		}


    }
}
