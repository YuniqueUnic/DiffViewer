using DiffViewer.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using VSTSDataProvider.Models;

namespace DiffViewer.Managers;


public class VSTSDataManager
{
    public static TestSuite TestSuite { get; private set; }
    public static TestPlan TestPlan { get; private set; }

    public static VSTSDataProvider.ViewModels.MainWindowViewModel VSTSDataProviderInstance { get; private set; } = new();
    public static VSTSDataProvider.Common.VSTSDataProcessing? VSTSDataProcessingInstance { get; private set; }
    public static ExecuteVSTSModel.RootObject? ExecuteRootObject { get; private set; }
    public static QueryVSTSModel.RootObject? QueryRootObject { get; private set; }
    public static bool IsSucceedPreLoadData { get; private set; } = false;

    public static bool CheckModels(ExecuteVSTSModel.RootObject exeModel , QueryVSTSModel.RootObject querModel)
    {
        if( exeModel == null || querModel == null ) { return false; }

        if( exeModel.count != querModel.count ) { return false; }

        if( exeModel.value[0].testPlan.id != querModel.value[0].testPlan.id ) return false;

        TestPlan = new TestPlan() { ID = exeModel.value[0].testPlan.id , Name = exeModel.value[0].testPlan.name };
        TestSuite = new TestSuite() { ID = querModel.value[0].testSuite.id , Name = querModel.value[0].testSuite.name };

        TestPlan.ChildTestSuite = TestSuite;
        TestSuite.ParentTestPlan = TestPlan;

        return true;
    }

    public static bool IsValidUrl(string url)
    {
        bool isTestPlanSuiteIdFound = false;
        try
        {
            VSTSDataProvider.Common.VSTSDataProcessing.TryGetTestPlanSuiteId(url , out isTestPlanSuiteIdFound);
        }
        catch( Exception ex )
        {
            isTestPlanSuiteIdFound = false;
            App.Logger.Error(ex , $"Error on checking url: {url}");
        }

        return isTestPlanSuiteIdFound;
    }

