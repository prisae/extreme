using System;
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
        }

        private void AppendToFile(string status)
        {
            //_streamWriter.WriteLine($"status={status}");

    

            //_streamWriter.WriteLine("Test1");
            //_streamWriter.WriteLine("Test333");
            //_streamWriter.WriteLine("Test44444");
            //_streamWriter.WriteLine($"Test5555555{(DateTime.Now - CreationTime).TotalSeconds} {status}");

            //string str = GetString(status);

            //_streamWriter.WriteLine("Test2");

            if (!string.IsNullOrEmpty(status))
            {
                var time = (DateTime.Now - CreationTime).TotalSeconds;
                var str = string.Format($"[{time:######000.00} s] {status}");
                _streamWriter.WriteLine(str);
                
              //  _streamWriter.WriteLine(str);
                //_streamWriter.Flush();
                //File.AppendAllText(_fileName, str);
            }
        }

        private string GetString(string status)
        {
            _streamWriter.WriteLine("GetString 1");
            var time = (DateTime.Now - CreationTime).TotalSeconds;
            _streamWriter.WriteLine("GetString 2");
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
