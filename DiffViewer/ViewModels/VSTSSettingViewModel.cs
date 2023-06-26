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

        RefreshAccessInfos();
    }
    public void RefreshAccessInfos( )
    {
        if( AppConfigManager.AccessCode is not null )
        {
            this.VstsUrl = AppConfigManager.AccessCode.Url ?? string.Empty;
            this.AccessToken = AppConfigManager.AccessCode.Token ?? string.Empty;
            this.AccessCookie = AppConfigManager.AccessCode.Cookie ?? string.Empty;
        }

        SucceedSave = CheckAccessInfosEqual();
    }

    private bool CheckAccessInfosEqual( )
    {
        if( AppConfigManager.AccessCode.Token != AccessToken )
        {
            return false;
        }
        else if( AppConfigManager.AccessCode.Cookie != AccessCookie )
        {
            return false;
        }
        else if( AppConfigManager.AccessCode.Url != VstsUrl )
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    [ObservableProperty]
    public string _vstsUrl = string.Empty;
    partial void OnVstsUrlChanged(string value)
    {
        SucceedSave = CheckAccessInfosEqual();
    }


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveAccessCodeCommand))]
    public string _accessToken = string.Empty;
    partial void OnAccessTokenChanged(string value)
    {
        SucceedSave = CheckAccessInfosEqual();
    }


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveAccessCodeCommand))]
    public string _accessCookie = string.Empty;
    partial void OnAccessCookieChanged(string value)
    {
        SucceedSave = CheckAccessInfosEqual();
    }


    [ObservableProperty]
    public bool _isAccessByToken = true;

    [ObservableProperty]
    public bool _canSave;

    [ObservableProperty]
    public bool _succeedSave;

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
        _logger.Information($"AccessCode Url:{VstsUrl}");
        _logger.Information($"AccessCode Token:{AccessToken}");
        _logger.Information($"AccessCode Cookie:{AccessCookie}");

        AppConfigManager.AccessCode = new()
        {
            Url = this.VstsUrl ,
            Token = this.AccessToken ,
            Cookie = this.AccessCookie
        };

        SucceedSave = CheckAccessInfosEqual();
    }

}
