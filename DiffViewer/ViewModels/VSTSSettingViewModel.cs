using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiffViewer.Managers;
using DiffViewer.Managers.Helper;
using Serilog;

namespace DiffViewer.ViewModels;

public partial class VSTSSettingViewModel : ObservableObject
{
    private ILogger _logger;

    public VSTSSettingViewModel(ILogger logger)
    {
        this._logger = logger;

        if( AppConfigManager.AccessCode is not null )
        {
            this.VstsUrl = AppConfigManager.AccessCode.Url ?? string.Empty;
            this.AccessToken = AppConfigManager.AccessCode.Token ?? string.Empty;
            this.AccessCookie = AppConfigManager.AccessCode.Cookie ?? string.Empty;
        }
    }

    [ObservableProperty]
    public string _vstsUrl = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveAccessCodeCommand))]
    public string _accessToken = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveAccessCodeCommand))]
    public string _accessCookie = string.Empty;

    [ObservableProperty]
    public bool _isAccessByToken = true;

    [ObservableProperty]
    public bool _canSave;

    private bool CheckTokenOrCookie( )
    {
        if( AccessToken.IsNullOrWhiteSpaceOrEmpty() && AccessCookie.IsNullOrWhiteSpaceOrEmpty() )
        {
            CanSave = false;
            return false;
        }
        else
        {
            CanSave = true;
            return true;
        }
    }

    [RelayCommand(CanExecute = (nameof(CheckTokenOrCookie)))]
    public void SaveAccessCode( )
    {
        _logger.Information("SaveCommand called");
        _logger.Information($"AccessCode Token:{AccessToken}");
        _logger.Information($"AccessCode Cookie:{AccessCookie}");

        AppConfigManager.AccessCode = new()
        {
            Url = this.VstsUrl ,
            Token = this.AccessToken ,
            Cookie = this.AccessCookie
        };

    }
}
