
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

public static class VisaClientFactory
{
    public static HttpClient Create(
        Uri baseAddress,
        X509Certificate2 clientCertificate,
        TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(baseAddress);
        ArgumentNullException.ThrowIfNull(clientCertificate);

        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(clientCertificate);

        return new HttpClient(handler)
        {
            BaseAddress = baseAddress,
            Timeout = timeout
        };
    }
}
