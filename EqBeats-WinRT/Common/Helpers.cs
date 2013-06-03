using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace EqBeats_WinRT.Common {
    public static class Helpers {
        public static MediaElement FindMediaElement(this Window window) {
            try {
                var rootContent = VisualTreeHelper.GetChild(window.Content, 0);
                return (MediaElement)VisualTreeHelper.GetChild(rootContent, 0);
            } catch {
                return null;
            }
        }
    }
}
