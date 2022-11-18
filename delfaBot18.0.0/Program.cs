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
using Telegram.Bot.Types.InlineQueryResults;

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
       new KeyboardButton [] { "Меню курсов", "О нас 📓" }
    })
    { ResizeKeyboard = true };

    InlineKeyboardMarkup mainMenu = new(new[]
    {
        
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Меню курсов", callbackData: "menuCourses"),
            InlineKeyboardButton.WithCallbackData(text: "Авторизация", callbackData: "authoriseInline"),
        },
    });
    InlineKeyboardMarkup mainMenuWhithPhone = new(new[]
    {
        
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Меню курсов", callbackData: "menuCourses"),
            InlineKeyboardButton.WithCallbackData(text: "Связаться с администратором", callbackData: "inlAnsverForManager"),
        },
    });

    InlineKeyboardMarkup inlNotifications = new(new[]
    {
        
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Да", callbackData: "notificationsYes"),
            InlineKeyboardButton.WithCallbackData(text: "Нет", callbackData: "notificationsNo"),
        },
    });
    InlineKeyboardMarkup inlAbout = new(new[]
    {   
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Контактные данные", callbackData: "contactData"),
        },
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Фотогалерея", callbackData: "photogalleryData"),
        },
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Лицензия", callbackData: "licenseData")
        },
    });
    InlineKeyboardMarkup inlManagerCall = new(new[]
    {
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Да, хочу", callbackData: "yes"),
            InlineKeyboardButton.WithCallbackData(text: "Нет, я просто смотрю", callbackData: "no"),
        },
    });
    ReplyKeyboardRemove hide = new ReplyKeyboardRemove();
    //var url = "https://api.telegram.org/bot5183249647:AAHCx42xlNoIEZ51EXA2qo0lJe0e4mp_J4M/sendMessage?chat_id=995734455^&text=123"; //отправка сообщения HTTPS GET запросом

    //var request = WebRequest.Create(url);
    //request.Method = "GET";

    //using var webResponse = request.GetResponse();
    //using var webStream = webResponse.GetResponseStream();

    //using var reader = new StreamReader(webStream);
    //var data = reader.ReadToEnd();

    //Console.WriteLine(data);


    //await botClient.SendTextMessageAsync(chatId, "https://t.me/Flame_chanel \n\nНапишите мне, если встретите ошибку");
    try
    {
        if (ss != "")
        {
            FileStream notificationsStream = new FileStream(@"C:\Users\Администратор\Documents\data1c\telegramBotmessages.txt", FileMode.Append); // Логи

            string sMessage = "message = " + ss + "\t" + DateTime.Now + "\t" + chatId + "\t" + username;
            StreamWriter streamWriter = new StreamWriter(notificationsStream);

            streamWriter.WriteLine(sMessage);

            streamWriter.Close();
            notificationsStream.Close();
        }
        if (ss.ToLower() == "/start" || ss.ToLower() == "старт" || ss.ToLower() == "start" || ss.ToLower() == "ыефке" || ss.ToLower() == "cnfhn" || ss.ToLower() == "/старт")
        {
            if (user.Rows.Count != 0)
            {
                if (user.Rows[0][12].ToString() != "" && user.Rows[0][4].ToString() != "1")
                {
                    if (user.Rows[0][6].ToString().Length < 2)
                    {
                        await botClient.SendTextMessageAsync(chatId, $"Добро пожаловать, {name}", replyMarkup: authorize);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, $"Добро пожаловать, {user.Rows[0][6]}", replyMarkup: authorize);
                    }
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
                else {
                    await botClient.SendTextMessageAsync(chatId, "Привет 👋🏽\r\nЯ, автоматизированный помощник учебного центра \"Дельфа\" \U0001f9be\r\n", replyMarkup: hide);
                    await botClient.SendTextMessageAsync(chatId, "\r\nЕсли Вы являетесь нашим клиентом или хотите зарегистрироваться, нажмите:  _*Авторизация*_ \r\n\nЕсли Вы хотите ознакомиться с перечнем курсов, нажмите: _*Меню курсов*_", replyMarkup: mainMenuWhithPhone, parseMode: ParseMode.MarkdownV2);
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

        if(ss == "О нас 📓")
        { await botClient.SendTextMessageAsync(chatId, "Учебный центр __*[«Дельфа»](https://delfa72.ru)*__ – это современное образовательное учреждение, специализирующееся в обучении в сфере IT-технологий 🖥\r\n27 лет успешной работы на рынке образовательных услуг.\r\n\r\n".Replace("-", "\\-").Replace("+", "\\+").Replace(".", "\\.")
            , parseMode: ParseMode.MarkdownV2, replyMarkup: inlAbout); }

        if (user.Rows.Count != 0)
        {
            //if (ss == "Рассылка ✉")
            //{
            //    await botClient.SendTextMessageAsync(chatId, "Вы хотите отказаться от рассылки?", replyMarkup: inlNotifications);
            //}
            if (ss == "Оценки 𝟝")
            {
                for (int i = 0; i < user.Rows.Count; i++)
                {
                    string group = user.Rows[i][16].ToString();
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
                                        for (int b = 1; b < splittedMark.Count() - 1; b++) // Наводим марафетъ
                                        {
                                            if (splittedMark[b].Length < 3)
                                            {
                                                if (splittedMark[b] == "5")
                                                {
                                                    f += "-5️⃣      ";
                                                }
                                                else if (splittedMark[b] == "4")
                                                {
                                                    f += "-4️⃣      ";
                                                }
                                                else if (splittedMark[b] == "3")
                                                {
                                                    f += "-3️⃣      ";
                                                }
                                                else if (splittedMark[b] == "2")
                                                {
                                                    f += "-2️⃣      ";
                                                }
                                                else if (splittedMark[b] == "1")
                                                {
                                                    f += "-1️⃣      ";
                                                }
                                                else if (splittedMark[b] == "н")
                                                {
                                                    f += "-❌      ";
                                                }
                                                else if (splittedMark[b] == "+")
                                                {
                                                    f += "-✅      ";
                                                }
                                                else { f += "-" + splittedMark[b] + "     "; }
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

                                        DataTable kidName = await Select($"select name from uucKidsTelegram where code = '{user.Rows[i][15]}'");
                                        if (user.Rows[i][15].ToString() == "")
                                        {
                                            await botClient.SendTextMessageAsync(chatId, $"Ваши оценки за предмет \"_*{sMass[1].Replace("-", "\\-").Replace("+", "\\+").Replace(".", "\\.").Replace("(", "\\(").Replace(")","\\)")}*_\": " + "\n\n" + f.Replace("/", "\\.").Replace("-", "\\-").Replace("+", "\\+"), parseMode: ParseMode.MarkdownV2, replyMarkup: authorize);
                                        }
                                        else
                                        {
                                            for (int m = 0; m < kidName.Rows.Count; m++)
                                            {
                                                string imya = kidName.Rows[m][0].ToString();
                                                await botClient.SendTextMessageAsync(chatId, $"Оценки _{imya.Replace("-", "\\-").Replace("+", "\\+").Replace(".", "\\.").Replace("(", "\\(").Replace(")","\\)")}_ за предмет \"_*{sMass[1].Replace("-", "\\-").Replace("+", "\\+").Replace(".", "\\.").Replace("(", "\\(").Replace(")","\\)")}*_\": "
                                                    + "\n\n" + f.Replace("/", "\\.").Replace("-", "\\-").Replace("+", "\\+"), parseMode: ParseMode.MarkdownV2, replyMarkup: authorize);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (ss == "Расписание 📅")
            {
                Regex rSubb = new Regex("(сб)|(субб)|(суббота)");
                Regex rVoskr = new Regex("(вс)|(воскр)|(воскресенье)");
                Regex rPoned = new Regex("(пн)|(пон)|(понедельник)");
                Regex rVtornik = new Regex("(вт)|(втор)|(вторник)");
                Regex rSreda = new Regex("(ср)|(сред)|(среда)");
                Regex rChetv = new Regex("(чт)|(чет)|(четверг)");
                Regex rPyatn = new Regex("(пт)|(пят)|(пятница)");

                Regex[] regexes = { rPoned, rPyatn, rSreda, rSubb, rVoskr, rVtornik, rChetv };

                if (user.Rows.Count > 0)
                {
                    for (int n = 0; n < user.Rows.Count; n++)
                    {
                        string group = "";
                        if (user.Rows[n][15].ToString() == "")
                        {
                            group = user.Rows[n][16].ToString();
                            string data = "";
                            for (int m = 0; m < regexes.Count(); m++)
                            {
                                MatchCollection match = regexes[m].Matches(group.ToLower());
                                if (match.Count > 0)
                                {
                                    Regex time = new Regex("(\\d.?\\.\\d.?\\-\\d.?\\.\\d.?)|(\\d\\:\\d?)|(\\d.?\\.\\d.?)");
                                    Match match1 = time.Match(group.ToLower());
                                    string[] strings = regexes[m].ToString().Split("|(");
                                    data += strings[2].Replace(")", " ") + match1.ToString().Replace(".", ":") + " \n";
                                }
                            }
                            await botClient.SendTextMessageAsync(chatId, "Ваше расписание \\- " + "*" + group.Replace("-", "\\-").Replace("+", "\\+").Replace(".", "\\.").Replace("(", "\\(").Replace(")","\\)") + "   " + data.Replace("-", "\\-").Replace("+", "\\+").Replace(".", "\\.").Replace("(", "\\(").Replace(")","\\)") + "*", parseMode: ParseMode.MarkdownV2, replyMarkup: authorize);
                            data = "";
                        }
                        else
                        {
                            group = user.Rows[n][16].ToString();
                            string data = "";
                            for (int m = 0; m < regexes.Count(); m++)
                            {
                                MatchCollection match = regexes[m].Matches(group.ToLower());
                                if (match.Count > 0)
                                {
                                    Console.WriteLine("raspisanie");
                                    Regex time = new Regex("(\\d.?\\.\\d.?\\-\\d.?\\.\\d.?)|(\\d\\:\\d?)|(\\d.?\\.\\d.?)");
                                    Match match1 = time.Match(group.ToLower());
                                    string[] strings = regexes[m].ToString().Split("|(");
                                    data += strings[2].Replace(")", " ") + match1.ToString().Replace(".", ":") + " \n";
                                }
                            }
                            DataTable kidName = await Select($"select name from uucKidsTelegram where code = '{user.Rows[n][15]}'");
                            for (int m = 0; m < kidName.Rows.Count; m++)
                            {
                                await botClient.SendTextMessageAsync(chatId, $"Расписание {kidName.Rows[m][0]} \\- " + "*" + group.Replace("-", "\\-").Replace("+", "\\+").Replace(".", "\\.").Replace("(", "\\(").Replace(")","\\)") + "   " + data.Replace("-", "\\-").Replace("+", "\\+").Replace(".", "\\.").Replace("(", "\\(").Replace(")","\\)") + "*", parseMode: ParseMode.MarkdownV2, replyMarkup: authorize);
                                data = "";
                            }
                        }
                    }
                }
                else { await botClient.SendTextMessageAsync(chatId, "Вы еще не записаны ни на один курс\\.\nОзнакомиться с перечнем курсов можно на *сайте* \\-  или в разделе *\"Меню курсов\"*: ", replyMarkup: mainMenu, parseMode: ParseMode.MarkdownV2); }
            }
        }
        if (ss == "Меню курсов" && user.Rows.Count != 0)
        {
            await botClient.SendTextMessageAsync(chatId, "Блок находится в разработке, ознакомиться с курсами вы можете на нашем сайте - https://delfa72.ru/", replyMarkup: authorize);
        }
        if (user.Rows.Count != 0)
        {
            if (ss != "" && user.Rows[0][7].ToString() == "True")
            {
                if (ss.Length > 49)
                {
                    await botClient.SendTextMessageAsync(chatId, "Сообщение слишком большое.\nПожалуйста, уменьшите количество символов", replyMarkup: hide);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Хотите, чтобы с Вами связался менеджер, для записи на курс?", replyMarkup: inlManagerCall);
                    requestUserName = ss;
                    await Select($"Update usersDelfaTelegram set requestUserName = '{requestUserName}' where chatId = '{chatId}'");
                }
            }
        }

        //StreamReader streamReader = new StreamReader("C://Users/Администратор/Desktop/Projects/data1c/uchenik.txt");
        //string text2 = streamReader.ReadToEnd();
        //streamReader.Close();

        Console.WriteLine(ss + "\tcounter: " + counter + "\tblock: " + block + "\t" + name);
    }
    catch (Exception ex)
    {
        FileStream notificationsStream = new FileStream("C:\\Users\\Администратор\\Documents\\data1c\\telegramBot\\ErrorsLogs.txt", FileMode.Append); // Логи

        string sMessage = ex.ToString() + "\n\n" + DateTime.Now;
        StreamWriter streamWriter = new StreamWriter(notificationsStream);

        streamWriter.WriteLine(sMessage);

        streamWriter.Close();
        notificationsStream.Close();
        Console.WriteLine("ErrorsLogs");
    }
}

async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
{
    ReplyKeyboardRemove hide = new ReplyKeyboardRemove();

    InlineKeyboardMarkup inlCallbackToClient = new(new[]
    {
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Принять заявку", callbackData: "getCallback")
        },
    });
    InlineKeyboardMarkup inlBackToAbout = new(new[]
    {
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "backAbout"),
        }
    });
    InlineKeyboardMarkup inlAcceptCallback = new(new[]
    {
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Заявку принял - " + callbackQuery.From.FirstName + " " + callbackQuery.From.LastName + "   время: " + DateTime.Now, callbackData: "changeButtonAccept")
        },
    });
    InlineKeyboardMarkup inlHide = new(new[]
    {
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "", callbackData: "changeButtonHide")
        },
    });
    InlineKeyboardMarkup inlAbout = new(new[]
    {
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Контактные данные", callbackData: "contactData"),
        },
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Фотогалерея", callbackData: "photogalleryData"),
        },
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Лицензия", callbackData: "licenseData")
        },
    });
    InlineKeyboardMarkup mainMenu = new(new[]
    {
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Меню курсов", callbackData: "menuCourses"),
            InlineKeyboardButton.WithCallbackData(text: "Авторизация", callbackData: "authoriseInline"),
        },
    });
    InlineKeyboardMarkup mainMenuWhithPhone = new(new[]
    {
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Меню курсов", callbackData: "menuCourses"),
            InlineKeyboardButton.WithCallbackData(text: "Связаться с администратором", callbackData: "inlAnsverForManager"),
        },
    });
    InlineKeyboardMarkup inlManagerCall = new(new[]
    { 
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Да, хочу", callbackData: "yes"),
            InlineKeyboardButton.WithCallbackData(text: "Нет, я просто смотрю", callbackData: "no"),
        },
    });

    DataTable user = new DataTable();
    DataTable kids = new DataTable();

    if (callbackQuery.Message.Chat.Id.ToString() != "-1001895459769")
    {
        user = await Select($"select * from usersDelfaTelegram where chatId = '{callbackQuery.Message.Chat.Id}'");

        string Id1c = "";
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

    if(callbackQuery.Data == "backAbout")
    {
        await botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "Учебный центр __*[«Дельфа»](https://delfa72.ru)*__ – это современное образовательное учреждение, специализирующееся в обучении в сфере IT-технологий 🖥\r\n27 лет успешной работы на рынке образовательных услуг.\r\n\r\n".Replace("-", "\\-").Replace("+", "\\+").Replace(".", "\\."), parseMode: ParseMode.MarkdownV2, replyMarkup: inlAbout);
    }

    if(callbackQuery.Data == "contactData")
    {
        await botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "Наш __*[сайт](https://delfa72.ru)*__\n\nАдреса:\nул. Республики, 61\r\nпн - пт 8:00 до 20:00\r\nсб 9:00 до 20:00\r\nвс 9:00 до 19:00\n\nул. Е. Богдановича, 8 к. 1/1\r\nпн - пт 8:00 до 20:00\r\nсб 9:00 до 20:00\r\nвс 9:00 до 21:00".Replace("-", "\\-").Replace("+", "\\+").Replace(".", "\\.")
            + "\n\nКонтактные данные:\n+73452387777\r\n+78002501110\r\nПочта: info@delfa72.ru".Replace("-", "\\-").Replace("+", "\\+").Replace(".", "\\.").Replace("(", "\\(").Replace(")","\\)"), parseMode: ParseMode.MarkdownV2, replyMarkup: inlBackToAbout);
    }

    if(callbackQuery.Data == "photogalleryData")
    {
        await botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "__*[Фотогалерея](https://delfa72.ru/galereya/)*__", parseMode: ParseMode.MarkdownV2, replyMarkup: inlBackToAbout);
    }

    if(callbackQuery.Data == "licenseData")
    {
        await botClient.SendPhotoAsync(callbackQuery.Message.Chat.Id, "https://delfa72.ru/upload/iblock/5fd/5fd8531d0066d9221312c5c9870edf28.jpg");
        await botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "Лицензия на осуществление образовательной деятельности __*№ 026 от 23\\.04\\.2018*__", parseMode: ParseMode.MarkdownV2, replyMarkup: inlBackToAbout);
    }

    if (callbackQuery.Data == "getCallback") // Принять заявку на беседу с клиентом
    {
        await botClient.EditMessageReplyMarkupAsync(chatId: "-1001895459769", callbackQuery.Message.MessageId, replyMarkup: inlAcceptCallback);
    }

    if(callbackQuery.Data == "inlAnsverForManager")
    {
        if (user.Rows.Count != 0)
        {
            if (user.Rows[0][7].ToString() == "True")
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Хотите, чтобы с Вами связался менеджер, для записи на курс?", replyMarkup: inlManagerCall);
            }
        }
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

        //await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
    }

    if (callbackQuery.Data == "yes") //Заявка отправлена в беседу админов
    {
        if (user.Rows.Count != 0)
        {
            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Хорошо, я передам менеджеру Ваши контактные данные.\nОн свяжется с Вами в ближайшее время", replyMarkup: hide);
            await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, replyMarkup: inlHide);
            await Select($"update usersDelfaTelegram set registrationCheck = 'False' where chatId = '{callbackQuery.Message.Chat.Id}'");
            await botClient.SendTextMessageAsync("-1001895459769", "Пришла заявка от " + user.Rows[0][14] + "\nНомер телефона: " + user.Rows[0][4], replyMarkup: inlCallbackToClient);
        }
    }
    else if (callbackQuery.Data == "no") // Заявка отклонена пользователем
    {
        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Жаль, надеюсь, Вы скоро к нам вернётесь!", replyMarkup: mainMenuWhithPhone);
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
        FileStream notificationsStream = new FileStream("C:\\Users\\Администратор\\Documents\\data1c\\telegramBot\\ErrorsLogs.txt", FileMode.Append); // Логи

        string sMessage = ErrorMessage.ToString() + "\n   " + DateTime.Now;
        StreamWriter streamWriter = new StreamWriter(notificationsStream);

        streamWriter.WriteLine(sMessage);

        streamWriter.Close();
        notificationsStream.Close();
        Console.WriteLine("ErrorsLogs");
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
           new KeyboardButton [] { "Меню курсов", "О нас 📓" }
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
                await botClient.SendTextMessageAsync(chatId, $"Добро пожаловать, {user.Rows[0][6]}", replyMarkup: authorize);
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

            await botClient.SendTextMessageAsync(chatId, "Я не нашел Вас в базе.\nНе могли бы Вы отправить мне своё имя, чтобы я знал как к Вам обращаться.", replyMarkup: hide);

            await Select($"update usersDelfaTelegram set registrationCheck = 'True' where chatId = '{chatId}'");

            await Select($"update usersDelfaTelegram set block = 1 where chatId = '{chatId}';" +
            $" update usersDelfaTelegram set counter = 1 where chatId = '{chatId}'");
        }
    }
    catch (Exception ex)
    {
        FileStream notificationsStream = new FileStream("C:\\Users\\Администратор\\Documents\\data1c\\telegramBot\\ErrorsLogs.txt", FileMode.Append); // Логи

        string sMessage = ex.ToString() + "\n\n" + DateTime.Now;
        StreamWriter streamWriter = new StreamWriter(notificationsStream);

        streamWriter.WriteLine(sMessage);

        streamWriter.Close();
        notificationsStream.Close();
        Console.WriteLine("ErrorsLogs");
    }
    return;
}

static async Task<DataTable> Select(string selectSQL)
{
    DataTable data = new DataTable("dataBase");

    //string path = "ConnectionString.txt";

    //string text = System.IO.File.ReadAllText(path);

    //string[] vs = text.Split('"');

    SqlConnection sqlConnection = new SqlConnection($"server=SERVER-1C;Trusted_connection=yes;DataBase=delfaTelegramBotData_18.00.0;User= ;PWD= ");
    sqlConnection.Open();

    SqlCommand sqlCommand = sqlConnection.CreateCommand();
    sqlCommand.CommandText = selectSQL;

    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
    sqlDataAdapter.Fill(data);

    sqlCommand.Dispose();
    sqlDataAdapter.Dispose();
    sqlConnection.Close();

    return data;
}