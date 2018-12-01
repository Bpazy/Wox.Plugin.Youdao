using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Wox.Plugin.Youdao
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        private readonly Settings _settings;

        public SettingsControl(SettingsViewModel viewModel)
        {
            InitializeComponent();
            _settings = viewModel.Settings;
            DataContext = viewModel;
        }

        private void OnApplyBTClick(object sender, RoutedEventArgs e)
        {
            try
            {
                (this.DataContext as SettingsViewModel)?.Save();
                (sender as Button).IsEnabled = false;
            }
            catch (System.Exception)
            { }
        }

        private void appKeyTBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.applyBT.IsEnabled = true;
        }

        private void secretKeyTBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.applyBT.IsEnabled = true;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
            e.Handled = true;
        }
    }
}