using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using LoanManagement.Service.Services.AllEntries.LoanOfficer;
using LoanManagement.Service.Services.Loans;
using LoanManagement.Service.Services.Repayments;
using System.Linq;

namespace LoanManagement.Bots.LoanOfficerBot
{
    public class LoanOfficerBotCommands
    {
        private readonly ILoanService _loanService;
        private readonly ILoanOfficerService _officerService;
        private readonly IRepaymentService _repaymentService;

        private Dictionary<long, string> _userStates = new();
        private Dictionary<long, dynamic> _tempData = new();

        public LoanOfficerBotCommands(
            ILoanService loanService,
            ILoanOfficerService officerService,
            IRepaymentService repaymentService)
        {
            _loanService = loanService;
            _officerService = officerService;
            _repaymentService = repaymentService;
        }

        // -------------------- Handle Text Messages --------------------
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
                {
                    string message = update.Message.Text;
                    long chatId = update.Message.Chat.Id;

                    if (!_userStates.ContainsKey(chatId))
                        _userStates[chatId] = "MainMenu";

                    await ProcessMessageAsync(botClient, chatId, message);
                }
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    long chatId = update.CallbackQuery.Message.Chat.Id;
                    await ProcessCallback(botClient, update.CallbackQuery);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoanOfficerBot Error: {ex.Message}");
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"LoanOfficerBot Error: {exception.Message}");
            return Task.CompletedTask;
        }

        // -------------------- Process Messages --------------------
        private async Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message)
        {
            string state = _userStates[chatId];

            switch (state)
            {
                case "MainMenu":
                    await ShowMainMenu(botClient, chatId);
                    break;

                case "ApprovingLoan":
                    await HandleLoanApproval(botClient, chatId, message);
                    break;

                case "RejectingLoan":
                    await HandleLoanRejection(botClient, chatId, message);
                    break;

                default:
                    await botClient.SendMessage(chatId, "Unknown command. Use /start to return to main menu.");
                    _userStates[chatId] = "MainMenu";
                    break;
            }
        }

        // -------------------- Main Menu --------------------
        private async Task ShowMainMenu(ITelegramBotClient botClient, long chatId)
        {
            InlineKeyboardMarkup buttons = new(new[]
            {
                new [] { InlineKeyboardButton.WithCallbackData("Pending Loans", "PendingLoans") },
                new [] { InlineKeyboardButton.WithCallbackData("Loan History", "LoanHistory") },
                new [] { InlineKeyboardButton.WithCallbackData("Profile Settings", "ProfileSettings") },
            });

            await botClient.SendMessage(chatId, "Loan Officer Menu:", replyMarkup: buttons);
        }

        // -------------------- Process Callback Buttons --------------------
        private async Task ProcessCallback(ITelegramBotClient botClient, Telegram.Bot.Types.CallbackQuery callback)
        {
            long chatId = callback.Message.Chat.Id;
            string data = callback.Data;

            switch (data)
            {
                case "PendingLoans":
                    var pending = _loanService.GetLoansWaitingForOfficer().ToList();
                    if (!pending.Any())
                        await botClient.SendMessage(chatId, "No pending loans at the moment.");
                    else
                    {

                        await botClient.SendMessage(chatId, pending.ToString());
                        InlineKeyboardMarkup actionButtons = new(new[]
                        {
                            new [] { InlineKeyboardButton.WithCallbackData("Approve Loan", "ApproveLoan") },
                            new [] { InlineKeyboardButton.WithCallbackData("Reject Loan", "RejectLoan") },
                            new [] { InlineKeyboardButton.WithCallbackData("Back", "Back") }
                        });
                        await botClient.SendMessage(chatId, "Select an action:", replyMarkup: actionButtons);
                    }
                    break;

                case "LoanHistory":
                    var loans = await _loanService.GetAllLoansAsync();
                    string history = string.Join("\n", loans.Select(l => $"LoanId: {l.Id}, User: {l.CustomerId}, Status: {l.Status}"));
                    await botClient.SendMessage(chatId, history);
                    break;

                case "ProfileSettings":
                    await botClient.SendMessage(chatId, "Profile settings coming soon...");
                    break;

                case "ApproveLoan":
                    _userStates[chatId] = "ApprovingLoan";
                    await botClient.SendMessage(chatId, "Enter Loan ID to approve:");
                    break;

                case "RejectLoan":
                    _userStates[chatId] = "RejectingLoan";
                    await botClient.SendMessage(chatId, "Enter Loan ID to reject:");
                    break;

                case "Back":
                    _userStates[chatId] = "MainMenu";
                    await ShowMainMenu(botClient, chatId);
                    break;

                default:
                    await botClient.SendMessage(chatId, "Unknown command. Use /start to return to main menu.");
                    break;
            }
        }

        // -------------------- Approve Loan --------------------
        private async Task HandleLoanApproval(ITelegramBotClient botClient, long chatId, string message)
        {
            if (int.TryParse(message, out int loanId))
            {
                await _loanService.ApproveLoanAsync(loanId);
                await botClient.SendMessage(chatId, $"Loan {loanId} approved successfully.");
                _userStates[chatId] = "MainMenu";
            }
            else
            {
                await botClient.SendMessage(chatId, "Invalid Loan ID. Please enter a number.");
            }
        }

        // -------------------- Reject Loan --------------------
        private async Task HandleLoanRejection(ITelegramBotClient botClient, long chatId, string message)
        {
            string[] parts = message.Split(";", 2);
            if (parts.Length < 2)
            {
                await botClient.SendMessage(chatId, "Please enter in format: LoanId;Reason");
                return;
            }

            if (int.TryParse(parts[0], out int loanId))
            {
                string reason = parts[1];
                var officerUsername = "Officer"; // Replace with actual username from context if needed
                await _loanService.RejectLoanAsync(loanId);
                await botClient.SendMessage(chatId, $"Loan {loanId} rejected for reason: {reason}");
                _userStates[chatId] = "MainMenu";
            }
            else
            {
                await botClient.SendMessage(chatId, "Invalid Loan ID. Please enter a number.");
            }
        }
    }
}
