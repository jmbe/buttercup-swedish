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
using System.Collections.Generic;
using Buttercup.Control.Model.Entities;

namespace Buttercup.Control.Model
{
    /// <summary>
    /// Interfact that defines the attributes of a PackageInspector regardless of the version
    /// of DAISY files the inspector is interogating.
    /// </summary>
    public interface IPackageInspector : IObservableProgress
    {
        #region Data Members (3)

        bool IsValid { get; }

        IPackage Package { get; }

        Dictionary<string, Stream> PackageFiles { get; set; }

        List<string> ValidationErrors { get; }

        #endregion Data Members

        #region Operations (1)

        void ProcessPackage(Stream zipStream);

        #endregion Operations
    }
}
