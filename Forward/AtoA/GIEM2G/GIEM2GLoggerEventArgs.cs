using System;


namespace Extreme.Cartesian.Giem2g
{
	public class GIEM2GLoggerEventArgs: EventArgs
	{
		public string Message;
		public GIEM2GLoggerEventArgs (string message)
		{
			Message = message;
		}
	}
}

