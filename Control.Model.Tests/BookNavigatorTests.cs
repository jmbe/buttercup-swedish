using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Buttercup.Control.Model.Entities;
using System.Windows.Resources;
using System.Xml.Linq;
using Buttercup.Control.Model;

namespace Control.Model.Tests
{
    [TestClass]
    public class BookNavigatorTests
    {
        Book CurrentBook { get; set; }
        BookNavigator Navigator { get; set; }

        Book MSAABook { get; set; }
        BookNavigator MSAANavigator { get; set; }

        [TestInitialize]
        public void StartUp()
        {
            if (CurrentBook == null)
            {
                StreamResourceInfo info = App.GetResourceStream(new Uri(Config.ResourcePath_New_Zealand_Book_Xml, UriKind.Relative));
                CurrentBook = new Book();
                CurrentBook.Xml = XDocument.Load(info.Stream);

                info = App.GetResourceStream(new Uri(Config.ResourcePath_New_Zealand_Book_Ncx, UriKind.Relative));
				XDocument ncxDoc = XDocument.Load(info.Stream);
                CurrentBook.TableOfContents = new TableOfContents(ncxDoc);
				CurrentBook.PageList = new PageList(ncxDoc);
            }

            if (Navigator == null)
            {
                Navigator = new BookNavigator(CurrentBook);
            }

            if (MSAABook == null)
            {
                StreamResourceInfo info = App.GetResourceStream(new Uri(Config.ResourcePath_MSAA_Book_Xml, UriKind.Relative));
                MSAABook = new Book();
                MSAABook.Xml = XDocument.Load(info.Stream);

                info = App.GetResourceStream(new Uri(Config.ResourcePath_MSAA_Book_Ncx, UriKind.Relative));
                XDocument ncxDoc = XDocument.Load(info.Stream);
                MSAABook.TableOfContents = new TableOfContents(ncxDoc);
                MSAABook.PageList = new PageList(ncxDoc);
            }

            if (MSAANavigator == null)
            {
                MSAANavigator = new BookNavigator(MSAABook);
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
        }


        [TestMethod]
        public void MoveNext_Can_Navigate_Into_List()
        {
            MSAANavigator.SeekToElementId("dtb60");
            MSAANavigator.MoveNext();
            MSAANavigator.MoveNext();

            Assert.IsTrue(MSAANavigator.CurrentElementID == "dtb62");
        }

        [TestMethod]
        public void MoveNext_Can_Navigate_Sentences_Within_List()
        {
            MSAANavigator.SeekToElementId("dtb62");
            MSAANavigator.MoveNext();

            Assert.IsTrue(MSAANavigator.CurrentElementID == "dtb63");
        }

        [TestMethod]
        public void MovePrevious_Can_Navigate_Into_List()
        {
            MSAANavigator.SeekToElementId("dtb72");
            MSAANavigator.MovePrevious();

            Assert.IsTrue(MSAANavigator.CurrentElementID == "dtb71");
        }

        [TestMethod]
        public void MovePrevious_Can_Navigate_Sentences_Within_List()
        {
            MSAANavigator.SeekToElementId("dtb68");
            MSAANavigator.MovePrevious();

            Assert.IsTrue(MSAANavigator.CurrentElementID == "dtb67");
        }

        [TestMethod]
        public void Initial_Element_Is_Correct()
        {
            //Need to reconstruct the navigator before this test to ensure it is fresh.
            Navigator = new BookNavigator(CurrentBook);
            XElement firstSpeakableElement = Navigator.CurrentElement;

            // Should be 
            //  <doctitle id="ghGen2" smilref="New_Zealand.smil#ghGen2">The Countries: New Zealand</doctitle>
            Assert.IsTrue(firstSpeakableElement.Name.LocalName == "pagenum" &&
                            firstSpeakableElement.Attribute("id").Value == "gh_pageRef1");

        }


        [TestMethod]
        public void Initial_Location_Correct()
        {
            //Need to reconstruct the navigator before this test to ensure it is fresh.
            Navigator = new BookNavigator(CurrentBook);
            // The doctitle is actually not within the level 1 grouping - so effectively this is
            // level zero.
            Assert.IsTrue(Navigator.CurrentLevel == 0);
        }


        [TestMethod]
        public void Setting_Invalid_CurrentElementID_CausesException()
        {
            try
            {
                Navigator.CurrentElementID = "EyeOfTheTiger";

                Assert.Fail("No exception thrown when a non existent value for CurrentElementId is set");
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }
        }


        [TestMethod]
        public void Setting_NonSpeakable_CurrentElementID_CausesException()
        {
            /*	Fragment being tested:
                <p id="ghGen130">
                    <sent id="ghGen131" smilref="New_Zealand.smil#ghGen131">OFFICIAL NAME: New Zealand</sent>
                </p>
             */
            try
            {
                Navigator.CurrentElementID = "ghGen130";

                Assert.Fail("No exception thrown when a non-speakable element is specified.");
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }
        }


        [TestMethod]
        public void Setting_Valid_CurrentElementID_IsCorrect()
        {
            Navigator.CurrentElementID = "gh_pageRef1";
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            //  <pagenum id="gh_pageRef1" smilref="New_Zealand.smil#gh_pageRef1">1</pagenum>
            Assert.IsTrue(currentElement.Name.LocalName == "pagenum" &&
                            currentElement.Attribute("id").Value == "gh_pageRef1");
        }



        /// <summary>
        /// Tests MoveNext from the first page number to the first heading (both at the same hierarchical level).
        /// </summary>
        /// <remarks>All tests run sequentially WITHOUT re-initialisation in between, therefore the
        /// state at the start of a test is consistent with the start of the previously-executed test.
        /// THE CATCH: Tests are executed IN ALPHABETICAL ORDER - NOT the order specified in the source.</remarks>
        [TestMethod]
        public void MoveNext_FromFirstPageRef_Returns_FirstHeading()
        {
            Navigator.CurrentElementID = "gh_pageRef1";
            XElement currentElement = Navigator.CurrentElement;
            // Should be:
            //  <pagenum id="gh_pageRef1" smilref="New_Zealand.smil#gh_pageRef1">1</pagenum>
            Assert.IsTrue(currentElement.Name.LocalName == "pagenum" &&
                            currentElement.Attribute("id").Value == "gh_pageRef1");

            Navigator.MoveNext();
            currentElement = Navigator.CurrentElement;
            // Should be 
            //  <h1 id="ghGen3" smilref="New_Zealand.smil#ghGen3">The Countries: New Zealand</h1>
            Assert.IsTrue(currentElement.Name.LocalName == "h1" &&
                            currentElement.Attribute("id").Value == "ghGen3");
        }


        #region AtEndOfBook Tests

        [TestMethod]
        public void AtEndOfBook_FromStart_IsFalse()
        {
            //Need to reconstruct the navigator before this test to ensure it is fresh.
            Navigator = new BookNavigator(CurrentBook);
            Assert.IsFalse(Navigator.AtEndOfBook, "AtEndOfBook should be false.");
        }


        [TestMethod]
        public void AtEndOfBook_FromFirstHeading_IsFalse()
        {
            Navigator.CurrentElementID = "ghGen3";
            Assert.IsFalse(Navigator.AtEndOfBook, "AtEndOfBook should be false.");
        }


        [TestMethod]
        public void AtEndOfBook_FromMidContent_IsFalse()
        {
            Navigator.CurrentElementID = "ghGen218";
            Assert.IsFalse(Navigator.AtEndOfBook, "AtEndOfBook should be false.");
        }


        [TestMethod]
        public void AtEndOfBook_FromLastSpeakableElement_IsFalse()
        {
            Navigator.CurrentElementID = "ghGen903";
            Assert.IsFalse(Navigator.AtEndOfBook, "AtEndOfBook should be false.");
        }


        [TestMethod]
        public void AtEndOfBook_FromEndOfBook_IsTrue()
        {
            Navigator.CurrentElementID = "ghGen903";
            Navigator.MoveNext();

            Assert.AreEqual("ghGen903", Navigator.CurrentElementID, "Should be at end of book, so CurrentElement should remain the same.");
            Assert.IsTrue(Navigator.AtEndOfBook, "AtEndOfBook should be true.");
        }

        #endregion


        #region MoveNext Tests

        /// <summary>
        /// Tests MoveNext where both elements are at the same hierarchical level.
        /// </summary>
        [TestMethod]
        public void MoveNext_FromFirstHeading_Returns_FirstImage()
        {
            Navigator.CurrentElementID = "ghGen3";

            Navigator.MoveNext();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            //  <img src="img/Page01a.jpg" alt="A picture of New Zealand." id="ghGen5"/>
            Assert.IsTrue(currentElement.Name.LocalName == "img" &&
                            currentElement.Attribute("id").Value == "ghGen5");
        }


        /// <summary>
        /// Tests MoveNext where the next element is at a deeper hierarchical level.
        /// </summary>
        [TestMethod]
        public void MoveNext_FromHeading_MovingDeeper()
        {
            /*	Fragment being tested:
                <h1 id="ghGen129" smilref="New_Zealand.smil#ghGen129">Fast Facts</h1>
                <p id="ghGen130">
                    <sent id="ghGen131" smilref="New_Zealand.smil#ghGen131">OFFICIAL NAME: New Zealand</sent>
                </p>
             */
            Navigator.CurrentElementID = "ghGen129";

            Navigator.MoveNext();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            //  <sent id="ghGen131" smilref="New_Zealand.smil#ghGen131">OFFICIAL NAME: New Zealand</sent>
            Assert.IsTrue(currentElement.Name.LocalName == "sent" &&
                            currentElement.Attribute("id").Value == "ghGen131");
        }


        /// <summary>
        /// Tests MoveNext where the next element is at a deeper hierarchical level, but where traversal
        /// requires jumping up to a higher level.
        /// </summary>
        [TestMethod]
        public void MoveNext_FromSentence_ToDeeperSubheading()
        {
            /*	Fragment being tested:
                <p id="ghGen132">
                    <sent id="ghGen133" smilref="New_Zealand.smil#ghGen133">CAPITAL: Wellington</sent>
                </p>
                <level2>
                    <h2 id="ghGen134" smilref="New_Zealand.smil#ghGen134">LAND</h2>
             */
            Navigator.CurrentElementID = "ghGen133";

            Navigator.MoveNext();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            //  <h2 id="ghGen134" smilref="New_Zealand.smil#ghGen134">LAND</h2>
            Assert.IsTrue(currentElement.Name.LocalName == "h2" &&
                            currentElement.Attribute("id").Value == "ghGen134");
        }


        /// <summary>
        /// Tests MoveNext where the next element is at a higher hierarchical level (i.e. the current sub-section
        /// has ended. Traversal requires moving both up and down the hierarchy.
        /// </summary>
        [TestMethod]
        public void MoveNext_FromPageNumber_ToHigherHeading()
        {
            /*	Fragment being tested:
                    <pagenum id="gh_pageRef7" smilref="New_Zealand.smil#gh_pageRef7">7</pagenum>
                </level2>
            </level1>
            <level1>
                <h1 id="ghGen179" smilref="New_Zealand.smil#ghGen179">Timeline</h1>
             */
            Navigator.CurrentElementID = "gh_pageRef7";

            Navigator.MoveNext();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            //  <h1 id="ghGen179" smilref="New_Zealand.smil#ghGen179">Timeline</h1>
            Assert.IsTrue(currentElement.Name.LocalName == "h1" &&
                            currentElement.Attribute("id").Value == "ghGen179");
        }


        /// <summary>
        /// Tests MoveNext from the last speakable element.
        /// </summary>
        [TestMethod]
        public void MoveNext_AtEndOfBook()
        {
            /*	Fragment being tested:
                <p id="ghGen902">               
                    <sent id="ghGen903" smilref="New_Zealand.smil#ghGen903"> c. golf</sent>
                </p>
            </level1>
        </rearmatter>
    </book>
</dtbook>
             */
            Navigator.CurrentElementID = "ghGen903";

            Navigator.MoveNext();
            Assert.AreEqual("ghGen903", Navigator.CurrentElementID, "CurrentElementID should be that of the last speakable element.");
        }

        #endregion


        #region MovePrevious Tests

        /// <summary>
        /// Tests MovePrevious where both elements are at the same hierarchical level.
        /// </summary>
        [TestMethod]
        public void MovePrevious_FromAuthor_Returns_FirstImage()
        {
            Navigator.CurrentElementID = "ghGen6";

            Navigator.MovePrevious();
            XElement currentElement = Navigator.CurrentElement;
            // Should be:
            //  <img src="img/Page01a.jpg" alt="A picture of New Zealand." id="ghGen5"/>
            Assert.IsTrue(currentElement.Name.LocalName == "img" &&
                            currentElement.Attribute("id").Value == "ghGen5");
        }


        /// <summary>
        /// Tests MovePrevious where the previous element is at a higher hierarchical level.
        /// </summary>
        [TestMethod]
        public void MovePrevious_FromSentence_MovingUpHierarchy()
        {
            /*	Fragment being tested:
                <h1 id="ghGen129" smilref="New_Zealand.smil#ghGen129">Fast Facts</h1>
                <p id="ghGen130">
                    <sent id="ghGen131" smilref="New_Zealand.smil#ghGen131">OFFICIAL NAME: New Zealand</sent>
                </p>
             */
            Navigator.CurrentElementID = "ghGen131";

            Navigator.MovePrevious();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            //  <h1 id="ghGen129" smilref="New_Zealand.smil#ghGen129">Fast Facts</h1>
            Assert.IsTrue(currentElement.Name.LocalName == "h1" &&
                            currentElement.Attribute("id").Value == "ghGen129");
        }


        /// <summary>
        /// Tests MovePrevious where the previous element is at a higher overall hierarchical level, but where traversal
        /// requires diving to a higher level.
        /// </summary>
        [TestMethod]
        public void MovePrevious_FromSentence_ToDeeperSubheading()
        {
            /*	Fragment being tested:
                <p id="ghGen132">
                    <sent id="ghGen133" smilref="New_Zealand.smil#ghGen133">CAPITAL: Wellington</sent>
                </p>
                <level2>
                    <h2 id="ghGen134" smilref="New_Zealand.smil#ghGen134">LAND</h2>
             */
            Navigator.CurrentElementID = "ghGen134";

            Navigator.MovePrevious();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            //  <sent id="ghGen133" smilref="New_Zealand.smil#ghGen133">CAPITAL: Wellington</sent>
            Assert.IsTrue(currentElement.Name.LocalName == "sent" &&
                            currentElement.Attribute("id").Value == "ghGen133");
        }


        /// <summary>
        /// Tests MovePrevious where the previous element is at a deeper hierarchical level. Traversal requires moving both up and down the hierarchy.
        /// </summary>
        [TestMethod]
        public void MovePrevious_FromHeading_ToDeeperPageNumber()
        {
            /*	Fragment being tested:
                    <pagenum id="gh_pageRef7" smilref="New_Zealand.smil#gh_pageRef7">7</pagenum>
                </level2>
            </level1>
            <level1>
                <h1 id="ghGen179" smilref="New_Zealand.smil#ghGen179">Timeline</h1>
             */
            Navigator.CurrentElementID = "ghGen179";

            Navigator.MovePrevious();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            //  <pagenum id="gh_pageRef7" smilref="New_Zealand.smil#gh_pageRef7">7</pagenum>
            Assert.IsTrue(currentElement.Name.LocalName == "pagenum" &&
                            currentElement.Attribute("id").Value == "gh_pageRef7");
        }


        /// <summary>
        /// Tests MovePrevious from the first speakable element (i.e. start of book).
        /// This is outside the level1 tag.
        /// </summary>
        [TestMethod]
        public void MovePrevious_FromFirstSpeakableElement()
        {
            /*	Fragment being tested:
                <frontmatter>
                    <doctitle id="ghGen2" smilref="New_Zealand.smil#ghGen2">The Countries: New Zealand</doctitle>
                    <level1>
             */
            Navigator.CurrentElementID = "ghGen2";

            Navigator.MovePrevious();
            XElement currentElement = Navigator.CurrentElement;
            // Should be same element:
            //  <doctitle id="ghGen2" smilref="New_Zealand.smil#ghGen2">The Countries: New Zealand</doctitle>
            Assert.IsTrue(currentElement.Name.LocalName == "doctitle" &&
                            currentElement.Attribute("id").Value == "ghGen2");
        }


        /// <summary>
        /// Tests MovePrevious from the first level1 (speakable) element. This should go to the doctitle outside level1.
        /// </summary>
        [TestMethod]
        public void MovePrevious_FromFirstLevel1Element()
        {
            /*	Fragment being tested:
                <frontmatter>
                    <doctitle id="ghGen2" smilref="New_Zealand.smil#ghGen2">The Countries: New Zealand</doctitle>
                    <level1>
                        <pagenum id="gh_pageRef1" smilref="New_Zealand.smil#gh_pageRef1">1</pagenum>
             */
            Navigator.CurrentElementID = "gh_pageRef1";

            Navigator.MovePrevious();
            XElement currentElement = Navigator.CurrentElement;
            // Should be same element:
            //  <pagenum id="gh_pageRef1" smilref="New_Zealand.smil#gh_pageRef1">1</pagenum>
            Assert.IsTrue(currentElement.Name.LocalName == "pagenum" &&
                            currentElement.Attribute("id").Value == "gh_pageRef1");
        }

        #endregion


        #region DownLevel Tests

        /// <summary>
        /// Tests DownLevel where both headings are at the same hierarchical level.
        /// </summary>
        [TestMethod]
        public void DownLevel_FromFirstHeading_Returns_SecondHeading()
        {
            Navigator.CurrentElementID = "ghGen3";
            XElement currentElement = Navigator.CurrentElement;
            // Should be:
            //  <h1 id="ghGen3" smilref="New_Zealand.smil#ghGen3">The Countries: New Zealand</h1>
            Assert.IsTrue(currentElement.Name.LocalName == "h1" &&
                            currentElement.Attribute("id").Value == "ghGen3");

            Navigator.DownLevel();
            currentElement = Navigator.CurrentElement;
            // Should be 
            //  <h1 id="ghGen24" smilref="New_Zealand.smil#ghGen24">Library of Congress Cataloging-in-Publication Data</h1>
            Assert.IsTrue(currentElement.Name.LocalName == "h1" &&
                            currentElement.Attribute("id").Value == "ghGen24");
        }


        /// <summary>
        /// Tests DownLevel where the next heading is at a higher hierarchical level than the current subheading.
        /// </summary>
        [TestMethod]
        public void DownLevel_FromSubheading_Returns_NextHeading()
        {
            Navigator.CurrentElementID = "ghGen165";

            Navigator.DownLevel();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            // <h1 id="ghGen179" smilref="New_Zealand.smil#ghGen179">Timeline</h1>
            Assert.IsTrue(currentElement.Name.LocalName == "h1" &&
                            currentElement.Attribute("id").Value == "ghGen179");
        }


        /// <summary>
        /// Tests DownLevel where the next heading is at a lower hierarchical level than the current subheading.
        /// </summary>
        [TestMethod]
        public void DownLevel_FromHeading_Returns_Subheading()
        {
            Navigator.CurrentElementID = "ghGen129";

            Navigator.DownLevel();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            // <h2 id="ghGen134" smilref="New_Zealand.smil#ghGen134">LAND</h2>
            Assert.IsTrue(currentElement.Name.LocalName == "h2" &&
                            currentElement.Attribute("id").Value == "ghGen134");
        }


        /// <summary>
        /// Tests DownLevel where the current element is not a heading/subheading.
        /// </summary>
        [TestMethod]
        public void DownLevel_FromMidText_Returns_NextSubheading()
        {
            Navigator.CurrentElementID = "ghGen158";

            Navigator.DownLevel();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            // <h2 id="ghGen165" smilref="New_Zealand.smil#ghGen165">ECONOMY</h2>
            Assert.IsTrue(currentElement.Name.LocalName == "h2" &&
                            currentElement.Attribute("id").Value == "ghGen165");
        }


        /// <summary>
        /// Tests DownLevel where the current element is not a heading/subheading, and at a lower hierarchical level.
        /// </summary>
        [TestMethod]
        public void DownLevel_FromMidText_Returns_NextHeading()
        {
            Navigator.CurrentElementID = "ghGen171";

            Navigator.DownLevel();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            // <h1 id="ghGen179" smilref="New_Zealand.smil#ghGen179">Timeline</h1>
            Assert.IsTrue(currentElement.Name.LocalName == "h1" &&
                            currentElement.Attribute("id").Value == "ghGen179");
        }

        #endregion


        #region UpLevel Tests

        /// <summary>
        /// Tests UpLevel where both headings are at the same hierarchical level.
        /// </summary>
        [TestMethod]
        public void UpLevel_FromSecondHeading_Returns_FirstHeading()
        {
            Navigator.CurrentElementID = "ghGen24";

            Navigator.UpLevel();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            //  <h1 id="ghGen3" smilref="New_Zealand.smil#ghGen3">The Countries: New Zealand</h1>
            Assert.IsTrue(currentElement.Name.LocalName == "h1" &&
                            currentElement.Attribute("id").Value == "ghGen3");
        }


        /// <summary>
        /// Tests UpLevel where the previous heading is at a higher hierarchical level than the current subheading.
        /// </summary>
        [TestMethod]
        public void UpLevel_FromSubheading_Returns_Heading()
        {
            Navigator.CurrentElementID = "ghGen134";

            Navigator.UpLevel();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            // <h1 id="ghGen129" smilref="New_Zealand.smil#ghGen129">Fast Facts</h1>
            Assert.IsTrue(currentElement.Name.LocalName == "h1" &&
                            currentElement.Attribute("id").Value == "ghGen129");
        }


        /// <summary>
        /// Tests UpLevel where the previous (sub)heading is at a lower hierarchical level than the current heading.
        /// </summary>
        [TestMethod]
        public void UpLevel_FromHeading_Returns_PreviousSubheading()
        {
            Navigator.CurrentElementID = "ghGen179";

            Navigator.UpLevel();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            // <h2 id="ghGen165" smilref="New_Zealand.smil#ghGen165">ECONOMY</h2>
            Assert.IsTrue(currentElement.Name.LocalName == "h2" &&
                            currentElement.Attribute("id").Value == "ghGen165");
        }


        /// <summary>
        /// Tests UpLevel where the current element is not a heading/subheading.
        /// </summary>
        /// <remarks>This should return the previous [sub]heading, not the heading of the current section.</remarks>
        [TestMethod]
        public void UpLevel_FromMidText_Returns_PreviousHeading()
        {
            Navigator.CurrentElementID = "ghGen140";

            Navigator.UpLevel();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            // <h1 id="ghGen129" smilref="New_Zealand.smil#ghGen129">Fast Facts</h1>
            Assert.IsTrue(currentElement.Name.LocalName == "h2" &&
                            currentElement.Attribute("id").Value == "ghGen134");
        }


        /// <summary>
        /// Tests UpLevel where the current element is not a heading/subheading, and where the previous heading is
        /// at a lower hierarchical level in a previous subsection.
        /// </summary>
        /// <remarks>This should return the previous subheading, not the heading of the current section.</remarks>
        [TestMethod]
        public void UpLevel_FromMidText_Returns_PreviousSubheading()
        {
            Navigator.CurrentElementID = "ghGen188";

            Navigator.UpLevel();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            // <h2 id="ghGen165" smilref="New_Zealand.smil#ghGen165">ECONOMY</h2>
            Assert.IsTrue(currentElement.Name.LocalName == "h1" &&
                            currentElement.Attribute("id").Value == "ghGen179");
        }

        #endregion

        #region NextPage Tests

        [TestMethod]
        public void NextPage_FromPageRef()
        {
            Navigator.CurrentElementID = "gh_pageRef1";
            Navigator.NextPage();

            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            //  <pagenum id="gh_pageRef20" smilref="New_Zealand.smil#gh_pageRef20">20</pagenum>
            Assert.IsTrue(currentElement.Name.LocalName == "pagenum" &&
                            currentElement.Attribute("id").Value == "gh_pageRef2");
        }

        [TestMethod]
        public void NextPage_FromSentenceInCurrentPage()
        {
            /*	Fragment being tested:
                <sent id="ghGen232" smilref="New_Zealand.smil#ghGen232">of the islands led other explorers there.</sent>
             */
            Navigator.CurrentElementID = "ghGen232";

            Navigator.NextPage();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            //  <pagenum id="gh_pageRef7" smilref="New_Zealand.smil#gh_pageRef7">7</pagenum>
            Assert.IsTrue(currentElement.Name.LocalName == "pagenum" &&
                            currentElement.Attribute("id").Value == "gh_pageRef10");
        }

        [TestMethod]
        public void NextPage_FromSentenceInCurrentPage_CurrentPageNumInPreviousHeading()
        {
            /*	Fragment being tested:
                <h1 id="ghGen213" smilref="New_Zealand.smil#ghGen213">History</h1>
             */
            Navigator.CurrentElementID = "ghGen213";

            Navigator.NextPage();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            //  <pagenum id="gh_pageRef7" smilref="New_Zealand.smil#gh_pageRef7">7</pagenum>
            Assert.IsTrue(currentElement.Name.LocalName == "pagenum" &&
                            currentElement.Attribute("id").Value == "gh_pageRef9");
        }

        #endregion

        #region PrevPage Tests

        [TestMethod]
        public void PrevPage_FromPageRef()
        {
            Navigator.CurrentElementID = "gh_pageRef7";
            Navigator.PrevPage();

            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            //  <pagenum id="gh_pageRef6" smilref="New_Zealand.smil#gh_pageRef6">6</pagenum>
            Assert.IsTrue(currentElement.Name.LocalName == "pagenum" &&
                            currentElement.Attribute("id").Value == "gh_pageRef6");
        }

        [TestMethod]
        public void PrevPage_FromSentenceInCurrentPage()
        {
            /*	Fragment being tested:
                <sent id="ghGen232" smilref="New_Zealand.smil#ghGen232">of the islands led other explorers there.</sent>
             */
            Navigator.CurrentElementID = "ghGen232";

            Navigator.PrevPage();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            //  <pagenum id="gh_pageRef9" smilref="New_Zealand.smil#gh_pageRef9">9</pagenum>
            Assert.IsTrue(currentElement.Name.LocalName == "pagenum" &&
                            currentElement.Attribute("id").Value == "gh_pageRef9");
        }

        [TestMethod]
        public void PrevPage_FromSentenceInCurrentPage_CurrentPageNumInPreviousHeading()
        {
            /*	Fragment being tested:
                <h1 id="ghGen213" smilref="New_Zealand.smil#ghGen213">History</h1>
             */
            Navigator.CurrentElementID = "ghGen213";

            Navigator.PrevPage();
            XElement currentElement = Navigator.CurrentElement;
            // Should be 
            //  <pagenum id="gh_pageRef8" smilref="New_Zealand.smil#gh_pageRef8">8</pagenum>
            Assert.IsTrue(currentElement.Name.LocalName == "pagenum" &&
                            currentElement.Attribute("id").Value == "gh_pageRef8");
        }

        #endregion
    }
}
