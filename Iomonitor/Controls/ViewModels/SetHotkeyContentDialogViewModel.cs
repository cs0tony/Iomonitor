using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Iomonitor.Controls.ViewModels
{
    public partial class SetHotkeyContentDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _primaryButtonEnabled = true;

        [ObservableProperty]
        private string _warnTextVisible = "Hidden";

        [ObservableProperty]
        private string _currentSetedHotkey = "";

        public SetHotkeyContentDialogViewModel(string currentHotkey)
        {
            _currentSetedHotkey = currentHotkey;
        }

        [RelayCommand]
        private void KeyDownHandleForWindow(KeyEventArgs e)
        {
            // 检查是否按下了Win键
            bool isWinPressed = Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);
            // 检查是否按下了Ctrl键
            bool isCtrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            // 检查是否按下了Shift键
            bool isShiftPressed = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
            // 检查是否按下了Alt键
            bool isAltPressed = (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;

            List<string> combList = new List<string>();
            if (isWinPressed) combList.Add("Win");
            if (isCtrlPressed) combList.Add("Control");
            if (isShiftPressed) combList.Add("Shift");
            if (isAltPressed) combList.Add("Alt");
            if (!(e.Key == Key.LWin ||
                e.Key == Key.RWin ||
                e.Key == Key.LeftCtrl ||
                e.Key == Key.RightCtrl ||
                e.Key == Key.LeftShift ||
                e.Key == Key.RightShift ||
                e.Key == Key.LeftAlt ||
                e.Key == Key.RightAlt ||
                e.Key == Key.System))
            {
                if (combList.Count == 0)
                {
                    WarnTextVisible = "Visible";
                    PrimaryButtonEnabled = false;
                }
                else
                {
                    WarnTextVisible = "Hidden";
                    PrimaryButtonEnabled = true;
                }
                combList.Add(e.Key.ToString());
            }
            else
            {
                WarnTextVisible = "Hidden";
                PrimaryButtonEnabled = false;
            }
            CurrentSetedHotkey = string.Join("+", combList);
            e.Handled = true;
        }
    }
}
