//
// Copyright The Microcks Authors.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//

using Microcks.Testcontainers.Model;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microcks.Testcontainers;

/// <summary>
/// Extensions for MicrocksContainer.
/// </summary>
public static class MicrocksContainerExtensions
{
    private static string _metricsAPIDatePattern = "yyyyMMdd";

    /// <summary>
    /// HttpClient to use for Microcks.
    /// </summary>
    public static HttpClient Client => _lazyClient.Value;
    private static Lazy<HttpClient> _lazyClient = new Lazy<HttpClient>(() => new HttpClient()
    {
        DefaultRequestHeaders =
        {
            Accept = { MediaTypeWithQualityHeaderValue.Parse(MediaTypeNames.Application.Json) },
            CacheControl = CacheControlHeaderValue.Parse("no-cache")
        }
    });

    /// <summary>
    /// Tests an endpoint with a given TestRequest by sending the request to the Microcks container
    /// and polling for the test result until the test is completed or the timeout is reached.
    /// </summary>
    /// <param name="container">The Microcks container to test against.</param>
    /// <param name="testRequest">The TestRequest containing the details of the test to be performed.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A Task representing the asynchronous operation, with a TestResult as the result.</returns>
    /// <exception cref="Exception">Thrown if the test could not be launched
    /// or if there was an error during the test execution.</exception>
    public static async Task<TestResult> TestEndpointAsync(
        this MicrocksContainer container, TestRequest testRequest, CancellationToken cancellationToken = default)
    {
        string httpEndpoint = container.GetHttpEndpoint() + "api/tests";
        var content = new StringContent(JsonSerializer.Serialize(testRequest), Encoding.UTF8, MediaTypeNames.Application.Json);
        var responseMessage = await Client.PostAsync(httpEndpoint, content, cancellationToken);

        if (responseMessage.StatusCode == HttpStatusCode.Created)
        {
            var responseContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

            var testResult = JsonSerializer.Deserialize<TestResult>(responseContent);
            var testResultId = testResult.Id;
            container.Logger.LogDebug("Test launched with ID: {TestResultId}, new polling for progression", testResultId);

            try
            {
                await WaitForConditionAsync(async () => !(await RefreshTestResultAsync(httpEndpoint, testResultId, cancellationToken)).InProgress,
                    atMost: TimeSpan.FromMilliseconds(3000).Add(testRequest.Timeout),
                    delay: TimeSpan.FromMilliseconds(100),
                    interval: TimeSpan.FromMilliseconds(200),
                    cancellationToken);
            }
            catch (TaskCanceledException taskCanceledException)
            {
                container.Logger.LogWarning(
                    taskCanceledException,
                    "Test timeout reached, stopping polling for test {TestEndpoint}", testRequest.TestEndpoint);
            }

            return await RefreshTestResultAsync(httpEndpoint, testResultId, cancellationToken);
        }
        else
        {
            throw new InvalidOperationException("Couldn't launch new test on Microcks. Please check Microcks container logs");
        }
    }

    private static async Task<TestResult> RefreshTestResultAsync(string httpEndpoint, string testResultId, CancellationToken cancellationToken = default)
    {
        var refreshTestResultEndpoint = $"{httpEndpoint}/{testResultId}";
        var result = await Client.GetAsync(refreshTestResultEndpoint, cancellationToken);
        var jsonResult = await result.Content.ReadAsStringAsync(cancellationToken);
        var testResult = JsonSerializer.Deserialize<TestResult>(jsonResult);
        return testResult;
    }

    private static async Task WaitForConditionAsync(Func<Task<bool>> condition, TimeSpan atMost, TimeSpan delay, TimeSpan interval, CancellationToken cancellationToken = default)
    {
        // Delay before first check
        await Task.Delay(delay, cancellationToken);

        // Cancel after atMost
        using var atMostCancellationToken = new CancellationTokenSource(atMost);
        // Create linked token so we can be cancelled either by caller or by timeout
        using var cancellationTokenSource = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken, atMostCancellationToken.Token);

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

