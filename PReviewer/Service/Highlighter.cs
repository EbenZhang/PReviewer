using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace PReviewer.Service
{
    public struct Section
    {
        public int Start;
        public int End;
        public int DelimiterLength;

        public Section(int start, int end, int delimiterLength)
        {
            Start = start;
            End = end;
            DelimiterLength = delimiterLength;
        }

        public override string ToString()
        {
            return string.Format("Start {0}, End {1}, Len {2}, EOL {3}", Start, End, End - Start + 1, DelimiterLength);
        }
    }

    public class HighlighterHelper
    {
        public static List<Tuple<Section, Section>> GetIntersections(List<Section> minusSections,
            List<Section> plusSections)
        {
            var ret = new List<Tuple<Section, Section>>();
            var preSucceedIndex = 0;
            foreach (var plus in plusSections)
            {
                var minusIndex = minusSections.FindIndex(preSucceedIndex, r =>
                   r.End + r.DelimiterLength == plus.Start);
                if (minusIndex == -1) continue;
                preSucceedIndex = minusIndex;
                ret.Add(new Tuple<Section, Section>(minusSections[minusIndex], plus));
            }
            return ret;
        }
    }
    class Highlighter : IBackgroundRenderer
    {
        private List<Section> _plusLinesSections;
        private List<Section> _minusLinesSections;
        private List<Section> _headers;

        private static readonly Color PlusLineColor = Color.FromRgb(200, 255, 200);
        private static readonly Color MinusLineColor = Color.FromRgb(255, 200, 200);
        private static readonly Color HeaderLineColor = Color.FromRgb(230, 230, 230);
        private static readonly Color PlusLineMarkerColor = Color.FromRgb(135, 255, 135);
        private static readonly Color MinusLineMarkerColor = Color.FromRgb(255, 150, 150);

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            var visualLines = textView.VisualLines;
            if (visualLines.Count == 0)
            {
                return;
            }

            CalculateSections(visualLines);

            HighlightLinesBackground(textView, drawingContext);

            AddMarkerForDifferences(textView, drawingContext);
        }

        private void CalculateSections(IEnumerable<VisualLine> visualLines)
        {
            _plusLinesSections = new List<Section>();
            _minusLinesSections = new List<Section>();
            _headers = new List<Section>();
            foreach (var lineSegment in visualLines)
            {
                if (lineSegment.VisualLength == 0) { continue; }
                var firstChar = lineSegment.Document.GetCharAt(lineSegment.StartOffset);
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

        private void AddMarkerForDifferences(TextView textView, DrawingContext drawingContext)
        {
            if (!_minusLinesSections.Any())
            {
                return;
            }

            if (!_plusLinesSections.Any())
            {
                return;
            }

            var intersections = HighlighterHelper.GetIntersections(_minusLinesSections, _plusLinesSections);

            foreach (var intersection in intersections)
            {
                MarkDifferenceForSection(intersection.Item1, intersection.Item2, textView, drawingContext);
            }
        }

        private static void MarkDifferenceForSection(Section minusSection, Section plusSection,
            TextView textView, DrawingContext drawingContext)
        {
            var doc = textView.Document;
            var minusIter = minusSection.Start + 1; // +1 for '-' sign
            var plusIter = plusSection.Start + 1;

            while (minusIter < minusSection.End
                   && plusIter < plusSection.End)
            {
                if (!doc.GetCharAt(minusIter).Equals(
                    doc.GetCharAt(plusIter)))
                {
                    break;
                }
                minusIter++;
                plusIter++;
            }

            var startPosForPlusSection = plusIter;
            var startPosForMinusSection = minusIter;

            minusIter = minusSection.End - 1;
            plusIter = plusSection.End - 1;
            while (minusIter > minusSection.Start
                   && plusIter > plusSection.Start)
            {
                if (!doc.GetCharAt(minusIter).Equals(
                    doc.GetCharAt(plusIter)))
                {
                    break;
                }
                minusIter--;
                plusIter--;
            }

            var endPosForPlusSection = plusIter;
            var endPosForMinusSection = minusIter;

            
            if (endPosForPlusSection > startPosForPlusSection)
            {
                var range = new TextSegment()
                {
                    StartOffset = startPosForPlusSection,
                    EndOffset = endPosForPlusSection,
                    Length = endPosForPlusSection - startPosForPlusSection + 1,
                };

                DrawMarker(textView, drawingContext, range, PlusLineMarkerColor);
            }

            if (endPosForMinusSection > startPosForMinusSection)
            {
                var range = new TextSegment()
                {
                    StartOffset = startPosForMinusSection,
                    EndOffset = endPosForMinusSection,
                    Length = endPosForMinusSection - startPosForMinusSection + 1,
                };
                DrawMarker(textView, drawingContext, range, MinusLineMarkerColor);
            }
        }

        private static void DrawMarker(TextView textView, DrawingContext drawingContext,
            ISegment range,
            Color color)
        {
            var geoBuilder = new BackgroundGeometryBuilder {AlignToWholePixels = true, CornerRadius = 3};
            geoBuilder.AddSegment(textView, range);
            var geometry = geoBuilder.CreateGeometry();
            if (geometry == null) return;
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            drawingContext.DrawGeometry(brush, null, geometry);
        }

        private static void AddToSection(IList<Section> existingSections, VisualLine lineSegment)
        {
            var delimiterLen = lineSegment.LastDocumentLine.DelimiterLength;
            if (!existingSections.Any())
            {
                existingSections.Add(new Section(lineSegment.StartOffset, 
                    lineSegment.StartOffset + lineSegment.VisualLength,
                    delimiterLen));
            }
            else
            {
                var lastSection = existingSections.Last();
                if (lineSegment.StartOffset == lastSection.End + lastSection.DelimiterLength)
                {
                    var newEnd = lineSegment.StartOffset + lineSegment.VisualLength;
                    existingSections.RemoveAt(existingSections.Count - 1);
                    existingSections.Add(new Section(lastSection.Start, newEnd, delimiterLen));
                }
                else
                {
                    existingSections.Add(new Section(lineSegment.StartOffset, lineSegment.StartOffset + lineSegment.VisualLength, delimiterLen));
                }
            }
        }

        public KnownLayer Layer
        {
            get { return KnownLayer.Background; }
        }

        private void HighlightLinesBackground(TextView textView, DrawingContext drawingContext)
        {
            HighlightSections(_minusLinesSections, textView, drawingContext, MinusLineColor);
            HighlightSections(_plusLinesSections, textView, drawingContext, PlusLineColor);
            HighlightSections(_headers, textView, drawingContext, HeaderLineColor);
        }

        private static void HighlightSections(IEnumerable<Section> sections, TextView textView, 
            DrawingContext drawingContext, Color color)
        {
            foreach (var section in sections)
            {
                var range = new TextSegment()
                {
                    StartOffset = section.Start,
                    EndOffset = section.End,
                    Length = section.End - section.Start + 1,
                };

                DrawMarker(textView, drawingContext, range, color);
            }
        }
    }
}
