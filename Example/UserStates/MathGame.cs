using Example.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Example.UserStates;

public class MathGame : UserState
{
    private enum MathOperation
    {
        Add,
        Subtract,
        Multiply
    }

    private struct MathProblem
    {
        public int First;
        public int Second;
        public int Answer;
        public MathOperation Operation;


        public static MathProblem Create()
        {
            var operation = (MathOperation)Random.Shared.Next(0, 3);
            var first = 0;
            var second = 0;
            var answer = 0;
            switch (operation)
            {
                case MathOperation.Add:
                    first = Random.Shared.Next(5, 500);
                    second = Random.Shared.Next(5, 500);
                    answer = first + second;
                    break;
                case MathOperation.Subtract:
                    first = Random.Shared.Next(5, 500);
                    second = Random.Shared.Next(5, first);
                    answer = first - second;
                    break;

                case MathOperation.Multiply:
                    first = Random.Shared.Next(2, 100);
                    second = Random.Shared.Next(2, 15);
                    answer = first * second;
                    break;
            }

            return new MathProblem()
            {
                First = first,
                Second = second,
                Operation = operation,
                Answer = answer
            };
        }

        public bool Solve(int answer) => Answer == answer;
    }

    private MathProblem _current;
    private int _solved;

    public override void Enter(UserStateManager manager, long userId, TelegramBotClient client)
    {
        base.Enter(manager, userId, client);
        Next();
    }

    public override void Exit()
    {
        Client.SendTextMessageAsync(UserId, $"Решено: `{_solved}`", ParseMode.Markdown);
    }

    public override void Update(Message message)
    {
        if (int.TryParse(message.Text, out var answer) == false)
        {
            Client.SendTextMessageAsync(UserId, "В ответе должно быть `целое число`", ParseMode.Markdown);
            return;
        }

        if (_current.Solve(answer) == false)
        {
            Client.SendTextMessageAsync(UserId, "Ответ неверный");
            return;
        }

        _solved++;
        Next();
    }

    private void Next()
    {
        _current = MathProblem.Create();

        var operation = _current.Operation switch
        {
            MathOperation.Add => '+',
            MathOperation.Subtract => '-',
            MathOperation.Multiply => '*',
            _ => throw new NotImplementedException(),
        };

        Client.SendTextMessageAsync(UserId, $"{_current.First} {operation} {_current.Second} = `?`", ParseMode.Markdown);
    }
}
