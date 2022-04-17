using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Example.UserStates;

public class MultiplyTableGame : UserState
{
    private struct MathProblem
    {
        public int First;
        public int Second;
        public int Answer;

        public static MathProblem Create()
        {
            var first = 0;
            var second = 0;
            var answer = 0;

            first = Random.Shared.Next(1, 10);
            second = Random.Shared.Next(1, 10);
            answer = first * second;

            return new MathProblem
            {
                First = first,
                Second = second,
                Answer = answer
            };
        }

        public bool Solve(int answer) => Answer == answer;
    }

    private MathProblem _current;
    private int _solved;

    public override async Task Enter(UserStateManager manager, long userId, TelegramBotClient client)
    {
        _ = base.Enter(manager, userId, client);
        await Client.SendTextMessageAsync(userId, $"Для выхода введи /multiplytable или /stop");
        await NextProblem();
    }

    public override async Task Exit() => await Client.SendTextMessageAsync(UserId, $"Игра закончена. Решено: `{_solved}`", ParseMode.Markdown);

    public override async void Update(Message message)
    {
        if (int.TryParse(message.Text, out var answer) == false)
        {
            await Client.SendTextMessageAsync(UserId, "В ответе должно быть `целое число`", ParseMode.Markdown);
            return;
        }

        if (_current.Solve(answer) == false)
        {
            await Client.SendTextMessageAsync(UserId, "Ответ неверный 😥");
            return;
        }

        _solved++;
        await NextProblem();
    }

    private async Task NextProblem()
    {
        _current = MathProblem.Create();
        await Client.SendTextMessageAsync(UserId, $"{_current.First} * {_current.Second} = <code>?</code>", ParseMode.Html);
    }
}

