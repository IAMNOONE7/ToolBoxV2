using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ToolBoxV2.Presentation.WPF.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value?.ToString() == parameter?.ToString();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? Enum.Parse(targetType, parameter.ToString()) : Binding.DoNothing;
    }
}
