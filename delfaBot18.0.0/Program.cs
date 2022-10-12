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
cts.Cancel();

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
    if (update.Message.Type == MessageType.Contact)
    {
        await GetContactAsync(botClient, MessageType.Contact, update.Message);
        return;
    }
}

async Task HandleMessage(ITelegramBotClient botClient, Message message)
{
    var username = message.Chat.Username;
    var name = message.Chat.FirstName + " " + message.Chat.LastName;
    var telegramId = message.From.Id;
    var chatId = message.Chat.Id;
    var ss = message.Text;

    var user = await Select($"select * from usersDelfaTelegram where telegramId like '{telegramId}'");

    ReplyKeyboardMarkup shareContact = new(new[]
    {
        KeyboardButton.WithRequestContact("Отправить контакт")
    });

    if (ss.ToLower() == "/start" || ss.ToLower() == "старт" || ss.ToLower() == "start" || ss.ToLower() == "ыефке" || ss.ToLower() == "cnfhn" || ss.ToLower() == "/старт")
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, "Доброго времени суток, я - автоматизированный менеджер 'Дельфа'." +
         "\n\nДавайте авторизуемся!\nОтправьте мне свой контакт по кнопке ниже и я найду вас в базе.", replyMarkup: shareContact);

        await Select($"update usersDelfaTelegram set block = REPLACE(block, 0, 1) where telegramId = '{chatId}'; update usersDelfaTelegram set counter = REPLACE(counter, 0, 1) where telegramId = '{chatId}'");
    }

    DataTable data = await Select("select * from usersDelfaTelegram");

    if (message.Type == MessageType.Contact)
    {
        if (user.Rows.Count == 0)
        {
            await Select($"INSERT INTO usersDelfaTelegram (nickname, telegramId, username, phoneNumber, chatId) values('{name}', '{telegramId}', '{username}', '1', '{chatId}')");

            await botClient.SendTextMessageAsync(chatId, "Я не нашел Вас с базе.\nНе могли бы Вы отправить мне своё ФИО, чтобы я знал как к Вам обращаться.");

            await Select($"update usersDelfaTelegram set registrationCheck = REPLACE(registrationCheck, False, True) where telegramId = '{chatId}'");
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId, $"Добро пожаловать, {(await Select($"select name from usersDelfaTelegram where chatId = '{chatId}'")).ToString()}");
        }    
    }
    if (user.Rows[0][7].ToString() == "True")
    {
        await botClient.SendTextMessageAsync(chatId, "Проверьте правильность введённых данных!\n" + $"{message.Text}");
    }

    //string stringData = "";

    //for (int i = 0; i < data.Rows.Count; i++)
    //{
    //    stringData += data.Rows[i][0];
    //}

    //await botClient.SendTextMessageAsync(message.Chat.Id, $"You said:\n{message.Text}");
}

async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
{
    await botClient.SendTextMessageAsync(
        callbackQuery.Message.Chat.Id,
        $"You choose with data: {callbackQuery.Data}"
        );
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

async Task GetContactAsync(ITelegramBotClient botClient, MessageType messageType, Message message)
{
    //await botClient.SendTextMessageAsync(message.Chat.Id, $"You said:\n{message.Contact.PhoneNumber}");

    DataTable data = await Select("select * from usersDelfaTelegram");

    var username = message.Chat.Username;
    var name = message.Chat.FirstName + " " + message.Chat.LastName;
    var telegramId = message.From.Id;
    var chatId = message.Chat.Id;

    var user = await Select($"select phoneNumber from usersDelfaTelegram where telegramId like '{telegramId}'");

    ReplyKeyboardMarkup authorize = new(new[]
    {
        new KeyboardButton [] {"Оценки", "Это не я!"},
        new KeyboardButton[] { "Расписание" }
    }
    )
    { ResizeKeyboard = true };

    for (int i = 0; i < user.Rows.Count; i++)
    {
        if (user.Rows[i][0].ToString() == "1")
        {
            await Select($"update usersDelfaTelegram set phoneNumber = REPLACE(phoneNumber, 1, {message.Contact.PhoneNumber}) where telegramId = '{chatId}'");
            Console.WriteLine("Number added");
            break;
        }
        else
        {
            var names = await Select($"select nickname from usersDelfaTelegram where telegramId = '{chatId}'");
            await botClient.SendTextMessageAsync(chatId, "Добро пожаловать, " + $"{names.Rows[0][0]}", replyMarkup:authorize);
            break;
        }
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

    return data;
}