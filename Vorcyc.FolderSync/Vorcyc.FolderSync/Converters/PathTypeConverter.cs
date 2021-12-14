using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Vorcyc.FolderSync.Converters
{
    public class PathTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var pt = (PathType)value;
            switch (pt)
            {
                case PathType.Folder:
                    return "文件夹";
                case PathType.File:
                    return "文件";
                default:
                    throw new InvalidProgramException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
