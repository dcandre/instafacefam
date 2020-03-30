using System;
using System.Globalization;
using Xamarin.Forms;

namespace InstaFaceFam.NewsFeed
{
    public class StringNullOrEmptyBoolConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var theString = value as string;
            return !string.IsNullOrEmpty(theString);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
