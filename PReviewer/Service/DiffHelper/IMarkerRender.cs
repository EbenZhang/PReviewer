using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace PReviewer.Service.DiffHelper
{
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
}