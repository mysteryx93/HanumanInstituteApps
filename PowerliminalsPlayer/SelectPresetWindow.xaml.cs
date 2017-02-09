using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PowerliminalsPlayer {
    /// <summary>
    /// Interaction logic for SelectPresetWindow.xaml
    /// </summary>
    public partial class SelectPresetWindow : Window {
        internal ObservableCollection<PresetItem> list;
        internal string Result;
        internal bool isLoad = false;

        public SelectPresetWindow() {
            InitializeComponent();
        }

        public static string InstanceSave(Window owner, ObservableCollection<PresetItem> list) {
            SelectPresetWindow Form = new SelectPresetWindow();
            Form.Owner = owner;
            Form.list = list;
            Form.PresetList.DataContext = list;
            Form.isLoad = false;
            Form.NameBox.Focus();
            Form.NameBox.SelectAll();
            Form.ShowDialog();
            return Form.Result;
        }

        public static string InstanceLoad(Window owner, ObservableCollection<PresetItem> list) {
            SelectPresetWindow Form = new SelectPresetWindow();
            Form.Owner = owner;
            Form.PresetList.DataContext = list;
            Form.list = list;
            Form.isLoad = true;
            Form.Title = "Load Preset...";
            Form.NameBox.Visibility = Visibility.Hidden;
            Form.PresetList.Margin = new Thickness(Form.PresetList.Margin.Left, 0, Form.PresetList.Margin.Right, Form.PresetList.Margin.Bottom);
            Form.ShowDialog();
            return Form.Result;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            if (isLoad && list.Count > 0) {
                PresetList.SelectedIndex = 0;
                PresetList.Focus();
                ((ListBoxItem)PresetList.ItemContainerGenerator.ContainerFromIndex(0)).Focus();
            }
        }

        private void PresetList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            NameBox.Text = (string)PresetList.SelectedValue;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            Result = NameBox.Text.Trim();
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void PresetList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dataContext is PresetItem && e.LeftButton == MouseButtonState.Pressed) {
                OkButton_Click(null, null);
            }
        }

        private void DelButton_Click(object sender, RoutedEventArgs e) {
            if (PresetList.SelectedItem != null) {
                list.Remove((PresetItem)PresetList.SelectedItem);
                NameBox.Focus();
            }
        }
    }
}
