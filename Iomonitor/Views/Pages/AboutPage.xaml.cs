using Iomonitor.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Iomonitor.Views.Pages
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : INavigableView<AboutViewModel>
    {
        public AboutViewModel ViewModel { get; }
        public AboutPage(AboutViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