    internal static async Task ImportArtifactAsync(this MicrocksContainer container, string artifact, bool mainArtifact, CancellationToken cancellationToken = default)
    {
        string url = $"{container.GetHttpEndpoint()}api/artifact/upload" + (mainArtifact ? "" : "?mainArtifact=false");
        var result = await container.UploadFileToMicrocksAsync(artifact, url, cancellationToken);
        if (result.StatusCode != HttpStatusCode.Created)
        {
            throw new InvalidOperationException($"Artifact has not been correctly imported: {result.StatusCode}");
        }
        container.Logger.LogInformation("Artifact {ArtifactPath} has been imported", artifact);
    }

    internal static async Task<HttpResponseMessage> UploadFileToMicrocksAsync(this MicrocksContainer container, string filepath, string url, CancellationToken cancellationToken = default)
    {
        var fileBytes = await File.ReadAllBytesAsync(filepath, cancellationToken);
        
        using (var form = new MultipartFormDataContent())
        {
            using var snapContent = new ByteArrayContent(fileBytes);
            snapContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);
            form.Add(snapContent, "file", Path.GetFileName(filepath));

            return await Client.PostAsync(url, form, cancellationToken);
        }
    }

    internal static async Task ImportSnapshotAsync(this MicrocksContainer container, string snapshot, CancellationToken cancellationToken = default)
    {
        string url = $"{container.GetHttpEndpoint()}api/import";
        var result = await container.UploadFileToMicrocksAsync(snapshot, url, cancellationToken);

        if (result.StatusCode != HttpStatusCode.Created)
        {
            throw new InvalidOperationException($"Snapshot has not been correctly imported: {result.StatusCode}");
        }
        container.Logger.LogInformation("Snapshot {SnapshotPath} has been imported", snapshot);
    }

    internal static async Task CreateSecretAsync(this MicrocksContainer container, Model.Secret secret, CancellationToken cancellationToken = default)
    {
        string url = $"{container.GetHttpEndpoint()}api/secrets";
        var content = new StringContent(JsonSerializer.Serialize(secret), Encoding.UTF8, "application/json");

        var result = await Client.PostAsync(url, content, cancellationToken);

        if (result.StatusCode != HttpStatusCode.Created)
        {
            throw new InvalidOperationException("Secret has not been correctly created");
        }
        container.Logger.LogInformation("Secret {SecretName} has been created", secret.Name);
    }

    internal static async Task DownloadArtifactAsync(this MicrocksContainer container, RemoteArtifact remoteArtifact, bool main, CancellationToken cancellationToken = default)
    {
        var content = new StringContent("mainArtifact=" + main + "&url=" + remoteArtifact.Url, Encoding.UTF8, "application/x-www-form-urlencoded");
        if (remoteArtifact.SecretName != null)
        {
            content = new StringContent("mainArtifact=" + main + "&url=" + remoteArtifact.Url + "&secretName=" + remoteArtifact.SecretName, Encoding.UTF8, "application/x-www-form-urlencoded");
        }
        var result = await Client
            .PostAsync($"{container.GetHttpEndpoint()}api/artifact/download", content, cancellationToken);

        if (result.StatusCode != HttpStatusCode.Created)
        {
            throw new InvalidOperationException("Artifact has not been correctly downloaded");
        }
        container.Logger.LogInformation("Artifact {ArtifactUrl} has been downloaded", remoteArtifact.Url);
    }

    /// <summary>
    /// Retrieve messages exchanged during a test on an endpoint (for further investigation or checks).
    /// </summary>
    /// <param name="container">Microcks container</param>
    /// <param name="testResult">The test result to retrieve messages from</param>
    /// <param name="operationName">The name of the operation to retrieve messages to test result</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of RequestResponsePair</returns>
    /// <exception cref="MicrocksException">If messages have not been correctly retrieved</exception>
    public static async Task<List<RequestResponsePair>> GetMessagesForTestCaseAsync(this MicrocksContainer container,
        TestResult testResult, string operationName, CancellationToken cancellationToken = default)
    {
        var operation = operationName.Replace('/', '!');
        var testCaseId = $"{testResult.Id}-{testResult.TestNumber}-{HttpUtility.UrlEncode(operation)}";
        var url = $"{container.GetHttpEndpoint()}api/tests/{testResult.Id}/messages/{testCaseId}";
        var response = await Client.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            return await response.Content.ReadFromJsonAsync<List<RequestResponsePair>>(cancellationToken);
        }
        else
        {
            throw new MicrocksException($"Couldn't retrieve messages for test case {operationName} on test {testResult.Id}");
        }
    }

    /// <summary>
    /// Retrieve event messages received during a test on an endpoint (for further investigation or checks).
    /// </summary>
    /// <param name="container">Microcks container</param>
    /// <param name="testResult">The test result to retrieve events from</param>
    /// <param name="operationName">The name of the operation to retrieve events to test result</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of UnidirectionalEvent</returns>
    /// <exception cref="MicrocksException">If events have not been correctly retrieved</exception>
    public static async Task<List<UnidirectionalEvent>> GetEventMessagesForTestCaseAsync(this MicrocksContainer container,
        TestResult testResult, string operationName, CancellationToken cancellationToken = default)
    {
        var operation = operationName.Replace('/', '!');
        var testCaseId = $"{testResult.Id}-{testResult.TestNumber}-{HttpUtility.UrlEncode(operation)}";
        var url = $"{container.GetHttpEndpoint()}api/tests/{testResult.Id}/events/{testCaseId}";
        var response = await Client.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            return await response.Content.ReadFromJsonAsync<List<UnidirectionalEvent>>(cancellationToken);
        }
        else
        {
            throw new MicrocksException($"Couldn't retrieve events for test case {operationName} on test {testResult.Id}");
        }
    }

    /// <summary>
    /// Verify if a service has been invoked at least once at a given date.
    /// </summary>
    /// <param name="container">Microcks container</param>
    /// <param name="serviceName">Service name</param>
    /// <param name="serviceVersion">Service version</param>
    /// <param name="invocationDate">Date of invocation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the service has been invoked at least once, false otherwise</returns>
    public static async Task<bool> VerifyAsync(this MicrocksContainer container, string serviceName, string serviceVersion, DateOnly? invocationDate = null, CancellationToken cancellationToken = default)
    {
        var dailyInvocationStatistic = await container.GetServiceInvocationsAsync(serviceName, serviceVersion, invocationDate, cancellationToken);
        if (dailyInvocationStatistic != null)
        {
            return dailyInvocationStatistic.DailyCount > 0;
        }
        return false;
    }

    /// <summary>
    /// Get the number of invocations for a service at a given date.
    /// </summary>
    /// <param name="container">Microcks container</param>
    /// <param name="serviceName">Service name</param>
    /// <param name="serviceVersion">Service version</param>
    /// <param name="invocationDate">Date of invocation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of invocations</returns>
    public static async Task<long> GetServiceInvocationsCountAsync(this MicrocksContainer container, string serviceName, string serviceVersion, DateOnly? invocationDate = null, CancellationToken cancellationToken = default)
    {
        var dailyInvocationStatistic = await container.GetServiceInvocationsAsync(serviceName, serviceVersion, invocationDate, cancellationToken);
        if (dailyInvocationStatistic != null)
        {
            return dailyInvocationStatistic.DailyCount;
        }
        return 0;
    }

    internal static async Task<DailyInvocationStatistic> GetServiceInvocationsAsync(this MicrocksContainer container, string serviceName, string serviceVersion, DateOnly? invocationDate = null, CancellationToken cancellationToken = default)
    {
        // Encode service name and version and take care of replacing '+' by '%20' as metrics API
        // does not handle '+' in URL path.
        var encodedName = HttpUtility.UrlEncode(serviceName).Replace("+", "%20");
        var encodedVersion = HttpUtility.UrlEncode(serviceVersion).Replace("+", "%20");
        var url = $"{container.GetHttpEndpoint()}api/metrics/invocations/{encodedName}/{encodedVersion}";

        if (invocationDate != null)
        {
            url += $"?day={invocationDate.Value.ToString(_metricsAPIDatePattern)}";
        }

        // Wait to avoid race condition issue when requesting Microcks Metrics REST API.
        Thread.Sleep(100);

        var response = await Client.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK && response.Content.Headers.ContentLength > 0)
        // Deserialize the response content to DailyInvocationStatistic object
        // and return it.
        {
            return await response.Content.ReadFromJsonAsync<DailyInvocationStatistic>(cancellationToken);
        }
        return null;
    }
}
