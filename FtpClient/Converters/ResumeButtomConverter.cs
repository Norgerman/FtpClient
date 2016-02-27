using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace FtpClient.Converters
{
    public class ResumeButtomConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class ResumeButtomConverterExtension : MarkupExtension
    {

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new ResumeButtomConverter();
        }
    }
}
