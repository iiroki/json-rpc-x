namespace JsonRpcX.Options;

public class JsonRpcExceptionOptions
{
    /// <summary>
    /// Error code when authentication fails.
    /// </summary>
    public int AuthenticationErrorCode = 100;

    /// <summary>
    /// Error code when authorization fails.
    /// </summary>
    public int AuthorizationErrorCode = 101;
}
