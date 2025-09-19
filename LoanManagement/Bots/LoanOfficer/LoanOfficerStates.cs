using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanManagement.UI.Bots.LoanOfficer
{
    public enum LoanOfficerStates
    {
        MainMenu,
        RejectLoan_Reason,
        UpdateProfile_Username,
        UpdateProfile_Password,
        ReviewLoanApplication,
        RejectLoanReason,
        UpdateProfile_FullName,
        UpdateProfile_Email,
        UpdateProfile_Phone
    }
}
