// LoanManagement/Bots/AdminBot/AdminBotHandler.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using LoanManagement.DataAccess.Context;
using LoanManagement.Service.Services.AllEntries.Admin;
using LoanManagement.Service.Services.AllEntries.LoanOfficer;
using LoanManagement.Service.Services.AllEntries.Users;
using LoanManagement.Service.Services.Loans;
using LoanManagement.Service.Services.Repayments;
using LoanManagement.Service.Services.Reports;
using LoanManagement.UI.Bots.Admin;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LoanManagement.Bots.AdminBot
{
    public class AdminBotHandler
    {
        private readonly TelegramBotClient _botClient;
        private readonly AdminBotCommands _commands;
        private readonly AppDBContext appDB;
        private readonly TelegramBotClient telegramBotClient;

        public AdminBotHandler(TelegramBotClient botClient,
            IUserService userService,
            ILoanService loanService,
            IRepaymentService repaymentService,
            IReportService reportService,
            IAdminService adminService,
            ILoanOfficerService loanOfficerService)
        {
            _botClient = botClient;
            _commands = new AdminBotCommands(_botClient, userService, loanService, repaymentService, reportService, adminService, loanOfficerService);
        }

        // Func<ITelegramBotClient, Update, CancellationToken, Task>
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update is null) return;

            // Message text (button clicks as text for reply keyboard or callback for inline)
            if (update.Message != null && update.Message.Type == MessageType.Text)
            {
                try
                {
                    await _commands.ProcessMessage(update.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Admin process error: {ex}");
                    await _botClient.SendMessage(update.Message.Chat.Id, "Error processing command.");
                }
            }
            else if (update.CallbackQuery != null)
            {
                try
                {
                    // With this corrected line:  
                    await AdminBotCommands.ProcessCallback(telegramBotClient, update.CallbackQuery, appDB);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Admin callback error: {ex}");
                    await _botClient.SendMessage(update.CallbackQuery.Message.Chat.Id, "Error processing callback.");
                }
            }
        }

        // Func<ITelegramBotClient, Exception, CancellationToken, Task>
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Telegram error: {exception}");
            return Task.CompletedTask;
        }
    }
}
