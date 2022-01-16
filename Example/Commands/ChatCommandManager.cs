using Example.Logging;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Example.Commands;

internal delegate Task<bool> ExecuteChatCommand(ChatCommandContext context);

[AttributeUsage(AttributeTargets.Method)]
public class ChatCommandAttribute : Attribute
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool Hidden { get; private set; }

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
    public TelegramBotClient Client { get; private set; }
    public Message Message { get; private set; }
    public string[] Args { get; private set; }

    public ChatCommandContext(TelegramBotClient client, Message message)
    {
        Client = client;
        Message = message;
        var splitted = message.Text.Split(' ');
        Args = new string[splitted.Length - 1];
        Array.Copy(splitted, 1, Args, 0, splitted.Length - 1);
    }
}

internal class ChatCommandInfo
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

public class ChatCommandManager
{
    private readonly HashSet<ChatCommandInfo> _commands;
    private readonly TelegramBotClient _client;

    public ChatCommandManager(TelegramBotClient client)
    {
        _client = client;
        _commands = ResolveCommandMethods();
    }

    public bool TryExecuteCommand(string commandString, Message message)
    {
        commandString = commandString.TrimStart('/');
        var command = _commands.FirstOrDefault(cmd => string.Compare(cmd.Name, commandString, StringComparison.OrdinalIgnoreCase) == 0);

        if (command == default)
            return false;

        Logger.Log($"{message.From!.Username} executing command {command.Name}");
        var result = command.Execute(_client, message);

        if (result == false)
            Logger.Log($"{message.From.Username} tried to execute {command.Name} but it's failed", LogSeverity.WARNING);

        return result;
    }

    public BotCommand[] GetBotCommands()
    {
        var commands = _commands.Where(x => x.Hidden == false);
        var result = new BotCommand[commands.Count()];
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

    private static HashSet<ChatCommandInfo> ResolveCommandMethods()
    {
        var methodInfos =
            Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .SelectMany(type => type.GetMethods())
            .Where(method => method.GetCustomAttribute<ChatCommandAttribute>() != null);

        var result = new HashSet<ChatCommandInfo>();

        foreach (var method in methodInfos)
        {
            var command = Delegate.CreateDelegate(typeof(ExecuteChatCommand), method, false);
            if (command == null)
            {
                Logger.Log($"{method.DeclaringType.FullName}.{method.Name} can't be chat command", LogSeverity.ERROR);
                continue;
            }

            result.Add(new ChatCommandInfo((ExecuteChatCommand)command, method.GetCustomAttribute<ChatCommandAttribute>()!));
        }

        var hidden = result.Where(x => x.Hidden == true);
        Logger.Log($"Loaded {result.Count} commands: {string.Join(", ", result.Select(x => x.Name))}");
        Logger.Log($"{hidden.Count()} hidden commands: {string.Join(", ", hidden.Select(x => x.Name))}");
        return result;
    }
}

