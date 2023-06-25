//using DiffViewer.Managers.Helper;
//using System;
//using VSTSDataProvider.Common.Helpers;
//using VSTSDataProvider.Models;

//namespace DiffViewer.Managers;


//public class VSTSDataManager
//{
//    public TestSuite TestSuite { get; set; }
//    public TestPlan TestPlan { get; set; }

//    public VSTSDataProvider.ViewModels.MainWindowViewModel VSTSDataProviderInstance { get; set; } = new();

//    public void getData( )
//    {
//        if( AppConfigManager.AccessCode is null ) { throw new NullReferenceException("AccessCode is null"); }

//        if( AppConfigManager.AccessCode.Url is null ) { throw new NullReferenceException("AccessCode.Url is null"); }

//        TestPlanSuiteId testPlanSuiteId = VSTSDataProvider.Common.VSTSDataProcessing.TryGetTestPlanSuiteId(AppConfigManager.AccessCode.Url , out bool succeedGET);

//        if( !succeedGET ) { throw new ArgumentException("Can't get TestPlanSuiteId, AccessCode.Url is not right."); }

//        VSTSDataProviderInstance.TestPlanID = testPlanSuiteId.PlanId.ToString();
//        VSTSDataProviderInstance.TestSuiteID = testPlanSuiteId.SuiteId.ToString();

//        if( AppConfigManager.AccessCode.Cookie.IsNullOrWhiteSpaceOrEmpty() && AppConfigManager.AccessCode.Token.IsNullOrWhiteSpaceOrEmpty() )
//        {
//            throw new ArgumentException("AccessCode.Token and AccessCode.Cookie are both null or empty.");
//        }

//        if( AppConfigManager.AccessCode.Cookie.IsNullOrWhiteSpaceOrEmpty() )
//        {
//            VSTSDataProviderInstance.AccessToken = AppConfigManager.AccessCode.Token;
//            VSTSDataProviderInstance.IsAccessByToken = true;
//        }
//        else if( AppConfigManager.AccessCode.Token.IsNullOrWhiteSpaceOrEmpty() )
//        {
//            VSTSDataProviderInstance.Cookie = AppConfigManager.AccessCode.Cookie;
//            VSTSDataProviderInstance.IsAccessByToken = false;
//        }

//        VSTSDataProviderInstance.
//    }

//    //private VSTSDataProvider.ViewModels.MainWindowViewModel GetVSTSDataProvider( )
//    //{
//    //    VSTSDataProvider.Common.VSTSDataProcessing mVSTSDataProvider;
//    //    TestPlanSuiteId m_IDGroup;
//    //    bool m_succeedMatch = false;

//    //    if( IsCompleteUrlUpdated )
//    //    {
//    //        m_IDGroup = VSTSDataProvider.Common.VSTSDataProcessing.TryGetTestPlanSuiteId(CompleteUrl , out m_succeedMatch);
//    //        if( m_succeedMatch )
//    //        {
//    //            TestPlanID = m_IDGroup.PlanId.ToString();
//    //            TestSuiteID = m_IDGroup.SuiteId.ToString();
//    //        }

//    //        mVSTSDataProvider = new VSTSDataProvider.Common.VSTSDataProcessing().SetTestPlanSuiteID(m_IDGroup.PlanId , m_IDGroup.SuiteId);
//    //        mVSTSDataProvider = IsAccessByToken ? mVSTSDataProvider.SetToken(AccessToken) : mVSTSDataProvider.SetCookie(Cookie);
//    //    }
//    //    else
//    //    {
//    //        if( isValidID(out m_IDGroup) )
//    //        {
//    //            mVSTSDataProvider = new VSTSDataProvider.Common.VSTSDataProcessing().SetTestPlanSuiteID(m_IDGroup.PlanId , m_IDGroup.SuiteId);
//    //            mVSTSDataProvider = IsAccessByToken ? mVSTSDataProvider.SetToken(AccessToken) : mVSTSDataProvider.SetCookie(Cookie);
//    //        }
//    //        else
//    //        {
//    //            m_IDGroup = VSTSDataProvider.Common.VSTSDataProcessing.TryGetTestPlanSuiteId(CompleteUrl , out m_succeedMatch);
//    //            if( m_succeedMatch )
//    //            {
//    //                TestPlanID = m_IDGroup.PlanId.ToString();
//    //                TestSuiteID = m_IDGroup.SuiteId.ToString();
//    //            }

//    //            mVSTSDataProvider = new VSTSDataProvider.Common.VSTSDataProcessing().SetTestPlanSuiteID(m_IDGroup.PlanId , m_IDGroup.SuiteId);
//    //            mVSTSDataProvider = IsAccessByToken ? mVSTSDataProvider.SetToken(AccessToken) : mVSTSDataProvider.SetCookie(Cookie);
//    //        }
//    //    }

//    //    IsCompleteUrlUpdated = false;
//    //    return mVSTSDataProvider;
//    //}
//}
