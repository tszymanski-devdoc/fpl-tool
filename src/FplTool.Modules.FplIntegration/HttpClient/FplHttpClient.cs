namespace FplTool.Modules.FplIntegration.HttpClient;

internal sealed class FplHttpClient
{
    public System.Net.Http.HttpClient Client { get; }

    public FplHttpClient(System.Net.Http.HttpClient client)
    {
        Client = client;
    }
}
