// LoanManagement/Bots/NotificationHub.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoanManagement.Bots
{
    // In-memory notification hub for cross-bot messages (works if all bots run in same process)
    public static class NotificationHub
    {
        // loanId -> list of pending admin chatIds who requested officer action
        private static readonly ConcurrentDictionary<int, int> _loanAdminRequester = new();

        // register admin request
        public static void RegisterAdminRequest(int loanId, int adminChatId)
        {
            _loanAdminRequester[loanId] = adminChatId;
        }

        // take admin chat for loanId (if present)
        public static bool TryTakeAdminRequester(int loanId, out int adminChatId)
        {
            if (_loanAdminRequester.TryRemove(loanId, out var id))
            {
                adminChatId = id;
                return true;
            }
            adminChatId = 0;
            return false;
        }
    }
}
