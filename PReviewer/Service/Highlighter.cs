using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using DiffMatchPatch;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace PReviewer.Service
{
    public class Section
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
            return string.Format("Start {0}, End {1}, Len {2}, EOL {3}, Total: {4}",
                Start, End, LenWithoutEol, DelimiterLength, LenWithEol);
        }

        public int LenWithEol
        {
            get { return End - Start + DelimiterLength; }
        }

        public int LenWithoutEol
        {
            get { return End - Start; }
        }

        public class SectionLocator : IComparer<Section> 
        {
            public int Compare(Section x, Section y)
            {
                if (y.Start >= x.Start && y.Start < x.End)
                {
                    return 0;
                }
                if (y.Start < x.Start)
                {
                    return 1;
                }
                // (y.Start >= x.End)
                return -1;
            }
        }
    }

    public interface IMarkerRender
    {
        void DrawMarker(ISegment range, Color color);
    }

    class MarkerRender : IMarkerRender
    {
        private readonly TextView _textView;
        private readonly DrawingContext _drawingContext;

        public MarkerRender(TextView textView, DrawingContext drawingContext)
        {
            _textView = textView;
            _drawingContext = drawingContext;
        }

        public void DrawMarker(ISegment range, Color color)
        {
            var geoBuilder = new BackgroundGeometryBuilder { AlignToWholePixels = true, CornerRadius = 3 };
            geoBuilder.AddSegment(_textView, range);
            var geometry = geoBuilder.CreateGeometry();
            if (geometry == null) return;
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            _drawingContext.DrawGeometry(brush, null, geometry);
        }
    }

    public class HighlighterHelper
    {
        public static readonly Color PlusLineMarkerColor = Color.FromRgb(135, 255, 135);
        public static readonly Color MinusLineMarkerColor = Color.FromRgb(255, 150, 150);
        private static readonly Color PlusLineColor = Color.FromRgb(200, 255, 200);
        private static readonly Color MinusLineColor = Color.FromRgb(255, 200, 200);
        private static readonly Color HeaderLineColor = Color.FromRgb(230, 230, 230);

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

        public static void AddToSection(List<Section> existingSections, Section newSection)
        {
            var index = existingSections.BinarySearch(newSection, new Section.SectionLocator());

            if (index >= 0) return;

            var insertPos = ~index;
            var prePos = insertPos - 1;
            var nextPos = insertPos;
            var preItem = prePos >= 0 ? existingSections[prePos] : null;
            var nextItem = nextPos < existingSections.Count ? existingSections[nextPos] : null;

            if (preItem != null)
            {
                if (TryMergeSections(preItem, newSection))
                {
                    if (nextItem != null && TryMergeSections(preItem, nextItem))
                    {
                        existingSections.RemoveAt(nextPos);
                        return;
                    }
                    return;
                }
            }

            if (nextItem != null)
            {
                if (TryMergeSections(newSection, nextItem))
                {
                    nextItem.Start = newSection.Start;
                    nextItem.End = newSection.End;
                    nextItem.DelimiterLength = newSection.DelimiterLength;
                    return;
                }
            }

            existingSections.Insert(insertPos, newSection);
        }

        private static bool TryMergeSections(Section preItem, Section newSection)
        {
            if (preItem.End + preItem.DelimiterLength != newSection.Start)
            {
                return false;
            }

            preItem.End = newSection.End;
            preItem.DelimiterLength = newSection.DelimiterLength;
            return true;
        }

        private static void MarkRange(IMarkerRender render, int start, int len, Color color)
        {
            var range = new TextSegment()
            {
                StartOffset = start,
                Length = len,
            };
            render.DrawMarker(range, color);
        }
        public static void MarkDifferenceForSection(Section minusSection, Section plusSection,
            IMarkerRender markerRender, ITextSource doc,
            IDiffer differ)
        {
            var minusText = doc.GetText(minusSection.Start, minusSection.LenWithEol);
            var plusText = doc.GetText(plusSection.Start, plusSection.LenWithEol).Replace("\n+", "\n-");
            if (plusText.StartsWith("+"))
            {
                plusText = "-" + plusText.Remove(0, 1);
            }
            var diffsForPlusSection = differ.GetDiffs(minusText, plusText);

            var pos4Minus = minusSection.Start;
            var pos4Plus = plusSection.Start;
            foreach (var diff in diffsForPlusSection)
            {
                switch (diff.operation)
                {
                    case Operation.EQUAL:
                    {
                        pos4Minus += diff.text.Length;
                        pos4Plus += diff.text.Length;
                        break;
                    }
                    case Operation.DELETE:
                    {
                        MarkRange(markerRender, pos4Minus, diff.text.Length, MinusLineMarkerColor);
                        pos4Minus += diff.text.Length;
                        break;
                    }

                    case Operation.INSERT:
                    {
                        MarkRange(markerRender, pos4Plus, diff.text.Length, PlusLineMarkerColor);
                        pos4Plus += diff.text.Length;
                        break;
                    }
                }
            }
        }
    }
    class Highlighter : IBackgroundRenderer
    {
        private readonly List<Section> _plusLinesSections = new List<Section>();
        private readonly List<Section> _minusLinesSections = new List<Section>();
        private readonly List<Section> _headers = new List<Section>();

        private static readonly IDiffer _differ = new GoogleDifferAdp();

        private static readonly Color PlusLineColor = Color.FromRgb(200, 255, 200);
        private static readonly Color MinusLineColor = Color.FromRgb(255, 200, 200);
        private static readonly Color HeaderLineColor = Color.FromRgb(230, 230, 230);

        public Highlighter(ITextEditorComponent textView)
        {
            textView.Document.TextChanged += (sender, args) =>
            {
                _plusLinesSections.Clear();
                _minusLinesSections.Clear();
                _headers.Clear();
            };
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            var visualLines = textView.VisualLines;
            if (visualLines.Count == 0)
            {
                return;
            }

            var markerRender = new MarkerRender(textView, drawingContext);

            CalculateSections(visualLines);

            HighlightLinesBackground(markerRender);

            AddMarkerForDifferences(markerRender, textView.Document);
        }

        private void CalculateSections(IEnumerable<VisualLine> visualLines)
        {
            foreach (var lineSegment in visualLines)
            {
                if (lineSegment.VisualLength == 0) { continue; }

                var eolDeviation = lineSegment.VisualLength == lineSegment.VisualLengthWithEndOfLineMarker?
                    0 : lineSegment.VisualLengthWithEndOfLineMarker - lineSegment.VisualLength;

                var section = new Section(lineSegment.StartOffset,
                    lineSegment.LastDocumentLine.EndOffset,
                    lineSegment.LastDocumentLine.DelimiterLength);

                var firstChar = lineSegment.Document.GetCharAt(lineSegment.StartOffset);
                switch (firstChar)
                {
                    case '+':
                        HighlighterHelper.AddToSection(_plusLinesSections, section);
                        break;
                    case '-':
                        HighlighterHelper.AddToSection(_minusLinesSections, section);
                        break;
                    case '@':
                    case '\\':
                        HighlighterHelper.AddToSection(_headers, section);
                        break;
                }
            }
        }

        private void AddMarkerForDifferences(IMarkerRender markerRender, ITextSource textSource)
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
                HighlighterHelper.MarkDifferenceForSection(intersection.Item1,
                    intersection.Item2, markerRender, textSource, _differ);
            }
        }
        
        public KnownLayer Layer
        {
            get { return KnownLayer.Background; }
        }

        private void HighlightLinesBackground(IMarkerRender markerRender)
        {
            HighlightSections(_minusLinesSections, MinusLineColor, markerRender);
            HighlightSections(_plusLinesSections, PlusLineColor, markerRender);
            HighlightSections(_headers, HeaderLineColor, markerRender);
        }

        private static void HighlightSections(IEnumerable<Section> sections, Color color, IMarkerRender markerRender)
        {
            foreach (var section in sections)
            {
                var range = new TextSegment()
                {
                    StartOffset = section.Start,
                    EndOffset = section.End,
                    Length = section.End - section.Start + 1,
                };

                markerRender.DrawMarker(range, color);
            }
        }
    }
}
