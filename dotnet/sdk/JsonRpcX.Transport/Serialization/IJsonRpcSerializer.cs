namespace JsonRpcX.Transport.Serialization;

public interface IJsonRpcSerializer<T> : IJsonRpcRequestSerializer<T>, IJsonRpcResponseSerializer<T>;
