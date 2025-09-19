// LoanManagement/Bots/UserBot/UserBotHandler.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using LoanManagement.Service;
using LoanManagement.Domain;
using LoanManagement.Service.Services.AllEntries.Users;
using LoanManagement.Service.Services.Loans;
using LoanManagement.Service.Services.Repayments;
using LoanManagement.Service.Services.AllEntries.Users.Models;
using LoanManagement.Service.Services.Loans.Models;
using LoanManagement.Service.Services.Repayments.Models;
using System.Text;

namespace LoanManagement.Bots.UserBot
{
    public class UserBotHandler
    {
        private readonly TelegramBotClient _bot;
        private readonly IUserService _userService;
        private readonly ILoanService _loanService;
        private readonly IRepaymentService _repayment_service;
        private readonly IRepaymentService _repaymentService;

        // per-chat state
        private readonly ConcurrentDictionary<long, string> _state = new();
        private readonly ConcurrentDictionary<long, Dictionary<string, string>> _tmp = new();

        public UserBotHandler(TelegramBotClient bot, IUserService userService, ILoanService loanService, IRepaymentService repaymentService)
        {
            _bot = bot;
            _userService = userService;
            _loanService = loanService;
            _repayment_service = repaymentService;
            _repaymentService = repaymentService;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update is null) return;
            if (update.Message != null && update.Message.Type == MessageType.Text)
            {
                var msg = update.Message;
                var chatId = msg.Chat.Id;
                var text = msg.Text?.Trim() ?? "";

                if (text.Equals("/start", StringComparison.OrdinalIgnoreCase))
                {
                    await ShowMainMenu(chatId);
                    return;
                }

                if (_state.TryGetValue(chatId, out var st) && st != "MAIN")
                {
                    await HandleState(chatId, st, text);
                    return;
                }

                // simple text commands for quick testing
                switch (text.ToLower())
                {
                    case "my active loans":
                        await Action_ShowActiveLoans(chatId);
                        break;
                    case "loan history":
                        await Action_ShowLoanHistory(chatId);
                        break;
                    default:
                        await _bot.SendMessage(chatId, "Unknown. Use /start to open menu.");
                        break;
                }
            }
            else if (update.CallbackQuery != null)
            {
                await ProcessCallback(update.CallbackQuery);
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"UserBot error: {exception}");
            return Task.CompletedTask;
        }

        private async Task ShowMainMenu(long chatId)
        {
            var kb = new InlineKeyboardMarkup(new[]
            {
                new[]{ InlineKeyboardButton.WithCallbackData("Register","reg") },
                new[]{ InlineKeyboardButton.WithCallbackData("Login","login") },
                new[]{ InlineKeyboardButton.WithCallbackData("Apply for Loan","apply") },
                new[]{ InlineKeyboardButton.WithCallbackData("Active Loans","active") },
                new[]{ InlineKeyboardButton.WithCallbackData("Repay Loan","repay") },
                new[]{ InlineKeyboardButton.WithCallbackData("Loan History","history") }
            });
            await _bot.SendMessage(chatId, "User Menu:", replyMarkup: kb);
        }

        private async Task ProcessCallback(Telegram.Bot.Types.CallbackQuery cb)
        {
            var chatId = cb.Message!.Chat.Id;
            var d = cb.Data ?? "";
            switch (d)
            {
                case "reg":
                    _state[chatId] = "REG_USERNAME";
                    _tmp[chatId] = new Dictionary<string, string>();
                    await _bot.SendMessage(chatId, "Enter username:");
                    break;
                case "login":
                    _state[chatId] = "LOGIN_USERNAME";
                    _tmp[chatId] = new Dictionary<string, string>();
                    await _bot.SendMessage(chatId, "Enter username to login:");
                    break;
                case "apply":
                    _state[chatId] = "APPLY_AMOUNT";
                    _tmp[chatId] = new Dictionary<string, string>();
                    await _bot.SendMessage(chatId, "Enter loan amount:");
                    break;
                case "active":
                    await Action_ShowActiveLoans(chatId);
                    break;
                case "repay":
                    _state[chatId] = "REPAY_LOANID";
                    _tmp[chatId] = new Dictionary<string, string>();
                    await _bot.SendMessage(chatId, "Enter loan id to repay:");
                    break;
                case "history":
                    await Action_ShowLoanHistory(chatId);
                    break;
                default:
                    await _bot.SendMessage(chatId, "Unknown action.");
                    break;
            }
            try { await _bot.AnswerCallbackQuery(cb.Id); } catch { }
        }

