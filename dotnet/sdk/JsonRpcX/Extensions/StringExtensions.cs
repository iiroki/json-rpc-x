using System.Text;

namespace JsonRpcX.Extensions;

public static class StringExtensions
{
    public static byte[] GetUtf8Bytes(this string str) => Encoding.UTF8.GetBytes(str);
}
