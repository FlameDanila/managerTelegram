using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Data;
using System.IO;
using System;
using System.Threading;
using Telegram.Bot.Types.ReplyMarkups;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Formats.Asn1;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Net;
using System.Runtime.CompilerServices;

var botClient = new TelegramBotClient("5183249647:AAHCx42xlNoIEZ51EXA2qo0lJe0e4mp_J4M");

using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};
botClient.StartReceiving(
    updateHandler: HandleUpdatesAsync,
    pollingErrorHandler: HandleErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

Timer _timer = null;

void TimerCallback(Object o)
{
    Console.WriteLine("In TimerCallback: " + DateTime.Now);
}

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
//cts.Cancel();

async Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Type == UpdateType.Message && update?.Message?.Text != null)
    {
        await HandleMessage(botClient, update.Message);
        return;
    }
    if (update.Type == UpdateType.CallbackQuery)
    {
        await HandleCallbackQuery(botClient, update.CallbackQuery);
        return;
    }
    if (update.Message != null)
    {
        if (update.Message.Type == MessageType.Contact)
        {
            await GetContactAsync(botClient, MessageType.Contact, update.Message);
            return;
        }
    }
}

async Task HandleMessage(ITelegramBotClient botClient, Message message)
{
    var username = message.Chat.Username;
    var name = message.Chat.FirstName + " " + message.Chat.LastName;
    var telegramId = message.From.Id;
    var chatId = message.Chat.Id;
    var ss = message.Text;

    string requestUserName = "Пусто";

    DataTable user = await Select($"select * from usersDelfaTelegram where chatId like '{chatId}'");
    string Id1c = "";
    string block = "";
    string counter = "";

    if (user.Rows.Count != 0)
    {
        block = user.Rows[0][8].ToString();
        counter = user.Rows[0][9].ToString();
        Id1c = user.Rows[0][12].ToString();
    }

    DataTable kids = await Select($"select * from uucKidsTelegram where accountableId like '{Id1c}'");
    ReplyKeyboardMarkup shareContact = new(new[] { KeyboardButton.WithRequestContact("➡Отправить контакт⬅") });

    ReplyKeyboardMarkup authorize = new(new[]
    {
       new KeyboardButton [] { "Оценки 𝟝",  "Расписание 📅"},
       new KeyboardButton [] { "Рассылка ✉", "Меню курсов" }
    })
    { ResizeKeyboard = true };

    InlineKeyboardMarkup mainMenu = new(new[]
    {
        // first row
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Меню курсов", callbackData: "menuCourses"),
            InlineKeyboardButton.WithCallbackData(text: "Авторизация", callbackData: "authoriseInline"),
        },
    });

    InlineKeyboardMarkup inlNotifications = new(new[]
    {
        // first row
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Да", callbackData: "notificationsYes"),
            InlineKeyboardButton.WithCallbackData(text: "Нет", callbackData: "notificationsNo"),
        },
    });
    InlineKeyboardMarkup inlManagerCall = new(new[]
    {
        // first row
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Да, хочу", callbackData: "yes"),
            InlineKeyboardButton.WithCallbackData(text: "Нет, я просто смотрю", callbackData: "no"),
        },
    });
    ReplyKeyboardRemove hide = new ReplyKeyboardRemove();
    //var url = "https://api.telegram.org/bot5183249647:AAHCx42xlNoIEZ51EXA2qo0lJe0e4mp_J4M/sendMessage?chat_id=995734455^&text=123";

    //var request = WebRequest.Create(url);
    //request.Method = "GET";

    //using var webResponse = request.GetResponse();
    //using var webStream = webResponse.GetResponseStream();

    //using var reader = new StreamReader(webStream);
    //var data = reader.ReadToEnd();

    //Console.WriteLine(data);
    //await botClient.SendTextMessageAsync(chatId, "https://t.me/Flame_chanel \n\nНапишите омне, если встретите ошибку");
    try
    {
        if (ss != "")
        {
            if (user.Rows.Count != 0) // Логи
            {
                FileStream notificationsStream = new FileStream(@"C://Users/Администратор/Desktop/Projects/messages.txt", FileMode.Append);

                string sMessage = "message = " + ss + "\n" + DateTime.Now + "\n" + chatId + "\n" + username + "\nblock - " + user.Rows[0][8] + "\tCounter - " + user.Rows[0][9] + "\n\n";
                StreamWriter streamWriter = new StreamWriter(notificationsStream);
                streamWriter.Write(sMessage);
                streamWriter.Close();
                notificationsStream.Close();
            }
        }
        if (ss.ToLower() == "/start" || ss.ToLower() == "старт" || ss.ToLower() == "start" || ss.ToLower() == "ыефке" || ss.ToLower() == "cnfhn" || ss.ToLower() == "/старт")
        {
            if (user.Rows.Count != 0)
            {
                if (user.Rows[0][4].ToString() != "1")
                {
                    await botClient.SendTextMessageAsync(chatId, $"Добро пожаловать, {message.Chat.FirstName} {message.Chat.LastName}",
                        replyMarkup: authorize);

                    await Select($"update usersDelfaTelegram set block = 2 where chatId = '{chatId}';" +
                    $" update usersDelfaTelegram set counter = 1 where chatId = '{chatId}'");
                    
                    if (user.Rows[0][13].ToString() == "False" && user.Rows[0][12].ToString() != "") // Создание исключения из рассылки смс (Готово)
                    {
                        FileStream notificationsStream = new FileStream(@"//SERVER-1C/data1c/notifications/notifications.txt", FileMode.Append);
                        StreamWriter streamWriter = new StreamWriter(notificationsStream);
                        DataTable notificationsCode = await Select($"select code from usersDelfaTelegram where chatId = '{chatId}'");
                        streamWriter.WriteLine(notificationsCode.Rows[0][0].ToString() + ";" + chatId + ";");
                        streamWriter.Close();
                        notificationsStream.Close();
                        await Select($"update usersdelfatelegram set notifications = 'true' where chatId = {chatId}");
                        await botClient.SendTextMessageAsync(chatId, "Отличные новости\\! \n\nТеперь, все уведомления, включая оценки, будут приходить Вам _*только*_ в телеграм\\.", replyMarkup: authorize, parseMode: ParseMode.MarkdownV2);
                    }
                    
                }
            }
            else 
            {
                await botClient.SendTextMessageAsync(chatId, "Привет 👋🏽\r\nЯ, автоматизированный помощник учебного центра \"Дельфа\" \U0001f9be\r\n", replyMarkup: hide);
                await botClient.SendTextMessageAsync(chatId, "\r\nЕсли Вы являетесь нашим клиентом или хотите зарегистрироваться, нажмите:  _*Авторизация*_ \r\n\nЕсли Вы хотите ознакомиться с перечнем курсов, нажмите: _*Меню курсов*_", replyMarkup: mainMenu, parseMode: ParseMode.MarkdownV2);

                await Select($"update usersDelfaTelegram set block = 0 where chatId = '{chatId}';" +
                $" update usersDelfaTelegram set counter = 0 where chatId = '{chatId}'");
            }
        }

            //MatchCollection regex = Regex.Matches(text2, ";?.*(\\s?)");

        if (ss == "Рассылка ✉")
        {
            await botClient.SendTextMessageAsync(chatId, "Вы хотите отказаться от рассылки?", replyMarkup: inlNotifications);
        }
        if (ss == "Оценки 𝟝")
        {
            string group = user.Rows[0][16].ToString();

            string[] strings = Directory.GetFiles("\\\\SERVER-1C\\data1c\\marks\\period");

            if (group != "")
            {
                foreach (string s in strings) // Это группы
                {
                    if (s.Contains(group)) // Проверка на наличие группы юзера в папке
                    {
                        string[] sMass = s.Split("Оценки ");
                        sMass[1] = sMass[1].Replace(".txt", "");

                        StreamReader streamReader = new StreamReader(s); // Достаём нужные оценки
                        string marks = streamReader.ReadToEnd();
                        streamReader.Close();

                        string[] marksOfUser = marks.Split("\n");

                        foreach (string mark in marksOfUser) // Делим оценки на столбцы
                        {
                            if (mark.Contains(user.Rows[0][12].ToString())) // Проверяем есть ли у этого юзера оценки в группе
                            {
                                string[] splittedMark = mark.Split(";");

                                string f = "";
                                for (int b = 1; b < splittedMark.Count(); b++) // Наводим марафетъ
                                {
                                    if (splittedMark[b].Length < 3)
                                    {
                                        f += "-" + splittedMark[b] + "      ";
                                    }
                                    else
                                    {
                                        if (b % 2 == 0 && b != 3)
                                        {
                                            f += "\n";
                                        }
                                        f += splittedMark[b];
                                    }
                                }
                                await botClient.SendTextMessageAsync(chatId, $"Ваши оценки за предмет \"{sMass[1]}\": "+"\n" + f.Replace("/", "."));
                            }
                        }
                    }
                }
            }
        }
        if (ss == "Расписание 📅")
        {
            await botClient.SendTextMessageAsync(chatId, "Блок не готов");
        }
        if(ss == "Меню курсов")
        {
            await botClient.SendTextMessageAsync(chatId, "Блок находится в разработке, ознакомиться с курсами вы можете на нашем сайте - https://delfa72.ru/");
        }

        //if (ss == "Ошибка авторизации!")
        //{
        //    await botClient.SendTextMessageAsync(chatId, "Если у вас возникли проблемы с авторизацией, или я вывел неверное имя, пожалуйста, сообщите об ошибке _*одним*_ сообщением ниже", parseMode: ParseMode.MarkdownV2);
        //}
        if (user.Rows.Count != 0)
        {
            if (ss != "" && user.Rows[0][7].ToString() == "True")
            {
                await botClient.SendTextMessageAsync(chatId, "Хотите, чтобы с Вами связался менеджер, для записи на курс?", replyMarkup: inlManagerCall);
                await Select($"update usersDelfaTelegram set registrationCheck = 'False' where chatId = '{chatId}'");
                requestUserName = ss;
                await Select($"Update usersDelfaTelegram set requestUserName = '{requestUserName}' where chatId = '{chatId}'");
            }
        }

        //StreamReader streamReader = new StreamReader("C://Users/Администратор/Desktop/Projects/data1c/uchenik.txt");
        //string text2 = streamReader.ReadToEnd();
        //streamReader.Close();

        Console.WriteLine(ss + "\tcounter: " + counter + "\tblock: " + block + "\t" + name);
    }
    catch (Exception ex)
    {
    }
}

