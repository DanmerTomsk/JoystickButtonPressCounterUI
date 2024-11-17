
using JoystickButtonPressCounterUI.Models;

namespace JoystickButtonPressCounterUI.Dtos
{
    public class Settings
    {
        public string? ComPortName { get; }

        public CounterModel[] Models { get; }

        public Settings(string? comPortName, CounterModel[] models)
        {
            ComPortName = comPortName;
            Models = models;
        }
    }
}
