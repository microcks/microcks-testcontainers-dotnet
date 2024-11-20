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

namespace Microcks.Testcontainers.Model;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// Builder for <see cref="Secret"/>.
/// </summary>
public class SecretBuilder
{
    private string _name;
    private string _token;
    private string _description;
    private string _caCertPem = null;
    private string _tokenHeader;
    private string _password;
    private string _username;

    public SecretBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public SecretBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public SecretBuilder WithToken(string token)
    {
        _token = token;
        return this;
    }

    public SecretBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public SecretBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public SecretBuilder WithTokenHeader(string tokenHeader)
    {
        _tokenHeader = tokenHeader;
        return this;
    }

    public SecretBuilder WithCaCertPem(string caCertPem)
    {
        _caCertPem = caCertPem;
        return this;
    }

    public Secret Build()
    {
        if (_name == null)
        {
            throw new ArgumentNullException("Name is required");
        }

        return new Secret()
        {
            Name = _name,
            Description = _description,
            Username = _username,
            Password = _password,
            Token = _token,
            CaCertPem = _caCertPem,
            TokenHeader = _tokenHeader
        };
    }
}
