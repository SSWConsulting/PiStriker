using Autofac;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Formatting.Raw;

namespace PiStriker
{
    public class SerilogModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => SerilogConfig.CreateLogger()).As<ILogger>()
                .SingleInstance();
        }


        public class SerilogConfig
        {
            public static ILogger CreateLogger()
            {
                var config = new LoggerConfiguration()
                    .MinimumLevel.Verbose();

                config = config.WriteTo.Sink(new ConsoleSink(new JsonFormatter()));


                Log.Logger = config.CreateLogger();
                return Log.Logger;
            }
        }
    }
}