using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CozyDesktopOffice
{
    public sealed class DesktopIntegration
    {
        private IntPtr _previousParent;

        public bool TryAttachToDesktop(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            var desktop = FindWindow("Progman", null);
            if (desktop == IntPtr.Zero)
            {
                return false;
            }

            SendMessageTimeout(desktop, 0x052C, IntPtr.Zero, "Desktop", 0x0000, 1000, out _);

            var worker = FindWorkerW();
            if (worker == IntPtr.Zero)
            {
                return false;
            }

            _previousParent = SetParent(handle, worker);
            if (_previousParent == IntPtr.Zero)
            {
                return false;
            }

            SetWindowPos(handle, IntPtr.Zero, 0, 0, (int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight, SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_SHOWWINDOW);
            return true;
        }

        public void DetachFromDesktop(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
            {
                return;
            }

            SetParent(handle, IntPtr.Zero);
        }

        private static IntPtr FindWorkerW()
        {
            var progman = FindWindow("Progman", null);
            if (progman == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            IntPtr shellView = IntPtr.Zero;
            IntPtr worker = IntPtr.Zero;

            shellView = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (shellView != IntPtr.Zero)
            {
                worker = FindWindowEx(shellView, IntPtr.Zero, "WorkerW", null);
                return worker;
            }

            var current = IntPtr.Zero;
            while (true)
            {
                current = FindWindowEx(IntPtr.Zero, current, "WorkerW", null);
                if (current == IntPtr.Zero)
                {
                    break;
                }

                var child = FindWindowEx(current, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (child != IntPtr.Zero)
                {
                    return current;
                }
            }

            return worker;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string? windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [Flags]
        private enum SetWindowPosFlags : uint
        {
            SWP_NOACTIVATE = 0x0010,
            SWP_SHOWWINDOW = 0x0040,
            SWP_NOMOVE = 0x0002,
            SWP_NOSIZE = 0x0001,
            SWP_NOREDRAW = 0x0008
        }
    }
}
