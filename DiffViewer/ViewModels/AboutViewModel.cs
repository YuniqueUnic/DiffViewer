using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using DiffViewer.Models;
using DiffViewer.Managers.Helper;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;

namespace VSTSDataProvider.ViewModels;

public partial class AboutViewModel :ObservableObject
{
    private ILogger _logger;
    public AboutViewModel(ILogger logger)
    {
        _logger= logger;
    }


    [RelayCommand]
    public void LoadAboutContent()
    {

    }

    /// <summary>
    /// Close Window by using WeakReferenceMessenger.
    /// </summary>
    [RelayCommand]
    public void CloseWindow()
    {
        _logger.Debug("CloseWindowCommand called");
        WeakReferenceMessenger.Default.Send(new DiffViewer.Messages.WindowActionMessage() { Sender = this, Message = "Close" });
    }

}
