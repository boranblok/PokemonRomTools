using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PkmnAdvanceTranslation.Util
{
    /// <summary>
    /// This control is used as a launchpad for a modal dialog.  Effectively you specify the configuration of the dialog on this control 
    ///   and set the workspace, which in turn displays the dialog.  After the dialog is closed.  The request close event is listened to so
    ///   that the workspace can control the closing of itself.  
    /// </summary>
    /// <remarks>
    /// Source from The Silver Method: http://www.thesilvermethod.com/Default.aspx?Id=ModalDialogManagerAsimpleapproachtodealingwithmodaldialogsinMVVM
    /// altered by Ben Blok, Edan Business solutions
    /// </remarks>
    public class BindableDialog : Control
    {
        private Window MyWindow;
        private Boolean _internalClose = false;
        private Boolean _externalClose = false;

        static BindableDialog()
        {
            //this control really is a blank template - it really just contains a UI presence so it can be declaratively 
            //be added to a page.  

            //the DataContext of this control must be set to the ViewModel that you wish to display in the dialog.  Also you must 
            //configure a DataTemplate that associates the ViewModel to the View that will be shown inside this dialog
            // e.g
            //         <DataTemplate DataType="{x:Type vm:MessageWindowViewModel}">
            //              <v:MessageWindow/>
            //          </DataTemplate>
            //Usually these datatemplates are defined in a global resource library such as App.xaml
            //If this is not configured propertly instead of seeing your control - you will just see the classname in the resulting dialog
        }

        /// <summary>
        /// This is invoked when the red X is clicked or a keypress closes the window - 
        /// </summary>
        public ICommand CloseCommand
        {
            get { return (ICommand)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }
        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register("CloseCommand", typeof(ICommand), typeof(BindableDialog), new UIPropertyMetadata(null));

        public Func<Boolean> CanCloseDialog
        {
            get { return (Func<Boolean>)GetValue(CanCloseDialogProperty); }
            set { SetValue(CanCloseDialogProperty, value); }
        }
        public static readonly DependencyProperty CanCloseDialogProperty =
            DependencyProperty.Register("CanCloseDialog", typeof(Func<Boolean>), typeof(BindableDialog), new UIPropertyMetadata(null));       

        /// <summary>
        /// This should be bound to IsOpen (or similar) in the ViewModel associated with ModalDialogManager
        /// </summary>
        /// <remarks>
        /// At the moment this only works one way, 
        /// after setting the IsOpen to true in the viewModel it should be set back to false 
        /// in the following line or in the close command.
        /// </remarks>
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(Boolean), typeof(BindableDialog), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsOpenChanged));

        public static void IsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BindableDialog dialogManager = d as BindableDialog;
            Boolean newVal = (Boolean)e.NewValue;
            if (newVal)
                dialogManager.Show();
            else
                dialogManager.Close();
        }

        private void Show()
        {
            if (MyWindow != null) Close();

            MyWindow = new Window();
            MyWindow.Closing += OnWindowClosing;
            MyWindow.Owner = GetParentWindow(this);
            MyWindow.Resources = this.Resources;

            MyWindow.DataContext = this.DataContext;
            MyWindow.SetBinding(Window.ContentProperty, "");

            MyWindow.Title = Title;
            MyWindow.Icon = Icon;
            MyWindow.Height = DialogHeight;
            MyWindow.Width = DialogWidth;
            MyWindow.ResizeMode = DialogResizeMode;
            MyWindow.SizeToContent = SizeToContent;
            MyWindow.WindowStartupLocation = WindowStartupLocation;
            MyWindow.ShowInTaskbar = ShowInTaskbar;
            MyWindow.ShowDialog();           
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (!_internalClose)
            {
                _externalClose = true;
                if (CanCloseDialog == null)
                {
                    if (CloseCommand != null) CloseCommand.Execute(null);
                }
                else
                {
                    if (CanCloseDialog())
                    {
                        if (CloseCommand != null) CloseCommand.Execute(null);
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }                
                _externalClose = false;
            }
        }

        private void Close()
        {
            _internalClose = true;

            if (!_externalClose) MyWindow.Close();
            MyWindow = null;
            _internalClose = false;
        }

        private Window GetParentWindow(FrameworkElement current)
        {
            if (current is Window)
                return current as Window;
            else if (current.Parent is FrameworkElement)
                return GetParentWindow(current.Parent as FrameworkElement);
            else
                return null;
        }


        #region DependencyProperties that control the look of the shown dialog

        public double DialogHeight
        {
            get { return (double)GetValue(DialogHeightProperty); }
            set { SetValue(DialogHeightProperty, value); }
        }
        public static readonly DependencyProperty DialogHeightProperty =
            DependencyProperty.Register("DialogHeight", typeof(double), typeof(BindableDialog));

        public double DialogWidth
        {
            get { return (double)GetValue(DialogWidthProperty); }
            set { SetValue(DialogWidthProperty, value); }
        }
        public static readonly DependencyProperty DialogWidthProperty =
            DependencyProperty.Register("DialogWidth", typeof(double), typeof(BindableDialog));

        public ResizeMode DialogResizeMode
        {
            get { return (ResizeMode)GetValue(DialogResizeModeProperty); }
            set { SetValue(DialogResizeModeProperty, value); }
        }
        public static readonly DependencyProperty DialogResizeModeProperty =
            DependencyProperty.Register("DialogResizeMode", typeof(ResizeMode), typeof(BindableDialog));

        public SizeToContent SizeToContent
        {
            get { return (SizeToContent)GetValue(SizeToContentProperty); }
            set { SetValue(SizeToContentProperty, value); }
        }

        public static readonly DependencyProperty SizeToContentProperty =
            DependencyProperty.Register("SizeToContent", typeof(SizeToContent), typeof(BindableDialog));

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(BindableDialog), new UIPropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(BindableDialog), new UIPropertyMetadata(null));

        public WindowStartupLocation WindowStartupLocation
        {
            get { return (WindowStartupLocation)GetValue(WindowStartupLocationProperty); }
            set { SetValue(WindowStartupLocationProperty, value); }
        }
        public static readonly DependencyProperty WindowStartupLocationProperty =
            DependencyProperty.Register("WindowStartupLocation", typeof(WindowStartupLocation), typeof(BindableDialog), new UIPropertyMetadata(WindowStartupLocation.CenterOwner));

        public Boolean ShowInTaskbar
        {
            get { return (Boolean)GetValue(ShowInTaskBarProperty); }
            set { SetValue(ShowInTaskBarProperty, value); }
        }
        public static readonly DependencyProperty ShowInTaskBarProperty =
            DependencyProperty.Register("ShowInTaskbar", typeof(Boolean), typeof(BindableDialog), new UIPropertyMetadata(true));
        #endregion
    }
}
