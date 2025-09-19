// LoanManagement/Bots/LoanOfficerBot/LoanOfficerCommands.cs
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using LoanManagement.Service;
using LoanManagement.Domain;
using LoanManagement.Bots;
using LoanManagement.Service.Services.AllEntries.LoanOfficer;
using LoanManagement.Service.Services.Loans;
using LoanManagement.Service.Services.AllEntries.Admin;
using LoanManagement.Service.Services.Repayments;
using LoanManagement.Service.Services.AllEntries.LoanOfficer.Models;
using LoanManagement.DataAccess.Context;
using LoanManagement.Service.Services.AllEntries.Users;
using LoanManagement.Service.Services.Reports;
using LoanManagement.Service.Services.AllEntries; // NotificationHub

namespace LoanManagement.Bots.LoanOfficerBot
{
    public class LoanOfficerCommands
    {
        private readonly TelegramBotClient _bot;
        private readonly ILoanOfficerService _officerService;
        private readonly ILoanService _loanService;
        private readonly IAdminService _adminService;
        private readonly IRepaymentService _repaymentService;

        // per-chat state
        private readonly ConcurrentDictionary<long, string> _state = new();
        private readonly ConcurrentDictionary<long, object> _temp = new();
        private TelegramBotClient bot;
        private UserService userService;
        private LoanService loanService;
        private RepaymentService repaymentService;
        private ReportService reportService;
        private AdminService adminService;
        private LoanOfficerService loanOfficerService;

        public LoanOfficerCommands(TelegramBotClient bot,
            Service.Services.AllEntries.Users.UserService userService,
            LoanService loanService1,
            ILoanOfficerService officerService,
            ILoanService loanService,
            IAdminService adminService,
            IRepaymentService repaymentService)
        {
            _bot = bot;
            _officerService = officerService;
            _loanService = loanService;
            _adminService = adminService;
            _repaymentService = repaymentService;
        }

        public LoanOfficerCommands(TelegramBotClient bot, UserService userService, LoanService loanService, RepaymentService repaymentService, ReportService reportService, AdminService adminService, LoanOfficerService loanOfficerService)
        {
            this.bot = bot;
            this.userService = userService;
            this.loanService = loanService;
            this.repaymentService = repaymentService;
            this.reportService = reportService;
            this.adminService = adminService;
            this.loanOfficerService = loanOfficerService;
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

        public async Task ProcessMessage(Message message)
        {
            var chatId = message.Chat.Id;
            var text = message.Text?.Trim() ?? "";

            if (text.Equals("/start", StringComparison.OrdinalIgnoreCase))
            {
                await ShowMenu(chatId);
                return;
            }

            if (_state.TryGetValue(chatId, out var st) && st != "MAIN")
            {
                await HandleStateInput(chatId, st, text);
                return;
            }

            // If Admin requested a loan to officer, NotificationHub holds adminChatId for loanId.
            // We'll also allow officer to view pending apps and accept/reject.

            switch (text.ToLower())
            {
                case "pending loans":
                    await ViewPendingLoans(chatId);
                    break;
                case "loan history":
                    await ViewHandledLoans(chatId);
                    break;
                case "manage profile":
                    _state[chatId] = "PROFILE_MENU";
                    await _bot.SendMessage(chatId, "Profile menu: Update / Delete (send 'update' or 'delete')");
                    break;
                default:
                    await _bot.SendMessage(chatId, "Unknown. Send /start for menu or 'pending loans'.");
                    break;
            }
        }

        public static async Task ProcessCallback(TelegramBotClient bot, Telegram.Bot.Types.CallbackQuery callback, AppDBContext db)
        {
            var chatId = callback.Message!.Chat.Id;
            var data = callback.Data ?? "";
            var parts = data.Split(':', 2);
            var action = parts[0];
            var param = parts.Length > 1 ? parts[1] : null;

            switch (action)
            {
                case "pending_list":
                    var pendingLoans = await new LoanOfficerService(db).GetPendingLoanApplications();
                    if (pendingLoans.Any())
                    {
                        foreach (var loan in pendingLoans)
                        {
                            var buttons = new[]
                            {
                        InlineKeyboardButton.WithCallbackData($"✅ Approve {loan.Id}", $"approve:{loan.Id}"),
                        InlineKeyboardButton.WithCallbackData($"❌ Reject {loan.Id}", $"reject:{loan.Id}")
                    };
                            await bot.SendMessage(chatId,
                                $"Loan {loan.Id}\nAmount: {loan.Amount}\nStatus: {loan.Status}",
                                replyMarkup: new InlineKeyboardMarkup(buttons));
                        }
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "No pending loans.");
                    }
                    break;

                case "approve":
                    if (int.TryParse(param, out var loanIdApprove))
                    {
                        await new LoanOfficerService(db).ApproveLoanApplication(loanIdApprove);
                        await bot.SendMessage(chatId, $"Loan {loanIdApprove} approved.");
                    }
                    break;

                case "reject":
                    if (int.TryParse(param, out var loanIdReject))
                    {
                        await new LoanOfficerService(db).RejectLoanApplication(loanIdReject, $"Officer{chatId}", "No reason provided");
                        await bot.SendMessage(chatId, $"Loan {loanIdReject} rejected.");
                    }
                    break;

                case "view_officer_detail":
                    if (int.TryParse(param, out var officerId))
                    {
                        var officer = await new LoanOfficerService(db).GetOfficerById(officerId);
                        if (officer != null)
                            await bot.SendMessage(chatId, $"Officer: {officer.Username}\nId: {officer.Id}");
                        else
                            await bot.SendMessage(chatId, "Officer not found.");
                    }
                    break;

                default:
                    await bot.SendMessage(chatId, "Unknown action.");
                    break;
            }

            try { await bot.AnswerCallbackQuery(callback.Id); } catch { }
        }


