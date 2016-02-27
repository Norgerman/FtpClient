using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace FtpClient.Converters
{
    public class BoolConverter : IValueConverter
    {
        private bool _reverse;

        public BoolConverter(bool reverse)
        {
            _reverse = reverse;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (_reverse)
                return !(bool)value;
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (_reverse)
                return !(bool)value;
            else
                return value;
        }
    }

    public sealed class BoolConverterExtension : MarkupExtension
    {
        private bool _reverse = false;

        public bool Reverse
        {
            set
            {
                _reverse = value;
            }

            get
            {
                return _reverse;
            }
        }

        public BoolConverterExtension()
        {
            _reverse = false;
        }

        public BoolConverterExtension(bool reverse)
        {
            _reverse = reverse;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new BoolConverter(Reverse);
        }
    }
}
