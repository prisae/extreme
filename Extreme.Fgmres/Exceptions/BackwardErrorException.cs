//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Runtime.Serialization;

namespace Extreme.Fgmres
{
    [Serializable]
    public class BackwardErrorException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public BackwardErrorException()
        {
        }

        public BackwardErrorException(string message) : base(message)
        {
        }

        public BackwardErrorException(string message, Exception inner) : base(message, inner)
        {
        }

        protected BackwardErrorException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }}
