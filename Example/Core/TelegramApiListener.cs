using Example.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Telegram.Bot.Types;

namespace Example.Core;

internal class TelegramApiListener
{
    public event Action<Update>? UpdateReceived;

    private readonly HttpListener _listener = new();
    private readonly TelegramBotConfiguration _configuration;

    public TelegramApiListener(TelegramBotConfiguration configuration)
    {
        _configuration = configuration;
        _listener.Prefixes.Add($"http://localhost:{_configuration.ListeningPort}/");
    }

    public async Task StartAsync()
    {
        Logger.Log($"Listening port {_configuration.ListeningPort}. Expected route {_configuration.Route}");

        _listener.Start();

        while (true)
        {
            try
            {
                ProcessRequest(await _listener.GetContextAsync());
            }
            catch (HttpListenerException) { }
        }
    }

    public void Stop()
    {
        _listener.Stop();
        Logger.Log("Listening stopped");
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        var request = context.Request;

        if (request.Url == null)
            return;

        if (request.HttpMethod != "POST")
        {
            Logger.Log($"Method {request.HttpMethod} is not allowed", LogSeverity.WARNING);
            context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            CommitResponse(context);
            return;
        }

        if (request.Url.AbsolutePath.TrimEnd('/') == _configuration.Route.TrimEnd('/'))
        {
            HandleUpdateRequest(request);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            CommitResponse(context);
            return;
        }

        Logger.Log($"Route {request.Url.AbsolutePath} is invalid", LogSeverity.WARNING);
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

        CommitResponse(context);
    }

    private async void HandleUpdateRequest(HttpListenerRequest request)
    {
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        var json = await reader.ReadToEndAsync();

        var update = JsonConvert.DeserializeObject<Update>(json);

        if (update != null)
        {
            UpdateReceived?.Invoke(update);
            return;
        }

        Logger.Log($"Received invalid update by {request.RemoteEndPoint.Address}", LogSeverity.WARNING);
    }

    private static void CommitResponse(HttpListenerContext context, string response = "")
    {
        if (response == "")
            response = $"{context.Response.StatusCode} - {context.Response.StatusDescription}";

        var buffer = new ReadOnlySpan<byte>(Encoding.Default.GetBytes(response));
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer);
        context.Response.Close();
    }
}