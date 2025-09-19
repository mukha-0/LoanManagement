using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using LoanManagement.Service.Services.AllEntries.LoanOfficer;
using LoanManagement.Service.Services.AllEntries.Users.Models;
using LoanManagement.Service.Services.AllEntries.Users;
using LoanManagement.Service.Services.Loans.Models;
using LoanManagement.Service.Services.Loans;
using LoanManagement.Service.Services.Repayments.Models;
using LoanManagement.Service.Services.Repayments;

namespace LoanManagement.Bots.UserBot
{
    public class UserBotCommands
    {
        private readonly IUserService _userService;
        private readonly ILoanService _loanService;
        private readonly IRepaymentService _repaymentService;
        private readonly ILoanOfficerService _officerService;

        private Dictionary<long, string> _userStates = new();
        private Dictionary<long, dynamic> _tempData = new();

        public UserBotCommands(
            IUserService userService,
            ILoanService loanService,
            IRepaymentService repaymentService,
            ILoanOfficerService officerService)
        {
            _userService = userService;
            _loanService = loanService;
            _repaymentService = repaymentService;
            _officerService = officerService;
        }

        // -------------------- Handle Updates --------------------
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
                Console.WriteLine($"UserBot Error: {ex.Message}");
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"UserBot Error: {exception.Message}");
            return Task.CompletedTask;
        }

        // -------------------- Process Text Messages --------------------
        private async Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message)
        {
            string state = _userStates[chatId];

            switch (state)
            {
                case "MainMenu":
                    await ShowMainMenu(botClient, chatId);
                    break;

                case "ApplyingLoan":
                    await HandleLoanApplication(botClient, chatId, message);
                    break;

                case "MakingPayment":
                    await HandlePayment(botClient, chatId, message);
                    break;

                case "UpdatingProfile":
                    await HandleProfileUpdate(botClient, chatId, message);
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
                new [] { InlineKeyboardButton.WithCallbackData("Apply for Loan", "ApplyLoan") },
                new [] { InlineKeyboardButton.WithCallbackData("Active Loans", "ActiveLoans") },
                new [] { InlineKeyboardButton.WithCallbackData("Loan History", "LoanHistory") },
                new [] { InlineKeyboardButton.WithCallbackData("Profile Settings", "ProfileSettings") }
            });

            await botClient.SendMessage(chatId, "User Menu:", replyMarkup: buttons);
        }

        // -------------------- Process Callback Buttons --------------------
        private async Task ProcessCallback(ITelegramBotClient botClient, Telegram.Bot.Types.CallbackQuery callback)
        {
            long chatId = callback.Message.Chat.Id;
            string data = callback.Data;

            switch (data)
            {
                case "ApplyLoan":
                    _userStates[chatId] = "ApplyingLoan";
                    _tempData[chatId] = new LoanCreateModel();
                    await botClient.SendMessage(chatId, "Enter loan amount:");
                    break;

                case "ActiveLoans":
                    var activeLoans = await _loanService.GetActiveLoansByUserAsync(chatId.ToString());
                    if (!activeLoans.Any())
                        await botClient.SendMessage(chatId, "No active loans found.");
                    else
                    {
                        string loansList = string.Join("\n", activeLoans.Select(l => $"LoanId: {l.Id}, Amount: {l.Amount}, Status: {l.Status}"));
                        await botClient.SendMessage(chatId, loansList);
                        InlineKeyboardMarkup payButton = new(new[]
                        {
                            new [] { InlineKeyboardButton.WithCallbackData("Make Payment", "MakePayment") }
                        });
                        await botClient.SendMessage(chatId, "Select an action:", replyMarkup: payButton);
                    }
                    break;

                case "LoanHistory":
                    var history = await _loanService.GetLoanHistoryOfUserAsync(chatId.ToString());
                    if (!history.Any())
                        await botClient.SendMessage(chatId, "No loan history found.");
                    else
                    {
                        string histList = string.Join("\n", history.Select(l => $"LoanId: {l.LoanId}, Amount: {l.Amount}, Status: {l.Status}"));
                        await botClient.SendMessage(chatId, histList);
                    }
                    break;

                case "ProfileSettings":
                    _userStates[chatId] = "UpdatingProfile";
                    await botClient.SendMessage(chatId, "Enter new full name (or type 'skip' to leave unchanged):");
                    break;

                case "MakePayment":
                    _userStates[chatId] = "MakingPayment";
                    await botClient.SendMessage(chatId, "Enter Loan ID for payment:");
                    break;

                default:
                    await botClient.SendMessage(chatId, "Unknown command. Use /start to return to main menu.");
                    break;
            }
        }

        // -------------------- Loan Application --------------------
        private async Task HandleLoanApplication(ITelegramBotClient botClient, long chatId, string message)
        {
            var tempLoan = _tempData[chatId] as LoanCreateModel;

            if (tempLoan.Amount == 0)
            {
                if (decimal.TryParse(message, out decimal amount))
                {
                    tempLoan.Amount = amount;
                    await botClient.SendMessage(chatId, "Enter loan duration in months:");
                }
                else
                {
                    await botClient.SendMessage(chatId, "Invalid amount. Enter numeric value:");
                }
            }
            else if (tempLoan.TermInMonths == 0)
            {
                if (int.TryParse(message, out int months))
                {
                    tempLoan.TermInMonths = months;
                    await botClient.SendMessage(chatId, "Enter purpose of loan:");
                }
                else
                {
                    await botClient.SendMessage(chatId, "Invalid input. Enter number of months:");
                }
            }
            else if (string.IsNullOrEmpty(tempLoan.Purpose))
            {
                tempLoan.Purpose = message;
                tempLoan.CustomerId = (int)chatId;

                await _loanService.ApplyForLoanAsync(tempLoan);
                await botClient.SendMessage(chatId, $"Loan application submitted successfully! Amount: {tempLoan.Amount}, Duration: {tempLoan.TermInMonths} months, Purpose: {tempLoan.Purpose}");
                _userStates[chatId] = "MainMenu";
            }
        }

        // -------------------- Payment --------------------
        private async Task HandlePayment(ITelegramBotClient botClient, long chatId, string message)
        {
            var tempPay = _tempData.ContainsKey(chatId) ? _tempData[chatId] as RepaymentCreateModel : new RepaymentCreateModel();

            if (tempPay.LoanId == 0)
            {
                if (int.TryParse(message, out int loanId))
                {
                    tempPay.LoanId = loanId;
                    await botClient.SendMessage(chatId, "Enter payment amount:");
                }
                else
                {
                    await botClient.SendMessage(chatId, "Invalid Loan ID. Enter a number:");
                }
            }
            else if (tempPay.Amount == 0)
            {
                if (decimal.TryParse(message, out decimal amount))
                {
                    tempPay.Amount = amount;
                    tempPay.UserId = (int)chatId; // Replace with actual user ID
                    await _repaymentService.MakeRepaymentAsync(tempPay);
                    await botClient.SendMessage(chatId, $"Payment of {amount} made successfully for Loan {tempPay.LoanId}");
                    _userStates[chatId] = "MainMenu";
                }
                else
                {
                    await botClient.SendMessage(chatId, "Invalid amount. Enter numeric value:");
                }
            }
        }

        // -------------------- Profile Update --------------------
        private async Task HandleProfileUpdate(ITelegramBotClient botClient, long chatId, string message)
        {
            var user = await _userService.GetUserByIdAsync((int)chatId); // Replace with actual mapping if needed

            if (user == null)
            {
                await botClient.SendMessage(chatId, "User not found.");
                _userStates[chatId] = "MainMenu";
                return;
            }

            if (!string.Equals(message, "skip", StringComparison.OrdinalIgnoreCase))
                user.FullName = message;

            await _userService.ChangePasswordAsync(new UserUpdateModel { FullName = user.FullName }, user.UserId);
            await botClient.SendMessage(chatId, "Profile updated successfully.");
            _userStates[chatId] = "MainMenu";
        }
    }
}
