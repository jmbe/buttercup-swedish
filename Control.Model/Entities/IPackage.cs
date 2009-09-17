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

namespace Buttercup.Control.Model.Entities
{
    /// <summary>
    /// This interface defines the attributes of a set of DAISY files - independent of the 
    /// version of the files.
    /// </summary>
    public interface IPackage
    {
        #region Data Members (5)

        string BookFolder { get; set; }

        string BookId { get; }

        string DtBookXmlPath { get; }

        int FileCount { get; }

        string NcxXmlPath { get; }

        #endregion Data Members
    }
}
