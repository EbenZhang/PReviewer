﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Interactivity;
using NUnit.Framework;
using PReviewer.UI;
using Shouldly;
using Section = System.Tuple<int, int>;
namespace PReviewer.Test
{
    [TestFixture]
    public class TestPatchHighlighter
    {
        [Test]
        public void TestSingleIntersection()
        {
            var minusSections = new List<Section>();
            var plusSections = new List<Section>();

            minusSections.Add(new Section(0, 101));
            minusSections.Add(new Section(200, 1010));
            plusSections.Add(new Section(1011, 1031));
            minusSections.Add(new Section(1200, 1310));
            var intersections = PatchHighlighter.GetIntersections(minusSections, plusSections);
            intersections.Count.ShouldBe(1);
            intersections.First().Item1.ShouldBe(minusSections[1]);
            intersections.First().Item2.ShouldBe(plusSections[0]);
        }

        [Test]
        public void TestMultipleIntersections()
        {
            var minusSections = new List<Section>();
            var plusSections = new List<Section>();

            minusSections.Add(new Section(0, 101));
            minusSections.Add(new Section(200, 1010));
            plusSections.Add(new Section(1011, 1031));
            minusSections.Add(new Section(1200, 1310));
            minusSections.Add(new Section(1320, 1380));
            minusSections.Add(new Section(1400, 1410));
            plusSections.Add(new Section(1411, 1490));
            minusSections.Add(new Section(1499, 1510));
            plusSections.Add(new Section(1522, 1590));
            var intersections = PatchHighlighter.GetIntersections(minusSections, plusSections);
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
            minusSections.Add(new Section(0, 101));
            minusSections.Add(new Section(200, 1010));
            plusSections.Add(new Section(1012, 1031));
            minusSections.Add(new Section(1200, 1310));
            minusSections.Add(new Section(1320, 1380));
            minusSections.Add(new Section(1400, 1410));
            plusSections.Add(new Section(1413, 1490));
            minusSections.Add(new Section(1499, 1510));
            plusSections.Add(new Section(1522, 1590));

            var intersections = PatchHighlighter.GetIntersections(minusSections, plusSections);
            intersections.Count.ShouldBe(0);
        }

        [Test]
        public void TestFirstPlusIsNotInAnyIntersection()
        {
            var minusSections = new List<Section>();
            var plusSections = new List<Section>();
            minusSections.Add(new Section(0, 101));
            minusSections.Add(new Section(200, 1010));
            plusSections.Add(new Section(1013, 1031));
            minusSections.Add(new Section(1200, 1310));
            minusSections.Add(new Section(1320, 1380));
            minusSections.Add(new Section(1400, 1410));
            plusSections.Add(new Section(1411, 1490));
            minusSections.Add(new Section(1499, 1510));
            plusSections.Add(new Section(1522, 1590));

            var intersections = PatchHighlighter.GetIntersections(minusSections, plusSections);
            intersections.Count.ShouldBe(1);
            intersections[0].Item1.ShouldBe(minusSections[4]);
            intersections[0].Item2.ShouldBe(plusSections[1]);
        }
    }
}
