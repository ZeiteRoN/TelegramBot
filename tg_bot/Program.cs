using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace tg_bot;

class Program
{
    static void Main(string[] args)
    {
        Host host = new Host("7601772680:AAHuzkjiLCuGSptAGliW8kTGDtxxPM87aUo");
        host.StartReceiving();
        Console.ReadLine();
    }
}