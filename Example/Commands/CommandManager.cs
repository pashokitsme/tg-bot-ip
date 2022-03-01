using System.Reflection;
using Telegram.Bot;

namespace Example.Commands
{
    public abstract class CommandManager<TCommandInfo>
    {
        protected readonly HashSet<TCommandInfo> CommandDelegates = new();
        protected readonly TelegramBotClient Client;

        protected CommandManager(TelegramBotClient client) => Client = client;

        public abstract void Register(object target);
        
        protected static List<MethodInfo> FindMethodsWithAttribute<TAttribute>(object target) where TAttribute : Attribute
        {
            return target.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static)
                .Where(method => method.GetCustomAttribute<TAttribute>() != null).ToList();
        }
    }
}
