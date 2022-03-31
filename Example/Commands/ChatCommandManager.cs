using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Example.Commands;

public delegate void ExecutableChatCommand(ChatCommandContext context);

[AttributeUsage(AttributeTargets.Method)]
public class ChatCommandAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }
    public bool Hidden { get; }

    public ChatCommandAttribute(string name, string desc, bool hidden = false)
    {
        if (name == "" || desc == "")
            throw new ArgumentException("Name and description of command can't be empty");

        Name = name;
        Description = desc;
        Hidden = hidden;
    }
}

public class ChatCommandContext
{
    public TelegramBotClient Client { get; }
    public Message Message { get; }
    public string[] Args { get; }

    public ChatCommandContext(TelegramBotClient client, Message message)
    {
        Client = client;
        Message = message;
        Args = message.Text.Split(' ').Skip(1).ToArray();
    }
}

public class ChatCommandInfo
{
    public string Name => _attribute.Name;
    public string Description => _attribute.Description;
    public bool Hidden => _attribute.Hidden;

    private readonly ExecutableChatCommand _command;
    private readonly ChatCommandAttribute _attribute;

    public ChatCommandInfo(ExecutableChatCommand command, ChatCommandAttribute attribute)
    {
        _command = command;
        _attribute = attribute;
    }

    public void Execute(TelegramBotClient client, Message message) => _command(new ChatCommandContext(client, message));
}

public class ChatCommandManager : CommandManager<ChatCommandInfo>
{
    public ChatCommandManager(TelegramBotClient client) : base(client) { }

    public BotCommand[] GetBotCommands()
    {
        var commands = CommandDelegates.Where(x => x.Hidden == false).ToArray();
        var result = new BotCommand[commands.Length];
        var index = 0;

        foreach (var command in commands)
        {
            result[index++] = new BotCommand()
            {
                Command = command.Name,
                Description = command.Description
            };
        }

        return result;
    }

    public void Execute(string commandString, Message message)
    {
        if (TryGet(commandString, out var command) == false)
            return;

        Logger.Log($"{message.From.Username} executing command {command.Name}");
        command.Execute(Client, message);
    }

    public override void Register(object target)
    {
        var methods = FindMethodsWithAttribute<ChatCommandAttribute>(target.GetType());
        CreateDelegates(methods, target);
    }

    public void Register(ChatCommandAttribute attr, ExecutableChatCommand command)
    {
        Logger.Log($"Registered chat-command {attr?.Name} as {command.Target.GetType().FullName}.{command.GetMethodInfo().Name}" + (attr.Hidden ? " (hidden)" : ""));
        CommandDelegates.Add(new ChatCommandInfo(command, attr));
    }

    public void Register<T>() where T : class
    {
        var methods = FindMethodsWithAttribute<ChatCommandAttribute>(typeof(T))
            .Where(method => method.IsStatic)
            .ToList();
        CreateDelegates(methods, null);
    }

    private void CreateDelegates(List<MethodInfo> methods, object target = null)
    {
        foreach (var method in methods)
        {
            var commandDelegate = method.IsStatic
                ? Delegate.CreateDelegate(typeof(ExecutableChatCommand), method, false)
                : Delegate.CreateDelegate(typeof(ExecutableChatCommand), target, method, false);

            if (commandDelegate is not ExecutableChatCommand command)
            {
                Logger.Log($"{method.DeclaringType?.FullName}.{method.Name} can't be chat command", LogSeverity.Error);
                continue;
            }

            var attr = method.GetCustomAttribute<ChatCommandAttribute>();
            CommandDelegates.Add(new ChatCommandInfo(command, attr));
            Logger.Log($"Registered chat-command {attr?.Name} as {method.DeclaringType?.FullName}.{method.Name}" + (attr.Hidden ? " (hidden)" : ""));
        }
    }

    private bool TryGet(string commandString, out ChatCommandInfo info)
    {
        info = CommandDelegates.FirstOrDefault(command => command.Name == commandString);
        return info != null;
    }
}

