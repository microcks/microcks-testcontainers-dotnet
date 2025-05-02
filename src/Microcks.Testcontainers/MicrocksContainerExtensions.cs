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
    /// Tests an endpoint with a given TestRequest by sending the request to the Microcks container
    /// and polling for the test result until the test is completed or the timeout is reached.
    /// </summary>
    /// <param name="container">The Microcks container to test against.</param>
    /// <param name="testRequest">The TestRequest containing the details of the test to be performed.</param>
    /// <returns>A Task representing the asynchronous operation, with a TestResult as the result.</returns>
    /// <exception cref="Exception">Thrown if the test could not be launched
    /// or if there was an error during the test execution.</exception>
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
            catch (TaskCanceledException taskCanceledException)
            {
                container.Logger.LogWarning(
                    taskCanceledException,
                    "Test timeout reached, stopping polling for test {testEndpoint}", testRequest.TestEndpoint);
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
        // Delay before first check
        await Task.Delay(delay);

        // Cancel after atMost
        using var cancellationTokenSource = new CancellationTokenSource(atMost);

        // Polling
        while (!await condition())
        {
            if (cancellationTokenSource.Token.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
            await Task.Delay(interval, CancellationToken.None);
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

    /// <summary>
    /// Retrieve messages exchanged during a test on an endpoint (for further investigation or checks).
    /// </summary>
    /// <param name="container">Microcks container</param>
    /// <param name="testResult">The test result to retrieve messages from</param>
    /// <param name="operationName">The name of the operation to retrieve messages to test result</param>
    /// <returns>List of RequestResponsePair</returns>
    /// <exception cref="MicrocksException">If messages have not been correctly retrieved</exception>
    public static async Task<List<RequestResponsePair>> GetMessagesForTestCaseAsync(this MicrocksContainer container,
        TestResult testResult, string operationName)
    {
        var operation = operationName.Replace('/', '!');
        var testCaseId = $"{testResult.Id}-{testResult.TestNumber}-{HttpUtility.UrlEncode(operation)}";
        var url = $"{container.GetHttpEndpoint()}api/tests/{testResult.Id}/messages/{testCaseId}";
        var response = await Client.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            return await response.Content.ReadFromJsonAsync<List<RequestResponsePair>>();
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
    /// <returns>List of UnidirectionalEvent</returns>
    /// <exception cref="MicrocksException">If events have not been correctly retrieved</exception>
    public static async Task<List<UnidirectionalEvent>> GetEventMessagesForTestCaseAsync(this MicrocksContainer container,
        TestResult testResult, string operationName)
    {
        var operation = operationName.Replace('/', '!');
        var testCaseId = $"{testResult.Id}-{testResult.TestNumber}-{HttpUtility.UrlEncode(operation)}";
        var url = $"{container.GetHttpEndpoint()}api/tests/{testResult.Id}/events/{testCaseId}";
        var response = await Client.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            return await response.Content.ReadFromJsonAsync<List<UnidirectionalEvent>>();
        }
        else
        {
            throw new MicrocksException($"Couldn't retrieve events for test case {operationName} on test {testResult.Id}");
        }
    }
}
