using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanManagement.UI.Bots.Admin
{
    public enum AdminStates
    {
        MainMenu,
        CreateUser_FullName,
        CreateUser_Username,
        CreateUser_Password,
        CreateUser_Email,
        CreateUser_Phone,
        CreateUser_Address,
        CreateOfficer_FullName,
        CreateOfficer_Username,
        CreateOfficer_Password,
        CreateOfficer_Email,
        CreateOfficer_Phone,
        DeleteUser,
        DeleteOfficer,
        RejectLoan_Reason
    }
}
