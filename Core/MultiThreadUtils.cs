using System;
using System.Threading.Tasks;

namespace Extreme.Cartesian.Core
{
    public static class MultiThreadUtils
    {


        static MultiThreadUtils()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount;
            
            var omp = Environment.GetEnvironmentVariable(@"OMP_NUM_THREADS");

            int degree;

            if (!string.IsNullOrEmpty(omp))
                if (int.TryParse(omp, out degree))
                    MaxDegreeOfParallelism = degree;

	    MaxDegreeOfParallelism=1; 
        }
public		static int NumberOfOpenMPThreads()
		{		
		    var omp = Environment.GetEnvironmentVariable(@"OMP_NUM_THREADS");
    			int degree=1;
    			if (!string.IsNullOrEmpty(omp))
				if (int.TryParse(omp, out degree))
					return degree;
			return 1;
		}

        public static int MaxDegreeOfParallelism { get; private set; }

        public static ParallelOptions CreateParallelOptions()
            => new ParallelOptions() { MaxDegreeOfParallelism = MaxDegreeOfParallelism };
    }
}
