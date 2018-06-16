namespace OpenId.LocalAuthorizationServer
{
    using System;
    using Microsoft.Extensions.Logging;

    public sealed class Logger : ILogger
    {
        private readonly Serilog.ILogger logger;

        public Logger(Serilog.ILogger logger)
        {
            this.logger = logger;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            var disposableState = state as IDisposable;
            return disposableState;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var messageFormat = formatter(state, exception);

            switch (logLevel)
            {
                case LogLevel.Error:
                    logger.Error(messageFormat);
                    break;

                case LogLevel.Information:
                    logger.Information(messageFormat);
                    break;

                case LogLevel.Warning:
                    logger.Warning(messageFormat);
                    break;
            }
        }
    }
}