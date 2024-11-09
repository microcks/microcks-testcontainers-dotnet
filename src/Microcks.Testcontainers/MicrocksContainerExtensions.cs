using Microcks.Testcontainers.Model;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microcks.Testcontainers;

/// <summary>
/// Extensions for MicrocksContainer.
/// </summary>
public static class MicrocksContainerExtensions
{
    /// <summary>
    /// HttpClient to use for Microcks.
    /// </summary>
    public static HttpClient Client => _lazyClient.Value;
    private static Lazy<HttpClient> _lazyClient = new Lazy<HttpClient>(() => new HttpClient()
    {
        DefaultRequestHeaders =
        {
            Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") },
            CacheControl = CacheControlHeaderValue.Parse("no-cache")
        }
    });

    /// <summary>
    /// Test an endpoint with a TestRequest.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="testRequest"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task<TestResult> TestEndpointAsync(
        this MicrocksContainer container, TestRequest testRequest)
    {
        string httpEndpoint = container.GetHttpEndpoint() + "api/tests";
        var content = new StringContent(JsonSerializer.Serialize(testRequest), Encoding.UTF8, "application/json");
        var responseMessage = await Client.PostAsync(httpEndpoint, content);

        if (responseMessage.StatusCode == HttpStatusCode.Created)
        {
            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            var testResult = JsonSerializer.Deserialize<TestResult>(responseContent);
            var testResultId = testResult.Id;
            container.Logger.LogDebug("Test launched with ID: {TestResultId}, new polling for progression", testResultId);

            try
            {
                await WaitForConditionAsync(async () => !(await RefreshTestResultAsync(httpEndpoint, testResultId)).InProgress,
                    atMost: TimeSpan.FromMilliseconds(1000).Add(testRequest.Timeout),
                    delay: TimeSpan.FromMilliseconds(100),
                    interval: TimeSpan.FromMilliseconds(200));

            }
            catch (TaskCanceledException)
            {
                container.Logger.LogWarning("Test timeout reached, stopping polling");
            }

            return await RefreshTestResultAsync(httpEndpoint, testResultId);
        }
        else
        {
            throw new Exception("Couldn't launch on new test on Microcks. Please check Microcks container logs");
        }
    }

    private static async Task<TestResult> RefreshTestResultAsync(string httpEndpoint, string testResultId)
    {
        var result = await Client.GetAsync(httpEndpoint + "/" + testResultId);
        var jsonResult = await result.Content.ReadAsStringAsync();
        var testResult = JsonSerializer.Deserialize<TestResult>(jsonResult);
        return testResult;
    }

    private static async Task WaitForConditionAsync(Func<Task<bool>> condition, TimeSpan atMost, TimeSpan delay, TimeSpan interval)
    {
        // Cancel after atMost
        using var cancellationTokenSource = new CancellationTokenSource(atMost);

        // Delay before first check
        await Task.Delay(delay, cancellationTokenSource.Token);

        // Polling
        while (!await condition())
        {
            if (cancellationTokenSource.Token.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
            await Task.Delay(interval, cancellationTokenSource.Token);
        }
    }

    internal static async Task ImportArtifactAsync(this MicrocksContainer container, string artifact, bool mainArtifact)
    {
        string url = $"{container.GetHttpEndpoint()}api/artifact/upload" + (mainArtifact ? "" : "?mainArtifact=false");
        var result = await container.UploadFileToMicrocksAsync(artifact, url);
        if (result.StatusCode != HttpStatusCode.Created)
        {
            throw new Exception($"Artifact has not been correctly imported: {result.StatusCode}");
        }
        container.Logger.LogInformation($"Artifact {artifact} has been imported");
    }

    internal static async Task<HttpResponseMessage> UploadFileToMicrocksAsync(this MicrocksContainer container, string filepath, string url)
    {
        using (var form = new MultipartFormDataContent())
        {
            using var snapContent = new ByteArrayContent(File.ReadAllBytes(filepath));
            snapContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            form.Add(snapContent, "file", Path.GetFileName(filepath));

            return await Client.PostAsync(url, form);
        }
    }

    internal static async Task ImportSnapshotAsync(this MicrocksContainer container, string snapshot)
    {
        string url = $"{container.GetHttpEndpoint()}api/import";
        var result = await container.UploadFileToMicrocksAsync(snapshot, url);

        if (result.StatusCode != HttpStatusCode.Created)
        {
            throw new Exception($"Snapshot has not been correctly imported: {result.StatusCode}");
        }
        container.Logger.LogInformation($"Snapshot {snapshot} has been imported");
    }

    internal static async Task CreateSecretAsync(this MicrocksContainer container, Model.Secret secret)
    {
        string url = $"{container.GetHttpEndpoint()}api/secrets";
        var content = new StringContent(JsonSerializer.Serialize(secret), Encoding.UTF8, "application/json");

        var result = await Client.PostAsync(url, content);

        if (result.StatusCode != HttpStatusCode.Created)
        {
            throw new Exception("Secret has not been correctly created");
        }
        container.Logger.LogInformation($"Secret {secret.Name} has been created");
    }

    internal static async Task DownloadArtifactAsync(this MicrocksContainer container, string remoteArtifactUrl, bool main)
    {
        var content = new StringContent("mainArtifact=" + main + "&url=" + remoteArtifactUrl, Encoding.UTF8, "application/x-www-form-urlencoded");
        var result = await Client
            .PostAsync($"{container.GetHttpEndpoint()}api/artifact/download", content);

        if (result.StatusCode != HttpStatusCode.Created)
        {
            throw new Exception("Artifact has not been correctly downloaded");
        }
        container.Logger.LogInformation($"Artifact {remoteArtifactUrl} has been downloaded");
    }
}
