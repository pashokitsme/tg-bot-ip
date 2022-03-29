using Example.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Telegram.Bot.Types;

namespace Example.Core;

public class TelegramApiListener
{
    public event Action<Update> UpdateReceived;

    private readonly HttpListener _listener = new();
    private readonly ITelegramBotConfiguration _configuration;

    public TelegramApiListener(ITelegramBotConfiguration configuration) => _configuration = configuration;

    public void Start()
    {
        Logger.Log($"Listening {_configuration.ListeningUrl}. Expected route {_configuration.Route}");
        _listener.Prefixes.Add(_configuration.ListeningUrl);
        _listener.Start();
        _listener.BeginGetContext(OnReceivedRequest, _listener);
    }

    public void Stop()
    {
        _listener.Stop();
        Logger.Log("Listening stopped");
    }

    private void OnReceivedRequest(IAsyncResult asyncResult)
    {
        var context = _listener.EndGetContext(asyncResult);
        _listener.BeginGetContext(OnReceivedRequest, _listener);

        var request = context.Request;
        var response = context.Response;

        if (request.Url == null)
            return;

        if (request.HttpMethod != "POST")
        {
            Logger.Log($"Request by {GetIPv4(request)} with method {request.HttpMethod} but required {HttpMethod.Post}", LogSeverity.Error);
            SetResponse(response, HttpStatusCode.MethodNotAllowed);
            return;
        }

        if (request.Url.AbsolutePath.TrimEnd('/') != _configuration.Route.TrimEnd('/'))
        {
            Logger.Log($"{GetIPv4(request)} tried to access {request.Url.AbsolutePath} but not found", LogSeverity.Error);
            SetResponse(response, HttpStatusCode.NotFound);
            return;
        }

        SetResponse(response, TryParseUpdate(request));
    }

    private HttpStatusCode TryParseUpdate(HttpListenerRequest request)
    {
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        try
        {
            var json = reader.ReadToEnd();
            var update = JsonConvert.DeserializeObject<Update>(json);

            if (update == null)
                throw new NullReferenceException(nameof(update));

            UpdateReceived?.Invoke(update);
            Logger.Log($"Received update by {GetIPv4(request)}");
            return HttpStatusCode.OK;
        }
        catch
        {
            Logger.Log($"Received invalid update by {GetIPv4(request)}", LogSeverity.Error);
            return HttpStatusCode.BadRequest;
        }
    }

    private static string GetIPv4(HttpListenerRequest request) => request.Headers["X-Forwarded-For"];

    private static void SetResponse(HttpListenerResponse response, HttpStatusCode statusCode, string message = null)
    {
        response.StatusCode = (int)statusCode;
        message ??= $"{response.StatusCode} - {response.StatusDescription}";
        var buffer = new ReadOnlySpan<byte>(Encoding.Default.GetBytes(message));
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer);
        response.Close();
    }
}