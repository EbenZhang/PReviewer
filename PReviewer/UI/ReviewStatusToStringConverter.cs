using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using PReviewer.Domain;

namespace PReviewer.UI
{
    [ValueConversion(typeof (ReviewStatus), typeof (string))]
    public class ReviewStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (ReviewStatus?) value;
            if (status == null)
            {
                return "Hasn't been reviewd";
            }
            switch (status.Value)
            {
                case ReviewStatus.Reviewed:
                    return "Reviewed";
                case ReviewStatus.ConfirmLater:
                    return "Partially reviewed, will come back later";
                case ReviewStatus.HasntBeenReviewed:
                    return "Hasn't been reviewd";
            }
            throw new InvalidEnumArgumentException(@"Unknow status: " + status);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}