    public static string TurnOutcomeToIdentialFormat(string outcome)
    {
        return outcome.ToLowerInvariant() switch
        {
            "passed" => "Passed",
            "failed" => "Failed",
            //"unspecified" => string.Empty,
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Get TestPlanSuiteId from VSTS_Uri user specified
    /// </summary>
    /// <param name="accessInfo"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<TestPlanSuiteId> GetTestPlanSuiteIdAsync(VSTSAccessInfo accessInfo)
    {
        var location = $" ({nameof(VSTSDataManager)}).({nameof(GetTestPlanSuiteIdAsync)})";

        if( accessInfo is null )
        {
            throw new ArgumentNullException(nameof(accessInfo));
        }

        if( accessInfo.Url is null )
        {
            throw new ArgumentNullException(nameof(accessInfo.Url));
        }
        bool isTestPlanSuiteIdFound = false;

        TestPlanSuiteId testPlanSuiteId = await TasksManager.RunTaskWithReturnAsync<TestPlanSuiteId>(( ) =>
        {
            return VSTSDataProvider.Common.VSTSDataProcessing.TryGetTestPlanSuiteId(accessInfo.Url , out isTestPlanSuiteIdFound);
        } , location , catchException: true);

        if( !isTestPlanSuiteIdFound )
        {
            throw new ArgumentException("Can't get TestPlanSuiteId, AccessCode.Url is not right.");
        }

        return testPlanSuiteId;
    }



    /// <summary>
    /// Before Load Data from VSTS, must PreLoadData from VSTS by using PredLoadVSTSDataAsync Method
    /// </summary>
    /// <param name="accessInfo"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<(bool, VSTSDataProvider.Common.VSTSDataProcessing)> PredLoadVSTSDataAsync(VSTSAccessInfo accessInfo)
    {
        TestPlanSuiteId testPlanSuiteId = await GetTestPlanSuiteIdAsync(accessInfo);

        VSTSDataProviderInstance.TestPlanID = testPlanSuiteId.PlanId.ToString();
        VSTSDataProviderInstance.TestSuiteID = testPlanSuiteId.SuiteId.ToString();

        if( string.IsNullOrWhiteSpace(accessInfo.Token) && string.IsNullOrWhiteSpace(accessInfo.Cookie) )
        {
            App.Logger.Error("AccessCode.Token and AccessCode.Cookie are both null or empty.");
            throw new ArgumentException("AccessCode.Token and AccessCode.Cookie are both null or empty.");
        }


        if( !string.IsNullOrWhiteSpace(accessInfo.Token) )
        {
            VSTSDataProviderInstance.AccessToken = accessInfo.Token;
            VSTSDataProviderInstance.IsAccessByToken = true;
        }
        else if( !string.IsNullOrWhiteSpace(accessInfo.Cookie) )
        {
            VSTSDataProviderInstance.Cookie = accessInfo.Cookie;
            VSTSDataProviderInstance.IsAccessByToken = false;
        }


        // Get VSTSDataProcessingInstance to get and process the VSTS data
        VSTSDataProcessingInstance = VSTSDataProviderInstance.GetVSTSDataProvider();

        VSTSDataProcessingInstance.UsingTokenToGET = VSTSDataProviderInstance.IsAccessByToken;

        App.Logger.Information("PreLoadData Started");

        bool isPreLoadDataSucceeded = await VSTSDataProcessingInstance.PreLoadData();

        IsSucceedPreLoadData = isPreLoadDataSucceeded;

        return (isPreLoadDataSucceeded, VSTSDataProcessingInstance);
    }

    /// <summary>
    /// Merge ExecuteModel and QueryModel into OTETestCase
    /// </summary>
    /// <param name="exeModel"></param>
    /// <param name="querModel"></param>
    /// <returns></returns>
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

                var OTE_TestCast = new OTETestCase()
                {
                    TestCaseId = v.workItem.id ,
                    Title = v.workItem.name ,
                    TestPointId = (int)v.pointAssignments.FirstOrDefault(point => point.id >= default(int))?.id ,
                    Configuration = v.pointAssignments.FirstOrDefault(point => point.configurationName != null)?.configurationName ,
                    AssignedTo = v.pointAssignments.FirstOrDefault(point => point.tester != null)?.tester.displayName ,
                    Outcome = TurnOutcomeToIdentialFormat(querModel.value.FirstOrDefault(tempQueryModel => tempQueryModel.testCaseReference.id == v.workItem.id)?.results.outcome ?? string.Empty) ,
                    RunBy = v.pointAssignments.FirstOrDefault(point => point.tester != null)?.tester.uniqueName ,
                };
                OTE_TestCast.SetIndex(currentIndex);
                OTE_TestCast.SetScriptName(v.workItem.fields.FirstOrDefault(field => field.scriptName != null)?.scriptName ?? string.Empty);

                return OTE_TestCast;
            }));
        });

        App.Logger.Information($"MergeModelstoOTETestCaseByAsync Done. Total: {exeModel.count + 1}");
        return DetailModels;
    }

    /// <summary>
    /// Load Data from VSTS and Cast it into OTE_TestCases
    /// Recommand to use Token to access VSTS ☆★☆★☆.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<ConcurrentBag<OTETestCase>> GET_OTETestCasesAsync(VSTSAccessInfo accessInfo , Action afterPreLoadDataAction = null)
    {
        var preLoadDataResults = await PredLoadVSTSDataAsync(accessInfo);

        if( !preLoadDataResults.Item1 )
        {
            App.Logger.Warning("Can't PreLoadData, AccessCode is not right.");

            throw new ArgumentException("Can't PreLoadData, AccessCode is not right.");
        }
        else if( preLoadDataResults.Item2 is not null )
        {
            if( afterPreLoadDataAction is not null ) { afterPreLoadDataAction(); }

            App.Logger.Information("Pre-Load Data Over, Start to Merge VSTS Data Models to OTETestCase...");

            ConcurrentBag<OTETestCase> oteTestCases = await MergeModelstoOTETestCaseByAsync(preLoadDataResults.Item2.ExeRootObject , preLoadDataResults.Item2.QueryRootObject);

            App.Logger.Information("Merge VSTS Data Models to OTETestCase Over.");

            return oteTestCases;
        }
        else
        {
            App.Logger.Error("Pre-Load Data Failed.");
            return null;
        }
    }



    /// <summary>
    /// Load Data from VSTS and Cast it into OTE_TestCases
    /// Recommand to use Token to access VSTS ☆★☆★☆.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<ConcurrentBag<OTEDetailTestCase>> GET_OTEDetailTestCasesAsync(VSTSAccessInfo accessInfo , Action afterPreLoadDataAction = null)
    {

        Func<ExecuteVSTSModel.RootObject , QueryVSTSModel.RootObject , ConcurrentBag<OTEDetailTestCase>> customRuleForOTEDetail = (exeModel , querModel) =>
        {
            int index = exeModel.count + 1;

            return new ConcurrentBag<OTEDetailTestCase>(exeModel.value.AsParallel().Select(v =>
            {
                int currentIndex = System.Threading.Interlocked.Decrement(ref index);
                var OTEDetail = new OTEDetailTestCase()
                {
                    TestCaseId = v.workItem.id ,
                    Title = v.workItem.name ,
                    TestPointId = (int)v.pointAssignments.FirstOrDefault(point => point.id >= default(int))?.id ,
                    Configuration = v.pointAssignments.FirstOrDefault(point => point.configurationName != null)?.configurationName ,
                    AssignedTo = v.pointAssignments.FirstOrDefault(point => point.tester != null)?.tester.displayName ,
                    Outcome = TurnOutcomeToIdentialFormat(querModel.value.FirstOrDefault(tempQueryModel => tempQueryModel.testCaseReference.id == v.workItem.id)?.results.outcome ?? string.Empty) ,
                    RunBy = v.pointAssignments.FirstOrDefault(point => point.tester != null)?.tester.uniqueName ,
                    ScriptName = v.workItem.fields.FirstOrDefault(field => field.scriptName != null)?.scriptName ?? string.Empty ,
                };
                OTEDetail.SetIndex(currentIndex);
                return OTEDetail;
            }));
        };

        return await GET_CustomModelAsync<OTEDetailTestCase>(accessInfo , customRuleForOTEDetail , afterPreLoadDataAction);
    }




    /// <summary>
    /// Load Data from VSTS and Cast it into Custom Models
    /// Recommand to use Token to access VSTS ☆★☆★☆.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<ConcurrentBag<T>> GET_CustomModelAsync<T>(VSTSAccessInfo accessInfo , Func<ExecuteVSTSModel.RootObject , QueryVSTSModel.RootObject , ConcurrentBag<T>> customRule , Action afterPreLoadDataAction = null)
    {
        var preLoadDataResults = await PredLoadVSTSDataAsync(accessInfo);

        if( !preLoadDataResults.Item1 )
        {
            App.Logger.Warning("Can't PreLoadData, AccessCode is not right.");

            throw new ArgumentException("Can't PreLoadData, AccessCode is not right.");
        }
        else if( preLoadDataResults.Item2 is not null )
        {
            if( afterPreLoadDataAction is not null ) { afterPreLoadDataAction(); }

            App.Logger.Information("Pre-Load Data Over, Start to Merge VSTS Data Models to CustomModels...");

            var customModels = await MergeByCustomRuleAsync(preLoadDataResults.Item2.ExeRootObject , preLoadDataResults.Item2.QueryRootObject , customRule);

            App.Logger.Information("Merge VSTS Data Models to CustomModels Over.");

            return customModels;
        }
        else
        {
            App.Logger.Error("Pre-Load Data Failed.");
            return null;
        }
    }


    /// <summary>
    /// Merge ExecuteVSTSModel and QueryVSTSModel by using custom rule
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="exeModel"></param>
    /// <param name="querModel"></param>
    /// <param name="customRule"></param>
    /// <returns></returns>
    public async static Task<ConcurrentBag<T>> MergeByCustomRuleAsync<T>(ExecuteVSTSModel.RootObject exeModel , QueryVSTSModel.RootObject querModel , Func<ExecuteVSTSModel.RootObject , QueryVSTSModel.RootObject , ConcurrentBag<T>> customRule)
    {
        App.Logger.Information("MergeByCustomRuleAsync Started");

        if( !CheckModels(exeModel , querModel) ) return null;

        var resultModels = new ConcurrentBag<T>();

        await Task.Run(( ) => { resultModels = customRule(exeModel , querModel); });

        App.Logger.Information($"MergeByCustomRuleAsync Done.");
        return resultModels;
    }
}
