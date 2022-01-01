using Example.Logging;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Example.Commands;

public delegate void ExecuteChatCommand(TelegramBotClient client, Message message);

[AttributeUsage(AttributeTargets.Method)]
public class ChatCommandAttribute : Attribute
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    public ChatCommandAttribute(string name, string desc = "")
    {
        Name = name;
        Description = desc;
    }
}

internal class ChatCommandInfo
{
    public string Name => _attribute.Name;
    public string Description => _attribute.Description;


    private readonly ExecuteChatCommand _command;
    private readonly ChatCommandAttribute _attribute;

    public ChatCommandInfo(ExecuteChatCommand command, ChatCommandAttribute attribute)
    {
        _command = command;
        _attribute = attribute;
    }

    public async Task ExecuteAsync(TelegramBotClient client, Message message)
    {
        await Task.Run(() => _command(client, message));
    }
}

internal class ChatCommandProvider
{
    public char Prefix => _prefix;

    private readonly HashSet<ChatCommandInfo> _commands;
    private readonly char _prefix;
    private readonly TelegramBotClient _client;

    public ChatCommandProvider(TelegramBotClient client, char prefix)
    {
        _client = client;
        _prefix = prefix;
        _commands = ResolveCommandMethods();
    }

    public async Task<bool> TryExecuteCommandAsync(string name, Message message)
    {
        name = name.TrimStart(_prefix);
        var command = _commands.FirstOrDefault(cmd => string.Compare(cmd.Name, name, StringComparison.OrdinalIgnoreCase) == 0);

        if (command == null)
            return false;

        Logger.Log($"{message.From!.Username} executing command {command.Name}");
        await command.ExecuteAsync(_client, message);
        return true;
    }

    public BotCommand[] GetBotCommands()
    {
        var result = new BotCommand[_commands.Count];
        foreach (var command in _commands)
        {
            result[0] = new BotCommand()
            {
                Command = command.Name,
                Description = command.Description
            };
        }

        return result;
    }

    private HashSet<ChatCommandInfo> ResolveCommandMethods()
    {
        var methodInfos = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .SelectMany(type => type.GetMethods())
            .Where(method => method.GetCustomAttribute<ChatCommandAttribute>() != null);

        var result = new HashSet<ChatCommandInfo>();

        foreach (var method in methodInfos)
            result.Add(new ChatCommandInfo(method.CreateDelegate<ExecuteChatCommand>(), method.GetCustomAttribute<ChatCommandAttribute>()!));

        return result;
    }
}

