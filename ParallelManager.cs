using System;
using System.Collections.Generic;
using Extreme.Core;

namespace Extreme.Parallel
{
    public class ParallelManager<T>
    {
        private readonly List<T> _tasks = new List<T>();

        private readonly int _size;
        private readonly int _rank;


        public ParallelManager(Mpi mpi)
        {
            _size = mpi.Size;
            _rank = mpi.Rank;
        }


        public ParallelManager(int size, int rank)
        {
            _size = size;
            _rank = rank;
        }

        public ParallelManager<T> WithTasks(params T[] task)
        {
            _tasks.AddRange(task);

            return this;
        }

        public void Run(Action<T[]> run)
        {
            var localTasks = GetLocalTasks(_tasks, _rank);
            run(localTasks);
        }

        public int[] GetAllStartIndecies()
        {
            var result = new int[_size];

            for (int i = 0; i < result.Length; i++)
                result[i] = GetStartIndex(_tasks, i);

            return result;
        }


        public int[] GetAllLength()
        {
            var result = new int[_size];

            for (int i = 0; i < result.Length; i++)
                result[i] = GetLength(_tasks, i);

            return result;
        }

        private T[] GetLocalTasks(List<T> tasks, int rank)
        {
            if (tasks.Count < _size)
            {
                if (rank < tasks.Count)
                    return new[] { tasks[rank] };

                return new T[0];
            }

            int start = GetStartIndex(tasks, rank);
            int length = GetLength(tasks, rank);

            var range = tasks.GetRange(start, length);
            return range.ToArray();
        }

        private int GetStartIndex(List<T> tasks, int rank)
        {
            int localSize = tasks.Count / _size;
            int reminder = tasks.Count - localSize * _size;

            return rank < reminder ? (localSize + 1) * rank :
                                     (localSize + 1) * reminder + localSize * (rank - reminder);
        }

        private int GetLength(List<T> tasks, int rank)
        {
            int localSize = tasks.Count / _size;
            int reminder = tasks.Count - localSize * _size;

            return rank < reminder ? localSize + 1 : localSize;
        }
    }
}
