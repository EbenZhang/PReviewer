using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ICSharpCode.TextEditor.Document;
using Section = System.Tuple<int, int>;

namespace PReviewer.UI
{
    public class PatchHighlighter
    {
        private readonly IDocument _document;
        private static readonly Color AddedColor = Color.FromArgb(200, 255, 200);
        private static readonly Color RemovedColor = Color.FromArgb(255, 200, 200);
        private static readonly Color HeaderColor = Color.FromArgb(230, 230, 230);
        private static readonly Color AddedMarkerColor = Color.FromArgb(135, 255, 135);
        private static readonly Color RemovedMarkerColor = Color.FromArgb(255, 150, 150);
        private static readonly Color MarkerForeColor = Color.Black;
        private List<Section> _plusLinesSections;
        private List<Section> _minusLinesSections;
        private List<Section> _headers;

        public PatchHighlighter(IDocument document)
        {
            _document = document;
        }

        public void HighlightLinesBackground()
        {
            HighlightSections(_minusLinesSections, RemovedColor);
            HighlightSections(_plusLinesSections, AddedColor);
            HighlightSections(_headers, HeaderColor);
        }

        private void HighlightSections(IEnumerable<Section> sections, Color color)
        {
            foreach (var section in sections)
            {
                _document.MarkerStrategy.AddMarker(new TextMarker(section.Item1,
                    section.Item2 - section.Item1, TextMarkerType.SolidBlock, color,
                    MarkerForeColor));
            }
        }

        private static void AddToSection(IList<Section> greens, LineSegment lineSegment)
        {
            if (!greens.Any())
            {
                greens.Add(new Section(lineSegment.Offset, lineSegment.Offset + lineSegment.Length));
            }
            else
            {
                var lastGreen = greens.Last();
                if (lineSegment.Offset == lastGreen.Item2 + 1)
                {
                    var newEnd = lineSegment.Offset + lineSegment.Length;
                    greens.RemoveAt(greens.Count - 1);
                    greens.Add(new Section(lastGreen.Item1, newEnd));
                }
                else
                {
                    greens.Add(new Section(lineSegment.Offset, lineSegment.Offset + lineSegment.Length));
                }
            }
        }

        public void AddMarkerForDifferences()
        {
            if (!_minusLinesSections.Any())
            {
                return;
            }

            if (!_plusLinesSections.Any())
            {
                return;
            }

            var intersections = GetIntersections(_minusLinesSections, _plusLinesSections);

            foreach (var intersection in intersections)
            {
                MarkDifferenceForSection(intersection.Item1, intersection.Item2);
            }
        }

        private void MarkDifferenceForSection(Section minusSection, Section plusSection)
        {
            var minusIter = minusSection.Item1 + 1; // +1 for '-' sign
            var plusIter = plusSection.Item1 + 1;

            while (minusIter <= minusSection.Item2
                   && plusIter <= plusSection.Item2)
            {
                if (!_document.GetCharAt(minusIter).Equals(
                    _document.GetCharAt(plusIter)))
                {
                    break;
                }
                minusIter++;
                plusIter++;
            }

            var startPosForPlusSection = plusIter;
            var startPosForMinusSection = minusIter;

            minusIter = minusSection.Item2;
            plusIter = plusSection.Item2;
            while (minusIter >= minusSection.Item1
                   && plusIter >= plusSection.Item1)
            {
                if (!_document.GetCharAt(minusIter).Equals(
                    _document.GetCharAt(plusIter)))
                {
                    break;
                }
                minusIter--;
                plusIter--;
            }

            var endPosForPlusSection = plusIter;
            var endPosForMinusSection = minusIter;

            var markerStrategy = _document.MarkerStrategy;

            if (endPosForPlusSection > startPosForPlusSection)
            {
                markerStrategy.AddMarker(new TextMarker(startPosForPlusSection,
                                                        endPosForPlusSection - startPosForPlusSection + 1,
                                                        TextMarkerType.SolidBlock, AddedMarkerColor,
                                                        MarkerForeColor));
            }

            if (endPosForMinusSection > startPosForMinusSection)
            {
                markerStrategy.AddMarker(new TextMarker(startPosForMinusSection,
                                                        endPosForMinusSection - startPosForMinusSection + 1,
                                                        TextMarkerType.SolidBlock, RemovedMarkerColor,
                                                        MarkerForeColor));
            }
        }

        private void CalculateSections()
        {
            _plusLinesSections = new List<Section>();
            _minusLinesSections = new List<Section>();
            _headers = new List<Section>();
            for (var line = 0; line < _document.TotalNumberOfLines; ++line)
            {
                var lineSegment = _document.GetLineSegment(line);
                if (lineSegment.TotalLength == 0) { continue; }
                var firstChar = _document.GetCharAt(lineSegment.Offset);
                switch (firstChar)
                {
                    case '+':
                        AddToSection(_plusLinesSections, lineSegment);
                        break;
                    case '-':
                        AddToSection(_minusLinesSections, lineSegment);
                        break;
                    case '@':
                    case '\\':
                        AddToSection(_headers, lineSegment);
                        break;
                }
            }
        }

        public void Highlight()
        {
            CalculateSections();
            AddMarkerForDifferences();
            HighlightLinesBackground();
        }

        public static List<Tuple<Section, Section>> GetIntersections(List<Section> minusSections,
            List<Section> plusSections)
        {
            var ret = new List<Tuple<Section, Section>>();
            var preSucceedIndex = 0;
            foreach (var plus in plusSections)
            {
                var minusIndex = minusSections.FindIndex(preSucceedIndex, r =>
                   r.Item2 + 1 == plus.Item1);
                if (minusIndex == -1) continue;
                preSucceedIndex = minusIndex;
                ret.Add(new Tuple<Section, Section>(minusSections[minusIndex], plus));
            }
            return ret;
        }
    }
}