        private async Task ShowMenu(long chatId)
        {
            var kb = new InlineKeyboardMarkup(new[]
            {
                new[]{ InlineKeyboardButton.WithCallbackData("Pending Loan Applications","pending_list") },
                new[]{ InlineKeyboardButton.WithCallbackData("Loan History","loan_history") },
                new[]{ InlineKeyboardButton.WithCallbackData("View repayment schedule (enter id)", "repayment_prompt") }
            });
            await _bot.SendMessage(chatId, "Loan Officer Menu:", replyMarkup: kb);
        }

        private async Task ViewPendingLoans(long chatId)
        {
            // use officer service GetPendingLoanApplications
            var pending = await _officerService.GetPendingLoanApplications();
            if (pending == null || !pending.Any())
            {
                await _bot.SendMessage(chatId, "No pending applications.");
                return;
            }

            foreach (var p in pending)
            {
                var kb = new InlineKeyboardMarkup(new[]
                {
                    new[]{ InlineKeyboardButton.WithCallbackData("Approve", $"approve:{p.Id}"),
                           InlineKeyboardButton.WithCallbackData("Reject", $"reject:{p.Id}") },
                    new[]{ InlineKeyboardButton.WithCallbackData("View Repayment Schedule", $"view_repayment_schedule:{p.Id}") }
                });
                await _bot.SendMessage(chatId, $"AppID:{p.Id} User:{p.CustomerId} Amount:{p.Amount} Status:{p.Status}", replyMarkup: kb);
            }
        }

        private async Task ViewHandledLoans(long chatId)
        {
            // Use officer service or loan service - use GetPendingLoanApplications?? use officer method GetPendingLoanApplications for pending; for history, use officer service GetAllOfficers? There is GetPendingLoanApplications and GetLoanApplicationDetails.
            var handled = await _officerService.GetPendingLoanApplications(); // fallback to pending, or if you have history method use that
            var sb = new StringBuilder();
            foreach (var l in handled) sb.AppendLine($"ID:{l.Id} User:{l.CustomerId} Amount:{l.Amount} Status:{l.Status}");
            await _bot.SendMessage(chatId, sb.ToString());
        }

