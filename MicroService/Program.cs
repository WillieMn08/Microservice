using Serilog;
using System.Configuration;
using Topshelf;

namespace MicroService
{
    class Program
    {
        public static void Main(string[] args)
        {
            var attribute = string.Empty;

            var serviceName = ConfigurationManager.AppSettings["ServiceName"];

            var seqServerUrl = ConfigurationManager.AppSettings["SeqServer"];

            ILogger config = new LoggerConfiguration()
                            .WriteTo.ColoredConsole()
                            .WriteTo.Seq(seqServerUrl)
                            .CreateLogger();

            HostFactory.Run(x =>
            {
                x.Service<Service>(s =>
                {
                    s.ConstructUsing(name => new Service
                    {
                        Attribute = attribute
                    });

                    s.WhenStarted(a => a.Start());

                    s.WhenStopped(a => a.Stop());
                });

                x.UseSerilog(config);

                x.RunAsLocalSystem();

                x.SetDescription("Description about microservice");

                x.SetDisplayName(serviceName);

                x.SetServiceName(serviceName);

                x.StartAutomatically();
            });
        }
    }
}
