using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Vorcyc.FolderSync.Converters
{
    public class SizeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var size = (double)(long)value;
            return $"{size}(bytes) , {size / 1024:#0.0}(kb) , {size / 1024 / 1024:#0.0}(mb) , {size / 1024 / 1024 / 1024:#0.0}(gb)";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
