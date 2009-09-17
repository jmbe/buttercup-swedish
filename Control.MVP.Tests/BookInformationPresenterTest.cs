using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Buttercup.Control.Model.Entities;
using Buttercup.Control.UI;
using Control.MVP.Presenters;
using Control.MVP.Views;

namespace Control.MVP.Tests
{

    //public partial class mockAppView : IApplicationView
    //{
    //    public event EventHandler<LoadZipPackageEventArgs> LoadZipPackage;
    //    public event EventHandler BookChanged;
    //    public Book CurrentBook { get; set; }
    //    public Reader BookReader { get; set; }
    //    public Surface DisplaySurface { get; set; }
    //    public double SaveProgress { get; set; }
    //    public string SaveProgressMessage { get; set; }
    //}

    //public partial class mockNavView : INavigationView
    //{
    //    public event EventHandler<ItemSelectedEventArgs> ItemSelected;
    //    public TableOfContents CurrentTableOfContents { get; set; }
    //    public IApplicationView ApplicationView { get; set; }
    //    public IPlayerView PlayerView { get; set; }
    //}

    //public partial class mockBookInfoView : IBookInformationView
    //{
    //    public Book CurrentBook { get; set; }
    //    public IApplicationView ApplicationView { get; set; }
    //}

    [TestClass]
    public class BookInformationPresenterTest
    {

        ApplicationPresenter AppPresenter;

        [TestInitialize]
        public void StartUp()
        {
            //mockAppView AppView = new mockAppView();
            //mockNavView NavView = new mockNavView();
            //mockBookInfoView InfoView = new mockBookInfoView();

            //Application Presenter will setup the Navigation and BookInformation Presenters as well
            //AppPresenter = new ApplicationPresenter(AppView, NavView, InfoView);

        }

        [TestMethod]
        public void LoadBookWithAllData()
        {

        }
    }
}
