//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extreme.Core
{
    public class LinuxMemoryFileReader
    {
        public struct MemoryInfo
        {
            public long Size;
            public long Resident;
            public long Share;
            public long Text;
            public long Lib;
            public long DataLoc;
            public long Dt;
        }

        public const string DefaultPath = @"/proc/self/statm";
        public const int DefaultPageSize = 4096;

        public static MemoryInfo ReadFile()
            => ReadFile(DefaultPath);

        public static MemoryInfo ReadFile(string fileName)
        {
            if (!File.Exists(fileName))
                return new MemoryInfo();

            var str = File.ReadAllLines(fileName)[0];
            var splitted = str.Split(' ');

            return new MemoryInfo()
            {
                Size = long.Parse(splitted[0]),
                Resident = long.Parse(splitted[1]),
                Share = long.Parse(splitted[2]),
                Text = long.Parse(splitted[3]),
                Lib = long.Parse(splitted[4]),
                DataLoc = long.Parse(splitted[5]),
                Dt = long.Parse(splitted[6]),
            };
        }

        public static decimal GetTotalMemoryInMiB()
        {
            var info = ReadFile();

            return info.DataLoc * DefaultPageSize / (1024 * 1024M);
        }
    }
}
