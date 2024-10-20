using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace tg_bot;

public class Host
{
    private TelegramBotClient _client;
    
    private static readonly string baseUrl = "http://api.openweathermap.org/data/2.5/weather";
    
    private Dictionary<string, string> englishPack;
    private Dictionary<string, string> ukrainianPack;
    private string _language = "English";
    private string _location = "Kyiv";
    private string _weatherAPI;
    

    private Dictionary<string, string> _choosenLangPack;
    private bool _waiting = false;
    private bool _recieving = true;
    
    public Host(string token, string weatherAPI)
    {
        _client = new TelegramBotClient(token);
        string jsonContent = System.IO.File.ReadAllText(@"C:\Users\stars\RiderProjects\tg_bot\tg_bot\Resources\languages.json");
        var allLanguages = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonContent);
        allLanguages.TryGetValue("english", out englishPack);
        allLanguages.TryGetValue("ukrainian", out ukrainianPack);
        _choosenLangPack = englishPack;
        _weatherAPI = weatherAPI;
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
            if(_recieving)
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
                        _language = "Ukrainian";
                        client.SendTextMessageAsync(update.Message.Chat.Id, _choosenLangPack["LangSwitch"],
                            replyToMessageId: update.Message.MessageId);
                        break;
                    }
                    case "English":
                    {
                        _choosenLangPack = englishPack;
                        _language = "English";
                        client.SendTextMessageAsync(update.Message.Chat.Id, _choosenLangPack["LangSwitch"],
                            replyToMessageId: update.Message.MessageId);
                        break;
                    }
                    case "Contacts":
                    {
                        client.SendTextMessageAsync(update.Message.Chat.Id, _choosenLangPack["Contacts"],
                            replyToMessageId: update.Message.MessageId);
                        startKeyboard(update.Message.Chat.Id);
                        Console.WriteLine();
                        break;
                    }
                    case "Back":
                    {
                        startKeyboard(update.Message.Chat.Id);
                        break;
                    }
                    case "Weather":
                    {
                        weatherKeyboard(update.Message.Chat.Id);
                        break;
                    }
                    case "Change location":
                    {
                        client.SendTextMessageAsync(update.Message.Chat.Id, _choosenLangPack["locationChanging"],
                            replyToMessageId: update.Message.MessageId);
                        _recieving = false;
                        _waiting = true;
                        break;
                    }
                    case "My city":
                    {
                        var weatherData = await GetWeatherAsync(_location);
                        client.SendTextMessageAsync(update.Message.Chat.Id, $"Weather in {_location}:\nTemp:{weatherData.Main.Temp}\nWeather:{weatherData.Weather[0].Description}",
                            replyToMessageId: update.Message.MessageId);
                        break;
                    }
                        
                }
                if (update.Message.Text == "/switch_language")
                {
                    if (_language == "English")
                    {
                        _choosenLangPack = ukrainianPack;
                        _language = "Ukrainian";
                    } 
                    else if (_language == "Ukrainian")
                    {
                        _choosenLangPack = englishPack;
                        _language = "English";
                    }
                    client.SendTextMessageAsync(update.Message.Chat.Id, _choosenLangPack["LangSwitch"], replyToMessageId: update.Message.MessageId);
                }
            }
            else if (_waiting)
            {
                _location = update.Message.Text;
                _recieving = true;
                _waiting = false;
                client.SendTextMessageAsync(update.Message.Chat.Id, _choosenLangPack["locationChanged"], replyToMessageId: update.Message.MessageId);
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
            new KeyboardButton[] { "Weather" },
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
    private async Task weatherKeyboard(long chatId)
    {
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "My city" },
            new KeyboardButton[] { "Change location" },
            new KeyboardButton[] { "Back" }
        })
        {
            ResizeKeyboard = true
        };
        await _client.SendTextMessageAsync(
            chatId: chatId,
            text:_choosenLangPack["weatherMenu"],
            replyMarkup: keyboard
        );
    }
    
    public async Task<WeatherResponse> GetWeatherAsync(string city)
    {
        using (HttpClient client = new HttpClient())
        {
            string requestUrl = $"{baseUrl}?q={city}&appid={_weatherAPI}&units=metric";
            HttpResponseMessage response = await client.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<WeatherResponse>(json);
            }
            else
            {
                Console.WriteLine($"API Error: {response.ReasonPhrase}");
                return null;
            }
        }
    }
}

public class WeatherResponse
{
    public MainInfo Main { get; set; }
    public WeatherInfo[] Weather { get; set; }
}

public class MainInfo
{
    public double Temp { get; set; }
}

public class WeatherInfo
{
    public string Description { get; set; }
}