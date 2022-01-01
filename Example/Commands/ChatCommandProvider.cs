using Example.Logging;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Example.Commands;

public delegate void ExecuteChatCommand(ChatCommandContext context);

[AttributeUsage(AttributeTargets.Method)]
public class ChatCommandAttribute : Attribute
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    public ChatCommandAttribute(string name, string desc)
    {
        if (name == "" || desc == "")
            throw new ArgumentException("Name and description of command can't be empty");

        Name = name;
        Description = desc;
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
        var splitted = message.Text!.Split(' ');
        Args = new string[splitted.Length - 1];
        Array.Copy(splitted, 1, Args, 0, splitted.Length - 1);
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
        await Task.Run(() => _command(new ChatCommandContext(client, message)));
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
        var index = 0;
        foreach (var command in _commands)
        {
            result[index++] = new BotCommand()
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

        Logger.Log($"Loaded {result.Count} commands: {string.Join(", ", result.Select(x => x.Name))}");
        return result;
    }
}

