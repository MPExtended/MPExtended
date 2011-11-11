using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MPExtended.Services.UserSessionService
{
    public class ScreenSaver
    {
        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        private const int SC_SCREENSAVE = 0xF140;
        private const int WM_SYSCOMMAND = 0x0112;

        public static void StartScreenSaver()
        {
            SendMessage(GetDesktopWindow(), WM_SYSCOMMAND, SC_SCREENSAVE, 0);
        }
    }
}
