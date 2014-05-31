using System;
using System.Runtime.Serialization;

namespace CsRopExample.SqlDatabase
{
    [Serializable]
    public class SqlException : Exception
    {
        public SqlException()
        {
        }

        public SqlException(string message) : base(message)
        {
        }

        public SqlException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SqlException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}