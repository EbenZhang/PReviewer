using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using PReviewer.Service.DiffHelper;

namespace PReviewer.UI
{
    class DiffViewerLineNumberCtrl : LineNumberMargin
    {
        const double TextHorizontalMargin = 4.0;

        private static readonly Pen NullPen;

        private int _maxValueOfLineNum = 0;

        static DiffViewerLineNumberCtrl()
        {
            NullPen = new Pen();
            NullPen.Freeze();
        }

        public DiffViewerLineNumberCtrl()
        {
            DiffLines = new Dictionary<int, DiffLineNum>();
        }

        private Typeface _typeFace;
        private FormattedText _FormattedMaxLineNumber;

        private Typeface TypeFace
        {
            get
            {
                return _typeFace ?? (_typeFace = CreateTypeface());
            }
        }

        private Typeface CreateTypeface()
        {
            return new Typeface((FontFamily)TextView.GetValue(TextBlock.FontFamilyProperty), (FontStyle)TextView.GetValue(TextBlock.FontStyleProperty), (FontWeight)TextView.GetValue(TextBlock.FontWeightProperty), (FontStretch)TextView.GetValue(TextBlock.FontStretchProperty));
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (!DiffLines.Any())
            {
                return new Size(0.0, 0.0);
            }

            var emSize = (double)GetValue(TextBlock.FontSizeProperty);

            var typeFace = TypeFace;
            _FormattedMaxLineNumber = new FormattedText(_maxValueOfLineNum.ToString(), 
                CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeFace, emSize, Brushes.White);
            return new Size(GetRequiredWidth(), 0.0);
        }

        private double GetRequiredWidth()
        {
            return _FormattedMaxLineNumber.Width * 2.0 + TextHorizontalMargin * 2.0;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (!DiffLines.Any())
            {
                return;
            }

            var totalWidth = GetRequiredWidth();
            var leftWidth = totalWidth/2.0;
            var rightWidth = ActualWidth - leftWidth;

            var visualLines = TextView.VisualLinesValid ? TextView.VisualLines : Enumerable.Empty<VisualLine>();
            foreach (var line in visualLines)
            {
                var rect = BackgroundGeometryBuilder.GetRectsFromVisualSegment(TextView, line, 
                    0, 1000).ToArray();
                var lineNumber = line.FirstDocumentLine.LineNumber;
                if (DiffLines.ContainsKey(lineNumber))
                {
                    var diffLine = DiffLines[lineNumber];
                    if (diffLine.Style != DiffLineNum.DiffLineStyle.Context)
                    {
                        var brush = default(Brush);
                        switch (diffLine.Style)
                        {
                            case DiffLineNum.DiffLineStyle.Plus:
                                brush = new SolidColorBrush(HighlighterHelper.PlusLineColor);
                                break;
                            case DiffLineNum.DiffLineStyle.Minus:
                                brush = new SolidColorBrush(HighlighterHelper.MinusLineColor);
                                break;
                            case DiffLineNum.DiffLineStyle.Header:
                                brush = new SolidColorBrush(HighlighterHelper.HeaderLineColor);
                                break;
                        }
                        
                        foreach (var rc in rect)
                        {
                            drawingContext.DrawRectangle(brush, NullPen, new Rect(0.0, rc.Top, leftWidth, rc.Height));

                            drawingContext.DrawRectangle(brush, NullPen, new Rect(leftWidth, rc.Top, rightWidth, rc.Height));
                        }
                    }

                    var emSize = (double)GetValue(TextBlock.FontSizeProperty);

                    if (diffLine.LeftLineNum != DiffLineNum.NotApplicableLineNum)
                    {
                        var ft = new FormattedText(diffLine.LeftLineNum.ToString(),
                            CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                            TypeFace, emSize, Brushes.Black);

                        drawingContext.DrawText(ft, new Point(TextHorizontalMargin, rect[0].Top));
                    }

                    if (diffLine.RightLineNum != DiffLineNum.NotApplicableLineNum)
                    {
                        var ft = new FormattedText(diffLine.RightLineNum.ToString(),
                            CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                            TypeFace, emSize, Brushes.Black);

                        drawingContext.DrawText(ft, new Point(TextHorizontalMargin + totalWidth / 2, rect[0].Top));
                    }
                }
            }
        }

        private Dictionary<int, DiffLineNum> DiffLines { get; set; }

        public void AddDiffLineNum(DiffLineNum diffLineNum)
        {
            DiffLines.Add(diffLineNum.LineNumIfDiff, diffLineNum);
            _maxValueOfLineNum = Math.Max(diffLineNum.LeftLineNum, diffLineNum.RightLineNum);
        }

        public void Clear()
        {
            DiffLines.Clear();
        }

        public string GetLineDesc(int lineNumInDiffFile)
        {
            DiffLineNum line;
            if (!DiffLines.TryGetValue(lineNumInDiffFile, out line)) return null;

            if (line.LeftLineNum != DiffLineNum.NotApplicableLineNum)
            {
                return "L" + line.LeftLineNum;
            }
            if (line.RightLineNum != DiffLineNum.NotApplicableLineNum)
            {
                return "R" + line.RightLineNum;
            }
            return null;
        }
    }
}
