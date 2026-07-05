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

namespace Microcks.Testcontainers.Tests;

public sealed class MicrocksBuilderImageTests
{
    private const string CustomImage = "quay.io/microcks/microcks-uber:1.14.0";

    [Fact]
    public void Build_WithCustomImage_ShouldUseProvidedImage()
    {
        var container = new MicrocksBuilder(CustomImage).Build();

        Assert.Equal(CustomImage, container.Image.FullName);
    }

    [Fact]
    public void Build_WithCustomImageAndAdditionalConfiguration_ShouldKeepProvidedImage()
    {
        var builder = new MicrocksBuilder(CustomImage)
            .WithEnvironment("FOO", "BAR");

        var container = builder.Build();

        Assert.Equal(CustomImage, container.Image.FullName);
    }

    [Fact]
    public async Task Build_WithCustomImageAcrossMultipleSteps_ShouldKeepProvidedImageAndEnvironment()
    {
        var builder = new MicrocksBuilder(CustomImage)
            .WithMainArtifacts("apipastries-openapi.yaml");

        builder = builder.WithEnvironment("FOO", "BAR");

        await using var container = builder.Build();
        await container.StartAsync(TestContext.Current.CancellationToken);

        Assert.Equal(CustomImage, container.Image.FullName);

        using var dockerClient = new Docker.DotNet.DockerClientBuilder().Build();
        var inspect = await dockerClient.Containers.InspectContainerAsync(
            container.Id,
            TestContext.Current.CancellationToken);

        Assert.Contains("FOO=BAR", inspect.Config.Env);
    }
}
