namespace Extreme.Parallel
{
    public static class ParallelUtils
    {
        public static int DetermineLength(int totalLayers, Mpi mpi)
        {
            return DetermineLength(totalLayers, mpi.Size, mpi.Rank);
        }

        public static int DetermineStart(int totalLayers, Mpi mpi)
        {
            return DetermineStart(totalLayers, mpi.Size, mpi.Rank);
        }

        //public static int DetermineLength(X3DModel model, Mpi parallel)
        //{
        //    return DetermineLength(model.Anomaly.Layers.Count, parallel);
        //}

        //public static int DetermineStart(X3DModel model, Mpi parallel)
        //{
        //    return DetermineStart(model.Anomaly.Layers.Count, parallel);
        //}

        public static int DetermineStart(int totalLayers, int size, int rank)
        {
            int step = totalLayers / size;

            return step * rank;
        }

        public static int DetermineLength(int totalLayers, int size, int rank)
        {
            int step = totalLayers / size;

            if (rank == size - 1)
            {
                return totalLayers - step * rank;
            }

            return step;
        }

        public static StartAndLength[] CalculateStartAndLengthForAllRanks(int totalLayers, Mpi mpi)
        {
            int size = mpi.Size;
            
            var result = new StartAndLength[size];

            for (int rank = 0; rank < result.Length; rank++)
            {
                int start = DetermineStart(totalLayers, size, rank);
                int length = DetermineLength(totalLayers, size, rank);

                result[rank] = new StartAndLength(start, length);
            }

            return result;
        }

        public class StartAndLength
        {
            private readonly int _start;
            private readonly int _length;

            public StartAndLength(int start, int length)
            {
                _start = start;
                _length = length;
            }

            public int Start
            {
                get { return _start; }
            }

            public int Length
            {
                get { return _length; }
            }
        }

    }
}
