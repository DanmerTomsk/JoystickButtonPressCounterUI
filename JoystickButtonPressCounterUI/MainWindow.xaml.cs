using JoystickButtonPressCounterUI.Models;
using JoystickButtonPressCounterUI.Observer;
using JoystickButtonPressCounterUI.ViewModels;
using Microsoft.Win32;
using System.IO;
using System.IO.Ports;
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
            var counterIndex = _bulletCounters.IndexOf(oldCounter);
            var newViewModel = new BulletCounterViewModel(newCounter);
            _bulletCounters[counterIndex] = newViewModel;
            parent.DataContext = newViewModel;
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
            RemoveAllCounters();

            foreach (var model in models)
            {
                AddNewCounter(model);
            }
        }

        private void AddNewCounter(CounterModel newCounter)
        {
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
            if (bulletCounter.Source.Order == CounterModel.EmptyOrderValue)
            {
                bulletCounter.Source.Order = (byte)(_bulletCounters.Count - 1);
            }
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

            SerialPortWorker.Singleton.SetPort(serialPortName);
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

        private void ComButtonsSettings_Click(object sender, RoutedEventArgs e)
        {
            var ComButtonsSettingsWindow = new ComButtonsSettingsWindow()
            {
                Owner = this,
            };

            ComButtonsSettingsWindow.Show();
        }
    }
}