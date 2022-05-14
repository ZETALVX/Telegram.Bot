using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Net;
using System.Net.Http;
using MihaZupan;

//VARIABLES

//Bot Token + proxy client
/*var proxy = new HttpToSocks5Proxy("127.0.0.1", 9050);
var httpClient = new HttpClient(new HttpClientHandler { Proxy = proxy, UseProxy = true });
var botClient = new TelegramBotClient("5194650604:AAEtzHRNw3fHzbTWksa5OkRSbaQjjZJZSTI", httpClient);*/

var botClient = new TelegramBotClient("TOKEN");

//block 
int blockLevel = 0;
bool messDeleted = false;
string[] badWords = new string[] { "bad word", "badword" };
string[] veryBadWords = new string[] { "very bad word", "verybadword" };

//Time
int year;
int month;
int day;
int hour;
int minute;
int second;

//Messages and user info
long chatId = 0;
string messageText;
int messageId;
string firstName;
string lastName;
long id;
Message sentMessage;

//poll info
int pollId = 0;

//----------------------//

//Read time and save variables
year = int.Parse(DateTime.UtcNow.Year.ToString());
month = int.Parse(DateTime.UtcNow.Month.ToString());
day = int.Parse(DateTime.UtcNow.Day.ToString());
hour = int.Parse(DateTime.UtcNow.Hour.ToString());
minute = int.Parse(DateTime.UtcNow.Minute.ToString());
second = int.Parse(DateTime.UtcNow.Second.ToString());
Console.WriteLine("Data: " + year + "/" + month + "/" + day);
Console.WriteLine("Time: " + hour + ":" + minute + ":" + second);

//cts token
using var cts = new CancellationTokenSource();

// Bot StartReceiving, does not block the caller thread. Receiving is done on the ThreadPool.
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = { } // receive all update types
};
botClient.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token);

var me = await botClient.GetMeAsync();

//write on console a hello message by bot 
Console.WriteLine($"\nHello! I'm {me.Username} and i'm your Bot!");

// Send cancellation request to stop bot and close console
Console.ReadKey();
cts.Cancel();

//----------------------//

