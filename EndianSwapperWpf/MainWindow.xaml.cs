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

namespace EndianSwapperWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataObject.AddPastingHandler(txtPTR, OnPaste);
            DataObject.AddPastingHandler(txtREF, OnPaste);
        }

        private void SetRef(String ptr)
        {
            txtREF.Text = ptr.Substring(4, 2) + ptr.Substring(2, 2) + ptr.Substring(0, 2) + "08";
            Clipboard.SetText(txtREF.Text);
        }

        private void SetPtr(String rfr)
        {
            txtPTR.Text = rfr.Substring(4, 2) + rfr.Substring(2, 2) + rfr.Substring(0, 2);
            Clipboard.SetText(txtPTR.Text);
        }

        private void btnToREF_Click(object sender, RoutedEventArgs e)
        {
            SetRef(txtPTR.Text);
        }

        private void btnToPTR_Click(object sender, RoutedEventArgs e)
        {
            SetPtr(txtREF.Text);
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
            if (!isText) return;

            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
            if (sender == txtPTR)
            {
                txtPTR.Text = text;
                SetRef(text);
            }
            else if (sender == txtREF)
            {
                txtREF.Text = text;
                SetPtr(text);
            }
            e.CancelCommand();
        }
    }
}
