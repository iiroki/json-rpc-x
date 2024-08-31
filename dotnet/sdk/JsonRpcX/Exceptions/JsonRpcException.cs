namespace JsonRpcX.Exceptions;

public class JsonRpcException(string msg, Exception? cause = null) : Exception(msg, cause);