async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
{
    ReplyKeyboardMarkup authorize = new(new[]
    {
        new KeyboardButton [] { "Оценки 𝟝", "Расписание 📅"},
        new KeyboardButton[] { "Рассылка ✉" }
    })
    { ResizeKeyboard = true };

    ReplyKeyboardRemove hide = new ReplyKeyboardRemove();

    InlineKeyboardMarkup inlCallbackToClient = new(new[]
    {
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Принять заявку", callbackData: "getCallback")
        },
    });
    InlineKeyboardMarkup inlAcceptCallback = new(new[]
    {
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Заявка принята", callbackData: "changeButtonAccept")
        },
    });
    InlineKeyboardMarkup inlHide = new(new[]
{
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "", callbackData: "changeButtonHide")
        },
    });

    DataTable user = new DataTable();
    DataTable kids = new DataTable();

    if (callbackQuery.Message.Chat.Id.ToString() != "-1001895459769")
    {
        user = await Select($"select * from usersDelfaTelegram where chatId = '{callbackQuery.Message.Chat.Id}'"); string Id1c = "";
        string block = "";
        string counter = "";

        if (user.Rows.Count != 0)
        {
            block = user.Rows[0][8].ToString();
            counter = user.Rows[0][9].ToString();
            Id1c = user.Rows[0][12].ToString();
        }

        kids = await Select($"select * from uucKidsTelegram where accountableId like '{Id1c}'");
    }
    if (callbackQuery.Data == "getCallback") // Принять заявку на беседу с клиентом -- 278
    {
        await botClient.SendTextMessageAsync("-1001895459769", "Заявку принял - " + callbackQuery.From.FirstName + " " + callbackQuery.From.LastName);
        await botClient.EditMessageReplyMarkupAsync(chatId: "-1001895459769", callbackQuery.Message.MessageId, replyMarkup: inlAcceptCallback);
    }

    if(callbackQuery.Data == "menuCourses")
    {
        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Блок находится в разработке, ознакомиться с курсами вы можете на нашем сайте - https://delfa72.ru/", replyMarkup:hide);
    }

    if(callbackQuery.Data == "authoriseInline")
    {
        ReplyKeyboardMarkup shareContact = new(new[] { KeyboardButton.WithRequestContact("➡Отправить контакт⬅") });

        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                "\n\nДавайте авторизуемся!\nОтправьте мне свой контакт по кнопке ниже и я найду вас в базе.\n\nВНИМАНИЕ! Мы не раскрываем личные данные наших клиентов третьим лицам.\n" +
                "Номер телефона используется исключительно для авторизации.", replyMarkup: shareContact);

        await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, replyMarkup: inlHide);
    }

    if (callbackQuery.Data == "yes") //Заявка отправлена в беседу админов
    {
        if (user.Rows.Count != 0)
        {
            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Хорошо, я передам менеджеру Ваши контактные данные.\nОн свяжется с Вами в ближайшее время", replyMarkup: hide);
            await botClient.SendTextMessageAsync("-1001895459769", "Пришла заявка от " + user.Rows[0][14] + "\nНомер телефона: " + user.Rows[0][4], replyMarkup: inlCallbackToClient);
        }
    }
    else if (callbackQuery.Data == "no") // Заявка отклонена пользователем
    {
        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Жаль, надеюсь, Вы скоро к нам вернётесь!", replyMarkup: authorize);
    }
    return;
}

Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Ошибка телеграм АПИ:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
        _ => exception.ToString()
    };
    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

