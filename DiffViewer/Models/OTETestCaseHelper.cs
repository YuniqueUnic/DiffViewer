using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiffViewer.Models;

public static class OTETestCaseHelper
{
    public static OTEDetailTestCase ConvertOTE2Detail(this OTETestCase oTE)
    {
        OTEDetailTestCase detail = new OTEDetailTestCase()
        {
            TestCaseId = oTE.TestCaseId ,
            Title = oTE.Title ,
            TestStep = oTE.TestStep ,
            StepAction = oTE.StepAction ,
            StepExpected = oTE.StepExpected ,
            TestPointId = oTE.TestPointId ,
            Configuration = oTE.Configuration ,
            AssignedTo = oTE.AssignedTo ,
            Outcome = oTE.Outcome ,
            Comment = oTE.Comment ,
            Defects = oTE.Defects ,
            RunBy = oTE.RunBy ,
            ScriptName = oTE.GetScriptName() ,
        };
        detail.SetIndex(oTE.GetIndex());
        return detail;
    }

    public static IList<OTEDetailTestCase> ConvertOTE2Detail(this IEnumerable<OTETestCase> oTEs)
    {
        IList<OTEDetailTestCase> details = new List<OTEDetailTestCase>();

        Parallel.ForEach(oTEs , oTE =>
        {
            details.Add(ConvertOTE2Detail(oTE));
        });

        return details;
    }
}
