using System.Configuration;
using Topshelf.Logging;

namespace MicroService.Utils
{
    public class Logger
    {
        private readonly LogWriter _logWriter;

        private readonly string _serviceName;

        public Logger()
        {
            _logWriter = HostLogger.Get<Service>();

            _serviceName = ConfigurationManager.AppSettings["ServiceName"];
        }

        public void Info(string message)
        {
            _logWriter.Info($"[{_serviceName}] {message}");
        }

        public void Error(string message)
        {
            _logWriter.Error($"[{_serviceName}] {message}");
        }
    }
}
