namespace JsonRpcX.Core.Serialization;

public interface IJsonRpcSerializer<T> : IJsonRpcRequestSerializer<T>, IJsonRpcResponseSerializer<T>;
