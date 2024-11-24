using System.IO.Ports;

namespace JoystickButtonPressCounterUI
{
    internal static class Configs
    {
        internal const int DefaultRequestIntervalInMilliseconds = 50;

        internal static int RequestIntervalInMilliseconds { get; set; }
    }
}
