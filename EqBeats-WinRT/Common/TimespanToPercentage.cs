using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EqBeats_WinRT.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace EqBeats_WinRT.Common {
    public class TimespanToPercentage : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            var time = (TimeSpan)value;
            var player = Window.Current.FindMediaElement();
            if (player == null || !player.NaturalDuration.HasTimeSpan) {
                return 0;
            }

            return (time.TotalMilliseconds / player.NaturalDuration.TimeSpan.TotalMilliseconds) * 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            var percent = (double)value;
            var player = Window.Current.FindMediaElement();

            if (player == null) return TimeSpan.FromSeconds(0);

            return !player.NaturalDuration.HasTimeSpan
                ? TimeSpan.FromSeconds(0)
                : TimeSpan.FromSeconds(player.NaturalDuration.TimeSpan.TotalSeconds * percent / 100);
        }
    }

    public class PercentToSongLength : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            var percent = (double)value;
            var player = Window.Current.FindMediaElement();

            if (player == null) return "00:00";

            if (double.IsNaN(percent)) {
                percent = 100;
            }

            var span = !player.NaturalDuration.HasTimeSpan
                ? TimeSpan.FromSeconds(0)
                : TimeSpan.FromSeconds(player.NaturalDuration.TimeSpan.TotalSeconds * percent / 100);
            return span.ToString("mm':'ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }

    public class DurationExistsToVis : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            var duration = (Duration)value;
            return duration.HasTimeSpan ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }

    public class DurationNotExistsToVis : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            var duration = (Duration)value;
            return !duration.HasTimeSpan ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
