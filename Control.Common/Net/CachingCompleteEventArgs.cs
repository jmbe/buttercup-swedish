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
using Buttercup.Control.Common.IO;

namespace Buttercup.Control.Common.Net
{
    public class CachingCompleteEventArgs : EventArgs
    {
        public CachingCompleteEventArgs(string fullname, DownloadCompleteEventArgs e)
        {
            FullName = fullname;
            DownloadArgs = e;
        }

        public string FullName
        {
            get; private set;
        }

        public DownloadCompleteEventArgs DownloadArgs
        {
            get; private set;
        }
    }
}