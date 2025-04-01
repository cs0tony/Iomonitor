using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Iomonitor.Interop;

namespace Iomonitor
{
    class CursorLock
    {
        private static bool locked = false;

        public static bool Locked 
        { 
            get => locked;
            set 
            {
                locked = value; 
            } 
        }

        public static RECT LockCursor()
        {
            RECT bounds = GetCursorBounds();
            // Confine the cursor to that monitor.
            Native.ClipCursor(bounds);
            Locked = true;
            return bounds;
        }

        public static void LockCursor(RECT rECT)
        {
            // Confine the cursor to that monitor.
            Native.ClipCursor(rECT);
            Locked = true;
        }

        public static void UnlockCursor()
        {
            Native.ClipCursor(null);
            Locked = false;
        }

        public static RECT GetClipCursor()
        {
            return Native.GetClipCursor();
        }

        public static RECT GetCursorBounds()
        {
            // Get the position of the cursor.
            System.Drawing.Point MousePoint = Native.GetCursorPos();
            // Get the bounds of the screen that the cursor is on.
            return Screen.GetBounds(MousePoint);
        }
    }
}
