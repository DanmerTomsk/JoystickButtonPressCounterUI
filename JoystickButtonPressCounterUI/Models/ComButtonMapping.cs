namespace JoystickButtonPressCounterUI.Models
{
    public sealed class ComButtonMapping
    {
        public byte JoystickButtonNumber { get; set; }

        public uint JoystickId { get; set; }

        public byte ComButtonNumber { get; }

        public ComButtonMapping(byte comButtonNumber)
        {
            ComButtonNumber = comButtonNumber;
        }
    }
}
