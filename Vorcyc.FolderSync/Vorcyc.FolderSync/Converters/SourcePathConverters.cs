using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Vorcyc.FolderSync.Converters
{
    [ValueConversion(typeof(Behaviour), typeof(Brush))]
    public class SourcePathColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = (Behaviour)value;
            if (b == Behaviour.Create)
                return new SolidColorBrush(Color.FromRgb(5, 166, 119));
            else if (b == Behaviour.Delete)
                return new SolidColorBrush(Color.FromRgb(238, 80, 80));
            else if (b == Behaviour.Override)
                return Brushes.Orange;

            //Vorcyc.ModernUI.Presentation.AppearanceManager.Current.
            //return new SolidColorBrush(Color.FromRgb(209, 209, 209))
            var normalBrush = (SolidColorBrush)App.Current.FindResource("DataGridForeground");
            return normalBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class SourcePathToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = (Behaviour)value;
            if (b == Behaviour.Create)
                return "目标文件或文件夹不存在，待创建";
            else if (b == Behaviour.Delete)
                return "该源不存在，需删除目标文件";
            else if (b == Behaviour.Override)
                return "源文件和目标文件大小不一致，需覆盖更新";

            return "源和目标都存在，保持不变";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
