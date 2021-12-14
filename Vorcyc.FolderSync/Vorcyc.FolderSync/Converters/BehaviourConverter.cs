using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Vorcyc.FolderSync.Converters
{
    public class BehaviourConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var pt = (PathType)values[0];
            var b = (Behaviour)values[1];

            if (b == Behaviour.Keep)
                return "维持现状";

            if (pt == PathType.File && b == Behaviour.Create)
                return "创建文件";
            else if (pt == PathType.File && b == Behaviour.Delete)
                return "删除文件";
            else if (pt == PathType.File && b == Behaviour.Override)
                return "覆盖更新";
            else if (pt == PathType.Folder && b == Behaviour.Create)
                return "创建目录";
            else if (pt == PathType.Folder && b == Behaviour.Delete)
                return "删除目录";

            throw new Exception();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
