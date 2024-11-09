using System.IO.Ports;

namespace JoystickButtonPressCounterUI
{
    internal static class Configs
    {
        internal static int RequestIntervalInMilliseconds { get; } = 50;

        internal static SerialPort CurrentSerialPort { get; set; }
    }
}
