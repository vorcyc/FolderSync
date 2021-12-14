using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Vorcyc.FolderSync
{
    using Microsoft.WindowsAPICodePack.Dialogs;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using System.Windows.Interop;
    using Vorcyc.ModernUI.Windows.Controls;
    using System.IO;


    public partial class MainWindow : ModernNormalWindow
    {

        private string _sourceFolder;
        private string _targetFolder;

        private Engine _engine = new Engine();

        public MainWindow()
        {
            InitializeComponent();
        }



        private string SelectFolder()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }
            return null;
        }

        private void btnSelectSourceFolder_Click(object sender, RoutedEventArgs e)
        {
            _sourceFolder = SelectFolder();
            txtSourceFolder.Text = _sourceFolder;
        }

        private void btnSelectTargetFolder_Click(object sender, RoutedEventArgs e)
        {
            _targetFolder = SelectFolder();
            txtTargetFolder.Text = _targetFolder;
        }

        private async void btnScan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_sourceFolder) || string.IsNullOrEmpty(_targetFolder))
            {
                ModernDialog.ShowMessage("请先选择源目录和目标目录，再扫描", "操作错误", MessageBoxButton.OK);
                return;
            }

            btnScan.IsEnabled = btnGo.IsEnabled = false;

            DG1.ItemsSource = null;

            var report = await _engine.ScanAsync(_sourceFolder, _targetFolder);

            var dlg = new ModernDialog
            {
                Title = "报告",
                Content = new ucReport() { DataContext = report }
            };
            dlg.Buttons = new Button[] { dlg.OkButton };
            dlg.ShowDialog();

            UpdateViewItems();

            btnGo.IsEnabled = true;
        }


        private async void btnGo_Click(object sender, RoutedEventArgs e)
        {
            btnGo.IsEnabled = false;
            btnScan.IsEnabled = false;

            Vorcyc.ModernUI.Presentation.AppearanceManager.Current.AccentColor = Color.FromRgb(235, 45, 45);

            await _engine.RunAsync();
            await _engine.ScanAsync(_sourceFolder, _targetFolder);
            DG1.ItemsSource = _engine.Items;


            Vorcyc.ModernUI.Presentation.AppearanceManager.Current.AccentColor = Color.FromRgb(126, 59, 188);
            btnScan.IsEnabled = true;
            btnGo.IsEnabled = false;

            ModernDialog.ShowMessage("同步完成", "提示", MessageBoxButton.OK, this);
        }

        private void ChangeViewItems(object sender, RoutedEventArgs e)
        {

            UpdateViewItems();
        }

        private void UpdateViewItems()
        {
            if (cbShowAll.IsChecked == true)
            {
                DG1.ItemsSource = _engine.Items;
                return;
            }
            else
            {
                if (rbShowCreate.IsChecked == true)
                {
                    DG1.ItemsSource = from pt in _engine.Items
                                      where pt.Behaviour == Behaviour.Create
                                      select pt;
                }
                else if (rbShowOverride.IsChecked == true)
                {
                    DG1.ItemsSource = from pt in _engine.Items
                                      where pt.Behaviour == Behaviour.Override
                                      select pt;
                }
                else if (rbShowDelete.IsChecked == true)
                {
                    DG1.ItemsSource = from pt in _engine.Items
                                      where pt.Behaviour == Behaviour.Delete
                                      select pt;
                }
                else if (rbShowKeep.IsChecked == true)
                {
                    DG1.ItemsSource = from pt in _engine.Items
                                      where pt.Behaviour == Behaviour.Keep
                                      select pt;
                }
            }

        }

    }
}
