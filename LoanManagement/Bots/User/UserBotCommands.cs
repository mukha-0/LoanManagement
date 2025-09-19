using LoanManagement.Service.Services.AllEntries.Users.Models;
using LoanManagement.Service.Services.AllEntries.Users;
using LoanManagement.Service.Services.Loans.Models;
using LoanManagement.Service.Services.Loans;
using LoanManagement.Service.Services.Repayments.Models;
using LoanManagement.Service.Services.Repayments;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using LoanManagement.DataAccess.Context;
using LoanManagement.Bots;

namespace LoanManagement.Bot
{
    public static class UserBotCommands
    {
        private static IUserService? _userService;
        private static ILoanService? _loanService;
        private static IRepaymentService? _repaymentService;

        // state handling
        private static readonly Dictionary<long, string> _userStates = new();
        private static readonly Dictionary<long, object> _tempData = new();
        private static TelegramBotClient _bot = null!;

        // track logged in user
        private static readonly Dictionary<long, int> _loggedInUsers = new();

        public static void Configure(
            IUserService userService,
            ILoanService loanService,
            IRepaymentService repaymentService)
        {
            _userService = userService;
            _loanService = loanService;
            _repaymentService = repaymentService;
        }

        public static async Task ProcessCommand(TelegramBotClient bot, Message message, AppDBContext db)
        {
            var chatId = message.Chat.Id;
            var text = message.Text!.Trim();

            if (text == "/start")
            {
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Apply for Loan", "apply_loan"),
                        InlineKeyboardButton.WithCallbackData("My Active Loans", "active_loans")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Loan History", "loan_history"),
                        InlineKeyboardButton.WithCallbackData("Make Repayment", "repayment")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Profile Settings", "profile"),
                    }
                });

