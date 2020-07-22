using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DicomFinder
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Title = "Dicom Finder " + Assembly.GetExecutingAssembly().GetName().Version;

            // FolderField.Text = @"C:\Testing\Images\CT_3D_EXTREMITY_1747";
            FolderField.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FolderField.Text = dialog.FileName;
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<FileContext>();
            DateTime startTime = DateTime.Now;
            var dicomDirectory = new DicomDirectory(OneFileCheckBox?.IsChecked ?? false, ValueCheckBox?.IsChecked ?? false);
            dicomDirectory.Search(list, FolderField.Text, GroupTextBox.Text, ElementTextBox.Text, ValueTextBox.Text, CountLabel);
            var endTime = DateTime.Now - startTime;

            FilesDataGrid.ItemsSource = list;

            TimeLabel.Content = "Time: " + (int)endTime.TotalMilliseconds;
            CountLabel.Content = "Count: " + list.Count;
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out _);
        }

        private void ValueCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            if (checkBox.IsChecked != null)
            {
                ValueTextBox.IsEnabled = (bool)checkBox.IsChecked;
            }
        }

        private void FilesDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridCellInfo cell = FilesDataGrid.CurrentCell;
            Clipboard.SetText((cell.Item as FileContext)?.Path ?? string.Empty);
        }
    }
}
