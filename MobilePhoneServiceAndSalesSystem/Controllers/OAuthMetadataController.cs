using Microsoft.AspNetCore.Mvc;

namespace MobilePhoneServiceAndSalesSystem.Controllers;

/// <summary>
/// OAuth Authorization Server Metadata endpoint for MCP compatibility
/// This is a minimal implementation for development/testing that indicates no authentication is required
/// </summary>
[ApiController]
public class OAuthMetadataController : ControllerBase
{
    /// <summary>
    /// OAuth 2.0 Authorization Server Metadata endpoint (RFC 8414)
    /// Returns metadata indicating this server doesn't require authentication for development
    /// </summary>
    [HttpGet("/.well-known/oauth-authorization-server")]
    public IActionResult GetAuthorizationServerMetadata()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        
        var metadata = new
        {
            issuer = baseUrl,
            // Indicate that we don't actually require authorization
            // MCP clients should proceed without OAuth when these are absent
            // But we need to return valid metadata for discovery to succeed
            token_endpoint = $"{baseUrl}/oauth/token",
            authorization_endpoint = $"{baseUrl}/oauth/authorize",
            registration_endpoint = $"{baseUrl}/oauth/register",
            
            // Supported grant types - empty to indicate none required
            grant_types_supported = new string[] { },
            
            // Response types supported
            response_types_supported = new string[] { },
            
            // No authentication required
            token_endpoint_auth_methods_supported = new[] { "none" },
            
            // PKCE not required since no auth
            code_challenge_methods_supported = new string[] { },
            
            // Scopes - empty since no auth required
            scopes_supported = new string[] { }
        };

        return Ok(metadata);
    }

    /// <summary>
    /// Dummy token endpoint - returns 404 to indicate it's not implemented
    /// MCP clients should fall back to no-auth mode
    /// </summary>
    [HttpPost("/oauth/token")]
    public IActionResult Token()
    {
        return NotFound(new { error = "token_endpoint_not_implemented", error_description = "This server does not require authentication" });
    }

    /// <summary>
    /// Dummy authorization endpoint - returns 404 to indicate it's not implemented
    /// </summary>
    [HttpGet("/oauth/authorize")]
    public IActionResult Authorize()
    {
        return NotFound(new { error = "authorization_not_required", error_description = "This server does not require authentication" });
    }

    /// <summary>
    /// Dummy registration endpoint - returns 404 to indicate it's not implemented
    /// </summary>
    [HttpPost("/oauth/register")]
    public IActionResult Register()
    {
        return NotFound(new { error = "registration_not_required", error_description = "This server does not require authentication" });
    }
}
