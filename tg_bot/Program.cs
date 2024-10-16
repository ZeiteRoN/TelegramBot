using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace tg_bot;

class Program
{
    static void Main(string[] args)
    {
        Host host = new Host("token");
        host.StartReceiving();
        Console.ReadLine();
    }
}
