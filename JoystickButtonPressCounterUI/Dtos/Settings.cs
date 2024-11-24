
using JoystickButtonPressCounterUI.Models;

namespace JoystickButtonPressCounterUI.Dtos
{
    public class Settings
    {
        public string? ComPortName { get; }

        public int? JoyInfoRequestDelayInMs { get; }

        public CounterModel[] Models { get; }

        public Settings(string? comPortName, CounterModel[] models, int? joyInfoRequestDelayInMs)
        {
            ComPortName = comPortName;
            Models = models;
            JoyInfoRequestDelayInMs = joyInfoRequestDelayInMs;
        }
    }
}
