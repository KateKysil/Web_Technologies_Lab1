using ClosedXML.Excel;
using LibraryDomain.Model;
using LibraryInfrastructure;
using LibraryInfrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

public class TelegramBotService
{
    private readonly TelegramBotClient _botClient;
    private readonly LibraryContext _db;

    public TelegramBotService(LibraryContext db, string botToken)
    {
        _db = db;
        _botClient = new TelegramBotClient(botToken);
    }

    public void Start()
    {
        var cts = new CancellationTokenSource();

        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions() { AllowedUpdates = { } }, // all updates
            cancellationToken: cts.Token
        );

        Console.WriteLine("Telegram Bot Started");
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        if (update.Type != Telegram.Bot.Types.Enums.UpdateType.Message) return;
        var message = update.Message!;
        var chatId = message.Chat.Id;

        if (message.Text == null) return;

        if (message.Text.StartsWith("/books"))
        {
            var books = await _db.Books
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres).ThenInclude(bg => bg.Genre)
                .ToListAsync();

            foreach (var book in books)
            {
                string authors = string.Join(", ", book.BookAuthors.Select(a => $"{a.Author.FirstName} {a.Author.LastName}"));
                string genres = string.Join(", ", book.BookGenres.Select(g => g.Genre.GenreName));

                string text = $" <b>{book.Title}</b>\n" +
                              $"ISBN: {book.Isbn}\n" +
                              $"Publisher: {book.Publisher.PublisherName}\n" +
                              $"Authors: {authors}\n" +
                              $"Genres: {genres}";

                if (!string.IsNullOrEmpty(book.BookCoverUrl))
                {
                    await botClient.SendPhotoAsync(
                        chatId: message.Chat.Id,
                        photo: book.BookCoverUrl,
                        caption: text,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                    );
                }
                else
                {
                    await botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: text,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                    );
                }
            }
        }
        else if (message.Text.StartsWith("/publisher_excel"))
        {
            // Generate Excel in memory
            using var stream = new MemoryStream();
            using var workbook = new XLWorkbook();

            var publishers = await _db.Publishers
                .Include(p => p.Books)
                    .ThenInclude(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                .ToListAsync();

            foreach (var publisher in publishers)
            {
                var worksheet = workbook.Worksheets.Add(publisher.PublisherName);

                worksheet.Cell(1, 1).Value = "Title";
                worksheet.Cell(1, 2).Value = "ISBN";
                worksheet.Cell(1, 3).Value = "Authors";

                worksheet.Row(1).Style.Font.Bold = true;

                int row = 2;
                foreach (var book in publisher.Books)
                {
                    worksheet.Cell(row, 1).Value = book.Title;
                    worksheet.Cell(row, 2).Value = book.Isbn;

                    var authors = book.BookAuthors.Select(ba => ba.Author.FirstName + " " + ba.Author.LastName);
                    worksheet.Cell(row, 3).Value = string.Join(", ", authors);
                    row++;
                }

                worksheet.Columns().AdjustToContents();
            }

            workbook.SaveAs(stream);
            stream.Position = 0;

            // Send file to user
            await botClient.SendDocumentAsync(
                chatId,
                new Telegram.Bot.Types.InputFileStream(stream, "publishers.xlsx"),
                caption: "Publishers and Books"
            );
        }
        else
        {
            await botClient.SendMessage(chatId,
                "Commands:\n/books - list all books\n/publisher_excel - sends excel file of books sorted in pages by publishers");
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken token)
    {
        Console.WriteLine(exception);
        return Task.CompletedTask;
    }
}
