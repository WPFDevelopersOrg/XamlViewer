using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Text.RegularExpressions;
using XamlService.Events;
using XamlUtil.Common;
using XamlUtil.Net;
using XamlViewer.Models;
using XamlViewer.Utils;

namespace XamlViewer.ViewModels
{
    public class DataViewModel : BindableBase
    {
        private AppData _appData = null;
        private IEventAggregator _eventAggregator = null;
        private IDialogService _dialogService = null;

        public DelegateCommand RequestCommand { get; private set; }

        public DataViewModel(IContainerExtension container, IEventAggregator eventAggregator)
        {
            _appData = container.Resolve<AppData>();

            _eventAggregator = eventAggregator;
            _dialogService = container.Resolve<IDialogService>();

            InitCommand();
            InitStatus();
        }

        #region Init

        private void InitCommand()
        {
            RequestCommand = new DelegateCommand(Request);
        }

        private void InitStatus()
        {
            if (IsSyncDataSource)
                UpdateDataSource(true);
        }

        #endregion

        #region Command

        private async void Request()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(RestApi) || !Regex.IsMatch(RestApi, @"^https?://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]$"))
                {
                    _dialogService.ShowMessage("Please check the rest api and try again.", MessageButton.OK, MessageType.Error);
                    return;
                }

                JsonString = await HttpUtil.GetString(RestApi);
            }
            catch { }
        }

        #endregion

        private bool _isSyncDataSource;
        public bool IsSyncDataSource
        {
            get { return _isSyncDataSource; }
            set
            {
                SetProperty(ref _isSyncDataSource, value);
                UpdateDataSource(value);
            }
        }

        private string _restApi;
        public string RestApi
        {
            get { return _restApi; }
            set { SetProperty(ref _restApi, value); }
        }

        public string JsonString
        {
            get { return _appData.Config.DataSourceJsonString; }
            set
            {
                if (_appData.Config.DataSourceJsonString == value)
                    return;

                _appData.Config.DataSourceJsonString = value;

                RaisePropertyChanged();
                UpdateDataSource(IsSyncDataSource);
            }
        }

        private void UpdateDataSource(bool isSync)
        {
            _eventAggregator.GetEvent<SyncDataSourceEvent>().Publish(isSync ? JsonString?.Trim() : null);
        }
    }
}
