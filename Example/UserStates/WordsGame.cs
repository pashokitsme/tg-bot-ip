using Example.Commands;
using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Example.UserStates;

public class WordsGame : UserState
{
    #region static

    private static readonly Dictionary<char, List<KeyValuePair<string, string>>> _words = new();
    private static readonly char[] _blackList = new char[]
    {
        'ь',
        'ъ',
        'ы'
    };

    private struct WordDefinition
    {
        public string Word;
        public string Definition;

        public WordDefinition(string word, string def) : this()
        {
            Word = word;
            Definition = def;
        }
    }

    static WordsGame()
    {
        using var reader = new StreamReader("nouns_def.txt");
        while (reader.EndOfStream == false)
        {
            var element = reader.ReadLine().Split(':');

            if (_words.ContainsKey(element[0][0]) == false)
                _words[element[0][0]] = new();

            _words[element[0][0]].Add(new KeyValuePair<string, string>(element[0], string.Concat(element.Skip(1))));
        }
        reader.Close();
    }

    #endregion

    private HashSet<string> _used;
    private string _last;
    private char _letter;

    public override async Task Enter(UserStateManager manager, long userId, TelegramBotClient client)
    {
        _ = base.Enter(manager, userId, client);
        await Client.SendTextMessageAsync(userId, "Для выхода нужно ввести команду /words ещё раз, а для того, чтобы узнать значение слова, нужно отправить `?`", ParseMode.Markdown);
        _used = new();
        await NextWord("а");
    }

    public override async Task Exit() => await Client.SendTextMessageAsync(UserId, "Игра в слова закончена");

    public override async void Update(Message message)
    {
        if (message == null || string.IsNullOrEmpty(message.Text) || message.Text[0] == '/')
            return;

        var word = message.Text.Split(' ')[0].ToLowerInvariant();

        if (word == "?")
        {
            await ReturnDefintion();
            return;
        }

        if (word[0] != _letter)
        {
            await Client.SendTextMessageAsync(UserId, $"Слово должно быть на букву `{_letter}`", ParseMode.Markdown);
            return;
        }

        if (_used.Contains(word))
        {
            await Client.SendTextMessageAsync(UserId, $"Слово `{word}` уже было использовано", ParseMode.Markdown);
            return;
        }

        if (_words[word[0]].Select(x => x.Key).Contains(word) == false)
        {
            await Client.SendTextMessageAsync(UserId, $"Я не знаю слово `{word}`", ParseMode.Markdown);
            return;
        }

        await NextWord(word);
    }

    private async Task ReturnDefintion()
    {
        var def = _words[_last[0]].FirstOrDefault(x => x.Key == _last).Value;
        if (def == null)
        {
            await Client.SendTextMessageAsync(UserId, $"Я не знаю");
            return;
        }

        await Client.SendTextMessageAsync(UserId, $"Значение слова<code> {_last} </code>- {def}", ParseMode.Html);
    }

    private async Task NextWord(string word)
    {
        var next = GetNotUsedWord(GetLastValideLetter(word));
        _letter = GetLastValideLetter(next);

        _used.Add(word);
        _used.Add(next);

        await Client.SendTextMessageAsync(UserId, $"Следующее слово `{next}`", ParseMode.Markdown);
        Logger.Log($"WordsGame[user {UserId}] update: {word} -> {next}");
    }

    private static char GetLastValideLetter(string word)
    {
        var chars = word.ToCharArray();
        for (var i = chars.Length - 1; i >= 0; i--)
        {
            if (_blackList.Contains(chars[i]))
                continue;

            return chars[i];
        }
        return chars[0];
    }

    private string GetNotUsedWord(char letter)
    {
        var word = _words[letter][Random.Shared.Next(0, _words[letter].Count)].Key;
        while (_used.Contains(word))
            word = _words[letter][Random.Shared.Next(0, _words[letter].Count)].Key;

        _last = word;
        return word;
    }
}