        private async Task HandleState(long chatId, string state, string text)
        {
            switch (state)
            {
                case "REG_USERNAME":
                    _tmp[chatId]["username"] = text;
                    _state[chatId] = "REG_PASSWORD";
                    await _bot.SendMessage(chatId, "Enter password:");
                    break;
                case "REG_PASSWORD":
                    var model = new UserRegisterModel { UserName = _tmp[chatId]["username"], PasswordHash = text };
                    await _userService.RegisterUserAsync(model);
                    _state.TryRemove(chatId, out _);
                    _tmp.TryRemove(chatId, out _);
                    await _bot.SendMessage(chatId, "Registration completed.");
                    break;
                case "LOGIN_USERNAME":
                    _tmp[chatId]["username"] = text; _state[chatId] = "LOGIN_PASSWORD";
                    await _bot.SendMessage(chatId, "Enter password:");
                    break;
                case "LOGIN_PASSWORD":
                    var loginModel = new UserLoginModel { Username = _tmp[chatId]["username"], Password = text };
                    var user = await _userService.LoginAsync(loginModel);
                    _state.TryRemove(chatId, out _);
                    _tmp.TryRemove(chatId, out _);
                    if (user != null) await _bot.SendMessage(chatId, $"Welcome {user.UserName}");
                    else await _bot.SendMessage(chatId, "Login failed.");
                    break;
                case "APPLY_AMOUNT":
                    _tmp[chatId]["amount"] = text;
                    _state[chatId] = "APPLY_TERM";
                    await _bot.SendMessage(chatId, "Enter term months:");
                    break;
                case "APPLY_TERM":
                    {
                        var lc = new LoanCreateModel
                        {
                            Amount = decimal.Parse(_tmp[chatId]["amount"]),
                            TermInMonths = int.Parse(text),
                            ChatId = int.Parse(chatId.ToString()) // ideally use logged in user id, but we use chatId string as a link
                        };
                        var loan = await _loanService.ApplyForLoanAsync(lc);
                        _state.TryRemove(chatId, out _); _tmp.TryRemove(chatId, out _);
                        await _bot.SendMessage(chatId, $"Loan applied. ID: {loan.LoanId}");
                    }
                    break;
                case "REPAY_LOANID":
                    _tmp[chatId]["loanId"] = text;
                    _state[chatId] = "REPAY_AMOUNT";
                    await _bot.SendMessage(chatId, "Enter repayment amount:");
                    break;
                case "REPAY_AMOUNT":
                    {
                        var repay = new RepaymentCreateModel
                        {
                            LoanId = int.Parse(_tmp[chatId]["loanId"]),
                            Amount = decimal.Parse(text)
                        };
                        await _repaymentService.MakeRepaymentAsync(repay);
                        _state.TryRemove(chatId, out _); _tmp.TryRemove(chatId, out _);
                        await _bot.SendMessage(chatId, "Repayment submitted.");
                    }
                    break;
                default:
                    _state.TryRemove(chatId, out _);
                    await _bot.SendMessage(chatId, "State cleared. Send /start.");
                    break;
            }
        }

        private async Task Action_ShowActiveLoans(long chatId)
        {
            // use chatId as userId string; in real app map chat->userId
            var loans = await _loanService.GetActiveLoansByUserAsync(chatId.ToString());
            if (loans == null || loans.Count == 0) { await _bot.SendMessage(chatId, "No active loans."); return; }
            var sb = new StringBuilder();
            foreach (var l in loans) sb.AppendLine($"ID:{l.Id} Amount:{l.Amount} Status:{l.Status} Officer:{l.AcceptedByOfficerUsername}");
            await _bot.SendMessage(chatId, sb.ToString());
        }

        private async Task Action_ShowLoanHistory(long chatId)
        {
            var q = await _loanService.GetLoanHistoryOfUserAsync(chatId.ToString());
            var list = q?.ToList();
            if (list == null || list.Count == 0) { await _bot.SendMessage(chatId, "No history."); return; }
            var sb = new StringBuilder();
            foreach (var l in list) sb.AppendLine($"ID:{l.CustomerId} Amount:{l.Amount} Status:{l.Status}");
            await _bot.SendMessage(chatId, sb.ToString());
        }
    }
}
