using System.Reflection;
using Telegram.Bot;

namespace Example.Commands
{
    internal abstract class CommandManager<TCommandInfo>
    {
        protected readonly HashSet<TCommandInfo> _commands = new();
        protected readonly TelegramBotClient _client;

        public CommandManager(TelegramBotClient client)
        {
            _client = client;
        }
        public abstract void Register(object target);

        protected static List<MethodInfo> FindMethodsWithAttribute<TAttribute>(object target) where TAttribute : Attribute
        {
            return target.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => method.GetCustomAttribute<TAttribute>() != null).ToList();
        }
    }
}
