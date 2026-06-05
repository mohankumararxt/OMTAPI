using OMT.DataService.Interface;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace OMT.DataService.Service
{
    public class TxtFileLogger : IActivityLogger
    {
        private readonly string _logFilePath;

        //public TxtFileLogger(IWebHostEnvironment env)
        //{
        //    _logFilePath = Path.Combine(env.ContentRootPath, "Logs", "activity_log.txt");
        //    Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));
        //}

        public TxtFileLogger(IHostEnvironment env)
        {
            var logFolder = Path.Combine(env.ContentRootPath, "Logs");

            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            _logFilePath = Path.Combine(logFolder, "activity_log.txt");
        }

        public void Log(string message)
        {
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
        }
    }

}
