using JoystickButtonPressCounterUI.Dtos;
using JoystickButtonPressCounterUI.Models;
using JoystickButtonPressCounterUI.Observer;
using JoystickButtonPressCounterUI.ViewModels;
using Microsoft.Win32;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JoystickButtonPressCounterUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<BulletCounterViewModel> _bulletCounters = new List<BulletCounterViewModel>();

        public MainWindow()
        {
            InitializeComponent();
            JoystickObserver.Observer.Start(Dispatcher);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var createNewDialog = new CounterInfoWindow
            {
                Owner = this
            };

            if (createNewDialog.ShowDialog() != true || createNewDialog.ResultCounter is null)
            {
                return;
            }

            var newCounter = createNewDialog.ResultCounter;
            AddNewCounter(newCounter);
        }

        private void ProgressBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var progressBar = (ProgressBar)sender;
            var counter = (BulletCounterViewModel)progressBar.DataContext;
            counter.ProgressBar_MouseDoubleClick(sender, e);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            FrameworkElement parent = (FrameworkElement)button.Parent;

            while (!(parent is ContentControl))
            {
                parent = (FrameworkElement)parent.Parent;
            }

            var oldCounter = (BulletCounterViewModel)button.DataContext;
            var editCounterDialog = new CounterInfoWindow(oldCounter.Source)
            {
                Owner = this
            };

            if (editCounterDialog.ShowDialog() != true || editCounterDialog.ResultCounter is null)
            {
                return;
            }

            var newCounter = editCounterDialog.ResultCounter;
            newCounter.Order = oldCounter.Source.Order;
            var counterIndex = _bulletCounters.IndexOf(oldCounter);
            var newViewModel = new BulletCounterViewModel(newCounter);
            _bulletCounters[counterIndex] = newViewModel;
            parent.DataContext = newViewModel;

            SerialPortWorker.Singleton.RemoveMapping(new JoyButtonInfo(oldCounter.Source.JoyId, oldCounter.Source.ButtonNumber));
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".dat";
            saveFileDialog.Filter = "dat|*.dat";
            if (saveFileDialog.ShowDialog(this) != true)
            {
                return;
            }

            var json = JsonSerializer.Serialize(_bulletCounters.Select(counter => counter.Source).ToArray());

            if (File.Exists(saveFileDialog.FileName))
            {
                File.Delete(saveFileDialog.FileName);
            }

            using var newFile = saveFileDialog.OpenFile();
            using var writer = new StreamWriter(newFile);
            writer.WriteLine(json);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".dat";
            openFileDialog.Filter = "dat|*.dat";
            if (openFileDialog.ShowDialog(this) != true)
            {
                return;
            }

            var json = File.ReadAllText(openFileDialog.FileName);
            var models = JsonSerializer.Deserialize<CounterModel[]>(json);
            if (models == null)
            {
                var messageBox = MessageBox.Show(this, "Загрузка профиля", "Не удалось распознать текст в профиле", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            models.OrderBy(model => model.Order).ToList();
            for (int i = 0; i < models.Length; i++)
            {
                if (models[i].Order == CounterModel.EmptyOrderValue)
                {
                    models[i].Order = (byte)i;
                }
            }

            RemoveAllCounters();

            foreach (var model in models)
            {
                AddNewCounter(model);
            }
        }

        private void AddNewCounter(CounterModel newCounter)
        {
            if (newCounter.Order == CounterModel.EmptyOrderValue)
            {
                newCounter.Order = (byte)(_bulletCounters.Count);
            }

            var bulletCounter = new BulletCounterViewModel(newCounter);

            CounterGrid.RowDefinitions.Add(new RowDefinition());

            var template = (ControlTemplate)this.FindResource("CounterProgressTemplate");

            var newContentControl = new ContentControl()
            {
                Template = template,
                DataContext = bulletCounter,
            };

            newContentControl.SetValue(Grid.RowProperty, CounterGrid.RowDefinitions.Count - 1);

            CounterGrid.Children.Add(newContentControl);
            _bulletCounters.Add(bulletCounter);
        }

        private void RemoveAllCounters()
        {
            CounterGrid.Children.Clear();
            CounterGrid.RowDefinitions.Clear();
            _bulletCounters.Clear();
        }

        private void TransparencySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }

        private void TransparancyOnMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
        }

        private void TransparancyOnMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.AllowsTransparency = true;
        }

        private void TopmostMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
        }

        private void TopmostMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;
        }

        private void ComItemsCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            var serialPortName = e.AddedItems[0]?.ToString();

            if (serialPortName is null)
            {
                return;
            }

            SetSerialPort(serialPortName);
        }

        private bool SetSerialPort(string serialPortName)
        {
            var isComSetted = SerialPortWorker.Singleton.TrySetPort(serialPortName);
            if (!isComSetted)
            {
                MessageBox.Show($"Не удалось выставить порт {serialPortName}. Проверьте, подключено ли устройство именно в этот порт", "Не удалось подключиться к COM", MessageBoxButton.OK, MessageBoxImage.Warning);               
            }
            return isComSetted;
        }

        private void ComMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            var ports = SerialPort.GetPortNames().ToList();
            var currentSelected = ComItemsCombobox.SelectedItem as string;
            var deletedItems = new List<string>();

            foreach (var item in ComItemsCombobox.Items.Cast<string>())
            {
                if (!ports.Contains(item))
                {
                    deletedItems.Add(item);
                    continue;
                }

                ports.Remove(item);
            }

            foreach (var item in deletedItems)
            {
                ComItemsCombobox.Items.Remove(item);
            }

            foreach (var item in ports)
            {
                ComItemsCombobox.Items.Add(item);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var locationFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var settingsFilePath = Path.Combine(locationFolder, "lastSettings.json");

            var currentSettings = new Settings(SerialPortWorker.Singleton.GetCurrentPortName(), _bulletCounters.Select(counter => counter.Source).ToArray(), (int)JoyInfoDelaySlider.Value);
            var json = JsonSerializer.Serialize(currentSettings);

            if (!File.Exists(settingsFilePath))
            {
                File.WriteAllText(settingsFilePath, json);
                return;
            }

            var prevSettings = File.ReadAllText(settingsFilePath);

            if (prevSettings == json)
            {
                return;
            }

            var shouldSave = MessageBox.Show("Сохранить изменения?", "Что-то поменялось", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (shouldSave == MessageBoxResult.Yes)
            {
                File.WriteAllText(settingsFilePath, json);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var locationFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var settingsFilePath = Path.Combine(locationFolder, "lastSettings.json");

            if (!File.Exists(settingsFilePath))
            {
                return;
            }

            var shouldLoadLastState = MessageBox.Show("Открыть настройки, которые были при выходе?", "Воротать как было?", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (shouldLoadLastState == MessageBoxResult.No)
            {
                JoyInfoDelaySlider.Value = Configs.DefaultRequestIntervalInMilliseconds;
                return;
            }

            var json = File.ReadAllText(settingsFilePath);
            var lastSettings = JsonSerializer.Deserialize<Settings>(json);

            if (lastSettings is null)
            {
                MessageBox.Show($"Не получилось открыть последние настройки. Скиньте, пожалуйста, файл {settingsFilePath} разработчику", "Не удалось =(", MessageBoxButton.OK, MessageBoxImage.Error);
                JoyInfoDelaySlider.Value = Configs.DefaultRequestIntervalInMilliseconds;
                return;
            }

            if (lastSettings.ComPortName is not null)
            {
                if (SetSerialPort(lastSettings.ComPortName))
                {
                    ComItemsCombobox.SelectedItem = lastSettings.ComPortName;
                }
            }

            if (lastSettings.JoyInfoRequestDelayInMs is null)
            {
                JoyInfoDelaySlider.Value = Configs.DefaultRequestIntervalInMilliseconds;
            }
            else
            {
                JoyInfoDelaySlider.Value = lastSettings.JoyInfoRequestDelayInMs.Value;
            }

            Array.ForEach(lastSettings.Models, AddNewCounter);
        }

        private void JoyInfoDelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Configs.RequestIntervalInMilliseconds = (int)e.NewValue;
        }
    }
}