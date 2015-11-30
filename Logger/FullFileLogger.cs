﻿using System;
using System.IO;
using Extreme.Core.Logger;

namespace Extreme.Core
{
    public class FullFileLogger : BaseLogger
    {
        private readonly string _fileName;

        private readonly StreamWriter _streamWriter;

        public FullFileLogger(string fileName, bool rewrite)
        {
            _fileName = fileName;

            if (rewrite)
                if (File.Exists(fileName))
                    File.Delete(fileName);

            _streamWriter = new StreamWriter(fileName);
            
            _streamWriter.WriteLine($"File logger started at {CreationTime}");
<<<<<<< HEAD
            _streamWriter.Flush();
=======
>>>>>>> origin/Inversion
            //File.AppendAllText(_fileName, );
        }

        private void AppendToFile(string status)
        {
            string str = GetString(status);

            if (!string.IsNullOrEmpty(status))
<<<<<<< HEAD
            {
                _streamWriter.WriteLine(str);
                _streamWriter.Flush();
            }
            //File.AppendAllText(_fileName, str);
=======
                _streamWriter.WriteLine(str);
                //File.AppendAllText(_fileName, str);
>>>>>>> origin/Inversion
        }

        private string GetString(string status)
        {
            var time = (DateTime.Now - CreationTime).TotalSeconds;
            return string.Format($"[{time:######000.00} s] {status}");
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

        public override void Dispose()
        {
            base.Dispose();
            _streamWriter.Dispose();
        }
    }
}
