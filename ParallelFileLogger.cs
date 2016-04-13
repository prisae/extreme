using System;
using Extreme.Core.Logger;
using Extreme.Core;
using System.IO;
namespace Extreme.Parallel.Logger
{
	public class ParallelFileLogger : BaseLogger
	{
		private readonly Mpi _mpi;
		private readonly string _name;
		private readonly int _rank;
		private readonly bool _permitToWrite;
		private readonly StreamWriter _streamWriter;
		public ParallelFileLogger(Mpi mpi, string fileName, bool rewrite,bool permit=false) 
			
		{
			_mpi = mpi;
			if (mpi == null) throw new ArgumentNullException(nameof(mpi));

			_name = mpi.GetProcessorName();
			_rank = mpi.Rank;

			if (_mpi.IsMaster) {
				_permitToWrite = true;
				if (rewrite)
				if (File.Exists (fileName))
					File.Delete (fileName);

				_streamWriter = new StreamWriter (fileName);
				_streamWriter.WriteLine ($"File logger started at {CreationTime}");
				_streamWriter.Flush ();
			} else {
				_permitToWrite = permit;
			}
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
			if (_permitToWrite) {
				if (!Filter (logLevel)) {
					var prefix = this.GetPrefix (logLevel);

					AppendToFile (prefix + message);
				}
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

