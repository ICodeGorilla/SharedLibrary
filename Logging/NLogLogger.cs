using System;
using System.Collections.Generic;
using NLog;
using NLog.Config;
using Shared.Logging.Contract;

namespace Shared.Logging
{
    public class NLogLogger : IGenericLogger
    {
        private readonly string _loggerName;

        public NLogLogger(string loggerName)
        {
            _loggerName = loggerName;
        }

        public NLogLogger()
        {
            _loggerName = "Standard logger";
        }

        private Logger Logger => LogManager.GetLogger(_loggerName);

        private static IEnumerable<LogLevel> AllLogLevels => new[]
        {LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal};

        public void Info(string message)
        {
            Logger.Info(message);
        }

        public void Warning(string message)
        {
            Logger.Warn(message);
        }

        public void Debug(string message)
        {
            Logger.Debug(message);
        }

        public void Error(string message)
        {
            Logger.Error(message);
        }

        public void Error(Exception x)
        {
            Logger.Error(x);
        }

        public void Error(string message, Exception x)
        {
            Logger.Error(x, message);
        }

        public void Fatal(string message)
        {
            Logger.Fatal(message);
        }

        public void Fatal(Exception x)
        {
            Logger.Fatal(x);
        }

        public void SetLogLevelToInfo()
        {
            Reconfigure(LogLevel.Info);
        }

        public void SetLogLevelToWarning()
        {
            Reconfigure(LogLevel.Warn);
        }

        public void SetLogLevelToDebug()
        {
            Reconfigure(LogLevel.Debug);
        }

        public void SetLogLevelToError()
        {
            Reconfigure(LogLevel.Error);
        }

        public void SetLogLevelToFatal()
        {
            Reconfigure(LogLevel.Fatal);
        }

        public Action DetermineBaseLogFunction(int level)
        {
            switch (level)
            {
                case 1: // LogLevel.Debug.Ordinal:
                    return SetLogLevelToDebug;
                case 2: // LogLevel.Info.Ordinal:
                    return SetLogLevelToInfo;
                case 3: // LogLevel.Warn.Ordinal:
                    return SetLogLevelToWarning;
                case 4: //LogLevel.Error.Ordinal:
                    return SetLogLevelToError;
                case 5: //LogLevel.Fatal.Ordinal:
                    return SetLogLevelToFatal;
                default:
                    return SetLogLevelToError;
            }
        }

        public void Error(string message, IDictionary<string, string> customProperties)
        {
            LogWithCustomProperties(message, customProperties, LogLevel.Error);
        }

        public void Info(string message, IDictionary<string, string> customProperties)
        {
            LogWithCustomProperties(message, customProperties, LogLevel.Info);
        }

        public void Warning(string message, IDictionary<string, string> customProperties)
        {
            LogWithCustomProperties(message, customProperties, LogLevel.Warn);
        }

        public void Debug(string message, IDictionary<string, string> customProperties)
        {
            LogWithCustomProperties(message, customProperties, LogLevel.Debug);
        }

        public void Fatal(string message, IDictionary<string, string> customProperties)
        {
            LogWithCustomProperties(message, customProperties, LogLevel.Fatal);
        }

        private static void Reconfigure(LogLevel logLevel)
        {
            UpdateAllRules(logLevel);
            LogManager.ReconfigExistingLoggers();
            //Call to update existing Loggers created with GetLogger() or GetCurrentClassLogger()
        }

        private static void UpdateAllRules(LogLevel logLevel)
        {
            foreach (var rule in LogManager.Configuration.LoggingRules)
            {
                UpdateAllLevels(logLevel, rule);
            }
        }

        private static void UpdateAllLevels(LogLevel logLevel, LoggingRule rule)
        {
            foreach (var loglevelToCheck in AllLogLevels)
            {
                SetLoggingLevel(logLevel, loglevelToCheck, rule);
            }
        }

        private static void SetLoggingLevel(LogLevel levelSet, LogLevel levelBeingSet, LoggingRule rule)
        {
            if (levelBeingSet.Ordinal >= levelSet.Ordinal)
            {
                rule.EnableLoggingForLevel(levelBeingSet);
            }
            else
            {
                rule.DisableLoggingForLevel(levelBeingSet);
            }
        }

        private void LogWithCustomProperties(string message, IDictionary<string, string> customProperties,
            LogLevel logLevel)
        {
            var logEvent = new LogEventInfo(logLevel, _loggerName, message);
            foreach (var customProperty in customProperties)
            {
                logEvent.Properties[customProperty.Key] = customProperty.Value;
            }
            Logger.Log(logEvent);
        }
    }
}