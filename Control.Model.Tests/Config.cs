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

namespace Control.Model.Tests
{
    internal class Config
    {

        public static string ResourcePath_MSAA_Book_Xml
        {
            get
            {
                return @"Control.Model.Tests;component/Files/MSAA-Wikipedia_for_Daisy.xml";
            }
        }

        public static string ResourcePath_MSAA_Book_Ncx
        {
            get
            {
                return @"Control.Model.Tests;component/Files/speechgen.ncx";
            }
        }

        public static string ResourcePath_New_Zealand_Book_Ncx
        {
            get
            {
                return @"Control.Model.Tests;component/Files/New_Zealand.ncx";
            }
        }
        
        public static string ResourcePath_New_Zealand_Book_Xml
        {
            get
            {
                return @"Control.Model.Tests;component/Files/New_Zealand.xml";
            }
        }

        public static string ResourcePath_InvalidPackage
        {
            get
            {
                return @"Control.Model.Tests;component/Files/InValid_Package.zip";
            }
        }

        public static string ResourcePath_Valid30Package
        {
            get
            {
                return @"Control.Model.Tests;component/Files/Valid_DAISY_3.0.zip";
            }
        }
    }
}
