using JoystickButtonPressCounterUI.Models;
using JoystickButtonPressCounterUI.Observer;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace JoystickButtonPressCounterUI.ViewModels
{
    internal class ComButtonMappingViewModel : INotifyPropertyChanged
    {
        private Brush _joystickNumberBorderColor;

        public event PropertyChangedEventHandler? PropertyChanged;

        private RelayCommand _chooseCommand;
        private byte _joystickButtonNumber;
        private uint _joystickId;

        public RelayCommand ChooseCommand
        {
            get
            {
                return _chooseCommand ??= new RelayCommand(obj =>
                    {
                        SetWaitingState();
                        JoystickObserver.Observer.SomeButtonPress += ButtonsObserver_SomeButtonPress;
                    });
            }
        }

        public ComButtonMapping Source { get; }

        public byte JoystickButtonNumber 
        { 
            get => _joystickButtonNumber;
            private set
            {
                _joystickButtonNumber = value;
                OnPropertyChanged(nameof(JoystickButtonNumber));
            }
        }

        public uint JoystickId 
        { 
            get => _joystickId;
            private set
            {
                _joystickId = value;
                OnPropertyChanged(nameof(JoystickId));
            }
        }

        public byte ComButtonNumber { get; }

        public Brush JoystickNumberBorderColor
        {
            get => _joystickNumberBorderColor;
            private set
            {
                _joystickNumberBorderColor = value;
                OnPropertyChanged(nameof(JoystickNumberBorderColor));
            }
        }

        public ComButtonMappingViewModel(ComButtonMapping source)
        {
            Source = source;
            ComButtonNumber = source.ComButtonNumber;
            JoystickButtonNumber = source.JoystickButtonNumber;
            JoystickNumberBorderColor = Brushes.Transparent;
        }

        public ComButtonMapping GetButtonMapping()
        {
            return new ComButtonMapping(ComButtonNumber)
            {
                JoystickButtonNumber = JoystickButtonNumber,
                JoystickId = JoystickId,
            };
        }

        public void SetWaitingState()
        {
            JoystickNumberBorderColor = Brushes.Blue;
        }

        public void SetNewJoystickButtonNumber(byte buttonNumber, uint joystickId)
        {
            JoystickButtonNumber = buttonNumber;
            JoystickId = joystickId;
            JoystickNumberBorderColor = Brushes.Transparent;
        }

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private void ButtonsObserver_SomeButtonPress(uint joyId, int allButtons)
        {
            JoystickObserver.Observer.SomeButtonPress -= ButtonsObserver_SomeButtonPress;
            var numbers = GetButtonsNumbers(allButtons);

            if (numbers.Length > 1)
            {
                JoystickObserver.Observer.SomeButtonPress += ButtonsObserver_SomeButtonPress;
                return;
            }

            var num = numbers.First();

            SetNewJoystickButtonNumber(num, joyId);
            ComButtonMapping newMapping = GetButtonMapping();
            SerialPortWorker.Singleton.AddMapping(newMapping);
        }

        private byte[] GetButtonsNumbers(int allButtons)
        {
            var result = new List<byte>();
            for (byte i = 0; i < 32; i++)
            {
                var changedButtonNumber = allButtons & (1 << i);
                if (changedButtonNumber == 0)
                    continue;

                result.Add(i);
            }

            return result.ToArray();
        }
    }
}
