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
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Xml.Linq;

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

Console.ReadLine();

void TimerCallback(Object o)
{
    Console.WriteLine("In TimerCallback: " + DateTime.Now);
}

var me = await botClient.GetMeAsync();

// Send cancellation request to stop bot
//cts.Cancel();
static string Rearm(string name)
{
    name = name.Replace("-", "\\-").Replace("+", "\\+").Replace(".", "\\.").Replace("(", "\\(").Replace(")", "\\)").Replace("/", "\\.");
    return name;
}
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
async Task HandleMessage(ITelegramBotClient botClient, Message message) //Сообщения
{
    var username = message.Chat.Username;
    var name = message.Chat.FirstName + " " + message.Chat.LastName;
    var telegramId = message.From.Id;
    var chatId = message.Chat.Id;
    var ss = message.Text;

    string requestUserName = "Пусто";

    DataTable user = await Select($"select * from usersDelfaTelegram where chatId like '{chatId}'");
    string Id1c = "";
    string code = "";
    string phone = "";

    if (user.Rows.Count != 0)
    {
        Id1c = user.Rows[0][12].ToString();
        code = user.Rows[0][9].ToString();
        phone = user.Rows[0][4].ToString();
    }

    DataTable kids = await Select($"select * from uucKidsTelegram where accountableId like '{Id1c}'");

    ReplyKeyboardMarkup shareContact = new(new[] { KeyboardButton.WithRequestContact("➡Отправить контакт⬅") });
    ReplyKeyboardMarkup authorizeRoman = new(new[]
    {
       new KeyboardButton [] { "Оценки 𝟝",  "Расписание 📅"},
       new KeyboardButton [] { "Меню курсов", "О нас 📓", "Настройки⚙"},
       new KeyboardButton [] { "Статистика💹", "Принести воды🚰", "Логи📋" }
    })
    { ResizeKeyboard = true };
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
    InlineKeyboardMarkup inlChanges = new(new[]
    {
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Добавить доверенного пользователя", callbackData: "appendNewUser"),
            InlineKeyboardButton.WithCallbackData(text: "Удалить аккаунт", callbackData: "deleteAccount"),
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
        if (ss.ToLower() == "/start" || ss.ToLower() == "старт" || ss.ToLower() == "start" || ss.ToLower() == "ыефке" || ss.ToLower() == "cnfhn" || ss.ToLower() == "/старт" || ss.ToLower() == "обновить")
        {
            if (user.Rows.Count != 0)
            {
                if (code != "" && phone.ToString() != "1")
                {
                    if(chatId.ToString() == "216348941" || chatId.ToString() == "995734455" || chatId.ToString() == "5709624457" || chatId.ToString() == "1938809927")
                    {
                        await botClient.SendTextMessageAsync(chatId, $"Добро пожаловать, {name}", replyMarkup: authorizeRoman);
                    }    
                    else if (user.Rows[0][6].ToString().Length < 2)
                    {
                        await botClient.SendTextMessageAsync(chatId, $"Добро пожаловать, {name}", replyMarkup: authorize);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, $"Добро пожаловать, {user.Rows[0][6]}", replyMarkup: authorize);
                    }
                    if (user.Rows[0][10].ToString() == "0" && user.Rows[0][6].ToString() != "") // Создание исключения из рассылки смс (Готово)
                    {
                        await Select($"update usersdelfatelegram set notifications = 'true' where chatId = {chatId}");
                        await botClient.SendTextMessageAsync(chatId, "Отличные новости\\! \n\nТеперь, все уведомления, включая оценки, будут приходить Вам _*только*_ в телеграм\\.", replyMarkup: authorize, parseMode: ParseMode.MarkdownV2);
                    }
                }
                else {
                    await botClient.SendTextMessageAsync(chatId, "Привет 👋🏽\r\nЯ, автоматизированный помощник учебного центра \"Дельфа\" \U0001f9be\r\n", replyMarkup: hide);
                    await botClient.SendTextMessageAsync(chatId, "\r\nЕсли Вы являетесь нашим клиентом или хотите зарегистрироваться, нажмите:  _*Связаться с администратором*_ \r\n\nЕсли Вы хотите ознакомиться с перечнем курсов, нажмите: _*Меню курсов*_", replyMarkup: mainMenuWhithPhone, parseMode: ParseMode.MarkdownV2);
                }
            }
            else 
            {
                await botClient.SendTextMessageAsync(chatId, "Привет 👋🏽\r\nЯ, автоматизированный помощник учебного центра \"Дельфа\" \U0001f9be\r\n", replyMarkup: hide);
                await botClient.SendTextMessageAsync(chatId, "\r\nЕсли Вы являетесь нашим клиентом или хотите зарегистрироваться, нажмите:  _*Авторизация*_ \r\n\nЕсли Вы хотите ознакомиться с перечнем курсов, нажмите: _*Меню курсов*_", replyMarkup: mainMenu, parseMode: ParseMode.MarkdownV2);
            }
        }

        //MatchCollection regex = Regex.Matches(text2, ";?.*(\\s?)");

        if(ss == "О нас 📓")
        { await botClient.SendTextMessageAsync(chatId, "Учебный центр __*[«Дельфа»](https://delfa72.ru)*__ \\- это современное образовательное учреждение, специализирующееся в обучении в сфере IT\\-технологий 🖥\r\n27 лет успешной работы на рынке образовательных услуг\\.\r\n\r\n"
            , parseMode: ParseMode.MarkdownV2, replyMarkup: inlAbout); }

        if (user.Rows.Count != 0)
        {
            //if (ss == "Рассылка ✉")
            //{
            //    await botClient.SendTextMessageAsync(chatId, "Вы хотите отказаться от рассылки?", replyMarkup: inlNotifications);
            //}
            if (ss == "Логи📋")
            {
                using (var stream = System.IO.File.OpenRead("C:\\Users\\Администратор\\Documents\\data1c\\telegramBotmessages.txt"))
                {
                    Telegram.Bot.Types.InputFiles.InputOnlineFile iof = new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream);
                    iof.FileName = "telegramBotmessages.txt";
                    await botClient.SendDocumentAsync(chatId, iof, "text");
                }
                using (var stream = System.IO.File.OpenRead("C:\\Users\\Администратор\\Documents\\data1c\\telegramBot\\ErrorsLogs.txt"))
                {
                    Telegram.Bot.Types.InputFiles.InputOnlineFile iof = new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream);
                    iof.FileName = "ErrorsLogs.txt";
                    await botClient.SendDocumentAsync(chatId, iof, "text");
                }
            }
            if (ss == "Настройки⚙")
            {
                await botClient.SendTextMessageAsync(chatId, "Выберите нужное меню:", replyMarkup:inlChanges);
            }
            if (ss == "Статистика💹")
            {
                DataTable stat = await Select("SELECT COUNT(id) FROM usersDelfaTelegram");
                DataTable statUsers = await Select("SELECT COUNT(id) FROM usersDelfaTelegram where telegramId is not null");
                DataTable statNew = await Select("SELECT COUNT(id) FROM usersDelfaTelegram where requestUserName is not null and name is not null");
                DataTable statNewUsers = await Select("SELECT COUNT(id) FROM usersDelfaTelegram where requestUserName is not null and name is null");
                StreamReader stream = new StreamReader("C:\\Users\\Администратор\\Documents\\data1c\\telegramBotmessages.txt");
                string[] messages = stream.ReadToEnd().Split("\n");
                stream.Close();
                await botClient.SendTextMessageAsync(chatId, "Статистика за всё время\\: \n\n" + "Всего клиентов в базе\\: *" + stat.Rows[0][0] + "*\nВсего пользователей\\: *" 
                    + statUsers.Rows[0][0] + "*\nНовых клиентов\\: *" + statNew.Rows[0][0] + "*\nНовых пользователей\\: *" + statNewUsers.Rows[0][0] + "*\nВсего сообщений\\: *" + messages.Count().ToString() + "*", parseMode: ParseMode.MarkdownV2);
            }
            if (ss == "Принести воды🚰")
            {
                await botClient.SendTextMessageAsync(chatId, "Уведомление отправлено, ожидайте💤");
                await botClient.SendTextMessageAsync("995734455", "Принеси Роману воды!");
                await botClient.SendTextMessageAsync("1938809927", "Принеси Роману воды!");
            }
            if (ss == "Оценки 𝟝")
            {
                DataTable kidid = await Select($"select code from uucKidsTelegram where accountableId = '{code}'");
                for (int d = 0; d < kidid.Rows.Count; d++)
                {
                    DataTable groupId = await Select($"Select idOfGroup from userGroupUnion where idOfUser = {code} or idOfUser = '{kidid.Rows[d][0]}'");
                    for (int i = 0; i < groupId.Rows.Count; i++)
                    {
                        DataTable group = await Select($"select groupOfCourse from groupsOfUsersTelegram where id = {groupId.Rows[i][0]}");
                        for (int j = 0; j < group.Rows.Count; j++)
                        {
                            string groupString = group.Rows[j][0].ToString();
                            string[] strings = Directory.GetFiles("C:\\Users\\Администратор\\Documents\\data1c\\marks\\period");
                            if (groupString != "")
                            {
                                foreach (string s in strings) // Это группы
                                {
                                    if (s.Contains(groupString)) // Проверка на наличие группы юзера в папке
                                    {
                                        string[] sMass = s.Split("Оценки ");
                                        sMass[1] = sMass[1].Replace(".txt", "");

                                        StreamReader streamReader = new StreamReader(s); // Достаём нужные оценки
                                        string marks = streamReader.ReadToEnd();
                                        streamReader.Close();

                                        string[] marksOfUser = marks.Split("\n");

                                        foreach (string mark in marksOfUser) // Делим оценки на столбцы
                                        {
                                            if (mark.Contains(code)) // Проверяем есть ли у этого юзера оценки в группе
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
                                                DataTable kidName = await Select($"select name from uucKidsTelegram where code = '{kidid.Rows[d][0]}'");
                                                if (user.Rows[i][12].ToString() == "")
                                                {
                                                    await botClient.SendTextMessageAsync(chatId, $"Ваши оценки за предмет \"_*{Rearm(sMass[1])}*_\": " + "\n\n" + Rearm(f), parseMode: ParseMode.MarkdownV2);
                                                }
                                                else
                                                {
                                                    for (int m = 0; m < kidName.Rows.Count; m++)
                                                    {
                                                        string imya = kidName.Rows[m][0].ToString();

                                                        await botClient.SendTextMessageAsync(chatId, $"Оценки _{Rearm(imya)}_ за предмет \"_*{Rearm(sMass[1])}*_\": "
                                                        + "\n\n" + Rearm(f), parseMode: ParseMode.MarkdownV2);
                                                    }
                                                }
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
                        string groupString = "";
                        string courseString = "";
                        DataTable group = await Select($"select idOfGroup from userGroupUnion where idOfUser = '{code}' or idOfUser = '{user.Rows[n][12]}'");
                        for (int i = 0; i < group.Rows.Count; i++)
                        {
                            if (user.Rows[n][12].ToString() == "")
                            {
                                DataTable dataTable = await Select($"select groupOfCourse, courseOfClass from groupsOfUsersTelegram where id = '{group.Rows[i][0]}'");
                                courseString = dataTable.Rows[0][1].ToString();
                                groupString = dataTable.Rows[0][0].ToString();
                                string data = "";
                                for (int m = 0; m < regexes.Count(); m++)
                                {
                                    MatchCollection match = regexes[m].Matches(groupString.ToLower());
                                    if (match.Count > 0)
                                    {
                                        Regex time = new Regex("(\\d.?\\.\\d.?\\-\\d.?\\.\\d.?)|(\\d\\:\\d?)|(\\d.?\\.\\d.?)");
                                        Match match1 = time.Match(groupString.ToLower());
                                        string[] strings = regexes[m].ToString().Split("|(");
                                        data += strings[2].Replace(")", " ") + match1.ToString().Replace(".", ":") + " \n";
                                    }
                                }
                                await botClient.SendTextMessageAsync(chatId, "Ваше расписание \\- " + "*" + Rearm(courseString) + "\n" + Rearm(data) + "*", parseMode: ParseMode.MarkdownV2);                                  
                                data = "";
                                break;
                            }
                            else
                            {
                                DataTable dataTable = await Select($"select groupOfCourse, courseOfClass from groupsOfUsersTelegram where id = '{group.Rows[i][0]}'");
                                courseString = dataTable.Rows[0][1].ToString();
                                groupString = dataTable.Rows[0][0].ToString();
                                string data = "";
                                for (int m = 0; m < regexes.Count(); m++)
                                {
                                    MatchCollection match = regexes[m].Matches(groupString.ToLower());
                                    if (match.Count > 0)
                                    {
                                        Regex time = new Regex("(\\d.?\\.\\d.?\\-\\d.?\\.\\d.?)|(\\d\\:\\d?)|(\\d.?\\.\\d.?)");
                                        Match match1 = time.Match(groupString.ToLower());
                                        string[] strings = regexes[m].ToString().Split("|(");
                                        data += strings[2].Replace(")", " ") + match1.ToString().Replace(".", ":") + " \n";
                                    }
                                }
                                DataTable kidName = await Select($"select distinct name from uucKidsTelegram where code = '{user.Rows[n][12]}'");
                                for (int m = 0; m < kidName.Rows.Count; m++)
                                {
                                    await botClient.SendTextMessageAsync(chatId, $"Расписание {kidName.Rows[m][0]} \\- " + "*" + Rearm(courseString) + "   " + Rearm(data) + "*", parseMode: ParseMode.MarkdownV2);
                                    data = "";
                                }
                                break;
                            }
                        }
                    }
                }
                else { await botClient.SendTextMessageAsync(chatId, "Вы еще не записаны ни на один курс\\.\nОзнакомиться с перечнем курсов можно на *сайте* \\-  или в разделе *\"Меню курсов\"*: ", replyMarkup: mainMenu, parseMode: ParseMode.MarkdownV2); }
            }
        }
        if (ss == "Меню курсов" && user.Rows.Count != 0)
        {
            await botClient.SendTextMessageAsync(chatId, "Блок находится в разработке, ознакомиться с курсами Вы можете на нашем сайте - https://delfa72.ru/");
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
                    await Select($"Update usersDelfaTelegram set requestUserName = N'{ss}' where chatId = '{chatId}'");
                }
            }
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
}
async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery) //Кнопки
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
    InlineKeyboardMarkup inlCourses = new(new[]
    {
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Меню курсов", callbackData: "menuCourses"),
        }
    });
    InlineKeyboardMarkup inlChanges = new(new[]
{
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Добавить доверенного пользователя", callbackData: "appendNewUser"),
            InlineKeyboardButton.WithCallbackData(text: "Удалить аккаунт", callbackData: "deleteAccount"),
        },
    });
    InlineKeyboardMarkup inlAcceptToDelete = new(new[]
    {
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "Да, я хочу удалить аккаунт", callbackData: "acceptDeleteAccount"),
            InlineKeyboardButton.WithCallbackData(text: "Нет", callbackData: "declineDeleteAccount"),
        }
    });

    DataTable user = new DataTable();
    DataTable kids = new DataTable();
    try
    {
        if (callbackQuery.Message.Chat.Id.ToString() != "-1001895459769")
        {
            user = await Select($"select * from usersDelfaTelegram where chatId = '{callbackQuery.Message.Chat.Id}'");

            string Id1c = "";

            if (user.Rows.Count != 0)
            {
                Id1c = user.Rows[0][12].ToString();
            }
            kids = await Select($"select * from uucKidsTelegram where accountableId like '{Id1c}'");
        }

        if (callbackQuery.Data == "declineDeleteAccount")
        {
            await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, replyMarkup: inlHide);
            await botClient.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
        }
        if (callbackQuery.Data == "backAbout")
        {
            await botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "Учебный центр __*[«Дельфа»](https://delfa72.ru)*__ \\- это современное образовательное учреждение, специализирующееся в обучении в сфере IT\\-технологий 🖥\r\n27 " +
                "лет успешной работы на рынке образовательных услуг\\.", parseMode: ParseMode.MarkdownV2, replyMarkup: inlAbout);
            Console.WriteLine(callbackQuery.Message.MessageId);
        }

        if(callbackQuery.Data == "contactData")
        {
            await botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "Наш __*[сайт](https://delfa72.ru)*__\n\nАдреса:\nул. Республики, 61\r\nпн - пт 8:00 до 20:00\r\nсб 9:00 до 20:00\r\nвс 9:00 до 19:00\n\nул. Е. Богдановича, 8 к. 1/1\r\nпн - пт 8:00 до 20:00\r\nсб 9:00 до 20:00\r\nвс 9:00 до 21:00".Replace("-", "\\-").Replace("+", "\\+").Replace(".", "\\.")
                + "\n\nКонтактные данные\\:\n\\+73452387777\r\n\\+78002501110\r\nПочта\\: info\\@delfa72\\.ru", parseMode: ParseMode.MarkdownV2, replyMarkup: inlBackToAbout);
        }

        if(callbackQuery.Data == "photogalleryData")
        {
            await botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "__*[Фотогалерея](https://delfa72.ru/galereya/)*__", parseMode: ParseMode.MarkdownV2, replyMarkup: inlBackToAbout);
        }

        if(callbackQuery.Data == "licenseData")
        {
            await botClient.SendPhotoAsync(callbackQuery.Message.Chat.Id, "https://delfa72.ru/upload/iblock/5fd/5fd8531d0066d9221312c5c9870edf28.jpg");
            Console.WriteLine(callbackQuery.Message.MessageId);
            await botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "Лицензия на осуществление образовательной деятельности __*№ 026 от 23\\.04\\.2018*__", parseMode: ParseMode.MarkdownV2, replyMarkup: inlBackToAbout);
            Console.WriteLine(callbackQuery.Message.MessageId);
        }

        if(callbackQuery.Data == "getCallback") // Принять заявку на беседу с клиентом
        {
            await botClient.EditMessageReplyMarkupAsync(chatId: "-1001895459769", callbackQuery.Message.MessageId, replyMarkup: inlAcceptCallback);
        }

        if(callbackQuery.Data == "changeButtonAccept")
        {
            await botClient.EditMessageReplyMarkupAsync(chatId: "-1001895459769", callbackQuery.Message.MessageId, replyMarkup: inlCallbackToClient);
        }

        if(callbackQuery.Data == "inlAnsverForManager")
        {
            if (user.Rows.Count != 0)
            {
                if (user.Rows[0][7].ToString() == "True")
                {
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Хотите, чтобы с Вами связался менеджер, для записи на курс?", replyMarkup: inlManagerCall);
                }
                else
                {
                    await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, replyMarkup: inlHide);
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вы уже отправили заявку на зачисление на курс\\, дождитесь\\, менеджер свяжется с Вами в ближайшее время\\!" +
                        "\n\n*Важно\\!* Наши менеджеры работают __только__ в будние дни с 09\\:00 до 18\\:00\\.\n\nА пока Вы ждёте ответа\\, Вы можете ознакомиться с нашими курсами\\:", parseMode: ParseMode.MarkdownV2, replyMarkup: inlCourses);
                }
            }
        }

        if(callbackQuery.Data == "menuCourses")
        {
            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Блок находится в разработке, ознакомиться с курсами Вы можете на нашем сайте - https://delfa72.ru/", replyMarkup:hide);
        }

        if(callbackQuery.Data == "authoriseInline")
        {
            user = await Select($"select * from usersDelfaTelegram where chatId = '{callbackQuery.Message.Chat.Id}'");
            if (user.Rows.Count != 0)
            {
                if (user.Rows[0][4].ToString() == "")
                {
                    ReplyKeyboardMarkup shareContact = new(new[] { KeyboardButton.WithRequestContact("➡Отправить контакт⬅") });
                    await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, replyMarkup: inlHide);
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                            "\n\nДавайте авторизуемся!\nОтправьте мне свой контакт по кнопке ниже и я найду вас в базе.\n\nВНИМАНИЕ! Мы не раскрываем личные данные наших клиентов третьим лицам.\n" +
                            "Номер телефона используется исключительно для авторизации.", replyMarkup: shareContact);
                }
                else
                {
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вы уже отправили свой номер. \nХотите, чтобы с Вами связался менеджер, для записи на курс?", replyMarkup: inlManagerCall);
                }
            }
            else
            {
                ReplyKeyboardMarkup shareContact = new(new[] { KeyboardButton.WithRequestContact("➡Отправить контакт⬅") });
                await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, replyMarkup: inlHide);
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                        "\n\nДавайте авторизуемся!\nОтправьте мне свой контакт по кнопке ниже и я найду вас в базе.\n\nВНИМАНИЕ! Мы не раскрываем личные данные наших клиентов третьим лицам.\n" +
                        "Номер телефона используется исключительно для авторизации.", replyMarkup: shareContact);
            }
        }
    
        if(callbackQuery.Data == "appendNewUser")
        {

        }

        if(callbackQuery.Data == "deleteAccount")
        {
            await botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "Вы точно хотите __*_безвозвратно_*__ удалить свой профиль\\?", ParseMode.MarkdownV2, replyMarkup: inlAcceptToDelete);
        }

        if(callbackQuery.Data == "acceptDeleteAccount")
        {
            await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, replyMarkup:inlHide);
            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вы успешно удалили аккаунт.", replyMarkup:hide);
            await Select($"delete from usersDelfaTelegram where chatId = '{callbackQuery.Message.Chat.Id}'");
        }

        if(callbackQuery.Data == "yes") //Заявка отправлена в беседу админов
        {
            if (user.Rows.Count != 0)
            {
                if (user.Rows[0][7].ToString() != "False")
                {
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Хорошо, я передам менеджеру Ваши контактные данные.\nОн свяжется с Вами в ближайшее время", replyMarkup: hide);
                    await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, replyMarkup: inlHide);
                    await Select($"update usersDelfaTelegram set registrationCheck = 'False' where chatId = '{callbackQuery.Message.Chat.Id}'");
                    await botClient.SendTextMessageAsync("-1001895459769", "Пришла заявка от " + user.Rows[0][11] + "\nНомер телефона: " + user.Rows[0][4], replyMarkup: inlCallbackToClient);
                }
                else {
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вы уже отправили заявку на зачисление на курс\\, дождитесь\\, менеджер свяжется с Вами в ближайшее время\\!" +
                        "\n\n*Важно\\!* наши менеджеры работают __только__ в будние дни с 09\\:00 до 18\\:00\\.\n\nА пока Вы ждёте ответа\\, Вы можете ознакомиться с нашими курсами\\:", parseMode: ParseMode.MarkdownV2, replyMarkup: inlCourses);
                }
            }
        }
        else if(callbackQuery.Data == "no") // Заявка отклонена пользователем
        {
            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Жаль, надеюсь, Вы скоро к нам вернётесь!", replyMarkup: mainMenuWhithPhone);
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
Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken) //Ошибки
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
}//Почта
async Task GetContactAsync(ITelegramBotClient botClient, MessageType messageType, Message message)//Телефон
{
    //await botClient.SendTextMessageAsync(message.Chat.Id, $"You said:\n{message.Contact.PhoneNumber}");
    var username = message.Chat.Username;
    var name = message.Chat.FirstName + " " + message.Chat.LastName;
    var chatId = message.Chat.Id;
    var ss = message.Text;
    var telegramId = message.From.Id;
    var phoneNumber = (message.Contact.PhoneNumber).Replace("+", "").Remove(0, 1);

    try
    {
        DataTable user = await Select($"select * from usersDelfaTelegram where phoneNumber like '%{(message.Contact.PhoneNumber).Replace("+","").Remove(0,1)}'");
        string Id1c = "";

        if (user.Rows.Count != 0)
        {
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
            await Select($"update usersDelfaTelegram set nickname = N'{name}', telegramId = N'{telegramId}', username = N'{username}', " +
                $"phoneNumber = N'{message.Contact.PhoneNumber}', chatId = N'{chatId}' where phoneNumber like '%{phoneNumber}'");

        }
        else if (user.Rows.Count == 0) // Если он НЕ найден в usersDelfaTelegram
        {
            await Select($"INSERT INTO usersDelfaTelegram (nickname, telegramId, username, phoneNumber, chatId) values(N'{name}', '{telegramId}', N'{username}', N'{message.Contact.PhoneNumber}', '{chatId}')");

            await botClient.SendTextMessageAsync(chatId, "Я не нашел Вас в базе.\nНе могли бы Вы отправить мне своё имя, чтобы я знал как к Вам обращаться.", replyMarkup: hide);

            await Select($"update usersDelfaTelegram set registrationCheck = 'True' where chatId = '{chatId}'");
        }
    }
    catch (Exception ex)
    {
        FileStream notificationsStream = new FileStream("C:\\Users\\Администратор\\Documents\\data1c\\telegramBot\\ErrorsLogs.txt", FileMode.Append); // Логи
        string sMessage = ex.ToString() + "\n\n" + DateTime.Now.ToString();
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
}//Запрос
