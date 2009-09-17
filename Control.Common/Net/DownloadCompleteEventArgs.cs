using System;
using System.ComponentModel;
using System.IO;
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
  public class DownloadCompleteEventArgs : AsyncCompletedEventArgs
  {
    private readonly Stream _result;

    public DownloadCompleteEventArgs(Stream result, Exception exception, bool cancelled, object userToken)
      : base(exception, cancelled, userToken)
    {
      _result = result;
    }

    public Stream Result
    {
      get
      {
        RaiseExceptionIfNecessary();
        return _result;
      }
    }
  }
}