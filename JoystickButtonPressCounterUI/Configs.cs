using System.IO.Ports;

namespace JoystickButtonPressCounterUI
{
    internal static class Configs
    {
        internal const uint MaxJoystickCount = 15;
        internal const uint MaxJoystickButtonsCount = 32;

        internal const int DefaultRequestIntervalInMilliseconds = 25;

        internal static int RequestIntervalInMilliseconds { get; set; }
    }
}
