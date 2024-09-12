namespace JsonRpcX.Client;

public interface IJsonRpcClientManager : IJsonRpcClientContainer
{
    void Add(IJsonRpcClient client);

    void Remove(string id);
}
