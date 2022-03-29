using Telegram.Bot;
using Telegram.Bot.Types;

namespace Example.UserStates;

public abstract class UserState
{
    protected TelegramBotClient Client;
    protected long UserId { get; private set; }
    protected UserStateManager Manager;

    public virtual void Enter(UserStateManager manager, long userId, TelegramBotClient client)
    {
        Client = client;
        UserId = userId;
        Manager = manager;
    }

    public abstract void Update(Message message);
    
    public abstract void Exit();
}