Task postMail()
{
    //using (MailMessage emailMessage = new MailMessage())
    //{
    //    emailMessage.From = new MailAddress("flamechnl@gmail.com", "Account2");
    //    emailMessage.To.Add(new MailAddress("denis_danila_den2@mail.com", "Account1"));
    //    emailMessage.Subject = "SUBJECT";
    //    emailMessage.Body = "BODY";
    //    emailMessage.Priority = MailPriority.Normal;
    //    using (SmtpClient MailClient = new SmtpClient("smtp.gmail.com", 465))
    //    {
    //        MailClient.EnableSsl = true;
    //        MailClient.Credentials = new System.Net.NetworkCredential("flamechnl@gmail.com", "password");
    //        MailClient.Send(emailMessage);
    //    }
    //}
    return Task.CompletedTask;
}

async Task GetContactAsync(ITelegramBotClient botClient, MessageType messageType, Message message)
{
    //await botClient.SendTextMessageAsync(message.Chat.Id, $"You said:\n{message.Contact.PhoneNumber}");
    var username = message.Chat.Username;
    var name = message.Chat.FirstName + " " + message.Chat.LastName;
    var chatId = message.Chat.Id;
    var ss = message.Text;
    var telegramId = message.From.Id;

    try
    {
        DataTable user = await Select($"select * from usersDelfaTelegram where phoneNumber like '%{(message.Contact.PhoneNumber).Replace("+","").Remove(0,1)}'");
        string Id1c = "";
        string block = "";
        string counter = "";

        if (user.Rows.Count != 0)
        {
            block = user.Rows[0][8].ToString();
            counter = user.Rows[0][9].ToString();
            Id1c = user.Rows[0][12].ToString();
        }

        DataTable kids = await Select($"select * from uucKidsTelegram where accountableId like '{Id1c}'");

        ReplyKeyboardMarkup authorize = new(new[]
        {
           new KeyboardButton [] { "Оценки 𝟝",  "Расписание 📅"},
           new KeyboardButton [] { "Рассылка ✉", "Меню курсов" }
        })
        { ResizeKeyboard = true };
        ReplyKeyboardRemove hide = new ReplyKeyboardRemove();


        Console.WriteLine(message.Contact.PhoneNumber);

        if (user.Rows.Count != 0) // Если он найден в usersDelfaTelegram (сломан)
        {
            if (user.Rows[0][6].ToString().Length < 2)
            {
                await botClient.SendTextMessageAsync(chatId, $"Добро пожаловать, {name}", replyMarkup: authorize);
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, $"Добро пожаловать, {user.Rows[0][6].ToString()}", replyMarkup: authorize);
            }
            await Select($"update usersDelfaTelegram set nickname = '{name}' where phoneNumber like '%{(message.Contact.PhoneNumber).Replace("+", "").Remove(0, 1)}'");
            await Select($"update usersDelfaTelegram set telegramId = '{telegramId}' where phoneNumber like '%{(message.Contact.PhoneNumber).Replace("+", "").Remove(0, 1)}'");
            await Select($"update usersDelfaTelegram set username = '{username}' where phoneNumber like '%{(message.Contact.PhoneNumber).Replace("+", "").Remove(0, 1)}'");
            await Select($"update usersDelfaTelegram set phoneNumber = '{message.Contact.PhoneNumber}' where phoneNumber like '%{(message.Contact.PhoneNumber).Replace("+", "").Remove(0, 1)}'");
            await Select($"update usersDelfaTelegram set chatId = '{chatId}' where phoneNumber like '%{(message.Contact.PhoneNumber).Replace("+", "").Remove(0, 1)}'");

            await Select($"update usersDelfaTelegram set block = 2 where chatId = '{chatId}';" +
            $" update usersDelfaTelegram set counter = 1 where chatId = '{chatId}'");
        }
        else if (user.Rows.Count == 0) // Если он НЕ найден в usersDelfaTelegram
        {
            await Select($"INSERT INTO usersDelfaTelegram (nickname, telegramId, username, phoneNumber, chatId) values('{name}', '{telegramId}', '{username}', '{message.Contact.PhoneNumber}', '{chatId}')");

            await botClient.SendTextMessageAsync(chatId, "Я не нашел Вас с базе.\nНе могли бы Вы отправить мне своё ФИО, чтобы я знал как к Вам обращаться.", replyMarkup: hide);

            await Select($"update usersDelfaTelegram set registrationCheck = 'True' where chatId = '{chatId}'");

            await Select($"update usersDelfaTelegram set block = 1 where chatId = '{chatId}';" +
            $" update usersDelfaTelegram set counter = 1 where chatId = '{chatId}'");
        }
    }
    catch (Exception ex)
    {
        await botClient.SendTextMessageAsync(995734455, "Ошибка:" + ex.ToString() + "\n\n" + chatId + "\t" + name);
    }
    return;
}

static async Task<DataTable> Select(string selectSQL)
{
    DataTable data = new DataTable("dataBase");

    string path = "ConnectionString.txt";

    string text = System.IO.File.ReadAllText(path);

    string[] vs = text.Split('"');

    SqlConnection sqlConnection = new SqlConnection($"server = {vs[1]};Trusted_connection={vs[3]};DataBase={vs[5]};User={vs[7]};PWD={vs[9]}");
    sqlConnection.Open();

    SqlCommand sqlCommand = sqlConnection.CreateCommand();
    sqlCommand.CommandText = selectSQL;

    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
    sqlDataAdapter.Fill(data);

    sqlConnection.Dispose();

    return data;
}