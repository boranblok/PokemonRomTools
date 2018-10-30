using Microsoft.Win32;
using PkmnAdvanceTranslation.Util;
using PkmnAdvanceTranslation.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace PkmnAdvanceTranslation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IOService
    {
        MainWindowViewModel vm;
        public MainWindow()
        {
            vm = new MainWindowViewModel(this);
            DataContext = vm;

            InitializeComponent();
        }

        public String OpenFileDialog(String defaultPath, String title)
        {
            var dialog = new OpenFileDialog();            
            if (!String.IsNullOrWhiteSpace(defaultPath) && Directory.Exists(defaultPath))
                dialog.InitialDirectory = defaultPath;
            if (!String.IsNullOrWhiteSpace(title))
                dialog.Title = title;
            var result = dialog.ShowDialog(this);
            if(result == true && File.Exists(dialog.FileName))
            {
                return dialog.FileName;
            }
            return null;
        }

        public String SaveFileDialog(String defaultPath, String defaultName, String title, String defaultEx)
        {
            var dialog = new SaveFileDialog();
            if (!String.IsNullOrWhiteSpace(defaultPath) && Directory.Exists(defaultPath))
                dialog.InitialDirectory = defaultPath;
            if (!String.IsNullOrWhiteSpace(defaultName))
                dialog.FileName = defaultName;
            if (!String.IsNullOrWhiteSpace(title))
                dialog.Title = title;
            if (!String.IsNullOrWhiteSpace(defaultEx))
                dialog.DefaultExt = defaultEx;
            var result = dialog.ShowDialog();
            if(result == true)
            {
                return dialog.FileName;
            }
            return null;
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {            
            
            var unsavedLineCount = vm.TranslationLines.Count(l => l.HasUnsavedChanges);
            if(unsavedLineCount > 0)
            {
                var message = String.Format("There are {0} lines still being edited on, do you want to save these?", unsavedLineCount);
                var result = MessageBox.Show(this, message, "Save edited lines?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
                switch(result)
                {
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        return;
                    case MessageBoxResult.No:
                        vm.DiscardEditedLines();
                        break;
                    case MessageBoxResult.Yes:
                        vm.SaveEditedLines();
                        break;
                }
            }
            var changedLineCount = vm.TranslationLines.Count(l => l.HasChangesInMemory);
            if (changedLineCount > 0)
            {
                var message = String.Format("Save changes to \"{0}\"?", vm.TranslationFile.FullName);
                var result = MessageBox.Show(this, message,  "Save changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
                switch(result)
                {
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        return;
                    case MessageBoxResult.Yes:
                        vm.SaveTranslationFileCommand.Execute(null);
                        break;
                }
            }
        }

        private void txtTranslated_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            txtUnTranslated.ScrollToVerticalOffset(txtUnTranslated.VerticalOffset + e.VerticalChange);
        }
    }
}
