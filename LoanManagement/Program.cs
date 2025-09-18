

using LoanManagement.UI.Bots.Admin;
using LoanManagement.UI.Bots.LoanOfficer;
using LoanManagement.UI.Bots.User;

namespace LoanManagement;

internal class Program
{
    static async Task Main(string[] args)
    {

        var tokenadmin = "8120631864:AAG6AczJc4SRuwV_MGet8iE9nGp0gLZ3MTQ";
        var adminBot = new AdminBot(tokenadmin);
        adminBot.Start();


        var tokenuser = "8360128523:AAEp4QApXZYCNBmFb55MV_phCBZo0eHOblo";
        var userBot = new UserBot(tokenuser);
        userBot.Start();


        //var tokenofficer = "YOUR_USER_BOT_TOKEN";
        //var loanOfficerBot = new LoanOfficerBot(tokenofficer);
        //loanOfficerBot.Start();
    }
}
