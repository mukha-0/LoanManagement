using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanManagement.UI.Bots.User
{
    public enum UserStates
    {
        MainMenu,
        ApplyLoan_Amount,
        ApplyLoan_TermMonths,
        ApplyLoan_InterestRate,
        MakeRepayment_Amount,
        UpdateProfile_Username,
        UpdateProfile_Password,
        ApplyLoan_Term,
        UpdateProfile_FullName,
        UpdateProfile_Email,
        UpdateProfile_Phone,
        DeleteAccount_Confirm,
        MakePayment_LoanId
    }
}
