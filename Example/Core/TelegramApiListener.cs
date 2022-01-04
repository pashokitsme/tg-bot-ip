using Example.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Telegram.Bot.Types;

namespace Example.Core;
internal class TelegramApiListener
{
    public event Action<Update>? UpdateReceived;
    public event Action? Stopped;

    private readonly HttpListener _listener = new();
    private readonly TelegramBotConfiguration _configuration;

    private bool _stop = false;

    public TelegramApiListener(TelegramBotConfiguration configuration)
    {
        _configuration = configuration;
        _listener.Prefixes.Add($"http://localhost:{_configuration.ListeningPort}/");
    }

    public async Task StartAsync()
    {
        Logger.Log($"Listening port {_configuration.ListeningPort}. Expected route {_configuration.Route}");

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

        if (request.HttpMethod != "POST")
        {
            Logger.Log($"Method {request.HttpMethod} is not allowed", LogSeverity.WARNING);
            context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            CommitResponse(context);
            return;
        }

        if (request.Url.AbsolutePath == _configuration.Route)
        {
            ProcessUpdateRequest(request);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            CommitResponse(context);
        }

        Logger.Log($"Route {request.Url.AbsolutePath} is invalid", LogSeverity.WARNING);
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

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