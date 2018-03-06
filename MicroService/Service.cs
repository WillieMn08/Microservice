using MicroService.Utils;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Configuration;
using System.Text;

namespace MicroService
{
    public class Service
    {
        public string Attribute { get; set; }

        private readonly ConnectionFactory _factory;

        private readonly IConnection _connection;

        private readonly IModel _channel;

        private readonly Logger _logger;

        public Service()
        {
            _factory = new ConnectionFactory
            {
                HostName = ConfigurationManager.AppSettings["RabbitMQServer"]
            };

            _connection = _factory.CreateConnection();

            _channel = _connection.CreateModel();

            _logger = new Logger();
        }

        public void Start()
        {
            _logger.Info($"Microservice has been Started");

            var OPERATION_NAME = "OPERATION_NAME";

            _channel.ExchangeDeclare(exchange: OPERATION_NAME, type: "direct");

            var queueName = _channel.QueueDeclare().QueueName;

            //Testing with two routing keys
            var KEY_1 = "KEY_1";

            var KEY_2_WITH_PARAMETER = "KEY_2_WITH_PARAMETER";

            _channel.QueueBind(queue: queueName, exchange: OPERATION_NAME, routingKey: KEY_1);

            _channel.QueueBind(queue: queueName, exchange: OPERATION_NAME, routingKey: KEY_2_WITH_PARAMETER);

            var basicConsumer = new EventingBasicConsumer(_channel);

            basicConsumer.Received += (model, e) =>
            {
                try
                {
                    var connectionString = ConfigurationManager.ConnectionStrings["Connection"].ToString();

                    var service = new Microservice();

                    if (e.RoutingKey == KEY_1)
                    {
                        //Do Something
                        service.Task(connectionString);
                    }
                    else if (e.RoutingKey == KEY_2_WITH_PARAMETER)
                    {
                        var body = e.Body;

                        var message = Encoding.UTF8.GetString(body);

                        var code = int.Parse(message);

                        //Do Something
                        service.Task(connectionString, code);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to receiving consumer data. {ErrorHandler.Handle(ex)}");
                }
            };

            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: basicConsumer);
        }

        public void Stop()
        {
            _channel.Close();
            _connection.Close();

            _logger.Info($"Microservice has been Stopped");
        }
    }
}
