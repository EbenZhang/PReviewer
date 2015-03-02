using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PReviewer.Model;

namespace PReviewer.UI
{
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class FileStatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value.ToString();
            switch (status)
            {
                case GitFileStatus.Modified:
                    return new BitmapImage(new Uri("/Images/modified.ico", UriKind.Relative));
                case GitFileStatus.New:
                    return new BitmapImage(new Uri("/Images/added.ico", UriKind.Relative));
                case GitFileStatus.Removed:
                    return new BitmapImage(new Uri("/Images/removed.ico", UriKind.Relative));
                case GitFileStatus.Renamed:
                    return new BitmapImage(new Uri("/Images/renamed.ico", UriKind.Relative));
            }
            throw new InvalidEnumArgumentException(@"Unknow status: " + status);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
