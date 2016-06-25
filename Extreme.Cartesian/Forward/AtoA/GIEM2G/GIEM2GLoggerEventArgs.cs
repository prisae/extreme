//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;


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

