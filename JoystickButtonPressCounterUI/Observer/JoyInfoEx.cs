using System.Runtime.InteropServices;

namespace JoystickButtonPressCounterUI.Observer
{
    [StructLayout(LayoutKind.Sequential)]
    struct JoyInfoEx
    {
        public int dwSize;
        public int dwFlags;
        public int dwXpos;
        public int dwYpos;
        public int dwZpos;
        public int dwRpos;
        public int dwUpos;
        public int dwVpos;
        public int dwButtons;
        public int dwButtonNumber;
        public int dwPOV;
        public int dwReserved1;
        public int dwReserved2;
    }
}