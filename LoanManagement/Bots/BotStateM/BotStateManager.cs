using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanManagement.UI.Bots.BotStateM
{
    public class BotStateManager<TState, TTemp>
    {
        private readonly Dictionary<long, TState> _states = new();
        private readonly Dictionary<long, TTemp> _tempData = new();
        private readonly Func<TState> _stateFactory;
        private readonly Func<TTemp> _tempFactory;

        public BotStateManager(Func<TState> stateFactory, Func<TTemp> tempFactory)
        {
            _stateFactory = stateFactory;
            _tempFactory = tempFactory;
        }

        public TState GetState(long chatId)
        {
            if (!_states.ContainsKey(chatId))
                _states[chatId] = _stateFactory.Invoke();
            return _states[chatId];
        }

        public void SetState(long chatId, TState state)
        {
            _states[chatId] = state;
        }

        public TTemp GetTempData(long chatId)
        {
            if (!_tempData.ContainsKey(chatId))
                _tempData[chatId] = _tempFactory.Invoke();
            return _tempData[chatId];
        }

        public void SetTempData(long chatId, TTemp temp)
        {
            _tempData[chatId] = temp;
        }
    }
}
