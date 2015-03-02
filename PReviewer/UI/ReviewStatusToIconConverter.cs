using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using PReviewer.Domain;

namespace PReviewer.UI
{
    [ValueConversion(typeof (ReviewStatus), typeof (BitmapImage))]
    public class ReviewStatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (ReviewStatus?) value;
            if (status == null)
            {
                return new BitmapImage(new Uri("/Images/new.png", UriKind.Relative));
            }
            switch (status.Value)
            {
                case ReviewStatus.Reviewed:
                    return new BitmapImage(new Uri("/Images/reviewed.png", UriKind.Relative));
                case ReviewStatus.ConfirmLater:
                    return new BitmapImage(new Uri("/Images/question.png", UriKind.Relative));
                case ReviewStatus.HasntBeenReviewed:
                    return new BitmapImage(new Uri("/Images/new.png", UriKind.Relative));
            }
            throw new InvalidEnumArgumentException(@"Unknow status: " + status);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}