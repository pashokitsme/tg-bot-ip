using Example.Logging;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Example.Commands;

public delegate Task<bool> ExecuteChatCommand(ChatCommandContext context);

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
        var splitted = message.Text.Split(' ');
        Args = new string[splitted.Length - 1];
        Array.Copy(splitted, 1, Args, 0, splitted.Length - 1);
    }
}

public class ChatCommandInfo
{
    public string Name => _attribute.Name;
    public string Description => _attribute.Description;
    public bool Hidden => _attribute.Hidden;

    private readonly ExecuteChatCommand _command;
    private readonly ChatCommandAttribute _attribute;

    public ChatCommandInfo(ExecuteChatCommand command, ChatCommandAttribute attribute)
    {
        _command = command;
        _attribute = attribute;
    }

    public bool Execute(TelegramBotClient client, Message message)
    {
        return _command(new ChatCommandContext(client, message)).GetAwaiter().GetResult();
    }
}

public class ChatCommandManager : CommandManager<ChatCommandInfo>
{
    public ChatCommandManager(TelegramBotClient client) : base(client)
    {
        GetBotCommands();
    }

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

    private ChatCommandInfo Get(string commandString)
    {
        commandString = commandString.TrimStart('/');
        return CommandDelegates.FirstOrDefault(cmd => string.Compare(cmd.Name, commandString, StringComparison.OrdinalIgnoreCase) == 0);
    }

    public bool TryExecute(string commandString, Message message)
    {
        var command = Get(commandString);

        if (command == default)
            return false;

        Logger.Log($"{message.From.Username} executing command {command.Name}");
        var result = command.Execute(Client, message);

        if (result == false)
            Logger.Log($"{message.From.Username} tried to execute {command.Name} but it's failed", LogSeverity.Warning);

        return result;
    }

    public override void Register(object target)
    {
        var methods = FindMethodsWithAttribute<ChatCommandAttribute>(target);

        foreach (var method in methods)
        {
            var commandDelegate = method.IsStatic
                ? Delegate.CreateDelegate(typeof(ExecuteChatCommand), method, false)
                : Delegate.CreateDelegate(typeof(ExecuteChatCommand), target, method, false);
            
            if (commandDelegate is not ExecuteChatCommand command)
            {
                Logger.Log($"{method.DeclaringType?.FullName}.{method.Name} can't be chat command", LogSeverity.Error);
                continue;
            }
            
            var attr = method.GetCustomAttribute<ChatCommandAttribute>();
            CommandDelegates.Add(new ChatCommandInfo(command, attr));
            Logger.Log($"Registered chatcommand {attr?.Name} as {method.DeclaringType?.FullName}.{method.Name}" + (attr.Hidden ? " (hidden)" : ""));
        }
    }
}

