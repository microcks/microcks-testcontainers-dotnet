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

using System.Linq;

namespace Microcks.Testcontainers.Tests.Async;

/// <summary>
/// Proves that the async protocols survive a builder clone/merge: combining
/// two connections (e.g. Kafka then AMQP) must keep BOTH protocols instead of
/// the later one overwriting the former.
/// </summary>
public sealed class MicrocksAsyncMinionProtocolsTests
{
    [Fact]
    public void Merge_ShouldCombineExtraProtocolsFromBothConfigurations()
    {
        var kafka = new MicrocksAsyncMinionConfiguration(extraProtocols: new[] { "KAFKA" });
        var amqp = new MicrocksAsyncMinionConfiguration(extraProtocols: new[] { "AMQP" });

        var merged = new MicrocksAsyncMinionConfiguration(kafka, amqp);

        Assert.Contains("KAFKA", merged.ExtraProtocols);
        Assert.Contains("AMQP", merged.ExtraProtocols);
    }

    [Fact]
    public void Merge_ShouldPreserveExistingProtocols_WhenNewConfigurationHasNone()
    {
        var kafka = new MicrocksAsyncMinionConfiguration(extraProtocols: new[] { "KAFKA" });
        var withoutProtocols = new MicrocksAsyncMinionConfiguration();

        var merged = new MicrocksAsyncMinionConfiguration(kafka, withoutProtocols);

        Assert.Equal(new[] { "KAFKA" }, merged.ExtraProtocols.ToArray());
    }
}
