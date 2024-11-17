using JoystickButtonPressCounterUI.Models;
using JoystickButtonPressCounterUI.Observer;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace JoystickButtonPressCounterUI.ViewModels
{
    public class BulletCounterViewModel : INotifyPropertyChanged, IDisposable
    {
        private double usedMilliseconds;
        private double remainderPercent;
        private Brush color;

        public event PropertyChangedEventHandler? PropertyChanged;

        internal CounterModel Source { get; }

        public string Name { get; }

        public int MaxMilliseconds { get; }

        public double UsedMilliseconds
        {
            get => usedMilliseconds;
            private set
            {
                usedMilliseconds = value;
                var remainderPercent = 100 * (1 - usedMilliseconds / MaxMilliseconds);
                RemainderPercent = remainderPercent < 0 ? 0 : remainderPercent;
            }
        }

        public double RemainderPercent
        {
            get => remainderPercent;
            private set
            {
                remainderPercent = value;
                Color = GetBrushByPercent(remainderPercent);
                OnPropertyChanged(nameof(RemainderPercent));
            }
        }

        public Brush Color
        {
            get => color;
            private set
            {
                color = value;
                OnPropertyChanged(nameof(Color));
            }
        }

        internal BulletCounterViewModel(CounterModel model)
        {
            Source = model;
            Name = model.Name;
            MaxMilliseconds = model.MaxMilliseconds;
            UsedMilliseconds = 0;

            JoystickObserver.Observer.AddToObserve(model.JoyId, model.ButtonNumber, ButtonPressed);
            var newMapping = new ComButtonMapping((byte)(model.Order + 1))
            {
                JoystickButtonNumber = (byte)model.ButtonNumber,
                JoystickId = model.JoyId,
            };
            SerialPortWorker.Singleton.AddMapping(newMapping);
        }

        public void ProgressBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            UsedMilliseconds = 0;
        }

        private void ButtonPressed(uint joyId, byte buttonNumber)
        {
            UsedMilliseconds += Configs.RequestIntervalInMilliseconds;
        }

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private Brush GetBrushByPercent(double percent)
        {
            byte red = 255;
            byte green = 255;
            if (percent <= 50)
            {
                green = (byte)(255 * (percent / 50));
            }
            else
            {
                red = (byte)(255 * ((100 - percent) / 50));
            }

            return new SolidColorBrush(System.Windows.Media.Color.FromRgb(red, green, 0));
        }

        public void Dispose()
        {
            JoystickObserver.Observer.RemoveFromObserve(Source.JoyId, Source.ButtonNumber, ButtonPressed);
        }
    }
}
