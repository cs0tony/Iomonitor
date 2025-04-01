using System.Configuration;
using Wpf.Ui.Appearance;
using Wpf.Ui;
using Forms = System.Windows.Forms;
using System.ComponentModel;
using Gma.System.MouseKeyHook;
using System.Diagnostics;
using Iomonitor.Interop;
using WinEventHook;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Threading;
using Iomonitor.Resources;
using Iomonitor.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace Iomonitor.Models
{
    class UserSettings : ConfigurationSection, INotifyPropertyChanged
    {
        public static UserSettings Instance { get; } = (UserSettings)App.Cfg.GetSection("userSettings");

        public event PropertyChangedEventHandler PropertyChanged;

        private IKeyboardMouseEvents? m_GlobalHook;

        private static RECT locked_Rect = new RECT();

        [ConfigurationProperty("cursorLocked", DefaultValue = true)]
        public bool CursorLocked
        {
            get => (bool)this["cursorLocked"];
            set
            {
                this["cursorLocked"] = value;
                OnCursorLockedChanged(value, nameof(CursorLocked));
            }
        }

        private void OnCursorLockedChanged(bool newValue, string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            SetCursorLocked(newValue);
            App.Cfg.Save();
        }

        private WindowEventHook hook = new WindowEventHook(WindowEvent.EVENT_SYSTEM_FOREGROUND, WindowEvent.EVENT_SYSTEM_DESKTOPSWITCH);
        private WindowEventHook hook2 = new WindowEventHook(WindowEvent.EVENT_SYSTEM_CAPTUREEND, WindowEvent.EVENT_SYSTEM_CAPTUREEND);
        private DispatcherTimer _timer;
        private int _tickCount = 0; // 记录触发次数

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(30); // 每30ms触发一次
            _timer.Tick += TimerTick;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            _tickCount++;
            // 在这里执行任务
            //CursorLock.GetCursorBounds().Equals(locked_Rect);
            if (CursorLock.Locked)
            {
                CursorLock.LockCursor(locked_Rect);
            }
            if (_tickCount >= 34) // 一秒钟后停止定时器
            {
                _timer.Stop();
            }
        }

        private void WinEventHandle(object? sender, WinEventHookEventArgs e)
        {
            if (e.EventType == WindowEvent.EVENT_SYSTEM_MOVESIZESTART ||
                e.EventType == WindowEvent.EVENT_SYSTEM_MINIMIZESTART ||
                e.EventType == WindowEvent.EVENT_SYSTEM_MOVESIZEEND ||
                e.EventType == WindowEvent.EVENT_SYSTEM_MINIMIZEEND ||
                e.EventType == WindowEvent.EVENT_SYSTEM_SWITCHSTART ||
                e.EventType == WindowEvent.EVENT_SYSTEM_DESKTOPSWITCH)
            {
                CursorLock.LockCursor(locked_Rect);
            }
            else if (e.EventType == WindowEvent.EVENT_SYSTEM_FOREGROUND)
            {
                CursorLock.LockCursor(locked_Rect);
                _tickCount = 0; // 重置计数器
                _timer.Start();
                hook2.TryUnhook();
                hook2.HookGlobal();
            }
        }

        private void WinEventHandle2(object? sender, WinEventHookEventArgs e)
        {
            CursorLock.LockCursor(locked_Rect);
            _tickCount = 0; // 重置计数器
            _timer.Start();
            hook2.TryUnhook();
        }

        private bool isMonitorExtend = Utils.IsCurrentdisplaySwitchModeExtend();

        private void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            isMonitorExtend = Utils.IsCurrentdisplaySwitchModeExtend();
            SetCursorLocked(CursorLocked);
        }

        private void initialKeyMouseGlobalHook()
        {
            if (m_GlobalHook == null)
            {
                m_GlobalHook = Hook.GlobalEvents();
            }
        }

        public void SetCursorLocked(bool value)
        {
            // 判断是否是扩展模式，如果不是扩展模式则没必要进行锁定
            if (value && isMonitorExtend)
            {
                initialKeyMouseGlobalHook();
                if (!hook.Hooked) hook.HookGlobal();
                locked_Rect = CursorLock.GetCursorBounds();
                CursorLock.LockCursor(locked_Rect);
            }
            else
            {
                if (CursorLock.Locked)
                {
                    hook.TryUnhook();
                    if (m_GlobalHook != null)
                    {
                        m_GlobalHook.Dispose();
                        m_GlobalHook = null;
                    }
                    CursorLock.UnlockCursor();
                }
            }
        }

        private void SetCursorLockedAtStart(bool value)
        {
            if (value && isMonitorExtend)
            {
                initialKeyMouseGlobalHook();
                hook.HookGlobal();
                locked_Rect = Utils.GetPrimaryScreenBounds();
                CursorLock.LockCursor(locked_Rect);
            }
            else
            {
                if (CursorLock.Locked)
                {
                    hook.TryUnhook();
                    if (m_GlobalHook != null)
                    {
                        m_GlobalHook.Dispose();
                        m_GlobalHook = null;
                    }
                    CursorLock.UnlockCursor();
                }
            }
        }

        private IKeyboardMouseEvents? m_PressGlobalHook;

        [ConfigurationProperty("pressToToggleLockHotkeyActivated", DefaultValue = true)]
        public bool PressToToggleLockHotkeyActivated
        {
            get => (bool)this["pressToToggleLockHotkeyActivated"];
            set
            {
                this["pressToToggleLockHotkeyActivated"] = value;
                OnPressToToggleLockHotkeyActivatedChanged(value, nameof(PressToToggleLockHotkeyActivated));
            }
        }

        private void OnPressToToggleLockHotkeyActivatedChanged(bool newValue, string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            SetPressToToggleLockHotkeyActivated(newValue);
            App.Cfg.Save();
        }

        private void SetPressToToggleLockHotkeyActivated(bool value)
        {
            if (value)
            {
                if (m_PressGlobalHook == null)
                {
                    m_PressGlobalHook = Hook.GlobalEvents();
                    m_PressGlobalHook.KeyDown += M_PressGlobalHook_KeyDown;
                    m_PressGlobalHook.KeyUp += M_PressGlobalHook_KeyUp;
                }
            }
            else
            {
                if (m_PressGlobalHook != null)
                {
                    m_PressGlobalHook.Dispose();
                    m_PressGlobalHook = null;
                }
            }
        }

        private void M_PressGlobalHook_KeyUp(object? sender, Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Forms.Keys.LControlKey || e.KeyCode == Forms.Keys.RControlKey)
            {
                if (m_PressGlobalHook != null) m_PressGlobalHook.KeyDown += M_PressGlobalHook_KeyDown;
                CursorLocked = !CursorLocked;
            }
        }

        private void M_PressGlobalHook_KeyDown(object? sender, Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Forms.Keys.LControlKey || e.KeyCode == Forms.Keys.RControlKey)
            {
                if (m_PressGlobalHook != null) m_PressGlobalHook.KeyDown -= M_PressGlobalHook_KeyDown;
                CursorLocked = !CursorLocked;
            }
        }

        [ConfigurationProperty("tapToToggleLockHotkey", DefaultValue = "Win+Shift+Z")]
        public string TapToToggleLockHotkey
        {
            get => (string)this["tapToToggleLockHotkey"];
            set
            {
                this["tapToToggleLockHotkey"] = value;
                OnTapToToggleLockHotkeyChanged(value, nameof(TapToToggleLockHotkey));
            }
        }

        private void OnTapToToggleLockHotkeyChanged(string newValue, string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // SetTapToToggleLockHotkey(newValue);
            App.Cfg.Save();
        }

        private void SetTapToToggleLockHotkey(string value)
        {
            // do nothing
        }

        private IKeyboardMouseEvents? m_TapGlobalHook;

        [ConfigurationProperty("tapToToggleLockHotkeyActivated", DefaultValue = true)]
        public bool TapToToggleLockHotkeyActivated
        {
            get => (bool)this["tapToToggleLockHotkeyActivated"];
            set
            {
                this["tapToToggleLockHotkeyActivated"] = value;
                OnTapToToggleLockHotkeyActivatedChanged(value, nameof(TapToToggleLockHotkeyActivated));
            }
        }

        private void OnTapToToggleLockHotkeyActivatedChanged(bool newValue, string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            SetTapToToggleLockHotkeyActivated(newValue);
            App.Cfg.Save();
        }

        private void ToggleLock()
        {
            CursorLocked = !CursorLocked;
        }

        private void SetTapToToggleLockHotkeyActivated(bool value)
        {
            if (value)
            {
                if (m_TapGlobalHook == null)
                {
                    m_TapGlobalHook = Hook.GlobalEvents();
                    m_TapGlobalHook.KeyDown += M_TapGlobalHook_KeyDown;
                }
            }
            else
            {
                if (m_TapGlobalHook != null)
                {
                    m_TapGlobalHook.Dispose();
                    m_TapGlobalHook = null;
                }
            }
        }

        /// <summary>
        /// 监听敲击键盘切换锁定状态的快捷键监听函数
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">键盘事件</param>
        private void M_TapGlobalHook_KeyDown(object? sender, Forms.KeyEventArgs e)
        {
            if (GetCombString(e) == TapToToggleLockHotkey)
            {
                ToggleLock();
                e.Handled = true;
            }
        }

        private string GetCombString(Forms.KeyEventArgs e)
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
            if (isAltPressed) combList.Add("Alt");
            if (isShiftPressed) combList.Add("Shift");
            combList.Add(e.KeyCode.ToString());
            return string.Join("+", combList);
        }

        private IKeyboardMouseEvents? m_SwitchDisplayModeGlobalHook;

        [ConfigurationProperty("switchDisplayModeHotkeyActivated", DefaultValue=true)]
        public bool SwitchDisplayModeHotkeyActivated
        {
            get => (bool)this["switchDisplayModeHotkeyActivated"];
            set
            {
                this["switchDisplayModeHotkeyActivated"] = value;
                OnSwitchDisplayModeHotkeyActivatedChanged(value, nameof(TapToToggleLockHotkeyActivated));
            }
        }

        private void OnSwitchDisplayModeHotkeyActivatedChanged(bool newValue, string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            SetSwitchDisplayModeHotkeyActivated(newValue);
            App.Cfg.Save();
        }

        private void SetSwitchDisplayModeHotkeyActivated(bool value)
        {
            if (value)
            {
                if (m_SwitchDisplayModeGlobalHook == null)
                {
                    m_SwitchDisplayModeGlobalHook = Hook.GlobalEvents();
                    m_SwitchDisplayModeGlobalHook.KeyDown += M_SwitchDisplayModeGlobalHook_KeyDown;
                    m_SwitchDisplayModeGlobalHook.KeyUp += M_SwitchDisplayModeGlobalHook_KeyUp;
                }
            }
            else
            {
                if (m_SwitchDisplayModeGlobalHook != null)
                {
                    m_SwitchDisplayModeGlobalHook.Dispose();
                    m_SwitchDisplayModeGlobalHook = null;
                }
            }
        }

        private bool IsWinAlt(Forms.KeyEventArgs e)
        {
            // 检查是否按下了Win键
            bool isWinPressed = Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);
            // 检查是否按下了Alt键
            bool isAltPressed = (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;
            return (isWinPressed && (e.KeyCode == Forms.Keys.LMenu || e.KeyCode == Forms.Keys.RMenu)) ||
                (isAltPressed && (e.KeyCode == Forms.Keys.LWin || e.KeyCode == Forms.Keys.RWin));
        }

        private void M_SwitchDisplayModeGlobalHook_KeyUp(object? sender, Forms.KeyEventArgs e)
        {
            
            // 判断是否是Win+Alt，如果是，则移除keyup监听，添加keydown监听
            if (IsWinAlt(e))
            {
                if (m_SwitchDisplayModeGlobalHook != null)
                {
                    m_SwitchDisplayModeGlobalHook.KeyUp -= M_SwitchDisplayModeGlobalHook_KeyUp;
                    m_SwitchDisplayModeGlobalHook.KeyDown += M_SwitchDisplayModeGlobalHook_KeyDown;
                    // 取消强制锁定光标，取消鼠标监听
                    SetCursorLocked(CursorLocked);
                    m_SwitchDisplayModeGlobalHook.MouseClick -= M_SwitchDisplayModeGlobalHook_MouseClick;
                }
            }
        }

        private void M_SwitchDisplayModeGlobalHook_KeyDown(object? sender, Forms.KeyEventArgs e)
        {
            // 判断是否是Win+Alt，如果是，则移除keydown监听，添加keyup监听
            if (IsWinAlt(e))
            {
                if (m_SwitchDisplayModeGlobalHook != null)
                {
                    m_SwitchDisplayModeGlobalHook.KeyDown -= M_SwitchDisplayModeGlobalHook_KeyDown;
                    m_SwitchDisplayModeGlobalHook.KeyUp += M_SwitchDisplayModeGlobalHook_KeyUp;
                    // 强制锁定光标，监听鼠标点击事件
                    SetCursorLocked(true);
                    m_SwitchDisplayModeGlobalHook.MouseClick += M_SwitchDisplayModeGlobalHook_MouseClick;
                }
            }
        }

        private void M_SwitchDisplayModeGlobalHook_MouseClick(object? sender, Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Right)
            {
                // 判断光标是否在当前屏幕边缘
                RECT currentBounds = CursorLock.GetCursorBounds();
                if (e.X == currentBounds.Left || e.X == currentBounds.Right - 1 || e.Y == currentBounds.Top || e.Y == currentBounds.Bottom - 1)
                {
                    if (isMonitorExtend)
                    {
                        if (SelectedDisplayModeIndex == 0) Utils.ChangeDisplayMode("/Internal");
                        else if (SelectedDisplayModeIndex == 2) Utils.ChangeDisplayMode("/external");
                        else Utils.ChangeDisplayMode("/Clone");
                    }
                    else Utils.ChangeDisplayMode("/extend");
                    if (m_SwitchDisplayModeGlobalHook != null) m_SwitchDisplayModeGlobalHook.MouseClick -= M_SwitchDisplayModeGlobalHook_MouseClick;
                }
            }
        }

        [ConfigurationProperty("selectedDisplayModeIndex", DefaultValue = 0)]
        public int SelectedDisplayModeIndex
        {
            get => (int)this["selectedDisplayModeIndex"];
            set
            {
                this["selectedDisplayModeIndex"] = value;
                OnSelectedDisplayModeIndexChanged(value, nameof(SelectedDisplayModeIndex));
            }
        }

        private void OnSelectedDisplayModeIndexChanged(int newValue, string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            App.Cfg.Save();
        }

        #region 切换多显示器模式热键相关属性和方法，由于暂时无法解决切换后阻止不了快捷键输入的问题，取消界面中的自定义快捷键模块，改用其他固定快捷键切换方式
        [ConfigurationProperty("monitorInternalHotkey", DefaultValue = "Win+Shift+X")]
        public string MonitorInternalHotkey
        {
            get => (string)this["monitorInternalHotkey"];
            set
            {
                this["monitorInternalHotkey"] = value;
                OnMonitorInternalHotkeyChanged(value, nameof(MonitorInternalHotkey));
            }
        }

        private void OnMonitorInternalHotkeyChanged(string newValue, string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            App.Cfg.Save();
        }

        private IKeyboardMouseEvents? m_MonitorInternalGlobalHook;

        [ConfigurationProperty("monitorInternalHotkeyActivated", DefaultValue = true)]
        public bool MonitorInternalHotkeyActivated
        {
            get => (bool)this["monitorInternalHotkeyActivated"];
            set
            {
                this["monitorInternalHotkeyActivated"] = value;
                OnMonitorInternalHotkeyActivatedChanged(value, nameof(MonitorInternalHotkeyActivated));
            }
        }

        private void OnMonitorInternalHotkeyActivatedChanged(bool newValue, string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            SetMonitorInternalHotkeyActivated(newValue);
            App.Cfg.Save();
        }

        private void SetMonitorInternalHotkeyActivated(bool value)
        {
            if (value)
            {
                if (m_MonitorInternalGlobalHook == null)
                {
                    m_MonitorInternalGlobalHook = Hook.GlobalEvents();
                    m_MonitorInternalGlobalHook.KeyDown += M_MonitorInternalGlobalHook_KeyDown;
                }
            }
            else
            {
                if (m_MonitorInternalGlobalHook != null)
                {
                    m_MonitorInternalGlobalHook.Dispose();
                    m_MonitorInternalGlobalHook = null;
                }
            }
        }

        /// <summary>
        /// 监听设置仅internal显示器的快捷键监听函数
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">键盘事件</param>
        private void M_MonitorInternalGlobalHook_KeyDown(object? sender, Forms.KeyEventArgs e)
        {
            if (GetCombString(e) == MonitorInternalHotkey)
            {
                // 设置仅internal显示器
                Utils.ChangeDisplayMode("/internal");
                e.Handled = true;
            }
        }

        [ConfigurationProperty("monitorCloneHotkey", DefaultValue = "Win+Shift+B")]
        public string MonitorCloneHotkey
        {
            get => (string)this["monitorCloneHotkey"];
            set
            {
                this["monitorCloneHotkey"] = value;
                OnMonitorCloneHotkeyChanged(value, nameof(MonitorCloneHotkey));
            }
        }

        private void OnMonitorCloneHotkeyChanged(string newValue, string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            App.Cfg.Save();
        }

        private IKeyboardMouseEvents? m_MonitorCloneGlobalHook;

        [ConfigurationProperty("monitorCloneHotkeyActivated", DefaultValue = true)]
        public bool MonitorCloneHotkeyActivated
        {
            get => (bool)this["monitorCloneHotkeyActivated"];
            set
            {
                this["monitorCloneHotkeyActivated"] = value;
                OnMonitorCloneHotkeyActivatedChanged(value, nameof(MonitorCloneHotkeyActivated));
            }
        }

        private void OnMonitorCloneHotkeyActivatedChanged(bool newValue, string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            SetMonitorCloneHotkeyActivated(newValue);
            App.Cfg.Save();
        }

        private void SetMonitorCloneHotkeyActivated(bool value)
        {
            if (value)
            {
                if (m_MonitorCloneGlobalHook == null)
                {
                    m_MonitorCloneGlobalHook = Hook.GlobalEvents();
                    m_MonitorCloneGlobalHook.KeyDown += M_MonitorCloneGlobalHook_KeyDown;
                }
            }
            else
            {
                if (m_MonitorCloneGlobalHook != null)
                {
                    m_MonitorCloneGlobalHook.Dispose();
                    m_MonitorCloneGlobalHook = null;
                }
            }
        }

        /// <summary>
        /// 监听设置仅internal显示器的快捷键监听函数
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">键盘事件</param>
        private void M_MonitorCloneGlobalHook_KeyDown(object? sender, Forms.KeyEventArgs e)
        {
            if (GetCombString(e) == MonitorCloneHotkey)
            {
                // 设置仅clone显示器
                Utils.ChangeDisplayMode("/clone");
                e.Handled = true;
            }
        }

        [ConfigurationProperty("monitorExtendHotkey", DefaultValue = "Win+Shift+N")]
        public string MonitorExtendHotkey
        {
            get => (string)this["monitorExtendHotkey"];
            set
            {
                this["monitorExtendHotkey"] = value;
                OnMonitorExtendHotkeyChanged(value, nameof(MonitorExtendHotkey));
            }
        }

        private void OnMonitorExtendHotkeyChanged(string newValue, string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            App.Cfg.Save();
        }

        private IKeyboardMouseEvents? m_MonitorExtendGlobalHook;

        [ConfigurationProperty("monitorExtendHotkeyActivated", DefaultValue = true)]
        public bool MonitorExtendHotkeyActivated
        {
            get => (bool)this["monitorExtendHotkeyActivated"];
            set
            {
                this["monitorExtendHotkeyActivated"] = value;
                OnMonitorExtendHotkeyActivatedChanged(value, nameof(MonitorExtendHotkeyActivated));
            }
        }

        private void OnMonitorExtendHotkeyActivatedChanged(bool newValue, string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            SetMonitorExtendHotkeyActivated(newValue);
            App.Cfg.Save();
        }

        private void SetMonitorExtendHotkeyActivated(bool value)
        {
            if (value)
            {
                if (m_MonitorExtendGlobalHook == null)
                {
                    m_MonitorExtendGlobalHook = Hook.GlobalEvents();
                    m_MonitorExtendGlobalHook.KeyDown += M_MonitorExtendGlobalHook_KeyDown;
                }
            }
            else
            {
                if (m_MonitorExtendGlobalHook != null)
                {
                    m_MonitorExtendGlobalHook.Dispose();
                    m_MonitorExtendGlobalHook = null;
                }
            }
        }

        /// <summary>
        /// 监听设置仅internal显示器的快捷键监听函数
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">键盘事件</param>
        private void M_MonitorExtendGlobalHook_KeyDown(object? sender, Forms.KeyEventArgs e)
        {
            if (GetCombString(e) == MonitorExtendHotkey)
            {
                // 设置仅extend显示器
                Utils.ChangeDisplayMode("/extend");
                e.Handled = true;
            }
        }

        [ConfigurationProperty("monitorExternalHotkey", DefaultValue = "Win+Shift+I")]
        public string MonitorExternalHotkey
        {
            get => (string)this["monitorExternalHotkey"];
            set
            {
                this["monitorExternalHotkey"] = value;
                OnMonitorExternalHotkeyChanged(value, nameof(MonitorExternalHotkey));
            }
        }

        private void OnMonitorExternalHotkeyChanged(string newValue, string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            App.Cfg.Save();
        }

        private IKeyboardMouseEvents? m_MonitorExternalGlobalHook;

        [ConfigurationProperty("monitorExternalHotkeyActivated", DefaultValue = true)]
        public bool MonitorExternalHotkeyActivated
        {
            get => (bool)this["monitorExternalHotkeyActivated"];
            set
            {
                this["monitorExternalHotkeyActivated"] = value;
                OnMonitorExternalHotkeyActivatedChanged(value, nameof(MonitorExternalHotkeyActivated));
            }
        }

        private void OnMonitorExternalHotkeyActivatedChanged(bool newValue, string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            SetMonitorExternalHotkeyActivated(newValue);
            App.Cfg.Save();
        }

        private void SetMonitorExternalHotkeyActivated(bool value)
        {
            if (value)
            {
                if (m_MonitorExternalGlobalHook == null)
                {
                    m_MonitorExternalGlobalHook = Hook.GlobalEvents();
                    m_MonitorExternalGlobalHook.KeyDown += M_MonitorExternalGlobalHook_KeyDown;
                }
            }
            else
            {
                if (m_MonitorExternalGlobalHook != null)
                {
                    m_MonitorExternalGlobalHook.Dispose();
                    m_MonitorExternalGlobalHook = null;
                }
            }
        }

        /// <summary>
        /// 监听设置仅internal显示器的快捷键监听函数
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">键盘事件</param>
        private void M_MonitorExternalGlobalHook_KeyDown(object? sender, Forms.KeyEventArgs e)
        {
            if (GetCombString(e) == MonitorExternalHotkey)
            {
                // 设置仅external显示器
                Utils.ChangeDisplayMode("/external");
                e.Handled = true;
            }
        }
        #endregion

        [ConfigurationProperty("selectedThemeIndex", DefaultValue = 1)]
        public int SelectedThemeIndex
        {
            get => (int)this["selectedThemeIndex"];
            set
            {
                this["selectedThemeIndex"] = value;
                OnSelectedThemeIndexChanged(value);
            }
        }

        private void OnSelectedThemeIndexChanged(int newValue)
        {
            ChangeTheme(newValue);
            App.Cfg.Save();
        }

        public void ChangeTheme(int parameter)
        {
            Window navigationWindow = App.GetService<INavigationWindow>() as Window;
            switch (parameter)
            {
                case 0:
                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    SystemThemeWatcher.UnWatch(navigationWindow);

                    break;

                case 1:
                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    SystemThemeWatcher.UnWatch(navigationWindow);

                    break;

                default:
                    SystemThemeManager.UpdateSystemThemeCache();
                    SystemTheme systemTheme = SystemThemeManager.GetCachedSystemTheme();
                    SystemThemeWatcher.Watch(navigationWindow);
                    switch (systemTheme)
                    {
                        case SystemTheme.Light:
                            ApplicationThemeManager.Apply(ApplicationTheme.Light);
                            break;
                        case SystemTheme.Dark:
                            ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                            break;
                        case SystemTheme.HC1:
                        case SystemTheme.HC2:
                            ApplicationThemeManager.Apply(ApplicationTheme.HighContrast);
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }

        [ConfigurationProperty("selectedLanguage", DefaultValue = "en-US")]
        public string SelectedLanguage
        {
            get => (string)this["selectedLanguage"];
            set
            {
                this["selectedLanguage"] = value;
                OnSelectedLanguageChanged(value);
            }
        }

        private void OnSelectedLanguageChanged(string newValue)
        {
            ChangeLanguage(newValue);
            App.Cfg.Save();
        }

        public void ChangeLanguage(string value)
        {
            LanguageManager.Instance.ChangeLanguage(new System.Globalization.CultureInfo(value));
        }

        public void InitSettings()
        {
            InitializeTimer();
            SystemEvents.DisplaySettingsChanged += new EventHandler(OnDisplaySettingsChanged);
            hook.EventReceived += WinEventHandle;
            hook2.EventReceived += WinEventHandle2;
            SetCursorLockedAtStart(CursorLocked);
            SetTapToToggleLockHotkeyActivated(TapToToggleLockHotkeyActivated);
            SetPressToToggleLockHotkeyActivated(PressToToggleLockHotkeyActivated);
            SetSwitchDisplayModeHotkeyActivated(SwitchDisplayModeHotkeyActivated);
            //SetMonitorInternalHotkeyActivated(MonitorInternalHotkeyActivated);
            //SetMonitorCloneHotkeyActivated(MonitorCloneHotkeyActivated);
            //SetMonitorExtendHotkeyActivated(MonitorExtendHotkeyActivated);
            //SetMonitorExternalHotkeyActivated(MonitorExternalHotkeyActivated);
            //ChangeTheme(SelectedThemeOption);在MainWindow OnLoaded事件中修改主题
        }
    }
}
