using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace FtpClient.Converters
{
    public class ConnectButtonStatusConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool v1 = (bool)values[0];
            bool v2 = (bool)values[1];
            return !(v1 || v2);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class ConnectButtonStatusConverterExtension : MarkupExtension
    {

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new ConnectButtonStatusConverter();
        }
    }
}