        private async Task HandleStateInput(long chatId, string state, string input)
        {
            switch (state)
            {
                case "PROFILE_MENU":
                    if (input.Equals("update", StringComparison.OrdinalIgnoreCase))
                    {
                        _state[chatId] = "PROFILE_UPDATE_USERNAME";
                        await _bot.SendMessage(chatId, "Enter your new username:");
                    }
                    else if (input.Equals("delete", StringComparison.OrdinalIgnoreCase))
                    {
                        _state[chatId] = "PROFILE_DELETE_CONFIRM";
                        await _bot.SendMessage(chatId, "Send your officer id to delete your account:");
                    }
                    else
                    {
                        _state[chatId] = "MAIN";
                        await _bot.SendMessage(chatId, "Cancelled.");
                    }
                    break;

                case "PROFILE_UPDATE_USERNAME":
                    _temp[chatId] = input;
                    _state[chatId] = "PROFILE_UPDATE_PASSWORD";
                    await _bot.SendMessage(chatId, "Enter new password:");
                    break;

                case "PROFILE_UPDATE_PASSWORD":
                    {
                        // get officer id? Here assume officerId stored or use chatId mapping
                        var newUsername = _temp[chatId].ToString();
                        var newPassword = input;
                        // you need officer id — we'll ask user for officer id next or assume they supplied earlier. For simplicity, try to call CreateNewLoanOfficer? but better to require officer id earlier.
                        await _bot.SendMessage(chatId, "To update you must provide officer id. Send officer id now:");
                        _state[chatId] = "PROFILE_UPDATE_OFFICERID";
                        _temp[chatId] = new UsernamePassword { Username = newUsername, Password = newPassword };
                        break;
                    }

                case "PROFILE_UPDATE_OFFICERID":
                    if (int.TryParse(input, out var oid))
                    {
                        var up = (UsernamePassword)_temp[chatId];
                        var model = new OfficerUpdateModel
                        {
                            Username = up.Username,
                            Password = up.Password
                        };
                        await _officerService.UpdateLoanOfficer(oid, model);
                        await _bot.SendMessage(chatId, $"Officer {oid} updated.");
                    }
                    else await _bot.SendMessage(chatId, "Invalid id.");
                    _state[chatId] = "MAIN";
                    break;

                case "PROFILE_DELETE_CONFIRM":
                    if (int.TryParse(input, out var delId))
                    {
                        await _officerService.UpdateLoanOfficer(delId, new OfficerUpdateModel { Username = "", Password = "" }); // no explicit delete so call admin delete?
                        await _adminService.DeleteOfficerAsync(delId);
                        await _bot.SendMessage(chatId, $"Officer {delId} deleted.");
                    }
                    else await _bot.SendMessage(chatId, "Invalid id.");
                    _state[chatId] = "MAIN";
                    break;

                case "REJECT_REASON":
                    // temp contains loan id
                    if (_temp.TryGetValue(chatId, out var loanObj) && loanObj is int lid)
                    {
                        var reason = input;
                        var officerUsername = "officer"; // if you have mapping from chat to username fill here
                        await _officerService.RejectLoanApplication(lid, officerUsername, reason);
                        await _loanService.RejectLoanAsync(lid);
                        if (NotificationHub.TryTakeAdminRequester(lid, out var adminChat))
                        {
                            await _bot.SendMessage(adminChat, $"Loan {lid} rejected by officer. Reason: {reason}");
                        }
                        await _bot.SendMessage(chatId, $"Loan {lid} rejected with reason.");
                    }
                    _state[chatId] = "MAIN";
                    break;

                default:
                    _state[chatId] = "MAIN";
                    await _bot.SendMessage(chatId, "State cleared. Send /start.");
                    break;
            }
        }

        class UsernamePassword { public string Username; public string Password; }
    }
}
