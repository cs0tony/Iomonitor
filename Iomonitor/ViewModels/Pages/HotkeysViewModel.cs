using Iomonitor.Controls;
using Iomonitor.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace Iomonitor.ViewModels.Pages
{
    public partial class HotkeysViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        private readonly IContentDialogService _contentDialogService;

        public HotkeysViewModel(IContentDialogService contentDialogService)
        {
            _contentDialogService = contentDialogService;
        }

        [RelayCommand]
        private async Task OnShowDialog()
        {
            var hotkeyContentDialog = new SetHotkeyContentDialog(_contentDialogService.GetDialogHost(), UserSettings.Instance.TapToToggleLockHotkey);
            ContentDialogResult result = await hotkeyContentDialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.Primary:
                    UserSettings.Instance.TapToToggleLockHotkey = hotkeyContentDialog.ViewModel.CurrentSetedHotkey;
                    break;
                case ContentDialogResult.Secondary:
                    break;
                default: break;
            }
        }

        [RelayCommand]
        private async Task OnSetInternalHotkeyDialog()
        {
            var hotkeyContentDialog = new SetHotkeyContentDialog(_contentDialogService.GetDialogHost(), UserSettings.Instance.MonitorInternalHotkey);
            ContentDialogResult result = await hotkeyContentDialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.Primary:
                    UserSettings.Instance.MonitorInternalHotkey = hotkeyContentDialog.ViewModel.CurrentSetedHotkey;
                    break;
                case ContentDialogResult.Secondary:
                    break;
                default: break;
            }
        }

        [RelayCommand]
        private async Task OnSetCloneHotkeyDialog()
        {
            var hotkeyContentDialog = new SetHotkeyContentDialog(_contentDialogService.GetDialogHost(), UserSettings.Instance.MonitorCloneHotkey);
            ContentDialogResult result = await hotkeyContentDialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.Primary:
                    UserSettings.Instance.MonitorCloneHotkey = hotkeyContentDialog.ViewModel.CurrentSetedHotkey;
                    break;
                case ContentDialogResult.Secondary:
                    break;
                default: break;
            }
        }

        [RelayCommand]
        private async Task OnSetExtendHotkeyDialog()
        {
            var hotkeyContentDialog = new SetHotkeyContentDialog(_contentDialogService.GetDialogHost(), UserSettings.Instance.MonitorExtendHotkey);
            ContentDialogResult result = await hotkeyContentDialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.Primary:
                    UserSettings.Instance.MonitorExtendHotkey = hotkeyContentDialog.ViewModel.CurrentSetedHotkey;
                    break;
                case ContentDialogResult.Secondary:
                    break;
                default: break;
            }
        }

        [RelayCommand]
        private async Task OnSetExternalHotkeyDialog()
        {
            var hotkeyContentDialog = new SetHotkeyContentDialog(_contentDialogService.GetDialogHost(), UserSettings.Instance.MonitorExternalHotkey);
            ContentDialogResult result = await hotkeyContentDialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.Primary:
                    UserSettings.Instance.MonitorExternalHotkey = hotkeyContentDialog.ViewModel.CurrentSetedHotkey;
                    break;
                case ContentDialogResult.Secondary:
                    break;
                default: break;
            }
        }

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        public void OnNavigatedFrom() { }

        private void InitializeViewModel()
        {
            _isInitialized = true;
        }
    }
}