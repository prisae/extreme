//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.IO;
using Extreme.Core.Logger;

namespace Extreme.Core
{
    public class FileLogger : BaseLogger
    {
        private readonly StreamWriter _streamWriter;

        public FileLogger(string fileName, bool rewrite)
        {
            if (rewrite)
                if (File.Exists(fileName))
                    File.Delete(fileName);

            _streamWriter = new StreamWriter(fileName);
            _streamWriter.WriteLine($"File logger started at {CreationTime}");
            _streamWriter.Flush();
        }

        private void AppendToFile(string status)
        {
            if (!string.IsNullOrEmpty(status))
            {
                var time = (DateTime.Now - CreationTime).TotalSeconds;
                var str = string.Format($"[{time:######000.00} s] {status}");
                _streamWriter.WriteLine(str);
                _streamWriter.Flush();
            }
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
