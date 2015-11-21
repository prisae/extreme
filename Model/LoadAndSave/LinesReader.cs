using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extreme.Cartesian.Model
{
    public class LinesReader : IDisposable
    {
        private readonly StreamReader _sr;

        public LinesReader(string fileName)
        {
            _sr = new StreamReader(fileName);
        }

        public string[] ReadNextLines(int count)
        {
            var result = new string[count];

            for (int i = 0; i < count; i++)
                result[i] = _sr.ReadLine();

            return result;
        }

        public void Dispose()
        {
            _sr.Dispose();
        }
    }
}
