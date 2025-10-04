using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CastleStoryLauncher
{
    public partial class DependencyConflictWindow : Window
    {
        private List<DependencyConflict> conflicts;

        public DependencyConflictWindow(List<DependencyConflict> conflicts)
        {
            InitializeComponent();
            this.conflicts = conflicts;
            ConflictsListBox.ItemsSource = conflicts;
        }

        private void ResolveButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement auto-resolution logic
            MessageBox.Show("Auto-resolution feature will be implemented in a future update.", 
                "Feature Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    // Color converter for severity levels
    public class SeverityColorConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string severity)
            {
                return severity switch
                {
                    "Error" => new SolidColorBrush(Colors.Red),
                    "Warning" => new SolidColorBrush(Colors.Orange),
                    "Info" => new SolidColorBrush(Colors.LightBlue),
                    _ => new SolidColorBrush(Colors.White)
                };
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
