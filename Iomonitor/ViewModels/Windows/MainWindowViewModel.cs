using Iomonitor.Resources;
using System.Collections.ObjectModel;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace Iomonitor.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "Iomonitor";

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };
    }
}
