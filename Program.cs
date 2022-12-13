using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Dalorian_Bot.DataBaseUtils;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;
using Tomlyn;
using Dalorian_Bot.Service;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Dalorian_Bot
{

    class Program
    {
        static ITelegramBotClient bot = new TelegramBotClient("");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                -784948334,
                Newtonsoft.Json.JsonConvert.SerializeObject(update, Formatting.Indented));
            /*DbUtils.CreateTableOrNo();*/
            if(update.Type == UpdateType.Message)
            {
                var message = update.Message;
                
                if (message.Text == "/mykarma" || message.Text == "/mykarma@" + bot.GetMeAsync().Result.Username)
                {
                    Commands.MyKarmaCommand(botClient, update);
                }
                
                if (message.Text == "/topkarma" || message.Text == "/topkarma@" + bot.GetMeAsync().Result.Username)
                {
                    Commands.TopKarmaCommans(botClient, update);
                }
                
                if (message.Text == "+" /*|| message.Text.Contains("Спасибо") != null || message.Text.Contains("Спс")  ||
                    message.Text.Contains("Сяп") || message.Text.Contains("От души")*/)
                {
                    Commands.AddKarmaCommand(botClient, update);
                }


                if (message.Chat.Type == ChatType.Private)
                {
                    Commands.PrivateChatCommands(botClient, update);
                }

                /*foreach (var VARIABLE in message.NewChatMembers)
                {
                    
                }*/

                if (message.NewChatMembers != null)
                {
                    bool isIam = false;

                    foreach (var user in message.NewChatMembers)
                    {
                        if (user.Id == botClient.GetMeAsync().Id)
                        {
                            isIam = true;
                        }
                    }
                    
                    if (message.Chat.Id != -784948334 || isIam)
                    {
                        await botClient.SendTextMessageAsync(
                            message.Chat,
                            "Здарова кожаные. Я теперь это. Вобщем. Буду вам карму считать. Но я постепенно учусь," +
                            " и скоро буду уметь многое. Но пока что я сяду, и буду тихо пить чай, наблюдая за вами..." +
                            "\n \n P.S. Вы это, админа мне выдайте, а то я ваших сообщений не вижу.");
                    }
                    else if(message.Chat.Id == -784948334 && isIam)
                    {
                        await botClient.SendTextMessageAsync(
                            message.Chat,
                            "Я успешно добавлен в чат лога.");
                    }
                }
            }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(exception);
        }


        static void Main(string[] args)
        {
            if (!File.Exists("config-dal.toml"))
            {
                Console.WriteLine("OselBot is a Telegram admin bot. It is needed to prohibit sending messages on behalf of the channel, as well as to karma users\nA token is necessary for the work of the bot. To get the token, you need to create a bot here: @BotFather\nThe mysql database is also required for work.");
                Console.Write("Please enter the token from the bot: ");
                string tokenBot = Console.ReadLine();
                Console.Write("\nSpecify the address to the database: ");
                string addressDatabase = Console.ReadLine();
                Console.Write("Specify the name of the database: ");
                string nameDatabase = Console.ReadLine();
                Console.Write("Now the username: ");
                string nameUserDatabase = Console.ReadLine();
                Console.Write("And the password: ");
                string passwordUserDatabase = Console.ReadLine();
                
                using (FileStream fstream = new FileStream("config-dal.toml", FileMode.OpenOrCreate))
                {
                    string configText =
                            String.Format("botToken = '{0}'\naddressDatabase = '{1}'\nnameDatabase = '{2}'\nnameUserDatabase = '{3}'\npasswordUserDatabase = '{4}'",
                        tokenBot,
                        addressDatabase,
                        nameDatabase,
                        nameUserDatabase,
                        passwordUserDatabase);
                    Console.WriteLine(configText);
                    byte[] buffer = Encoding.Default.GetBytes(configText);
                    fstream.Write(buffer, 0, buffer.Length);
                }
            }
            using (FileStream fstream = File.OpenRead("config-dal.toml"))
            {
                byte[] buffer = new byte[fstream.Length];
                fstream.Read(buffer, 0, buffer.Length);
                string textFromFile = Encoding.Default.GetString(buffer);

                var model = Toml.ToModel(textFromFile);
                bot = new TelegramBotClient((string) model["botToken"]!);;
            }


            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = {}, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
}