using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        const string ExchangeName = "trigger";

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout);

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                Console.WriteLine(">>> Connected to message bus");
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> Couldn't connect to the message bus: {ex.Message}");
            }

        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine(">>> RabbitMQ connection shutdown");
        }

        public void PublishPlatformEvent(PlatformCreateEventDto platformPublishedDto)
        {
            var message = JsonSerializer.Serialize(platformPublishedDto);
            if (_connection.IsOpen)
            {
                Console.WriteLine(">>> RabbitMQ connection is open");
                SendMessage(message);
            }
            else
            {
                Console.WriteLine(">>> RabbitMQ connection is close");
            }
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: ExchangeName, routingKey: String.Empty, basicProperties: null, body: body);

            Console.WriteLine($">>> Message sent: {message}");
        }

        public void Dispose()
        {
            Console.WriteLine(">>> Message bus disposed");
            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }
    }
}