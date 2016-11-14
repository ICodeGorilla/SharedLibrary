using System;
using System.IO;
using System.Linq;

namespace Shared.Helper.Test
{
    public static class FileHelper
    {
        public static string GetTestFilePath(string relativeFolder, string testFile)
        {
            var startupPath = AppDomain.CurrentDomain.BaseDirectory;
            var pathItems = startupPath.Split(Path.DirectorySeparatorChar);
            var projectPath = string.Join(Path.DirectorySeparatorChar.ToString(), pathItems.Take(pathItems.Length - 3));
            return Path.Combine(projectPath, relativeFolder, testFile);
        }

        public static byte[] GetTestFileBytes(string testFile)
        {
            return File.ReadAllBytes(GetTestFilePath("TestFiles", testFile));
        }

        public static byte[] GetEmailTestFileBytes(string testFile)
        {
            return File.ReadAllBytes(GetTestFilePath(@"TestFiles\EmailtestFiles", testFile));
        }
    }
}