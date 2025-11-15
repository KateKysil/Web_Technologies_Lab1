using LibraryDomain.Model;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace LibraryInfrastructure
{
    public class RabbitMqPublisher
    {
        private readonly string _hostName = "localhost"; // change if needed
        private readonly string _queueName = "books_queue";

        public void Publish(BookCreatedMessage message)
        {
            var factory = new ConnectionFactory() { HostName = _hostName };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            channel.BasicPublish(exchange: "",
                                 routingKey: _queueName,
                                 basicProperties: null,
                                 body: body);
        }
    }
}