                await bot.SendMessage(chatId, "Welcome to the Loan System User Bot!\nChoose an option:", replyMarkup: keyboard);
            }
            else
            {
                await bot.SendMessage(chatId, "Unknown command. Use /start to open the menu.");
            }
        }

        public static async Task ProcessCommand(ITelegramBotClient bot, Message message)
        {
            var chatId = message.Chat.Id;
            var text = message.Text?.Trim();

            if (_userStates.ContainsKey(chatId))
            {
                await HandleState(bot, message, chatId, text!);
                return;
            }

            switch (text)
            {
                case "/start":
                    await ShowMainMenu(bot, chatId);
                    break;

                case "👤 Account":
                    await ShowAccountMenu(bot, chatId);
                    break;

                case "💰 Loans":
                    await ShowLoanMenu(bot, chatId);
                    break;

                case "💳 Repayments":
                    await ShowRepaymentMenu(bot, chatId);
                    break;

                default:
                    await bot.SendMessage(chatId, "❌ Unknown command. Use /start to see the menu.");
                    break;
            }
        }

        private static async Task ShowMainMenu(ITelegramBotClient bot, long chatId)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("👤 Account") },
                new[] { new KeyboardButton("💰 Loans"), new KeyboardButton("💳 Repayments") }
            })
            { ResizeKeyboard = true };

            await bot.SendMessage(chatId, "📌 User Menu:", replyMarkup: keyboard);
        }

        #region Account
        private static async Task ShowAccountMenu(ITelegramBotClient bot, long chatId)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("🔑 Register"), new KeyboardButton("🔓 Login") },
                new[] { new KeyboardButton("✏️ Change Password"), new KeyboardButton("🗑 Delete Account") },
                new[] { new KeyboardButton("⬅️ Back") }
            })
            { ResizeKeyboard = true };

            await bot.SendMessage(chatId, "👤 Account Menu:", replyMarkup: keyboard);
        }
        #endregion

        #region Loans
        private static async Task ShowLoanMenu(ITelegramBotClient bot, long chatId)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("📝 Apply Loan"), new KeyboardButton("📜 Loan History") },
                new[] { new KeyboardButton("📊 Active Loans"), new KeyboardButton("💵 Calculate Interest") },
                new[] { new KeyboardButton("⬅️ Back") }
            })
            { ResizeKeyboard = true };

            await bot.SendMessage(chatId, "💰 Loan Menu:", replyMarkup: keyboard);
        }
        #endregion

        #region Repayments
        private static async Task ShowRepaymentMenu(ITelegramBotClient bot, long chatId)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("💳 Make Repayment"), new KeyboardButton("📅 Repayment Schedule") },
                new[] { new KeyboardButton("📜 My Repayments"), new KeyboardButton("⚖️ Outstanding Balance") },
                new[] { new KeyboardButton("⬅️ Back") }
            })
            { ResizeKeyboard = true };

            await bot.SendMessage(chatId, "💳 Repayment Menu:", replyMarkup: keyboard);
        }
        #endregion

        private static async Task HandleState(ITelegramBotClient bot, Message message, long chatId, string text)
        {
            var state = _userStates[chatId];

            switch (state)
            {
                // ---------- Account ----------
                case "Register_Username":
                    _tempData[chatId] = new UserRegisterModel { UserName = text };
                    _userStates[chatId] = "Register_Password";
                    await bot.SendMessage(chatId, "Enter a password:");
                    break;

                case "Register_Password":
                    if (_tempData[chatId] is UserRegisterModel reg)
                    {
                        reg.PasswordHash = text;
                        await _userService!.RegisterUserAsync(reg);
                        await bot.SendMessage(chatId, "✅ User registered!");
                    }
                    ResetState(chatId);
                    await ShowMainMenu(bot, chatId);
                    break;

                case "Login_Username":
                    _tempData[chatId] = new UserLoginModel { Username = text };
                    _userStates[chatId] = "Login_Password";
                    await bot.SendMessage(chatId, "Enter your password:");
                    break;

                case "Login_Password":
                    if (_tempData[chatId] is UserLoginModel login)
                    {
                        login.Password = text;
                        var user = await _userService!.LoginAsync(login);
                        if (user != null)
                        {
                            _loggedInUsers[chatId] = user.UserId;
                            await bot.SendMessage(chatId, "🔓 Login successful!");
                        }
                        else
                        {
                            await bot.SendMessage(chatId, "❌ Invalid credentials.");
                        }
                    }
                    ResetState(chatId);
                    await ShowMainMenu(bot, chatId);
                    break;

                case "ChangePassword":
                    if (_loggedInUsers.ContainsKey(chatId))
                    {
                        var userId = _loggedInUsers[chatId];
                        var update = new UserUpdateModel { Password = text };
                        await _userService!.ChangePasswordAsync(update, userId);
                        await bot.SendMessage(chatId, "✅ Password updated.");
                    }
                    else
                        await bot.SendMessage(chatId, "❌ Please login first.");
                    ResetState(chatId);
                    await ShowMainMenu(bot, chatId);
                    break;

                // ---------- Loan ----------
                case "ApplyLoan_Amount":
                    var loanModel = new LoanCreateModel { Amount = decimal.Parse(text) };
                    loanModel.CustomerId = _loggedInUsers[chatId];
                    await _loanService!.ApplyForLoanAsync(loanModel);
                    await bot.SendMessage(chatId, "📝 Loan application submitted.");
                    ResetState(chatId);
                    await ShowMainMenu(bot, chatId);
                    break;

                // ---------- Repayments ----------
                case "MakeRepayment_Amount":
                    if (_loggedInUsers.ContainsKey(chatId))
                    {
                        var repayment = new RepaymentCreateModel
                        {
                            UserId = _loggedInUsers[chatId],
                            Amount = decimal.Parse(text)
                        };
                        await _repaymentService!.MakeRepaymentAsync(repayment);
                        await bot.SendMessage(chatId, "💳 Repayment successful.");
                    }
                    else
                        await bot.SendMessage(chatId, "❌ Please login first.");
                    ResetState(chatId);
                    await ShowMainMenu(bot, chatId);
                    break;
            }
        }
        public static async Task ProcessCallback(TelegramBotClient telegramBotClient, CallbackQuery callback, AppDBContext appDB)
        {
            try
            {
                var chatId = callback.Message.Chat.Id;

                switch (callback.Data)
                {
                    case "apply_loan":
                        _userStates[chatId] = "apply_loan_amount";
                        await _bot.SendMessage(chatId, "💵 Enter loan amount to apply:");
                        break;
                    case "make_payment":
                        _userStates[chatId] = "make_payment_loanid";
                        await _bot.SendMessage(chatId, "💳 Enter Loan ID to make payment for:");
                        break;

                    case "settings":
                        await ShowAccountMenu(_bot, chatId);
                        break;

                    case "delete_account":
                        _userStates[chatId] = "delete_account_confirm";
                        await _bot.SendMessage(chatId, "⚠️ Type 'CONFIRM' to delete your account. This cannot be undone.");
                        break;

                    default:
                        await _bot.SendMessage(chatId, "⚠️ Unknown option.");
                        break;
                }
            }
            catch (Exception ex)
            {
                await _bot.SendMessage(callback.Message.Chat.Id, $"❌ Error: {ex.Message}");
            }
        }

        private static void ResetState(long chatId)
        {
            _userStates.Remove(chatId);
            _tempData.Remove(chatId);
        }
    }
}
