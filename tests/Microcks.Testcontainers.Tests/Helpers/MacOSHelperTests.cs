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

using Microcks.Testcontainers.Helpers;

/// <summary>
/// Tests for <see cref="MacOSHelper"/>.
/// Note: This is a temporary fix and should be removed in the near future.
/// </summary>
public class MacOSHelperTests
{
    [Fact]
    public void GetJavaOptions_ShouldReturnOptions_WhenRunningOnMacOS()
    {
        // Arrange
        MacOSHelper.IsMacOS = true;
        // Act
        var options = MacOSHelper.GetJavaOptions();

        // Assert
        Assert.NotEmpty(options);
        Assert.Equal("-XX:UseSVE=0", options["JAVA_OPTIONS"]);

    }

    [Fact]
    public void GetJavaOptions_ShouldReturnEmpty_WhenNotRunningOnMacOSA()
    {
        // Arrange
        MacOSHelper.IsMacOS = false;
        // Act
        var options = MacOSHelper.GetJavaOptions();

        // Assert
        Assert.Empty(options);
    }
}
