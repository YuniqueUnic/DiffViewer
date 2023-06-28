using DiffViewer.Managers.Helper;
using DiffViewer.Models;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace DiffViewer.Managers;

internal class AboutManager
{

    public AboutManager( ) { GetSoftwareAssemblyInfos(); }

    #region FlowDocumentContent

    private const string MIT = "MIT";
    private const string Apache2 = "Apache-2.0";

    private string _softwareName = nameof(_softwareName);
    private string _softwareVersion = nameof(_softwareVersion);
    private string _copyrightOwner = nameof(_copyrightOwner);
    private string _releaseDate = nameof(_releaseDate);
    private string _company = nameof(_company);


    private List<OpenSourceProject> _openSourceLibraries = new List<OpenSourceProject>
    {
        new OpenSourceProject { Name = "CommunityToolkit.Mvvm", Version = "8.2.0", License = MIT, Url = "https://github.com/CommunityToolkit/dotnet" },
        new OpenSourceProject { Name = "Microsoft.Extensions.DependencyInjection", Version = "7.0.0", License = MIT, Url = "https://github.com/Microsoft/XamlBehaviorsWpf" },
        new OpenSourceProject { Name = "Microsoft.Xaml.Behaviors.Wpf", Version = "1.1.39", License = MIT, Url = "https://github.com/Microsoft/XamlBehaviorsWpf" },
        new OpenSourceProject { Name = "MvvmDialogs", Version = "9.1.0", License = Apache2, Url = "https://github.com/FantasticFiasco/mvvm-dialogs" },
        new OpenSourceProject { Name = "Serilog", Version = "3.0.1", License = Apache2, Url = "https://serilog.net/" },
        new OpenSourceProject { Name = "Serilog.Sinks.Console", Version = "4.1.0", License = Apache2, Url = "https://github.com/serilog/serilog-sinks-console" },
        new OpenSourceProject { Name = "Serilog.Sinks.File", Version = "5.0.0", License = Apache2, Url = "https://serilog.net/" }
    };

    private List<OpenSourceProject> _thirdPartyComponents = new List<OpenSourceProject>
    {
        new OpenSourceProject { Name = "IconPark", Version = "1.3.0", License = Apache2, Url = "https://iconpark.bytedance.com/official" },
        //new OpenSourceProject { Name = "ICON", Version = "", License = "", Url = "https://icons8.com/" },
        //new OpenSourceProject { Name = "第三方组件名称2", Version = "第三方组件版本号2", License = "第三方组件许可证2", Url = "http://Bing.com" },
        //new OpenSourceProject { Name = "第三方组件名称3", Version = "第三方组件版本号3", License = "第三方组件许可证3", Url = "http://Google.com" }
    };

    private List<OpenSourceProject> _specialThanks = new List<OpenSourceProject>
    {
            new OpenSourceProject { Name = "HYSYS", Version = "", License = "", Url = "https://www.aspentech.com/en/products/engineering/aspen-hysys/?src=web-apaccn" },
            new OpenSourceProject { Name = "AspenTech", Version = "", License = "", Url = "https://www.aspentech.cn/" },
    };

    public string SoftwareName
    {
        get => _softwareName;
        set => _softwareName = value;
    }

    public string SoftwareVersion
    {
        get => _softwareVersion;
        set => _softwareVersion = value;
    }

    public string CopyrightOwner
    {
        get => _copyrightOwner;
        set => _copyrightOwner = value;
    }

    public string ReleaseDate
    {
        get => _releaseDate;
        set => _releaseDate = value;
    }

    public List<OpenSourceProject> OpenSourceLibraries
    {
        get => _openSourceLibraries;
        set => _openSourceLibraries = value;
    }

    public List<OpenSourceProject> ThirdPartyComponents
    {
        get => _thirdPartyComponents;
        set => _thirdPartyComponents = value;
    }

    public List<OpenSourceProject> SpecialThanks
    {
        get => _specialThanks;
        set => _specialThanks = value;
    }

    public FlowDocument GenerateLicenseInfoDocument( )
    {
        FlowDocument flowDocument = new FlowDocument();
        AddParagraph(flowDocument , new Bold(new Run($"{SoftwareName} v{SoftwareVersion}")));
        AddParagraph(flowDocument , new Run($"{GetStringFromAppResources("CopyrightOwner")} {CopyrightOwner} {ReleaseDate}"));
        AddParagraph(flowDocument , new Run($"{GetStringFromAppResources("ThirdLibraries")}"));
        AddList(flowDocument , OpenSourceLibraries);
        AddParagraph(flowDocument , new Run($"{GetStringFromAppResources("ThirdComponents")}"));
        AddList(flowDocument , ThirdPartyComponents);
        AddParagraph(flowDocument , new Run($"{GetStringFromAppResources("SpecialThanks")}"));
        AddList(flowDocument , SpecialThanks);
        AddExtraInfosList(flowDocument , null);

        return flowDocument;
    }

    private void AddParagraph(FlowDocument flowDocument , Inline inline)
    {
        Paragraph paragraph = new Paragraph();
        paragraph.Inlines.Add(inline);
        flowDocument.Blocks.Add(paragraph);
    }

