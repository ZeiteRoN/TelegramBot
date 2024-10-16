using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace tg_bot;

public class Host
{
    private TelegramBotClient _client;
    
    private Dictionary<string, string> englishPack;
    private Dictionary<string, string> ukrainianPack;
    private string language = "English";
    

    private Dictionary<string, string> _choosenLangPack;
    
    public Host(string token)
    {
        _client = new TelegramBotClient(token);
        string jsonContent = System.IO.File.ReadAllText(@"C:\Users\stars\RiderProjects\tg_bot\tg_bot\Resources\languages.json");
        var allLanguages = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonContent);
        allLanguages.TryGetValue("english", out englishPack);
        allLanguages.TryGetValue("ukrainian", out ukrainianPack);
        _choosenLangPack = englishPack;
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
            switch (update.Message?.Text)
            {
                case "/start":
                {
                    startKeyboard(update.Message.Chat.Id);
                    break;
                }
                case "Language":
                {
                    languageKeyboard(update.Message.Chat.Id);
                    break;
                }
                case "Ukrainian":
                {
                    _choosenLangPack = ukrainianPack;
                    language = "Ukrainian";
                    client.SendTextMessageAsync(update.Message.Chat.Id, _choosenLangPack["LangSwitch"], replyToMessageId: update.Message.MessageId);
                    break;
                }
                case "English":
                {
                    _choosenLangPack = englishPack;
                    language = "English";
                    client.SendTextMessageAsync(update.Message.Chat.Id, _choosenLangPack["LangSwitch"], replyToMessageId: update.Message.MessageId);
                    break;
                }
                case "Contacts":
                {
                    client.SendTextMessageAsync(update.Message.Chat.Id, _choosenLangPack["Contacts"], replyToMessageId: update.Message.MessageId);
                    startKeyboard(update.Message.Chat.Id);
                    break;
                }
                case "Back":
                {
                    startKeyboard(update.Message.Chat.Id);
                    break;
                }
            }
            
            if (update.Message.Text == "/switch_language")
            {
                if (language == "English")
                {
                    _choosenLangPack = ukrainianPack;
                    language = "Ukrainian";
                } 
                else if (language == "Ukrainian")
                {
                    _choosenLangPack = englishPack;
                    language = "English";
                }
                client.SendTextMessageAsync(update.Message.Chat.Id, _choosenLangPack["LangSwitch"], replyToMessageId: update.Message.MessageId);
            }
        }
        await Task.CompletedTask;
    }
    
    private async Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        Console.WriteLine("Error:" + exception.Message);
        await Task.CompletedTask;
    }

    private async Task startKeyboard(long chatId)
    {
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Order" },
            new KeyboardButton[] { "Language" },
            new KeyboardButton[] { "Contacts" }

        })
        {
            ResizeKeyboard = true
        };
        await _client.SendTextMessageAsync(
            chatId: chatId,
            text:_choosenLangPack["Greeting"],
            replyMarkup: keyboard
        );
    }

    private async Task languageKeyboard(long chatId)
    {
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "English" },
            new KeyboardButton[] { "Ukrainian" },
            new KeyboardButton[] { "Back" }
        })
        {
            ResizeKeyboard = true
        };
        await _client.SendTextMessageAsync(
            chatId: chatId,
            text:_choosenLangPack["langOptions"],
            replyMarkup: keyboard
        );
    }
}