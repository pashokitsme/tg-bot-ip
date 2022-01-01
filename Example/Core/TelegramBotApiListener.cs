using Example.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Telegram.Bot.Types;

namespace Example.Core;
internal class TelegramBotApiListener
{
    public event Action<Update>? UpdateReceived;
    public event Action? Stopped;

    private readonly HttpListener _listener = new();
    private readonly TelegramBotConfiguration _configuration;

    private bool _stop = false;

    public TelegramBotApiListener(TelegramBotConfiguration configuration)
    {
        _configuration = configuration;
        _listener.Prefixes.Add(_configuration.ListeningAddress);
    }

    public async Task StartAsync()
    {
        Logger.Log($"Listening started up on: {_configuration.ListeningAddress}. Expected route {_configuration.Route}");

        _listener.Start();

        while (_stop == false)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                PreprocessReceivedRequest(context);
            }
            catch (HttpListenerException) { }
        }

        Logger.Log("Listening stopped");
    }

    public void Stop()
    {
        _stop = true;
        _listener.Stop();
        Stopped?.Invoke();
    }

    private void PreprocessReceivedRequest(HttpListenerContext context)
    {
        var request = context.Request;

        if (request.Url == null)
            return;

        Logger.Log($"{request.HttpMethod} request: {request.Url.AbsolutePath}");

        if (request.HttpMethod != "POST")
        {
            Logger.Log($"Method {request.HttpMethod} is not allowed", LogSeverity.WARNING);
            context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            CommitResponse(context);
            return;
        }

        switch (request.Url.AbsolutePath.TrimEnd('/'))
        {
            case "/update":
                ProcessUpdateRequest(request);
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                break;

            default:
                Logger.Log($"Route {request.Url.AbsolutePath} not defined", LogSeverity.WARNING);
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;
        }

        CommitResponse(context);
    }

    private async void ProcessUpdateRequest(HttpListenerRequest request)
    {
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        var json = await reader.ReadToEndAsync();

        var update = JsonConvert.DeserializeObject<Update>(json);

        if (update != null)
        {
            UpdateReceived?.Invoke(update);
            return;
        }

        Logger.Log($"Received invalid update by {request.RemoteEndPoint}", LogSeverity.WARNING);
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