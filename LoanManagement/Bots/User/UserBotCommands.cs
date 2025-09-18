using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Service.Services.Loans;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using LoanManagement.Service.Services.Loans.Models;
using LoanManagement.Service.Services.AllEntries.Users;
using LoanManagement.Service.Services.AllEntries.LoanOfficer;
using LoanManagement.Service.Services.Repayments;
using LoanManagement.Service.Services.Repayments.Models;

namespace LoanManagement.UI.Bots.User
{
    public static class UserBotCommands
    {
        private static long _currentChatId;
        public static LoanService loanService = new LoanService();
        public static LoanOfficerService officerService = new LoanOfficerService();
        public static RepaymentService repaymentService = new RepaymentService();

        public static async Task ProcessCommand(TelegramBotClient botClient, Message message)
        {
            _currentChatId = message.Chat.Id;
            string text = message.Text.ToLower();

            switch (text)
            {
                case "/start":
                    await ShowMainMenu(botClient);
                    break;
                case "apply loan":
                    await ApplyForLoan(botClient);
                    break;
                case "loan history":
                    await ShowLoanHistory(botClient);
                    break;
                case "active loans":
                    await ShowActiveLoans(botClient);
                    break;
                case "account settings":
                    await ManageProfile(botClient);
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
                new KeyboardButton[] { "Apply Loan", "Loan History" },
                new KeyboardButton[] { "Active Loans", "Account Settings" }
            })
            { ResizeKeyboard = true };

            await botClient.SendMessage(_currentChatId, "User Menu:", replyMarkup: keyboard);
        }
        #endregion

        #region Apply Loan
        private static async Task ApplyForLoan(TelegramBotClient botClient)
        {

            decimal amount = 1000;
            var model = new LoanCreateModel
            {
                Amount = amount,
                CustomerId = (int)_currentChatId
            };

            var loan = await loanService.ApplyForLoanAsync(model);
            await botClient.SendMessage(_currentChatId, $"Loan request submitted. LoanID: {loan.LoanId}\nWaiting for approval by Loan Officer.");
        }
        #endregion

        #region Loan History
        private static async Task ShowLoanHistory(TelegramBotClient botClient)
        {
            var loans = await loanService.GetLoanHistoryOfUserAsync(_currentChatId.ToString());
            if (loans.Count() == 0)
            {
                await botClient.SendMessage(_currentChatId, "No loan history yet.");
                return;
            }

            StringBuilder sb = new StringBuilder("Loan History:\n");
            var officerid = officerService.GetOfficerById((int)_currentChatId);
            foreach (var loan in loans)
            {
                sb.AppendLine($"LoanID: {loan.CustomerId}, Amount: {loan.Amount}, Status: {loan.Status}, Handled By: {officerid}");
            }

            await botClient.SendMessage(_currentChatId, sb.ToString());
        }
        #endregion

        #region Active Loans + Payment
        private static async Task ShowActiveLoans(TelegramBotClient botClient)
        {
            var loans = await loanService.GetActiveLoansByUserAsync(_currentChatId.ToString());
            if (loans.Count == 0)
            {
                await botClient.SendMessage(_currentChatId, "No active loans currently.");
                return;
            }

            foreach (var loan in loans)
            {
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Make Payment", $"pay_{loan.Id}")
                    }
                });
                await botClient.SendMessage(_currentChatId,
                    $"LoanID: {loan.Id}, Amount: {loan.Amount}, Status: {loan.Status}",
                    replyMarkup: keyboard);
            }
        }

        public static async Task MakePayment(TelegramBotClient telegramBotClient, int loanId, decimal amount)
        {
            var model = new RepaymentCreateModel
            {
                LoanId = loanId,
                Amount = amount
            };
            await repaymentService.MakeRepaymentAsync(model);
            await telegramBotClient.SendMessage(_currentChatId, $"Payment of {amount} made for LoanID: {loanId}");
        }
        #endregion

        #region Profile Management
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
