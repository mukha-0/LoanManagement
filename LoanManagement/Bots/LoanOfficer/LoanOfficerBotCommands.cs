using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Service.Services.Loans;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using LoanManagement.Service.Services.AllEntries.Admin;

namespace LoanManagement.UI.Bots.LoanOfficer
{
    public static class LoanOfficerBotCommands
    {
        private static long _currentChatId;
        private static int _selectedLoanId;
        public static LoanService LoanService = new LoanService();

        public static async Task ProcessCommand(TelegramBotClient botClient, Message message)
        {
            _currentChatId = message.Chat.Id;
            string text = message.Text.ToLower();

            switch (text)
            {
                case "/start":
                    await ShowMainMenu(botClient);
                    break;
                case "view pending loans":
                    await ShowPendingLoans(botClient);
                    break;
                case "loan history":
                    await ShowLoanHistory(botClient);
                    break;
                case "account settings":
                    await ManageProfile(botClient);
                    break;
                case "accept loan":
                    await AcceptLoan(botClient, _selectedLoanId);
                    break;
                case "reject loan":
                    await RejectLoan(botClient, _selectedLoanId);
                    break;
                default:
                    await botClient.SendMessage(_currentChatId, "Unknown command. Type /start to see menu.");
                    break;
            }
        }

        #region Main Menu
        private static async Task ShowMainMenu(TelegramBotClient botClient)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "View Pending Loans", "Loan History" },
                new KeyboardButton[] { "Account Settings" }
            })
            { ResizeKeyboard = true };

            await botClient.SendMessage(_currentChatId, "Loan Officer Menu:", replyMarkup: keyboard);
        }
        #endregion

        #region Pending Loans
        private static async Task ShowPendingLoans(TelegramBotClient botClient)
        {
            var loans = LoanService.GetLoansWaitingForOfficer();
            if (loans.Count() == 0)
            {
                await botClient.SendMessage(_currentChatId, "No pending loans at the moment.");
                return;
            }

            StringBuilder sb = new StringBuilder("Pending Loans:\n");
            foreach (var loan in loans)
            {
                sb.AppendLine($"LoanID: {loan.GetType()
                    .GetProperty("LoanID")?
                    .GetValue(loan)}, UserID: {loan.GetType()
                    .GetProperty("UserID")?
                    .GetValue(loan)}, Amount: {loan.GetType()
                    .GetProperty("Amount")?
                    .GetValue(loan)}, Status: {loan
                    .GetType().GetProperty("Status")?
                    .GetValue(loan)}");
            }

            await botClient.SendMessage(_currentChatId, sb.ToString());
        }
        public static async Task AcceptLoan(TelegramBotClient botClient, int loanId)
        {
            await LoanService.ApproveLoanAsync(loanId);
            await botClient.SendMessage(_currentChatId, $"Loan {loanId} accepted.");
        }
        public static async Task RejectLoan(TelegramBotClient botClient, int loanId)
        {
            await LoanService.RejectLoanAsync(loanId);
            await botClient.SendMessage (_currentChatId, $"Loan {loanId} rejected.");
        }
        #endregion

        #region Loan History
        private static async Task ShowLoanHistory(TelegramBotClient botClient)
        {
            var loans = await LoanService.GetAllLoansAsync();
            if (loans.Count == 0)
            {
                await botClient.SendMessage(_currentChatId, "No loan history yet.");
                return;
            }

            StringBuilder sb = new StringBuilder("Loan History:\n");
            foreach (var loan in loans)
            {
                sb.AppendLine($"LoanID: {loan.Id}, UserID: {loan.CustomerId}, Amount: {loan.Amount}, Status: {loan.Status}");
            }

            await botClient.SendMessage(_currentChatId, sb.ToString());
        }
        #endregion

        #region Profile
        private static async Task ManageProfile(TelegramBotClient botClient)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "Update Profile", "Delete Account" },
                new KeyboardButton[] { "Back" }
            })
            { ResizeKeyboard = true };

            await botClient.SendMessage(_currentChatId, "Account Settings:", replyMarkup: keyboard);
        }
        #endregion
    }
}
