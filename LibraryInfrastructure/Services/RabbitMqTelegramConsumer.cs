using LibraryDomain.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Telegram.Bot;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using System.Text;

namespace LibraryInfrastructure.Services
{
    public class RabbitMqTelegramConsumer : BackgroundService
    {
        private readonly string _hostName = "localhost";
        private readonly string _queueName = "books_queue";
        private readonly string _botToken;
        private readonly long _chatId;

        public RabbitMqTelegramConsumer(IConfiguration configuration)
        {
            _botToken = configuration["TelegramBot:Token"]!;
            _chatId = long.Parse(configuration["TelegramBot:ChatId"]!);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory() { HostName = _hostName };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<BookCreatedMessage>(Encoding.UTF8.GetString(body));

                if (message != null)
                {
                    await SendTelegramMessage(message);
                }
            };

            channel.BasicConsume(queue: _queueName,
                                 autoAck: true,
                                 consumer: consumer);

            return Task.CompletedTask;
        }

        private async Task SendTelegramMessage(BookCreatedMessage message)
        {
            var bot = new TelegramBotClient(_botToken);

            string text = $" New Book Added:\n" +
                          $"Title: {message.Title}\n";

            await bot.SendMessage(chatId: _chatId, text: text);
        }
    }
}
