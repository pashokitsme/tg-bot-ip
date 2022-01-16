using Example.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Telegram.Bot.Types;

namespace Example.Core;

internal class TelegramApiListener
{
    public event Action<Update> UpdateReceived;

    private readonly HttpListener _listener = new();
    private readonly TelegramBotConfiguration _configuration;

    public TelegramApiListener(TelegramBotConfiguration configuration)
    {
        _configuration = configuration;
        _listener.Prefixes.Add($"http://localhost:{_configuration.ListeningPort}/");
    }

    public void Start()
    {
        Logger.Log($"Listening port {_configuration.ListeningPort}. Expected route {_configuration.Route}");

        _listener.Start();
        _listener.BeginGetContext(OnReceivedRequest, _listener);
    }

    public void Stop()
    {
        _listener.Stop();
        Logger.Log("Listening stopped");
    }

    private async void OnReceivedRequest(IAsyncResult asyncResult)
    {
        var context = _listener.EndGetContext(asyncResult);
        _listener.BeginGetContext(OnReceivedRequest, _listener);

        var request = context.Request;
        var response = context.Response;

        if (request.Url == null)
            return;

        if (request.HttpMethod != "POST")
        {
            Logger.Log($"{request.UserHostName} request with method {request.HttpMethod}, but required {HttpMethod.Post}", LogSeverity.ERROR);

            SetResponse(response, HttpStatusCode.MethodNotAllowed);
            return;
        }

        if (request.Url.AbsolutePath.TrimEnd('/') != _configuration.Route.TrimEnd('/'))
        {
            Logger.Log($"{request.UserHostName} tried to access {request.Url.AbsolutePath}, but not found", LogSeverity.ERROR);
            SetResponse(response, HttpStatusCode.NotFound);
            return;
        }

        var result = await TryParseUpdate(request);
        SetResponse(response, result);
    }

    private async Task<HttpStatusCode> TryParseUpdate(HttpListenerRequest request)
    {
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        var json = await reader.ReadToEndAsync();

        var update = JsonConvert.DeserializeObject<Update>(json);

        if (update != null)
        {
            UpdateReceived?.Invoke(update);
            return HttpStatusCode.OK;
        }

        Logger.Log($"Received invalid update by {request.RemoteEndPoint.Address}", LogSeverity.ERROR);
        return HttpStatusCode.BadRequest;
    }

    private static void SetResponse(HttpListenerResponse response, HttpStatusCode statusCode, string message = "")
    {
        response.StatusCode = (int)statusCode;

        if (message == "")
            message = $"{response.StatusCode} - {response.StatusDescription}";

        var buffer = new ReadOnlySpan<byte>(Encoding.Default.GetBytes(message));
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer);
        response.Close();
    }
}