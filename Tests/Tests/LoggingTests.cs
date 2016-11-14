using System;
using Shared.Logging;
using Shared.Logging.Contract;
using NUnit.Framework;
using Tests.Helper;

namespace Tests.Tests
{
    [TestFixture]
    public class LoggingTests
    {
        private readonly IGenericLogger _logger;

        public LoggingTests()
        {
            _logger = new NLogLogger();
        }

        private static string[] GetTestingOutputFileLines => System.IO.File.ReadAllLines(FileHelper.GetTestFilePath(@"\Results", "NlogTestOutput.log"));

        //Given I have a message 
        //When I log it as debug message using nlog
        //Then if the log level is set to a lower value it will write the message to the appenders
        [Test]
        public void WriteMessageAsDebugTest()
        {
            TestStandardMessageWrite(_logger.Debug, "This is a debug message");
        }

        //Given I have a message 
        //When I log it as error message using nlog
        //Then if the log level is set to a lower value it will write the message to the appenders
        [Test]
        public void WriteMessageAsErrorTest()
        {
            TestStandardMessageWrite(_logger.Error, "This is a debug message");
        }

        //Given I have a message 
        //When I log it as error message using nlog
        //Then if the log level is set to a lower value it will write the message to the appenders
        [Test]
        public void WriteMessageAsErrorWithExceptionTest()
        {
            LoggerTestHelper.RunForcedExceptionWithMessage(_logger.Error);
        }

        //Given I have an exception 
        //When I log it as error using using nlog
        //Then if the log level is set to a lower value it will write the message to the appenders
        [Test]
        public void WriteExceptionErrorTest()
        {
            LoggerTestHelper.RunForcedException(_logger.Error);
        }

        //Given I have a message 
        //When I log it as Fatal message using nlog
        //Then if the log level is set to a lower value it will write the message to the appenders
        [Test]
        public void WriteMessageAsFatalTest()
        {
            TestStandardMessageWrite(_logger.Fatal, "This is a debug message");
        }

        //Given I have an exception
        //When I log it as Fatal message using nlog
        //Then if the log level is set to a lower value it will write the message to the appenders
        [Test]
        public void WriteExceptionAsFatalTest()
        {
            LoggerTestHelper.RunForcedException(_logger.Fatal);
        }

        //Given I have a message 
        //When I log it as Info message using nlog
        //Then if the log level is set to a lower value it will write the message to the appenders
        [Test]
        public void WriteMessageAsInfoTest()
        {
            TestStandardMessageWrite(_logger.Info, "This is a debug message");
        }

        //Given I have a log level of Error 
        //When I try to log an info message
        //Then it will not be written to the file
        [Test]
        public void ReconfigurationLogLevelWillNotWriteTest()
        {
            //Given
            var numberOfExistingMessages = GetTestingOutputFileLines.Length;
            LogAndCheckInfoMessage();
            _logger.SetLogLevelToFatal();

            //When
            _logger.Info("I will never be displayed in the file");

            //Then
            _logger.SetLogLevelToDebug();
            LogAndCheckInfoMessage();
            Assert.AreEqual(numberOfExistingMessages + 2, GetTestingOutputFileLines.Length, "Messages that should have been logged have not been logged");
        }

        //Given I have a log level of Error 
        //When I try to log a fatal message
        //Then it will be written to the file
        [Test]
        public void ReconfigurationLogLevelWillWriteTest()
        {
            //Given
            var numberOfExistingMessages = GetTestingOutputFileLines.Length;
            LogAndCheckInfoMessage();
            _logger.SetLogLevelToError();

            //When
            TestStandardMessageWrite(_logger.Fatal, "You should be able to see this");

            //Then
            _logger.SetLogLevelToDebug();
            LogAndCheckInfoMessage();
            Assert.AreEqual(numberOfExistingMessages + 3, GetTestingOutputFileLines.Length, "Messages that should have been logged have not been logged");
        }

        private void TestStandardMessageWrite(Action<string> writeLog, string message)
        {
            //Given
            var numberOfExistingMessages = GetTestingOutputFileLines.Length;

            //When
            writeLog(message);

            //Then
            Assert.IsTrue(GetTestingOutputFileLines[numberOfExistingMessages].Contains(message), "The message did not match the one written");
        }

        private void LogAndCheckInfoMessage()
        {
            TestStandardMessageWrite(_logger.Info, "You should be able to see this");
        }
    }
}
