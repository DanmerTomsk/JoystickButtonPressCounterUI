using JoystickButtonPressCounterUI.Models;

namespace JoystickButtonPressCounterUI.Dtos
{
    public record JoyButtonInfo (uint joyId, byte joyButtonNumber)
    {
        public JoyButtonInfo (ComButtonMapping mapping)
            : this(mapping.JoystickId, mapping.JoystickButtonNumber)
        {
        }
    }
}
