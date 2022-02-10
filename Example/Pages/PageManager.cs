using Example.Commands;
using Example.Commands.CallbackButtons;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Example.Pages
{
    internal class Page
    {
        private static int Indexer;

        public List<string> Content { get; set; } = new();
        public int Id { get; private set; }
        public ParseMode ParseMode { get; private set; }

        public Page(ParseMode parseMode = ParseMode.Markdown)
        {
            Id = Indexer++;

            ParseMode = parseMode;
        }
    }

    internal class PageManager
    {
        private readonly LinkedList<Page> _pages = new();
        private LinkedListNode<Page> _current;
        private readonly int _pageAmount;

        private List<List<InlineKeyboardButton>> Buttons = new()
        {
            new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData("Тест", ButtonId.PageTest.ToButtonIdString())
            }
        };


        private readonly TelegramBotClient _client;

        public PageManager(TelegramBotClient client)
        {
            _client = client;

            for (int i = 1; i <= 6; i++)
            {
                var page = new Page()
                {
                    Content = new List<string>()
                    {
                        $"Сообщение 1. Страница {i}",
                        $"Сообщение 2. Страница {i}",
                        $"Сообщение 3. Страница {i}",
                        $"Сообщение 4. Страница {i}",
                    }
                };

                _pages.AddLast(page);
            }

            _current = _pages.First;
            _pageAmount = _pages.Count;
        }

        #region Commands

        [ChatCommand("pages", "Тест страниц")]
        public async Task<bool> ShowPages(ChatCommandContext context)
        {
            var count = _current.Value.Content.Count;
            for (var i = 0; i < count - 1; i++)
            {
                await _client.SendTextMessageAsync(context.Message.Chat.Id, "...");
            }

            var message = await _client.SendTextMessageAsync(context.Message.Chat.Id, "...");


            await GeneratePageView(message.Chat.Id, message.MessageId);
            return true;
        }

        [CallbackCommand(ButtonId.NextPage)]
        private async Task<bool> OnNextPressed(CallbackCommandContext context)
        {
            _ = _client.AnswerCallbackQueryAsync(context.Callback.Id);
            _current = FindById(context.Args[1].ToInt()).Next;
            await GeneratePageView(context.Callback.Message.Chat.Id, context.Callback.Message.MessageId);
            return true;
        }

        [CallbackCommand(ButtonId.PreviousPage)]
        private async Task<bool> OnPreviousPressed(CallbackCommandContext context)
        {
            _ = _client.AnswerCallbackQueryAsync(context.Callback.Id);
            _current = FindById(context.Args[1].ToInt()).Previous;
            await GeneratePageView(context.Callback.Message.Chat.Id, context.Callback.Message.MessageId);
            return true;
        }

        [CallbackCommand(ButtonId.PageTest)]
        private Task<bool> OnPageTestPressed(CallbackCommandContext context)
        {
            _ = _client.AnswerCallbackQueryAsync(context.Callback.Id);
            return Task.FromResult(true);
        }

        [CallbackCommand(ButtonId.PageNumber)]
        private Task<bool> OnPageNumberPressed(CallbackCommandContext context)
        {
            _ = _client.AnswerCallbackQueryAsync(context.Callback.Id, $"Страница {_current.Value.Id + 1}/{_pageAmount}");
            return Task.FromResult(true);
        }
        #endregion

        private LinkedListNode<Page> FindById(int id)
        {
            var page = _pages.Where(page => page.Id == id).First();
            return _pages.Find(page);
        }

        private InlineKeyboardMarkup GetControlsMarkup()
        {
            var result = new List<List<InlineKeyboardButton>>(Buttons);
            var controlButtons = new List<InlineKeyboardButton>();
            if (_current.Previous != null)
                controlButtons.Add(InlineKeyboardButton.WithCallbackData("«", $"{ButtonId.PreviousPage.ToButtonIdString()};{_current.Value.Id}"));

            controlButtons.Add(InlineKeyboardButton.WithCallbackData((_current.Value.Id + 1).ToString(), ButtonId.PageNumber.ToButtonIdString()));

            if (_current.Next != null)
                controlButtons.Add(InlineKeyboardButton.WithCallbackData("»", $"{ButtonId.NextPage.ToButtonIdString()};{_current.Value.Id}"));

            result.Add(controlButtons);

            return new InlineKeyboardMarkup(result);
        }

        private async Task GeneratePageView(ChatId chatId, int lastMessageId)
        {
            var current = _current.Value;
            var messageId = lastMessageId - current.Content.Count + 1;
            var count = current.Content.Count;
            for (var i = 0; i < count - 1; i++)
            {
                await _client.EditMessageTextAsync(
                    chatId,
                    messageId++,
                    current.Content[i],
                    current.ParseMode,
                    replyMarkup: new InlineKeyboardMarkup(Buttons));
            }

            await _client.EditMessageTextAsync(chatId,
                lastMessageId,
                current.Content.Last(),
                current.ParseMode,
                replyMarkup: GetControlsMarkup());
        }
    }
}
