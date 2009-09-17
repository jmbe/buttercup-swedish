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

namespace Control.MVP
{
    public class LoadPackageEventArgs: EventArgs
    {
		#region Constructors (1) 

        public LoadPackageEventArgs(string packageFullPath)
            : base()
        {
            PackageFullPath = packageFullPath;
        }

		#endregion Constructors 

		#region Properties (1) 

        public string PackageFullPath { get; set; }

		#endregion Properties 
    }
}
