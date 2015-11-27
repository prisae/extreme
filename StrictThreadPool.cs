using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Extreme.Core
{



    public class StrictThreadPool
    {
        private class Task
        {
            public int StartInclusive { get; }
            public int EndExclusive { get; }
        
            public Task(int startInclusive, int endExclusive)
            {
                StartInclusive = startInclusive;
                EndExclusive = endExclusive;
            }
        }

        private readonly Thread[] _threads;
        private readonly Task[] _tasks;

        private bool _disposed = false;

        private readonly object _syncObject = new object();

        private readonly int _numberOfThreads;

        private Action<int> _currentAction;

        public StrictThreadPool(int numberOfThreads)
        {
            if (numberOfThreads < 1) throw new ArgumentOutOfRangeException(nameof(numberOfThreads));

            _numberOfThreads = numberOfThreads;

            _threads = new Thread[numberOfThreads - 1];
            _tasks = new Task[numberOfThreads - 1];

            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i] = new Thread(RunThreadWorkflow) { Name = $"Worker {i + 1} of {numberOfThreads}" };
                _threads[i].Start();
            }
        }


        public void Run(int startInclusive, int endExclusive, Action<int> action)
        {
            if (_numberOfThreads == 1)
                for (int i = startInclusive; i < endExclusive; i++)
                    action(i);

            else
            {
                _currentAction = action;
                Monitor.PulseAll(_syncObject);
            }
        }

        
        private void RunThreadWorkflow()
        {
            while (true)
            {
                Monitor.Wait(_syncObject);

            }
        }


    }
}
