using Dapper;
using MicroService.Models;
using MicroService.Utils;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;

namespace MicroService
{
    public class Microservice
    {
        private readonly Logger _logger;

        public Microservice()
        {
            _logger = new Logger();
        }

        public void Task(string connectionString)
        {
            _logger.Info($"Starting");

            try
            {
                ExecuteProcedure(connectionString);

                CallMicroService(true);

                _logger.Info($"Task has been completed.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed process. {ErrorHandler.Handle(ex)}.");
            }
        }

        public void Task(string connectionString, int departmentId)
        {
            _logger.Info($"Starting");

            try
            {
                GetUsers(connectionString, departmentId);

                CallMicroService(true, departmentId);

                _logger.Info($"Task has been completed.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed process. {ErrorHandler.Handle(ex)}.");
            }
        }

        private void CallMicroService(bool state, int code = 0)
        {
            _logger.Info($"Starting call another microservice.");

            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = ConfigurationManager.AppSettings["RabbitMQServer"]
                };

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var exchange = ConfigurationManager.AppSettings["OUTExchange"];

                    if (state)
                    {
                        var routingKey = ConfigurationManager.AppSettings["OUTKey_1"];
                        channel.BasicPublish(exchange: exchange, routingKey: routingKey);
                    }
                    else
                    {
                        var body = Encoding.UTF8.GetBytes(code.ToString());
                        var routingKey = ConfigurationManager.AppSettings["OUTKey_2_With_Parameter"];
                        channel.BasicPublish(exchange: exchange, routingKey: routingKey, body: body);
                    }
                }

                _logger.Info($"Call successfully.");
            }
            catch (Exception e)
            {
                _logger.Error($"Failed. {ErrorHandler.Handle(e)}.");
            }
        }

        private void ExecuteProcedure(string connectionString)
        {
            _logger.Info($"Starting Query");

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    connection.Execute($"EXEC [dbo].[PROCEDURE]", commandTimeout: 0);

                    _logger.Info($"Procedure has been completed.");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed procedure. {ErrorHandler.Handle(ex)}.");

                    throw ex;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private IEnumerable<User> GetUsers(string connectionString, int departmentId)
        {
            _logger.Info($"Starting Query");

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    return connection.Query<User>(@"EXEC [dbo].[PROCEDURE] @departmentId", new
                    {
                        //Parameters
                        departmentId = 0
                    },commandTimeout: 0);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed procedure. {ErrorHandler.Handle(ex)}.");

                    throw ex;
                }
                finally
                {
                    connection.Close();

                    _logger.Info($"Procedure has been completed.");
                }
            }
        }

    }
}
