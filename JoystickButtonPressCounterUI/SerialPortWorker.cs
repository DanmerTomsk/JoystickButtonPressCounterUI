using JoystickButtonPressCounterUI.Dtos;
using JoystickButtonPressCounterUI.Models;
using JoystickButtonPressCounterUI.Observer;
using System.IO.Ports;

namespace JoystickButtonPressCounterUI
{
    public sealed class SerialPortWorker
    {
        private SerialPort? _currentSerialPort;

        private Dictionary<JoyButtonInfo, List<byte>> _mapping = [];

        public static SerialPortWorker Singleton { get; } = new SerialPortWorker();

        public void AddMapping(ComButtonMapping mapping)
        {
            var joyButtonInfo = new JoyButtonInfo(mapping);
            if (_mapping.TryGetValue(joyButtonInfo, out var existingMapping))
            {
                existingMapping.Add(mapping.ComButtonNumber);
                return;
            }

            _mapping.Add(joyButtonInfo, [mapping.ComButtonNumber]);
        }

        public void RemoveMapping(ComButtonMapping comButtonMapping)
        {
            var joyButtonInfo = new JoyButtonInfo(comButtonMapping);
            _mapping.Remove(joyButtonInfo);
        }

        public void SetPort(string serialPortName)
        {
            if (_currentSerialPort != null)
            {
                _currentSerialPort.Close();
                _currentSerialPort.Dispose();
            }

            _currentSerialPort = new SerialPort(serialPortName);
            if (!_currentSerialPort.IsOpen)
            {
                try
                {
                    _currentSerialPort.Open();
                }
                catch(UnauthorizedAccessException) { }
            }

            JoystickObserver.Observer.ButtonsStateChanged -= Observer_ButtonsStateChanged;
            JoystickObserver.Observer.ButtonsStateChanged += Observer_ButtonsStateChanged;
        }

        private void Observer_ButtonsStateChanged(uint joyId, ButtonsStateChange[] changes)
        {            
            var commands = new List<byte>(changes.Length);
            foreach (var change in changes)
            {
                if (!_mapping.TryGetValue(change.joyButtonInfo, out var comButtonNumbers))
                    continue;

                foreach (var number in comButtonNumbers)
                {
                    var command = (number) * 2;

                    if (change.isPressed)
                    {
                        command--;
                    }
                    commands.Add((byte)command);
                }
            }

            if (commands.Count == 0)
                return;
            SendDataToSerial(commands.ToArray());
        }

        private void SendDataToSerial(byte[] commands)
        {
            _currentSerialPort?.Write(commands, 0, commands.Length);
            Console.WriteLine($"Send {string.Join(",", commands)}");
        }
    }
}