//Answer of the bot to the input.
async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Type != UpdateType.Message)
        return;
    // Only process text messages
    if (update.Message!.Type != MessageType.Text)
        return;

    //set variables
    chatId = update.Message.Chat.Id;
    messageText = update.Message.Text;
    messageId = update.Message.MessageId;
    firstName = update.Message.From.FirstName;
    lastName = update.Message.From.LastName;
    id = update.Message.From.Id;
    year = update.Message.Date.Year;
    month = update.Message.Date.Month;
    day = update.Message.Date.Day;
    hour = update.Message.Date.Hour;
    minute = update.Message.Date.Minute;
    second = update.Message.Date.Second;

    //when receive a message show data and time on console.
    Console.WriteLine("\nData message --> " + year + "/" + month + "/" + day + " - " + hour + ":" + minute + ":" + second);
    //show the message, the chat id and the user info on console.
    Console.WriteLine($"Received a '{messageText}' message in chat {chatId} from user:\n" + firstName + " - " + lastName + " - " + " 5873853");

    //set text all lowercase
    messageText = messageText.ToLower();

    // I insert this if to solve a bug, if you haven't problems you can removed it.
    if (messageText != null && int.Parse(day.ToString()) >= day && int.Parse(hour.ToString()) >= hour && int.Parse(minute.ToString()) >= minute && int.Parse(second.ToString()) >= second - 10)
    {
        //Block vulgarity - it is a numeric variable that corresponds to the 3 block levels, hard, medium and no blocks. You can set the level of the block either by modifying it in the code and restarting the bot, or directly in ghe group chat via the command /vulgarity.

        //code
        //if I write “/vulgarity”, the bot changes the state of the block.
        if (messageText == "/vulgarity")
        {
            switch (blockLevel)
            {
                case 0:
                    blockLevel = 1;
                    await botClient.SendTextMessageAsync
                    (
                    chatId: chatId,
                    text: "Vulgarity: \"Medium block\".",
                     cancellationToken: cancellationToken
                    );
                    return;

                case 1:
                    blockLevel = 2;
                    await botClient.SendTextMessageAsync
                    (
                    chatId: chatId,
                    text: "Vulgarity: \"Hard block\".",
                     cancellationToken: cancellationToken
                    );
                    return;
                case 2:
                    blockLevel = 0;
                    await botClient.SendTextMessageAsync
                    (
                    chatId: chatId,
                    text: "Vulgarity: \"Block disabled\".",
                     cancellationToken: cancellationToken
                    );
                    return;
            }
        }

        //Vulgarity block list - bad words
        for (int x = 0; x < badWords.Length; x++)
        {

            if (messageText.Contains(badWords[x]) && blockLevel == 2 && !messDeleted)
            {
                messDeleted = true;
                await botClient.DeleteMessageAsync(chatId, messageId);
                sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: firstName + " " + lastName + " you can't say that things",
            //replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
            }
        }
        //Vulgarity block list - very bad words
        for (int x = 0; x < veryBadWords.Length; x++)
        {
            if (messageText.Contains(veryBadWords[x]) && (blockLevel == 1 || blockLevel == 2) && !messDeleted)
            {
                messDeleted = true;
                await botClient.DeleteMessageAsync(chatId, messageId);
                sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: firstName + " " + lastName + " you can't say that things",
            //replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
            }
        }
        messDeleted = false;

        //if message is Hello .. bot answer Hello + name of user.
        if (messageText == "hello")
        {
            // Echo received message text
            sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Hello " + firstName + " " + lastName + "",
            //replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
        }

        //if message is "meme" .. bot answer with a meme image.
        if (messageText == "meme")
        {
            sentMessage = await botClient.SendPhotoAsync(
            chatId: chatId,
            photo: "https://i.redd.it/uhkj4abc96r61.jpg",
            caption: "<b>MEME</b>",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
        }

        //if message is "sound" .. bot answer with a Audio.
        if (messageText == "sound")
        {
            Message message = await botClient.SendAudioAsync(
             chatId: chatId,
             audio: "https://github.com/TelegramBots/book/raw/master/src/docs/audio-guitar.mp3",
             cancellationToken: cancellationToken);
        }

        //if message is "countdown" .. bot answer with a countdown video.
        if (messageText == "countdown")
        {
            Message message = await botClient.SendVideoAsync(
            chatId: chatId,
            video: "https://raw.githubusercontent.com/TelegramBots/book/master/src/docs/video-countdown.mp4",
            thumb: "https://raw.githubusercontent.com/TelegramBots/book/master/src/2/docs/thumb-clock.jpg",
            supportsStreaming: true,
            cancellationToken: cancellationToken);
        }

        //if message is "album" .. bot answer with multiple images.
        if (messageText == "album")
        {
            Message[] messages = await botClient.SendMediaGroupAsync(
            chatId: chatId,
            media: new IAlbumInputMedia[]
            {
                new InputMediaPhoto("https://cdn.pixabay.com/photo/2017/06/20/19/22/fuchs-2424369_640.jpg"),
                new InputMediaPhoto("https://cdn.pixabay.com/photo/2017/04/11/21/34/giraffe-2222908_640.jpg"),
            },
            cancellationToken: cancellationToken);
        }

        //if message is "doc" .. bot answer with a doc.
        if (messageText == "doc")
        {
            //Use sendDocument method to send general files.
            Message message = await botClient.SendDocumentAsync(
            chatId: chatId,
            document: "https://github.com/TelegramBots/book/raw/master/src/docs/photo-ara.jpg",
            caption: "<b>Ara bird</b>. <i>Source</i>: <a href=\"https://pixabay.com\">Pixabay</a>",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
        }

        //if message is "album" .. bot answer with multiple images.
        if (messageText == "gif")
        {
            //Use sendAnimation method to send animation files(GIF or H.264 / MPEG - 4 AVC video without sound).
            Message message = await botClient.SendAnimationAsync(
            chatId: chatId,
            animation: "https://raw.githubusercontent.com/TelegramBots/book/master/src/docs/video-waves.mp4",
            caption: "Waves",
            cancellationToken: cancellationToken);
        }


        //if message is "poll" .. create a poll.
        if (messageText == "poll")
        {
            //save the poll id message
            pollId = messageId + 1;

            Console.WriteLine($"\nPoll number: {pollId}!");
            Message pollMessage = await botClient.SendPollAsync(
            chatId: chatId,
            question: "How are you?",
            options: new[]
            {
                "Good!",
                "I could be better.."
            },
            cancellationToken: cancellationToken);
        }
        //if message is "close poll" .. close the pool.
        if (messageText == "close poll")
        {
            Console.WriteLine($"\nPoll number {pollId} is close!");
            Poll poll = await botClient.StopPollAsync(
            chatId: chatId,
            messageId: pollId,
            cancellationToken: cancellationToken);
        }


        /*This is the code to send a contact. Mandatory are the parameters chatId, phoneNumber and firstName.*/
        if (messageText == "send me the phone number of anna")
        {
            Message message = await botClient.SendContactAsync(
            chatId: chatId,
            phoneNumber: "+1234567890",
            firstName: "Anna",
            lastName: "Rossi",
            cancellationToken: cancellationToken);
        }

        //The code snippet below sends a venue with a title and a address as given parameters:
        if (messageText == "roma location")
        {
            Message message = await botClient.SendVenueAsync(
                chatId: chatId,
                latitude: 41.9027835f,
                longitude: 12.4963655,
                title: "Rome",
                address: "Rome, via Daqua 8, 08089",
                cancellationToken: cancellationToken);
        }

        //The code snippet below sends a location:
        if (messageText == "send me a location")
        {
            //The difference between sending a location and a venue is, that the venue requires a title and address. A location can be any given point as latitude and longitude.The following snippet shows how to send a location with the mandatory parameters:

            Message message = await botClient.SendLocationAsync(
                chatId: chatId,
                latitude: 41.9027835f,
                longitude: 12.4963655,
                cancellationToken: cancellationToken);
        }
    }

}

Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}
