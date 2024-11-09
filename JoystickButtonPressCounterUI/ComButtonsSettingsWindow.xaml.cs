using JoystickButtonPressCounterUI.Models;
using JoystickButtonPressCounterUI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace JoystickButtonPressCounterUI
{
    /// <summary>
    /// Логика взаимодействия для ComButtonsSettingsWindow.xaml
    /// </summary>
    public partial class ComButtonsSettingsWindow : Window
    {
        private byte _currentMappingsCount;

        public ComButtonsSettingsWindow()
        {
            InitializeComponent();
            for (int i = 0; i < 2; i++)
            {
                AddNewButtonMappingPanel();
            }
        }

        private void AddMappingButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewButtonMappingPanel();
        }

        private void AddNewButtonMappingPanel()
        {
            var newMapping = new ComButtonMapping(++_currentMappingsCount);
            var newViewModel = new ComButtonMappingViewModel(newMapping);

            var template = (ControlTemplate)this.FindResource("ComButtonMappingTemplate");

            var newContentControl = new ContentControl()
            {
                Template = template,
                DataContext = newViewModel,
            };

            ButtonsMappingStackPanel.Children.Add(newContentControl);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
