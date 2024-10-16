using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace tg_bot;

public class Host
{
    private TelegramBotClient _client;

    public Dictionary<string, string> LangPack_en = new Dictionary<string, string>
    {
        { "Greeting", "Hello, User" },
    };

    public Dictionary<string, string> LangPack_ua = new Dictionary<string, string>
    {
        { "Greeting", "Привіт, користувач" },
    };
    private Dictionary<string, string> _choosedLangPack;
    
    public Host(string token)
    {
        _client = new TelegramBotClient(token);
        _choosedLangPack = LangPack_en;
    }

    public void StartReceiving()
    {
        _client.StartReceiving(UpdateHandler, ErrorHandler);
        Console.WriteLine("Host was started!");
    }

    private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
    {
        Console.WriteLine(update.Message?.Text);
        if (update.Message != null)
        {
            if (update.Message.Text == "/start")
            {
                client.SendTextMessageAsync(update.Message.Chat.Id, _choosedLangPack["Greeting"], replyToMessageId: update.Message.MessageId);
            }
            if (update.Message.Text == "/switch_language")
            {
                _choosedLangPack = LangPack_ua;
                client.SendTextMessageAsync(update.Message.Chat.Id, "Мову було змінено!", replyToMessageId: update.Message.MessageId);
            }
        }
        await Task.CompletedTask;
    }
    
    private async Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        Console.WriteLine("Error:" + exception.Message);
        await Task.CompletedTask;
    }
}