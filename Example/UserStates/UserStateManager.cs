using Telegram.Bot;
using Telegram.Bot.Types;

namespace Example.UserStates;

public class UserStateManager
{
    private readonly Dictionary<long, UserState> _states = new();
    private readonly TelegramBotClient _client;

    public UserStateManager(TelegramBotClient client) => _client = client;

    public async void EnterOrExitIfSame<TUserState>(long userId) where TUserState : UserState, new()
    {

        if (_states.ContainsKey(userId) && _states[userId] is TUserState)
        {
            await Exit(userId);
            return;
        }

        Logger.Log($"Entering {typeof(TUserState)} for [{userId}]");

        if (_states.ContainsKey(userId))
            await Exit(userId);

        _states[userId] = new TUserState();
        await _states[userId].Enter(this, userId, _client);
    }

    public async void ExitIfState<TUserState>(long userId) where TUserState : UserState, new()
    {
        if (_states.ContainsKey(userId) == false)
            return;

        if (_states[userId] is not TUserState)
            return;

        Logger.Log($"Exiting from {typeof(TUserState)} for [{userId}]");
        await Exit(userId);
    }

    public async Task Exit(long userId)
    {
        Logger.Log($"Exiting from state for user [{userId}]");

        if (_states.ContainsKey(userId) == false)
            return;

        await _states[userId].Exit();
        _states.Remove(userId);
    }

    public void Update(long userId, Message message)
    {
        if (_states.ContainsKey(userId) == false)
            return;

        Logger.Log($"Update state for [{userId}]");
        _states[userId].Update(message);
    }

    public bool Has(long userId) => _states.ContainsKey(userId);

    public UserState Get(long userId) => _states[userId];
}
