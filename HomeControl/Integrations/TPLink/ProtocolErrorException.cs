namespace HomeControl.Integrations.TPLink
{
    public class ProtocolErrorException : Exception
    {
        public ProtocolErrorException(int errorCode, string errorMessage) : this(errorCode, errorMessage, null)
        {
            
        }

        public ProtocolErrorException(int errorCode, string errorMessage, Exception innerException) : base(errorMessage, innerException)
        {
            ErrorCode = errorCode;
        }

        public int ErrorCode { get; }
    }
}