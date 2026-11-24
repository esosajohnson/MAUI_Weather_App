using System.Net;
using System.Text;

namespace WeatherApp.Core.Tests.TestDoubles;

/// <summary>Returns canned responses and records every request, so HTTP services can be tested offline.</summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses = new();

    public List<(HttpMethod Method, string Url, string? Body)> Requests { get; } = [];

    public FakeHttpMessageHandler RespondWith(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        _responses.Enqueue(new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        return this;
    }

    public HttpClient CreateClient() => new(this);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add((request.Method, request.RequestUri!.ToString(), body));

        return _responses.Count > 0
            ? _responses.Dequeue()
            : throw new InvalidOperationException($"No response queued for {request.Method} {request.RequestUri}");
    }
}
