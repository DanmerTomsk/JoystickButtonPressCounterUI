namespace JoystickButtonPressCounterUI.Models
{
    
    public sealed class CounterModel
    {
        public const byte EmptyOrderValue = 255;

        public string Name { get; }

        public int MaxMilliseconds { get; }

        public byte ButtonNumber { get; }

        public uint JoyId { get; }

        public byte Order { get; set; } = EmptyOrderValue;

        public CounterModel(string name, int maxMilliseconds, byte buttonNumber, uint joyId)
        {
            Name = name;
            MaxMilliseconds = maxMilliseconds;
            ButtonNumber = buttonNumber;
            JoyId = joyId;
        }
    }
}
