using DiffViewer.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using VSTSDataProvider.Common.Helpers;
using VSTSDataProvider.Models;

namespace DiffViewer.Managers;


public class VSTSDataManager
{
    public static TestSuite TestSuite { get; set; }
    public static TestPlan TestPlan { get; set; }

    public static VSTSDataProvider.ViewModels.MainWindowViewModel VSTSDataProviderInstance { get; private set; } = new();
    public static VSTSDataProvider.Common.VSTSDataProcessing? VSTSDataProcessingInstance { get; private set; }
    public static ExecuteVSTSModel.RootObject? ExecuteRootObject { get; private set; }
    public static QueryVSTSModel.RootObject? QueryRootObject { get; private set; }


    public static bool CheckModels(ExecuteVSTSModel.RootObject exeModel , QueryVSTSModel.RootObject querModel)
    {
        if( exeModel == null || querModel == null ) { return false; }

        if( exeModel.count != querModel.count ) { return false; }

        if( exeModel.value[0].testPlan.id != querModel.value[0].testPlan.id ) return false;

        return true;
    }

    public static async Task<ConcurrentBag<OTETestCase>> GetOTETestCasesAsync( )
    {
        if( AppConfigManager.AccessCode is null ) { throw new NullReferenceException("AccessCode is null"); }

        if( AppConfigManager.AccessCode.Url is null ) { throw new NullReferenceException("AccessCode.Url is null"); }

        TestPlanSuiteId testPlanSuiteId = VSTSDataProvider.Common.VSTSDataProcessing.TryGetTestPlanSuiteId(AppConfigManager.AccessCode.Url , out bool succeedGET);

        if( !succeedGET ) { throw new ArgumentException("Can't get TestPlanSuiteId, AccessCode.Url is not right."); }

        VSTSDataProviderInstance.TestPlanID = testPlanSuiteId.PlanId.ToString();
        VSTSDataProviderInstance.TestSuiteID = testPlanSuiteId.SuiteId.ToString();

        if( AppConfigManager.AccessCode.Cookie.IsNullOrWhiteSpaceOrEmpty()
            && AppConfigManager.AccessCode.Token.IsNullOrWhiteSpaceOrEmpty() )
        {
            throw new ArgumentException("AccessCode.Token and AccessCode.Cookie are both null or empty.");
        }


        if( AppConfigManager.AccessCode.Cookie.IsNullOrWhiteSpaceOrEmpty() )
        {
            VSTSDataProviderInstance.AccessToken = AppConfigManager.AccessCode.Token;
            VSTSDataProviderInstance.IsAccessByToken = true;
        }
        else if( AppConfigManager.AccessCode.Token.IsNullOrWhiteSpaceOrEmpty() )
        {
            VSTSDataProviderInstance.Cookie = AppConfigManager.AccessCode.Cookie;
            VSTSDataProviderInstance.IsAccessByToken = false;
        }

        // Get VSTSDataProcessingInstance to get and process the VSTS data
        VSTSDataProcessingInstance = VSTSDataProviderInstance.GetVSTSDataProvider();

        VSTSDataProcessingInstance.UsingTokenToGET = VSTSDataProviderInstance.IsAccessByToken;

        App.Logger.Information("PreLoadData Started");

        var suceedPreLoadData = await VSTSDataProcessingInstance.PreLoadData();

        if( !suceedPreLoadData )
        {
            App.Logger.Information("Can't PreLoadData, AccessCode is not right.");
            throw new ArgumentException("Can't PreLoadData, AccessCode is not right.");
        }

        if( suceedPreLoadData )
        {
            App.Logger.Information("Pre-Load Data Over, Start Loading VSTS Data...");

            var OTETestCaseCollection = await MergeModelstoOTETestCaseByAsync(VSTSDataProcessingInstance.ExeRootObject , VSTSDataProcessingInstance.QueryRootObject);

            App.Logger.Information("Loading VSTS Data Over.");

            return OTETestCaseCollection;
        }


        return null;
    }

    public async static Task<ConcurrentBag<OTETestCase>> MergeModelstoOTETestCaseByAsync(ExecuteVSTSModel.RootObject exeModel , QueryVSTSModel.RootObject querModel)
    {
        App.Logger.Information("MergeModelstoOTETestCaseByAsync Started");
        if( !CheckModels(exeModel , querModel) ) return null;

        int index = exeModel.count + 1;

        var DetailModels = new ConcurrentBag<OTETestCase>();

        await Task.Run(( ) =>
        {
            DetailModels = new ConcurrentBag<OTETestCase>(exeModel.value.AsParallel().Select(v =>
            {
                int currentIndex = System.Threading.Interlocked.Decrement(ref index);
                //string a = v.workItem.fields.FirstOrDefault(field => field.description != null)?.description;
                return new OTETestCase()
                {
                    Index = currentIndex ,
                    TestCaseId = v.workItem.id ,
                    Title = v.workItem.name ,
                    TestPointId = (int)v.pointAssignments.FirstOrDefault(point => point.id >= default(int))?.id ,
                    Configuration = v.pointAssignments.FirstOrDefault(point => point.configurationName != null)?.configurationName ,
                    AssignedTo = v.pointAssignments.FirstOrDefault(point => point.tester != null)?.tester.displayName ,
                    Outcome = querModel.value.FirstOrDefault(tempQueryModel => tempQueryModel.testCaseReference.id == v.workItem.id)?.results.outcome ,
                    RunBy = v.pointAssignments.FirstOrDefault(point => point.tester != null)?.tester.uniqueName ,
                    ScriptName = v.workItem.fields.FirstOrDefault(field => field.scriptName != null)?.scriptName ,
                };
            }));
        });

        App.Logger.Information($"MergeModelstoOTETestCaseByAsync Done. Total: {exeModel.count + 1}");
        return DetailModels;
    }
}
