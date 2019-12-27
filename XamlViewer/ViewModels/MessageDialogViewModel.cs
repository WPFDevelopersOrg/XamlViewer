using System;
using System.Windows;
using System.Windows.Media;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using XamlViewer.Models;

namespace XamlViewer.ViewModels
{
    public class MessageDialogViewModel : BindableBase, IDialogAware
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        private Brush _geometryBrush = Brushes.Black;
        public Brush GeometryBrush
        {
            get { return _geometryBrush; }
            set { SetProperty(ref _geometryBrush, value); }
        }

        private Visibility _geometryVisibility = Visibility.Visible;
        public Visibility GeometryVisibility
        {
            get { return _geometryVisibility; }
            set { SetProperty(ref _geometryVisibility, value); }
        }

        private Geometry _messageGeometry;
        public Geometry MessageGeometry
        {
            get { return _messageGeometry; }
            set { SetProperty(ref _messageGeometry, value); }
        }

        private Visibility _ignoreVisibility = Visibility.Collapsed;
        public Visibility IgnoreVisibility
        {
            get { return _ignoreVisibility; }
            set { SetProperty(ref _ignoreVisibility, value); }
        }

        private Visibility _yesVisibility = Visibility.Collapsed;
        public Visibility YesVisibility
        {
            get { return _yesVisibility; }
            set { SetProperty(ref _yesVisibility, value); }
        }

        private Visibility _noVisibility = Visibility.Collapsed;
        public Visibility NoVisibility
        {
            get { return _noVisibility; }
            set { SetProperty(ref _noVisibility, value); }
        }

        private Visibility _okVisibility = Visibility.Collapsed;
        public Visibility OKVisibility
        {
            get { return _okVisibility; }
            set { SetProperty(ref _okVisibility, value); }
        }

        private Visibility _cancelVisibility = Visibility.Collapsed;
        public Visibility CancelVisibility
        {
            get { return _cancelVisibility; }
            set { SetProperty(ref _cancelVisibility, value); }
        }

        #region Command

        private DelegateCommand _ignoreCommand;
        public DelegateCommand IgnoreCommand
        {
            get { return _ignoreCommand ?? (_ignoreCommand = new DelegateCommand(() => CloseDialog(ButtonResult.Ignore))); }
        }

        private DelegateCommand _okCommand;
        public DelegateCommand OKCommand
        {
            get { return _okCommand ?? (_okCommand = new DelegateCommand(() => CloseDialog(ButtonResult.OK))); }
        }

        private DelegateCommand _cancelCommand;
        public DelegateCommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new DelegateCommand(() => CloseDialog(ButtonResult.Cancel))); }
        }

        private DelegateCommand _yesCommand;
        public DelegateCommand YesCommand
        {
            get { return _yesCommand ?? (_yesCommand = new DelegateCommand(() => CloseDialog(ButtonResult.Yes))); }
        }

        private DelegateCommand _noCommand;
        public DelegateCommand NoCommand
        {
            get { return _noCommand ?? (_noCommand = new DelegateCommand(() => CloseDialog(ButtonResult.No))); }
        }

        private void CloseDialog(ButtonResult result)
        {
            if (RequestClose != null)
                RequestClose.Invoke(new DialogResult(result));
        }

        #endregion

        #region IDialogAware Members

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            UpdateMessageButton(parameters.GetValue<MessageButton>("Button"));
            UpdateMessageImage(parameters.GetValue<MessageType>("Type"));
            Message = parameters.GetValue<string>("Message");
        }

        public event Action<IDialogResult> RequestClose;

        public string Title
        {
            get { return "Message"; }
        }

        #endregion

        private void UpdateMessageButton(MessageButton button)
        {
            switch (button)
            {
                case MessageButton.OK:
                    OKVisibility = Visibility.Visible;
                    break;

                case MessageButton.OKCancel:
                    OKVisibility = Visibility.Visible;
                    CancelVisibility = Visibility.Visible;
                    break;

                case MessageButton.YesNo:
                    YesVisibility = Visibility.Visible;
                    NoVisibility = Visibility.Visible;
                    break;

                case MessageButton.YesNoCancel:
                    YesVisibility = Visibility.Visible;
                    NoVisibility = Visibility.Visible;
                    CancelVisibility = Visibility.Visible;
                    break;
            }
        }

        private void UpdateMessageImage(MessageType type)
        {
            switch (type)
            {
                case MessageType.None:
                    GeometryVisibility = Visibility.Collapsed;
                    MessageGeometry = null;
                    break;

                case MessageType.Information:
                    GeometryBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0979D7"));
                    MessageGeometry = Application.Current.Resources["InfomationGeometryKey"] as Geometry;
                    break;

                case MessageType.Warning:
                    GeometryBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E7AF05"));
                    MessageGeometry = Application.Current.Resources["WarningGeometryKey"] as Geometry;
                    break;

                case MessageType.Error:
                    GeometryBrush = Brushes.Red;
                    MessageGeometry = Application.Current.Resources["ErrorGeometryKey"] as Geometry;
                    break;

                case MessageType.Question:
                    GeometryBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0979D7"));
                    MessageGeometry = Application.Current.Resources["QuestionGeometryKey"] as Geometry;
                    break;
            }
        }
    }
}
