using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ICSharpCode.TextEditor.Document;

namespace PReviewer.UI
{
    public class PatchHighlighter
    {
        private readonly IDocument _document;
        private static readonly Color AddedColor = Color.FromArgb(200, 255, 200);
        private static readonly Color RemovedColor = Color.FromArgb(255, 200, 200);
        private static readonly Color HeaderColor = Color.FromArgb(230, 230, 230);
        private static readonly Color MarkerForeColor = Color.Black;
        private List<Tuple<int, int>> _newLinesSections;
        private List<Tuple<int, int>> _oldLinesSections;
        private List<Tuple<int, int>> _headers;

        public PatchHighlighter(IDocument document)
        {
            _document = document;
        }

        public void HighlightLinesBackground()
        {
            HighlightSections(_oldLinesSections, RemovedColor);
            HighlightSections(_newLinesSections, AddedColor);
            HighlightSections(_headers, HeaderColor);
        }

        private void HighlightSections(IEnumerable<Tuple<int, int>> sections, Color color)
        {
            foreach (var section in sections)
            {
                _document.MarkerStrategy.AddMarker(new TextMarker(section.Item1,
                    section.Item2 - section.Item1, TextMarkerType.SolidBlock, color,
                    MarkerForeColor));
            }
        }

        private static void AddToSection(IList<Tuple<int, int>> greens, LineSegment lineSegment)
        {
            if (!greens.Any())
            {
                greens.Add(new Tuple<int, int>(lineSegment.Offset, lineSegment.Offset + lineSegment.Length));
            }
            else
            {
                var lastGreen = greens.Last();
                if (lineSegment.Offset == lastGreen.Item2 + 1)
                {
                    var newEnd = lineSegment.Offset + lineSegment.Length;
                    greens.RemoveAt(greens.Count - 1);
                    greens.Add(new Tuple<int, int>(lastGreen.Item1, newEnd));
                }
                else
                {
                    greens.Add(new Tuple<int, int>(lineSegment.Offset, lineSegment.Offset + lineSegment.Length));
                }
            }
        }


        private void CalculateSections()
        {
            _newLinesSections = new List<Tuple<int, int>>();
            _oldLinesSections = new List<Tuple<int, int>>();
            _headers = new List<Tuple<int, int>>();
            for (var line = 0; line < _document.TotalNumberOfLines; ++line)
            {
                var lineSegment = _document.GetLineSegment(line);
                if (lineSegment.TotalLength == 0) { continue; }
                var firstChar = _document.GetCharAt(lineSegment.Offset);
                switch (firstChar)
                {
                    case '+':
                        AddToSection(_newLinesSections, lineSegment);
                        break;
                    case '-':
                        AddToSection(_oldLinesSections, lineSegment);
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
            HighlightLinesBackground();
        }
    }
}
