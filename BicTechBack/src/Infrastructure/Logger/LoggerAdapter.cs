using BicTechBack.src.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BicTechBack.src.Infrastructure.Logging
{
    public class LoggerAdapter<T> : IAppLogger<T>
    {
        private readonly ILogger<T> _logger;

        public LoggerAdapter(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message, params object[] args) =>
            _logger.LogInformation(message, args);

        public void LogWarning(string message, params object[] args) =>
            _logger.LogWarning(message, args);

        public void LogError(string message, params object[] args) =>
            _logger.LogError(message, args);

        public void LogError(Exception exception, string message, params object[] args) =>
            _logger.LogError(exception, message, args);
    }
}