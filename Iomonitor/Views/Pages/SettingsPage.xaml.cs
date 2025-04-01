using Iomonitor.ViewModels.Pages;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace Iomonitor.Views.Pages
{
    public partial class SettingsPage : INavigableView<SettingsViewModel>
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void ComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 则防止事件继续传播给 ComboBox
            // e.Handled = true;
            ComboBox? comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                // 在滚轮事件开始前，将 ComboBox 的 Focusable 设置为 false
                comboBox.Focusable = false;

                // 在滚轮事件处理完成后，恢复 ComboBox 的 Focusable 设置为 true
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    comboBox.Focusable = true;
                }), DispatcherPriority.ContextIdle);
            }
        }
    }
}