    private void AddList(FlowDocument flowDocument , List<OpenSourceProject> items)
    {
        List list = new List();
        foreach( OpenSourceProject item in items )
        {
            ListItem listItem = new ListItem();

            if( !string.IsNullOrEmpty(item.Url) )
            {
                Hyperlink hyperlink = new Hyperlink(new Run($"{item.Name} " + (item.Version.IsNullOrWhiteSpaceOrEmpty() ? "" : $"v{item.Version}")));
                hyperlink.NavigateUri = new Uri(item.Url); // set the hyperlink target
                hyperlink.RequestNavigate += Hyperlink_RequestNavigate; // add event handler
                listItem.Blocks.Add(new Paragraph(hyperlink));
            }
            else
            {
                listItem.Blocks.Add(new Paragraph(new Run($"{item.Name} " + (item.Version.IsNullOrWhiteSpaceOrEmpty() ? "" : $"v{item.Version}"))));
            }

            if( !string.IsNullOrEmpty(item.License) )
            {
                listItem.Blocks.Add(new Paragraph(new Italic(new Run($"{GetStringFromAppResources("License")} {item.License}"))));
            }

            list.ListItems.Add(listItem);
        }
        flowDocument.Blocks.Add(list);
    }

    private void Hyperlink_RequestNavigate(object sender , RequestNavigateEventArgs e)
    {
        try
        {
            // open the link in the default browser
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri ,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
        catch( Exception ex )
        {
            App.Logger.Error(ex , "Hyperlink_RequestNavigate");
            // handle the exception
        }

        e.Handled = true;
    }

    //TODO: Add update/usage/etc infos
    private void AddExtraInfosList(FlowDocument flowDocument , List<OpenSourceProject> items)
    {
        AddParagraph(flowDocument , new Run($"{GetStringFromAppResources("Slogan")}"));
    }

    #endregion FlowDocumentContent

    public void GetSoftwareAssemblyInfos( )
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        AssemblyName assemblyName = assembly.GetName();

        string name = assemblyName.Name ?? "";
        string version = (assemblyName.Version ?? new Version("0.0.0.0")).ToString();
        string copyright = string.Empty;
        string releaseDate = string.Empty;
        string company = string.Empty;

        object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyTrademarkAttribute) , false);
        if( attributes.Length > 0 )
        {
            AssemblyTrademarkAttribute trademarkAttribute = (AssemblyTrademarkAttribute)attributes[0];
            copyright = trademarkAttribute.Trademark;
        }

        attributes = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute) , false);
        if( attributes.Length > 0 )
        {
            AssemblyInformationalVersionAttribute informationalVersionAttribute = (AssemblyInformationalVersionAttribute)attributes[0];
            releaseDate = informationalVersionAttribute.InformationalVersion;
            //releaseDate = informationalVersionAttribute.InformationalVersion.Split('+')[0];

        }

        attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute) , false);
        if( attributes.Length > 0 )
        {
            AssemblyCompanyAttribute companyAttribute = (AssemblyCompanyAttribute)attributes[0];
            company = companyAttribute.Company;
        }

        _softwareName = name;
        _softwareVersion = version;
        _copyrightOwner = copyright;
        _releaseDate = releaseDate;
        _company = company;
    }

    public async Task<List<OpenSourceProject>> GetNuGetProjectsAsync( )
    {
        List<OpenSourceProject> nuGetProjects = new();
        Dictionary<string , string> nugetPackages = GetNuGetPackages();
        using var client = new HttpClient();
        foreach( var item in nugetPackages )
        {
            string packageName = item.Key;
            string version = item.Value;
            var url = $"https://api.nuget.org/v3/registration5-semver1/{packageName.ToLowerInvariant()}/{version}.json";
            var response = await client.GetAsync(url);
            if( response.IsSuccessStatusCode )
            {
                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                var catalogEntry = document.RootElement.GetProperty("catalogEntry");
                var projectUrl = catalogEntry.GetProperty("projectUrl").GetString();
                var projectLicense = catalogEntry.GetProperty("licenseUrl").GetString();
                nuGetProjects.Add(new OpenSourceProject { Name = packageName , Version = version , Url = projectUrl , License = projectLicense });
            }
            else
            {
                App.Logger.Error($"{packageName} {version} can't found.");
            }
        }

        return nuGetProjects;

    }

    public Dictionary<string , string> GetNuGetPackages( )
    {
        var context = DependencyContext.Load(Assembly.GetEntryAssembly());
        var packages = context.RuntimeLibraries.Where(library => library.Type == "package");

        Dictionary<string , string> nugetPackages = new Dictionary<string , string>();
        foreach( var package in packages )
        {
            nugetPackages.Add(package.Name , package.Version);
            App.Logger.Information($"{package.Name} {package.Version} was in Assembly.");
        }
        return nugetPackages;
    }

    public string GetStringFromAppResources(string resourcekey)
    {
        string location = $"({nameof(AboutManager)}).({nameof(GetStringFromAppResources)})";
        try
        {
            string value = App.Current.Resources.MergedDictionaries[0][resourcekey].ToString() ?? "-".Repeat(6);
            location += $". Value found: {value}.";
            App.Logger.Information(location);
            return value;
        }
        catch( Exception e )
        {
            location += " cause problem.";
            App.Logger.Error(location , e);
            throw;
        }
    }

}
