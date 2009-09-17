using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Buttercup.Control.Common.Net
{
  public class DownloadProgressChangedEventArgs : ProgressChangedEventArgs
  {
    private readonly long _bytesReceived;
    private readonly long _totalBytesToReceive;

    public DownloadProgressChangedEventArgs(int progressPercentage, object userToken, long bytesReceived, long totalBytesToReceive)
      : base(progressPercentage, userToken)
    {
      _bytesReceived = bytesReceived;
      _totalBytesToReceive = totalBytesToReceive;
    }

    public long BytesReceived
    {
      get { return _bytesReceived; }
    }

    public long TotalBytesToReceive
    {
      get { return _totalBytesToReceive; }
    }
  }
}