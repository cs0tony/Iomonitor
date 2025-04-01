using Gma.System.MouseKeyHook;
using Iomonitor.Controls.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Forms = System.Windows.Forms;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Iomonitor.Controls
{
    /// <summary>
    /// SetHotkeyContentDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SetHotkeyContentDialog : ContentDialog
    {
        public SetHotkeyContentDialogViewModel ViewModel { get; }
        public SetHotkeyContentDialog(ContentPresenter? contentPresenter, string currentHotkey)
        : base(contentPresenter)
        {
            // 添加监听键盘
            ListenToKeyboard();
            Application.Current.Activated += OnGotFocus;
            Application.Current.Deactivated += OnLostFocus;
            ViewModel = new SetHotkeyContentDialogViewModel(currentHotkey);
            DataContext = this;
            InitializeComponent();
        }

        private string SetedHotkey = "";

        private IKeyboardMouseEvents? hotkeyGlobalHook;

        private void ListenToKeyboard()
        {
            if (hotkeyGlobalHook == null)
            {
                hotkeyGlobalHook = Hook.GlobalEvents();
                hotkeyGlobalHook.KeyDown += KeyDownHandle;
                hotkeyGlobalHook.KeyUp += KeyUpHandle;
            }
        }

        private void CancelListenToKeyboard()
        {
            if (hotkeyGlobalHook != null)
            {
                hotkeyGlobalHook?.Dispose();
                hotkeyGlobalHook = null;
            }
        }

        private List<string> combList = new List<string> { "", "", "", "", "" };

        private void KeyDownHandle(object? sender, Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Forms.Keys.LWin || e.KeyCode == Forms.Keys.RWin)
            {
                combList[0] = "Win";
            }
            else if (e.KeyCode == Forms.Keys.LControlKey || e.KeyCode == Forms.Keys.RControlKey || e.KeyCode == Forms.Keys.ControlKey)
            {
                combList[1] = "Control";
            }
            else if (e.KeyCode == Forms.Keys.LMenu || e.KeyCode == Forms.Keys.RMenu)
            {
                combList[2] = "Alt";
            }
            else if (e.KeyCode == Forms.Keys.LShiftKey || e.KeyCode == Forms.Keys.RShiftKey)
            {
                combList[3] = "Shift";
            }
            else
            {
                combList[4] = e.KeyCode.ToString();
            }
            
            if (combList[4] == "")
            {
                ViewModel.WarnTextVisible = "Hidden";
                ViewModel.PrimaryButtonEnabled = false;
            }
            else
            {
                if (combList[0] == "" &&
                    combList[1] == "" &&
                    combList[2] == "" &&
                    combList[3] == "")
                {
                    ViewModel.WarnTextVisible = "Visible";
                    ViewModel.PrimaryButtonEnabled = false;
                }
                else
                {
                    ViewModel.WarnTextVisible = "Hidden";
                    ViewModel.PrimaryButtonEnabled = true;
                }
            }
            ViewModel.CurrentSetedHotkey = string.Join("+", combList.Where(s => !string.IsNullOrEmpty(s)));
            e.Handled = true;
        }

        private void KeyUpHandle(object? sender, Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Forms.Keys.LWin || e.KeyCode == Forms.Keys.RWin)
            {
                combList[0] = "";
            }
            else if (e.KeyCode == Forms.Keys.LControlKey || e.KeyCode == Forms.Keys.RControlKey || e.KeyCode == Forms.Keys.ControlKey)
            {
                combList[1] = "";
            }
            else if (e.KeyCode == Forms.Keys.LMenu || e.KeyCode == Forms.Keys.RMenu)
            {
                combList[2] = "";
            }
            else if (e.KeyCode == Forms.Keys.LShiftKey || e.KeyCode == Forms.Keys.RShiftKey)
            {
                combList[3] = "";
            }
            else
            {
                combList[4] = "";
            }
            e.Handled = true;
        }

        private void OnGotFocus(object? sender, EventArgs e)
        {
            // 添加监听键盘
            ListenToKeyboard();
            //InputMethod.SetIsInputMethodEnabled(this, false);
        }

        private void OnLostFocus(object? sender, EventArgs e)
        {
            // 取消监听键盘
            CancelListenToKeyboard();
        }

        protected override void OnButtonClick(ContentDialogButton button)
        {
            // 取消监听键盘
            CancelListenToKeyboard();
            Application.Current.Activated -= OnGotFocus;
            Application.Current.Deactivated -= OnLostFocus;
            base.OnButtonClick(button);
        }
    }
}
