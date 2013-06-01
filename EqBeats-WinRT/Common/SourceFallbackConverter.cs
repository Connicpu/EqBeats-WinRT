using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace EqBeats_WinRT.Common {
    public class SourceFallbackConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return value ?? parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value;
        }
    }
}
