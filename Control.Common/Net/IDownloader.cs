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
using System.IO;

namespace Buttercup.Control.Common.Net
{
  public interface IDownloader
  {
    void DownloadAsync(Uri address, object userToken);
    event EventHandler<DownloadCompleteEventArgs> DownloadComplete;
    event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;
  }
}