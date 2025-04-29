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

using System.Data;
using System.Text.Json.Serialization;

namespace Microcks.Testcontainers.Model;

/// <summary>
/// The daily statistic of a service mock invocations
/// </summary>
public class DailyInvocationStatistic
{

    /// <summary>
    /// Id of statistic record.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Day of statistic record.
    /// </summary>
    [JsonPropertyName("day")]
    public string Day { get; set; }

    /// <summary>
    /// Service name of statistic record.
    /// </summary>
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }

    /// <summary>
    /// Service version of statistic record.
    /// </summary>
    [JsonPropertyName("serviceVersion")]
    public string ServiceVersion { get; set; }

    /// <summary>
    /// Daily count of statistic record.
    /// </summary>
    [JsonPropertyName("dailyCount")]
    public long DailyCount { get; set; }

    /// <summary>
    /// Hourly count of statistic record.
    /// </summary>
    [JsonPropertyName("hourlyCount")]
    public Dictionary<string, Object> HourlyCount { get; set; }

    /// <summary>
    /// Minute count of statistic record.
    /// </summary>
    [JsonPropertyName("minuteCount")]
    public Dictionary<string, Object> MinuteCount { get; set; }
}