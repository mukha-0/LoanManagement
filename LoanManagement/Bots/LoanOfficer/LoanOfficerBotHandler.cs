// LoanManagement/Bots/LoanOfficerBot/LoanOfficerHandler.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using LoanManagement.DataAccess.Context;
using LoanManagement.Service.Services.AllEntries.Admin;
using LoanManagement.Service.Services.AllEntries.LoanOfficer;
using LoanManagement.Service.Services.Loans;
using LoanManagement.Service.Services.Repayments;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LoanManagement.Bots.LoanOfficerBot
{
    public class LoanOfficerHandler
    {
        private readonly TelegramBotClient _bot;
        private readonly LoanOfficerCommands _commands;

        public LoanOfficerHandler(TelegramBotClient bot,
            ILoanOfficerService officerService,
            ILoanService loanService,
            IAdminService adminService,
            IRepaymentService repaymentService)
        {
            _bot = bot;
        }

        // Updated HandleUpdateAsync method to include the missing AppDBContext parameter for ProcessCallback
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update is null) return;

            if (update.Message != null && update.Message.Type == MessageType.Text)
            {
                await _commands.ProcessMessage(update.Message);
            }
            else if (update.CallbackQuery != null)
            {
                // Assuming AppDBContext is available in the current scope or can be resolved
                AppDBContext dbContext = await ResolveAppDBContext(); // Replace with actual method to get AppDBContext
                await LoanOfficerCommands.ProcessCallback(_bot, update.CallbackQuery, dbContext);
            }
        }

        // Updated the return type of ResolveAppDBContext to Task<AppDBContext> to fix CS1983 error.
        private static async Task<AppDBContext> ResolveAppDBContext()
        {
            AppDBContext context = new AppDBContext();
            await context.Database.EnsureCreatedAsync();
            var contextt = new AppDBContext
            {
                Admins = context.Admins,
                LoanOfficers = context.LoanOfficers,
                Users = context.Users,
                Loans = context.Loans,
                Repayments = context.Repayments
            };
            return contextt;
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"LoanOfficer bot error: {exception}");
            return Task.CompletedTask;
        }
    }
}
