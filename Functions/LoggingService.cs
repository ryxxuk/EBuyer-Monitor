﻿using System;
using System.IO;

namespace Site_Monitor_Base.Functions
{
    public class LoggingService
    {
        private readonly string _logDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, "logs");

        private static LoggingService _outputToFileSingleton;
        private static LoggingService outputToFileSingleton
        {
            get { return _outputToFileSingleton ??= new LoggingService(); }
        }

        public StreamWriter  sw { get; set; }

        public LoggingService()
        {
            EnsureLogDirectoryExists();
            InstantiateStreamWriter();
        }

        ~LoggingService()
        {
            if (sw == null) return;
            try
            {
                sw.Dispose();
            }
            catch (ObjectDisposedException) { } // object already disposed - ignore exception
        }

        public static void WriteLine(string str)
        {
            Console.WriteLine(str);
            outputToFileSingleton.sw.WriteLine("\n" + str);
        }

        public static void Write(string str)
        {
            Console.Write(str);
            outputToFileSingleton.sw.Write(str);
        }

        private void InstantiateStreamWriter()
        {
            var filePath = Path.Combine(_logDirPath, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")) + ".txt";
            try
            {
                sw = new StreamWriter(filePath)
                {
                    AutoFlush = true
                };
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new ApplicationException(
                    $"Access denied. Could not instantiate StreamWriter using path: {filePath}.", ex);
            }
        }

        private void EnsureLogDirectoryExists()
        {
            if (Directory.Exists(_logDirPath)) return;

            try
            {
                Directory.CreateDirectory(_logDirPath);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new ApplicationException(
                    $"Access denied. Could not create log directory using path: {_logDirPath}.", ex);
            }
        }
    }
}
