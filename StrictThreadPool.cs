using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Extreme.Core
{



    public class StrictThreadPool
    {
        private readonly Thread[] _threads;

        private bool _disposed = false;

        public StrictThreadPool(int numberOfThreads)
        {
            if (numberOfThreads < 1) throw new ArgumentOutOfRangeException(nameof(numberOfThreads));

            _threads = new Thread[numberOfThreads - 1];

            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i] = new Thread(RunThreadWorkflow) { Name = $"Worker {i + 1} of {numberOfThreads}" };
                _threads[i].Start();
            }
        }


        
        private void RunThreadWorkflow()
        {
            //Action task = null;
            //while (true) // loop until threadpool is disposed
            //{
            //    Task.WaitAll()
            //    Monitor.Enter();

            //    lock (this._tasks) // finding a task needs to be atomic
            //    {
            //        while (true) // wait for our turn in _workers queue and an available task
            //        {
            //            if (_disposed)
            //                return;

            //            // we can only claim a task if its our turn (this worker thread is the first entry in _worker queue) and there is a task available
            //            if (null != _threads.First && object.ReferenceEquals(Thread.CurrentThread, this._workers.First.Value) && this._tasks.Count > 0)
            //            {
            //                task = this._tasks.First.Value;
            //                this._tasks.RemoveFirst();
            //                this._workers.RemoveFirst();
            //                // pulse because current (First) worker changed (so that next available sleeping worker will pick up its task)
            //                Monitor.PulseAll(this._tasks);
            //                // we found a task to process, break out from the above 'while (true)' loop
            //                break; 
            //            }
            //            Monitor.Wait(this._tasks); // go to sleep, either not our turn or no task to process
            //        }
            //    }

            //    task(); // process the found task
            //    lock (this._tasks)
            //    {
            //        _threads.AddLast(Thread.CurrentThread);
            //    }
            //    task = null;
            //}
        }


    }
}
