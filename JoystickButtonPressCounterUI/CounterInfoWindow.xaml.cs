using JoystickButtonPressCounterUI.Models;
using JoystickButtonPressCounterUI.Observer;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace JoystickButtonPressCounterUI
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class CounterInfoWindow : Window, INotifyPropertyChanged
    {
        private int currentMax = 0;

        public int CurrentMax
        {
            get => currentMax;
            private set
            {
                currentMax = value;
                OnPropertyChanged(nameof(CurrentMax));
            }
        }
        private int CurrentButtonNumber { get; set; } = 0;

        private uint CurrentJoyId { get; set; } = 0;

        internal CounterModel? ResultCounter { get; private set; }

        public CounterInfoWindow()
        {
            InitializeComponent();
            JoystickObserver.Observer.SomeButtonPress += Observer_SomeButtonPress;
            this.DataContext = this;
        }

        public CounterInfoWindow(CounterModel model)
        {
            InitializeComponent();
            JoystickObserver.Observer.SomeButtonPress += Observer_SomeButtonPress;
            CurrentMax = model.MaxMilliseconds;
            CurrentJoyId = model.JoyId;
            CurrentButtonNumber = model.ButtonNumber;  
            this.DataContext = this;
            NameTextBox.Text = model.Name;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private void Observer_SomeButtonPress(uint joyId, int allButtons)
        {

            if (CurrentButtonNumber != allButtons || CurrentJoyId != joyId)
            {
                CurrentButtonNumber = allButtons;
                CurrentJoyId = joyId;
                CurrentMax = 0;
                return;
            }

            CurrentMax += Configs.RequestIntervalInMilliseconds;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var name = NameTextBox.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите имя счётчика", "Ошибка заполнения", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CurrentMax == 0)
            {
                MessageBox.Show("Нажмите кнопку на джойстике", "Ошибка заполнения", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ResultCounter = new CounterModel(NameTextBox.Text, CurrentMax, CurrentButtonNumber, CurrentJoyId);

            JoystickObserver.Observer.SomeButtonPress -= Observer_SomeButtonPress;
            DialogResult = true;
        }

        private void DropButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentMax = 0;
        }
    }
}
