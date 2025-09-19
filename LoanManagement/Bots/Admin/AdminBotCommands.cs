using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using LoanManagement.Service.Services.AllEntries.Admin;
using LoanManagement.Service.Services.AllEntries.LoanOfficer;
using LoanManagement.Service.Services.AllEntries.Users;
using LoanManagement.Service.Services.Loans;
using LoanManagement.Service.Services.Repayments;
using LoanManagement.Service.Services.Reports;

namespace LoanManagement.Bots.AdminBot
{
    public class AdminBotCommands
    {
        private readonly IUserService _userService;
        private readonly ILoanService _loanService;
        private readonly IRepaymentService _repaymentService;
        private readonly IAdminService _adminService;
        private readonly ILoanOfficerService _officerService;
        private readonly IReportService _reportService;

        private Dictionary<long, string> _userStates = new();
        private Dictionary<long, dynamic> _tempData = new();

        public AdminBotCommands(
            IUserService userService,
            ILoanService loanService,
            IRepaymentService repaymentService,
            IAdminService adminService,
            ILoanOfficerService officerService,
            IReportService reportService)
        {
            _userService = userService;
            _loanService = loanService;
            _repaymentService = repaymentService;
            _adminService = adminService;
            _officerService = officerService;
            _reportService = reportService;
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
                Console.WriteLine($"AdminBot Error: {ex.Message}");
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Admin Bot Error: {exception.Message}");
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

                case "CreatingUser":
                    await HandleCreatingUser(botClient, chatId, message);
                    break;

                case "CreatingOfficer":
                    await HandleCreatingOfficer(botClient, chatId, message);
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
                new [] { InlineKeyboardButton.WithCallbackData("User Service", "UserService") },
                new [] { InlineKeyboardButton.WithCallbackData("Loan Service", "LoanService") },
                new [] { InlineKeyboardButton.WithCallbackData("Payment Service", "PaymentService") },
                new [] { InlineKeyboardButton.WithCallbackData("Report Service", "ReportService") },
            });

            await botClient.SendMessage(chatId, "Select a service to manage:", replyMarkup: buttons);
        }

        // -------------------- Process Callback Buttons --------------------
        private async Task ProcessCallback(ITelegramBotClient botClient, Telegram.Bot.Types.CallbackQuery callback)
        {
            long chatId = callback.Message.Chat.Id;
            string data = callback.Data;

            switch (data)
            {
                case "UserService":
                    InlineKeyboardMarkup userButtons = new(new[]
                    {
                        new [] { InlineKeyboardButton.WithCallbackData("Create User", "CreateUser") },
                        new [] { InlineKeyboardButton.WithCallbackData("View Users", "ViewUsers") },
                        new [] { InlineKeyboardButton.WithCallbackData("Back", "Back") }
                    });
                    await botClient.SendMessage(chatId, "User Service Menu:", replyMarkup: userButtons);
                    break;

                case "LoanService":
                    InlineKeyboardMarkup loanButtons = new(new[]
                    {
                        new [] { InlineKeyboardButton.WithCallbackData("View All Loans", "ViewAllLoans") },
                        new [] { InlineKeyboardButton.WithCallbackData("View Pending Loans", "ViewPendingLoans") },
                        new [] { InlineKeyboardButton.WithCallbackData("Back", "Back") }
                    });
                    await botClient.SendMessage(chatId, "Loan Service Menu:", replyMarkup: loanButtons);
                    break;

                case "PaymentService":
                    InlineKeyboardMarkup paymentButtons = new(new[]
                    {
                        new [] { InlineKeyboardButton.WithCallbackData("View All Payments", "ViewAllPayments") },
                        new [] { InlineKeyboardButton.WithCallbackData("Back", "Back") }
                    });
                    await botClient.SendMessage(chatId, "Payment Service Menu:", replyMarkup: paymentButtons);
                    break;

                case "ReportService":
                    InlineKeyboardMarkup reportButtons = new(new[]
                    {
                        new [] { InlineKeyboardButton.WithCallbackData("View Reports", "ViewReports") },
                        new [] { InlineKeyboardButton.WithCallbackData("Back", "Back") }
                    });
                    await botClient.SendMessage(chatId, "Report Service Menu:", replyMarkup: reportButtons);
                    break;

                case "CreateUser":
                    _userStates[chatId] = "CreatingUser";
                    _tempData[chatId] = new UserRegisterModel();
                    await botClient.SendMessage(chatId, "Enter new user's full name:");
                    break;

                case "CreateOfficer":
                    _userStates[chatId] = "CreatingOfficer";
                    _tempData[chatId] = new OfficerCreateModel();
                    await botClient.SendMessage(chatId, "Enter new officer's full name:");
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

        // -------------------- Step-by-Step Create User --------------------
        private async Task HandleCreatingUser(ITelegramBotClient botClient, long chatId, string message)
        {
            var tempUser = _tempData[chatId] as UserRegisterModel;

            if (string.IsNullOrEmpty(tempUser.FullName))
            {
                tempUser.FullName = message;
                await botClient.SendMessage(chatId, "Enter username:");
            }
            else if (string.IsNullOrEmpty(tempUser.Username))
            {
                tempUser.Username = message;
                await botClient.SendMessage(chatId, "Enter password:");
            }
            else if (string.IsNullOrEmpty(tempUser.Password))
            {
                tempUser.Password = message;
                await _userService.RegisterUserAsync(tempUser);
                await botClient.SendMessage(chatId, $"User '{tempUser.Username}' created successfully!");
                _userStates[chatId] = "MainMenu";
            }
        }

        // -------------------- Step-by-Step Create Officer --------------------
        private async Task HandleCreatingOfficer(ITelegramBotClient botClient, long chatId, string message)
        {
            var tempOfficer = _tempData[chatId] as OfficerCreateModel;

            if (string.IsNullOrEmpty(tempOfficer.FullName))
            {
                tempOfficer.FullName = message;
                await botClient.SendMessage(chatId, "Enter username:");
            }
            else if (string.IsNullOrEmpty(tempOfficer.Username))
            {
                tempOfficer.Username = message;
                await botClient.SendMessage(chatId, "Enter password:");
            }
            else if (string.IsNullOrEmpty(tempOfficer.Password))
            {
                tempOfficer.Password = message;
                await _officerService.ApplyForLoanOfficer(tempOfficer);
                await botClient.SendMessage(chatId, $"Loan Officer '{tempOfficer.Username}' created successfully!");
                _userStates[chatId] = "MainMenu";
            }
        }
    }
}
