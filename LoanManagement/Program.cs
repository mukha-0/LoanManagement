

using LoanManagement.Bots.AdminBot;
using LoanManagement.DataAccess.Context;
using LoanManagement.UI.Bots.Admin;
using LoanManagement.UI.Bots.LoanOfficer;
using LoanManagement.UI.Bots.User;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using LoanManagement.Bots.LoanOfficerBot;
using Telegram.Bot.Types;
using LoanManagement.Bots;

namespace LoanManagement;

internal class Program
{


    static async Task Main(string[] args)
    {
        BotRunner.RunBot(AdminToken, OfficerToken, UserToken);
    }
}
