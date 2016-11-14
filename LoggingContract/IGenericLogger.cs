using System;
using System.Collections.Generic;

namespace Shared.Logging.Contract
{
    public interface IGenericLogger
    {
        void Info(string message);
        void Warning(string message);
        void Debug(string message);
        void Error(string message);
        void Error(string message, Exception x);
        void Error(Exception x);
        void Fatal(string message);
        void Fatal(Exception x);
        void SetLogLevelToInfo();
        void SetLogLevelToWarning();
        void SetLogLevelToDebug();
        void SetLogLevelToError();
        void SetLogLevelToFatal();
        Action DetermineBaseLogFunction(int level);
        void Error(string message, IDictionary<string, string> customProperties);
        void Info(string message, IDictionary<string, string> customProperties);
        void Warning(string message, IDictionary<string, string> customProperties);
        void Debug(string message, IDictionary<string, string> customProperties);
        void Fatal(string message, IDictionary<string, string> customProperties);
    }
}