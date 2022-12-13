using System.Text.RegularExpressions;
using Dalorian_Bot.DataBaseUtils;
using MySql.Data.MySqlClient;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Dalorian_Bot.Service;

public class Commands
{
    public static async void PrivateChatCommands(ITelegramBotClient botClient, Update update)
    {
        var message = update.Message;
        if (message.Text.ToLower() == "/start")
        {
            await botClient.SendTextMessageAsync(message.Chat, "Чего хотел, чужеземец? \n \n " +
                                                               "Я здесь не для того чтобы с тобой лясы точить, а чтобы следить за такими как ты. Либо ты меня нанимаешь к себе в группу, либо проваливай.");
        }
        else
        {
            await botClient.SendTextMessageAsync(message.Chat, "Я сказал все что посчитал нужным. Разговор окончен.");
        }
    }

    public static async void TopKarmaCommans(ITelegramBotClient botClient, Update update)
    {
        var message = update.Message;
        List<UserData> karmaUsers = DbUtils.GetAllKarma();
        var users = karmaUsers.OrderByDescending(x => x.Karma).ToArray();
        string topKarmaMessage = "";
        
        Console.WriteLine(karmaUsers.Count());
        
        if (karmaUsers.Count <= 10)
        {
            for (var i = 0;i+1 <= users.Count(); i++)
            {
                if (i + 1 <= users.Count())
                {
                    string FirstName;
                    string? lastName;
                    try
                    {
                        FirstName = Regex.Replace(
                            botClient.GetChatMemberAsync(message.Chat.Id, users[i].Id)
                                .Result
                                .User
                                .FirstName, "<&>", string.Empty);
                    }
                    catch (ArgumentNullException e)
                    {
                        FirstName = "";
                    }

                    try
                    {
                        lastName = Regex.Replace(botClient.GetChatMemberAsync(message.Chat.Id, users[i].Id)
                            .Result
                            .User
                            .LastName, "<&>", string.Empty);
                    }
                    catch (ArgumentNullException)
                    {
                        lastName = "";
                    }

                    var UserName = "";
                    if (lastName != null)
                    {
                        UserName = FirstName + " " + lastName;
                    }
                    else
                    {
                        UserName = FirstName;
                    }

                    topKarmaMessage +=
                        $"{(i + 1).ToString()}: <a href=\"tg://user?id={users[i].Id}\">{UserName}</a> - {users[i].Karma}\n";
                    Console.WriteLine(topKarmaMessage);
                }
                else
                {
                    break;
                }
            }

            await botClient.SendTextMessageAsync(message.Chat.Id, topKarmaMessage, parseMode: ParseMode.Html);
        }
        else
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Маловато-то пользователей для ТОП-а",
                parseMode: ParseMode.Html);
        }
    }

    public static async void MyKarmaCommand(ITelegramBotClient botClient, Update update)
    {
        var message = update.Message;
        if (DbUtils.checkUser(message.From.Id))
        {
            DbUtils.InsertUser(message.From.Id);
        }

        int karma = DbUtils.GetKarma(message.From.Id);
        if (karma != 0)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, $"Твоя карма: {karma}");
        }
    }

    public static async void AddKarmaCommand(ITelegramBotClient botClient, Update update)
    {
        var message = update.Message;
        if (message.ReplyToMessage != null)
        {
            if (message.ReplyToMessage.From.Id != botClient.GetMeAsync().Id)
            {
                if (message.ReplyToMessage.From.IsBot == false)
                {
                    if (message.ReplyToMessage.From.Id != message.From.Id)
                    {
                        MySqlConnection conn = DbUtils.GetDBConnection();
                        int karma = DbUtils.GetKarma(message.ReplyToMessage.From.Id);
                        int fromLast = DbUtils.GetDate(message.ReplyToMessage.From.Id);
                        /*Console.WriteLine(fromLast);*/

                        if (fromLast >= 15)
                        {
                            if (DbUtils.checkUser(message.ReplyToMessage.From.Id))
                            {
                                conn.Close();
                                DbUtils.InsertUser(message.ReplyToMessage.From.Id);
                            }

                            // (int)dateWithLastKarma.Seconds
                            if (karma != -1)
                            {
                                DbUtils.AddKarma(karma, message.ReplyToMessage.From.Id);
                            }

                            await botClient.SendTextMessageAsync(
                                message.Chat.Id,
                                $"+ 1 в карму <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\">{message.ReplyToMessage.From.FirstName}</a>",
                                parseMode: ParseMode.Html
                            );
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id,
                                "Эй-эй, не так быстро. Подожди немного, подумай.");
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id,
                            "Самолюбие хорошо в меру. Но тут ты маленько перегнул.");
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id,
                        "Бот не обломится, ему и без кармы хорошо живется.");
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Чужеземец, мне эта ваша карма не нужна, это я тут решаю, кому ее повышать, а кого закрыть в подвале и...");
            }
        }
        else
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "Так низзя. Нужно сначала ответить на сообщение.");
        }
    }
}