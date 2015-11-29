using System;
using System.IO;
using Extreme.Core.Logger;

namespace Extreme.Core
{
    public class FullFileLogger : BaseLogger
    {
        private readonly string _fileName;

        public FullFileLogger(string fileName, bool rewrite)
        {
            _fileName = fileName;

            if (rewrite)
                if (File.Exists(fileName))
                    File.Delete(fileName);


            File.AppendAllText(_fileName, $"File logger started at {CreationTime}");
        }

        private void AppendToFile(string status)
        {
            string str = GetString(status);

            if (!string.IsNullOrEmpty(status))
                File.AppendAllText(_fileName, str);
        }

        private string GetString(string status)
        {
            var time = (DateTime.Now - CreationTime).TotalSeconds;
            return string.Format($"[{time:######000.00} s] {status}\n");
        }

        public override void Write(int logLevel, string message)
        {
            if (!Filter(logLevel))
            {
                var prefix = this.GetPrefix(logLevel);

                AppendToFile(prefix + message);
            }
        }

        protected virtual bool Filter(int logLevel) => false;
    }
}
