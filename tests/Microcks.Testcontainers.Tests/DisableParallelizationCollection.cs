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

/// <summary>
/// Collection definition that disables parallel execution for tests.
/// This prevents issues on GitHub Actions agents with limited resources.
/// Use [Collection("DisableParallelization")] on test classes that should not run in parallel.
/// </summary>
[CollectionDefinition("DisableParallelization", DisableParallelization = true)]
public class DisableParallelizationCollection
{
}
