using Example.Logging;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Example.Commands.CallbackButtons;

internal delegate Task<bool> ExecuteCallbackCommand(CallbackCommandContext context);

[AttributeUsage(AttributeTargets.Method)]
internal class CallbackCommandAttribute : Attribute
{
    public ButtonId Id { get; private set; }

    public CallbackCommandAttribute(ButtonId id)
    {
        Id = id;
    }
}

internal class CallbackCommandContext
{
    public TelegramBotClient Client { get; private set; }
    public CallbackQuery Callback { get; private set; }

    public string[] Args { get; private set; }
    public CallbackCommandContext(TelegramBotClient client, CallbackQuery query)
    {
        Client = client;
        Callback = query;
        Args = query.Data.Split(';');
    }
}

internal class CallbackCommandInfo
{
    public ButtonId Id { get; private set; }

    private readonly ExecuteCallbackCommand _command;

    public CallbackCommandInfo(ButtonId id, ExecuteCallbackCommand action)
    {
        Id = id;
        _command = action;
    }

    public bool Execute(TelegramBotClient client, CallbackQuery callback)
    {
        return _command(new CallbackCommandContext(client, callback)).GetAwaiter().GetResult();
    }
}

internal class CallbackCommandManager : CommandManager<CallbackCommandInfo>
{
    public CallbackCommandManager(TelegramBotClient client) : base(client) { }

    public bool TryExecute(CallbackQuery callback)
    {

        var command = _commands.FirstOrDefault(info => callback.Data.Split(';')[0].ToInt() == info.Id.ToButtonIdInt());

        if (command == default)
            return false;

        Logger.Log($"{callback.From.Username} tap callback button {command.Id}");
        var result = command.Execute(_client, callback);

        if (result == false)
            Logger.Log($"{callback.From.Username} tried to execute {command.Id} but it's failed", LogSeverity.WARNING);

        
        return result;
    }

    public override void Register(object target)
    {
        var methods = FindMethodsWithAttribute<CallbackCommandAttribute>(target);

        foreach (var method in methods)
        {
            try
            {
                var command = method.CreateDelegate<ExecuteCallbackCommand>(target);
                var attr = method.GetCustomAttribute<CallbackCommandAttribute>();
                _commands.Add(new CallbackCommandInfo(attr.Id, command));
                Logger.Log($"Registered callback button {attr.Id} as {method.DeclaringType.FullName}.{method.Name}");
            }
            catch
            {
                Logger.Log($"{method.DeclaringType.FullName}.{method.Name} can't be callback button", LogSeverity.ERROR);
                continue;
            }
        }
    }
}
