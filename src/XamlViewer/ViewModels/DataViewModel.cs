using System;
using System.Windows;
using System.Text.RegularExpressions;

using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using XamlService.Events;
using XamlService.Payloads;
using XamlUtil.Common;
using XamlUtil.Net;
using XamlTheme.Controls;
using XamlViewer.Models;
using XamlViewer.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XamlViewer.ViewModels
{
    public class DataViewModel : BindableBase
    {
        private AppData _appData = null;
        private IEventAggregator _eventAggregator = null;
        private IDialogService _dialogService = null;
		
		private TextEditorEx _textEditor = null;

        public DelegateCommand<TextEditorEx> LoadedCommand { get; private set; }
		public DelegateCommand DelayArrivedCommand { get; private set; }
		
		public DelegateCommand ClearCommand { get; private set; }
		public DelegateCommand FormatCommand { get; private set; }
        public DelegateCommand RequestCommand { get; private set; }

        public DataViewModel(IContainerExtension container, IEventAggregator eventAggregator)
        {
            _appData = container.Resolve<AppData>();

            _eventAggregator = eventAggregator;
            _dialogService = container.Resolve<IDialogService>();

            InitEvent();
            InitCommand();
        }

        #region Init

        private void InitEvent()
		{
            _eventAggregator.GetEvent<SettingChangedEvent>().Subscribe(OnSettingChanged, ThreadOption.PublisherThread, false);
		}

        private void InitCommand()
        {
			LoadedCommand = new DelegateCommand<TextEditorEx>(Loaded);
            DelayArrivedCommand = new DelegateCommand(DelayArrived);
			
			ClearCommand = new DelegateCommand(Clear);
			FormatCommand = new DelegateCommand(Format);
            RequestCommand = new DelegateCommand(Request);
        }

        #endregion

		#region Event

        private void OnSettingChanged(ValueWithGuid<EditorSetting> valueWithGuid)
        {
            FontFamily = valueWithGuid.Value.FontFamily;
            FontSize = valueWithGuid.Value.FontSize;
            WordWrap = valueWithGuid.Value.WordWrap;
        }
		
		#endregion

        #region Command
		
		private void Loaded(TextEditorEx textEditor)
        {
            _textEditor = textEditor;
			
			if(_textEditor != null)
			{
				_textEditor.LoadSyntaxHighlighting(AppDomain.CurrentDomain.BaseDirectory + "Assets\\Json.xshd");
				_textEditor.Text = 	JsonString;
			}

            UpdateByJsonString();
            
            if (IsSyncDataSource)
                UpdateDataSource(true);
        }

        private void DelayArrived()
        {
			JsonString = _textEditor?.Text;
        }
		
        private void Clear()
		{
			_textEditor.Text = JsonString = string.Empty;
        }

        private void Format()
		{
			var text = _textEditor.Text;
			if(string.IsNullOrWhiteSpace(text))
				return;
			
			JsonString = JToken.Parse(text).ToString(Formatting.Indented);
            _textEditor.Text = JsonString;
		}

        private async void Request()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(RestApi) || !Regex.IsMatch(RestApi, @"^https?://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]$"))
                {
                    _dialogService.ShowMessage("Please check the rest api and try again.", MessageButton.OK, MessageType.Error);
                    return;
                }

                CanFetch = false;
				
				var json = await HttpUtil.GetString(RestApi);
				if(!string.IsNullOrWhiteSpace(json))
					json = JToken.Parse(json).ToString(Formatting.Indented);
				
				JsonString = json;
                _textEditor.Text = JsonString;
				
				CanFetch = true;
            }
            catch(Exception ex) 
            {
                System.Diagnostics.Trace.TraceError("[ Http GetString ] " + XamlUtil.Common.Common.GetExceptionStringFormat(ex));
				_dialogService.ShowMessage(ex.Message, MessageButton.OK, MessageType.Error);
            }
			finally
			{
				CanFetch = true;
			}
        }

        #endregion

        private string _fontFamily = "Calibri";
        public string FontFamily
        {
            get { return _fontFamily; }
            set { SetProperty(ref _fontFamily, value); }
        }

        private double _fontSize = 12d;
        public double FontSize
        {
            get { return _fontSize; }
            set { SetProperty(ref _fontSize, value); }
        }

        private bool _wordWrap = false;
        public bool WordWrap
        {
            get { return _wordWrap; }
            set { SetProperty(ref _wordWrap, value); }
        }
		
        public bool IsSyncDataSource
        {
            get { return _appData.Config.IsSyncDataSource; }
            set
            {
                if(_appData.Config.IsSyncDataSource == value)
                    return;
            
                _appData.Config.IsSyncDataSource = value;
                
                RaisePropertyChanged();
                UpdateDataSource(value);
            }
        }

        private bool _canClear = true;
        public bool CanClear
        {
            get { return _canClear; }
            set { SetProperty(ref _canClear, value); }
        }

        private bool _canFetch;
        public bool CanFetch
        {
            get { return _canFetch; }
            set { SetProperty(ref _canFetch, value); }
        }

        private string _restApi;
        public string RestApi
        {
            get { return _restApi; }
            set 
			{ 
			    SetProperty(ref _restApi, value); 
				CanFetch = !string.IsNullOrWhiteSpace(_restApi);
			}
        }

        public string JsonString
        {
            get { return _appData.Config.DataSourceJsonString; }
            set
            {
                if (_appData.Config.DataSourceJsonString == value)
                    return;

                _appData.Config.DataSourceJsonString = value;

                UpdateByJsonString();
                UpdateDataSource(IsSyncDataSource);
            }
        }

        private Visibility _jsonTipVisibility = Visibility.Visible;
        public Visibility JsonTipVisibility
        {
            get { return _jsonTipVisibility; }
            set { SetProperty(ref _jsonTipVisibility, value); }
        }

        private void UpdateDataSource(bool isSync)
        {
            _eventAggregator.GetEvent<SyncDataSourceEvent>().Publish(isSync ? JsonString?.Trim() : null);
        }
		
		private void UpdateByJsonString()
		{ 
			CanClear = !string.IsNullOrEmpty(JsonString);
            JsonTipVisibility =  CanClear ? Visibility.Collapsed : Visibility.Visible;
		}
    }
}
