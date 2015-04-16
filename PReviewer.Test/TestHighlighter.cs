using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PReviewer.Service;
using Shouldly;
namespace PReviewer.Test
{
    [TestFixture]
    public class TestHighlighter_Intersection
    {
        #region Intersection
        [Test]
        public void TestSingleIntersection()
        {
            var minusSections = new List<Section>();
            var plusSections = new List<Section>();

            minusSections.Add(new Section(0, 101, 1));
            minusSections.Add(new Section(200, 1010, 1));
            plusSections.Add(new Section(1011, 1031, 1));
            minusSections.Add(new Section(1200, 1310, 1));
            var intersections = HighlighterHelper.GetIntersections(minusSections, plusSections);
            intersections.Count.ShouldBe(1);
            intersections.First().Item1.ShouldBe(minusSections[1]);
            intersections.First().Item2.ShouldBe(plusSections[0]);
        }

        [Test]
        public void TestMultipleIntersections()
        {
            var minusSections = new List<Section>();
            var plusSections = new List<Section>();

            minusSections.Add(new Section(0, 101, 1));
            minusSections.Add(new Section(200, 1010, 1));
            plusSections.Add(new Section(1011, 1031, 1));
            minusSections.Add(new Section(1200, 1310, 1));
            minusSections.Add(new Section(1320, 1380, 1));
            minusSections.Add(new Section(1400, 1410, 1));
            plusSections.Add(new Section(1411, 1490, 1));
            minusSections.Add(new Section(1499, 1510, 1));
            plusSections.Add(new Section(1522, 1590, 1));
            var intersections = HighlighterHelper.GetIntersections(minusSections, plusSections);
            intersections.Count.ShouldBe(2);
            intersections.First().Item1.ShouldBe(minusSections[1]);
            intersections.First().Item2.ShouldBe(plusSections[0]);

            intersections[1].Item1.ShouldBe(minusSections[4]);
            intersections[1].Item2.ShouldBe(plusSections[1]);
        }

        [Test]
        public void TestNoIntersection()
        {
            var minusSections = new List<Section>();
            var plusSections = new List<Section>();
            minusSections.Add(new Section(0, 101, 1));
            minusSections.Add(new Section(200, 1010, 1));
            plusSections.Add(new Section(1012, 1031, 1));
            minusSections.Add(new Section(1200, 1310, 1));
            minusSections.Add(new Section(1320, 1380, 1));
            minusSections.Add(new Section(1400, 1410, 1));
            plusSections.Add(new Section(1413, 1490, 1));
            minusSections.Add(new Section(1499, 1510, 1));
            plusSections.Add(new Section(1522, 1590, 1));

            var intersections = HighlighterHelper.GetIntersections(minusSections, plusSections);
            intersections.Count.ShouldBe(0);
        }

        [Test]
        public void TestFirstPlusIsNotInAnyIntersection()
        {
            var minusSections = new List<Section>();
            var plusSections = new List<Section>();
            minusSections.Add(new Section(0, 101, 1));
            minusSections.Add(new Section(200, 1010, 1));
            plusSections.Add(new Section(1013, 1031, 1));
            minusSections.Add(new Section(1200, 1310, 1));
            minusSections.Add(new Section(1320, 1380, 1));
            minusSections.Add(new Section(1400, 1410, 1));
            plusSections.Add(new Section(1411, 1490, 1));
            minusSections.Add(new Section(1499, 1510, 1));
            plusSections.Add(new Section(1522, 1590, 1));

            var intersections = HighlighterHelper.GetIntersections(minusSections, plusSections);
            intersections.Count.ShouldBe(1);
            intersections[0].Item1.ShouldBe(minusSections[4]);
            intersections[0].Item2.ShouldBe(plusSections[1]);
        }

        [Test]
        public void TestIntersectionWithDifferentDelimiters()
        {
            var minusSections = new List<Section>();
            var plusSections = new List<Section>();

            minusSections.Add(new Section(0, 101, 1));
            minusSections.Add(new Section(200, 1010, 1));
            plusSections.Add(new Section(1011, 1031, 1));
            minusSections.Add(new Section(1200, 1310, 1));
            minusSections.Add(new Section(1320, 1380, 1));
            minusSections.Add(new Section(1400, 1410, 2));
            plusSections.Add(new Section(1412, 1490, 1));
            minusSections.Add(new Section(1499, 1510, 1));
            plusSections.Add(new Section(1522, 1590, 1));
            var intersections = HighlighterHelper.GetIntersections(minusSections, plusSections);
            intersections.Count.ShouldBe(2);
            intersections.First().Item1.ShouldBe(minusSections[1]);
            intersections.First().Item2.ShouldBe(plusSections[0]);

            intersections[1].Item1.ShouldBe(minusSections[4]);
            intersections[1].Item2.ShouldBe(plusSections[1]);
        }
        #endregion
    }

    [TestFixture]
    public class TestHighlighter_MergeSections
    {
        #region Add line to section

        [Test]
        public void GiveALineNotInAnyExistingSection_ANewSectionAdded()
        {
            var s1 = new Section(10, 20, 2);
            var s3 = new Section(80, 100, 2);
            var sections = new List<Section> { s1, s3 };

            var s2 = new Section(23, 78, 1);
            HighlighterHelper.AddToSection(sections, s2);

            sections.Count.ShouldBe(3);
            sections[0].ShouldBe(s1);
            sections[1].ShouldBe(s2);
            sections[2].ShouldBe(s3);
        }

        [Test]
        public void CanCombineWithNextSection()
        {
            var s1 = new Section(10, 20, 2);
            var s3 = new Section(80, 100, 2);
            var sections = new List<Section> { s1, s3 };

            var s2 = new Section(23, 79, 1);
            HighlighterHelper.AddToSection(sections, s2);

            sections.Count.ShouldBe(2);
            sections[0].ShouldBe(s1);
            sections[1].Start.ShouldBe(s2.Start);
            sections[1].DelimiterLength.ShouldBe(s3.DelimiterLength);
            sections[1].End.ShouldBe(s3.End);
        }

        [Test]
        public void CanCombineWithPreviousSection()
        {
            var s1 = new Section(10, 20, 2);
            var s3 = new Section(80, 100, 2);
            var sections = new List<Section> { s1, s3 };

            var s2 = new Section(22, 78, 1);
            HighlighterHelper.AddToSection(sections, s2);

            sections.Count.ShouldBe(2);
            sections[0].Start.ShouldBe(s1.Start);
            sections[0].End.ShouldBe(s2.End);
            sections[0].DelimiterLength.ShouldBe(s2.DelimiterLength);
            sections[1].ShouldBe(s3);
        }

        [Test]
        public void CanCombineWithBothNextAndPreviousSection()
        {
            var s1 = new Section(10, 20, 2);
            var s3 = new Section(80, 100, 2);
            var sections = new List<Section> { s1, s3 };

            var s2 = new Section(22, 79, 1);
            HighlighterHelper.AddToSection(sections, s2);

            sections.Count.ShouldBe(1);
            sections[0].Start.ShouldBe(s1.Start);
            sections[0].End.ShouldBe(s3.End);
            sections[0].DelimiterLength.ShouldBe(s3.DelimiterLength);
        }
        #endregion
    }
}
