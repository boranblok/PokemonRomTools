using Microsoft.Win32;
using PkmnAdvanceTranslation.Util;
using System;
using System.Collections.Generic;
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
        public MainWindow()
        {
            var textHandler = new TextHandler();
            var vm = new MainWindowViewModel(this, textHandler);
            DataContext = vm;

            InitializeComponent();
        }

        public String OpenFileDialog(String defaultPath, String title)
        {
            var dialog = new OpenFileDialog();
            if (!String.IsNullOrWhiteSpace(defaultPath) && Directory.Exists(defaultPath))
            {
                dialog.InitialDirectory = defaultPath;
            }
            if (!string.IsNullOrWhiteSpace(title))
            {
                dialog.Title = title;
            }
            var result = dialog.ShowDialog(this);
            if(result == true && File.Exists(dialog.FileName))
            {
                return dialog.FileName;
            }
            return null;
        }
    }
}
