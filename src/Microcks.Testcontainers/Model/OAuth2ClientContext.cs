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

using System.Text.Json.Serialization;

namespace Microcks.Testcontainers.Model;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// A volatile OAuth2 client context usually associated with a Test request.
/// </summary>
public class OAuth2ClientContext
{
    [JsonPropertyName("clientId")]
    public string ClientId { get; set; }

    [JsonPropertyName("clientSecret")]
    public string ClientSecret { get; set; }

    [JsonPropertyName("tokenUri")]
    public string TokenUri { get; set; }

    [JsonPropertyName("scopes")]
    public string Scopes { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("grantType")]
    public OAuth2GrantType GrantType { get; set; }    
}

/// <summary>
/// A Builder to create OAuth2ClientContext using a fluid APi.
/// </summary>
public class OAuth2ClientContextBuilder
{
    private readonly OAuth2ClientContext _context;

    public OAuth2ClientContextBuilder()
    {
        _context = new OAuth2ClientContext();
    }

    public OAuth2ClientContextBuilder WithClientId(string clientId)
    {
        _context.ClientId = clientId;
        return this;
    }

    public OAuth2ClientContextBuilder WithClientSecret(string clientSecret)
    {
        _context.ClientSecret = clientSecret;
        return this;
    }

    public OAuth2ClientContextBuilder WithTokenUri(string tokenUri)
    {
        _context.TokenUri = tokenUri;
        return this;
    }

    public OAuth2ClientContextBuilder WithScopes(string scopes)
    {
        _context.Scopes = scopes;
        return this;
    }

    public OAuth2ClientContextBuilder WithUsername(string username)
    {
        _context.Username = username;
        return this;
    }

    public OAuth2ClientContextBuilder WithPassword(string password)
    {
        _context.Password = password;
        return this;
    }

    public OAuth2ClientContextBuilder WithRefreshToken(string refreshToken)
    {
        _context.RefreshToken = refreshToken;
        return this;
    }

    public OAuth2ClientContextBuilder WithGrantType(OAuth2GrantType grantType)
    {
        _context.GrantType = grantType;
        return this;
    }

    public OAuth2ClientContext Build()
    {
        return _context;
    }
}
