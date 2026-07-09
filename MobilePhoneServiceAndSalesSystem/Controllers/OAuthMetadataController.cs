using Microsoft.AspNetCore.Mvc;

namespace MobilePhoneServiceAndSalesSystem.Controllers;

/// <summary>
/// OAuth Authorization Server Metadata endpoint for MCP compatibility
/// This is a minimal implementation for development/testing that indicates no authentication is required
/// </summary>
[ApiController]
public class OAuthMetadataController : ControllerBase
{
    [HttpGet("/.well-known/oauth-authorization-server")]
    public IActionResult GetAuthorizationServerMetadata()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        
        var metadata = new
        {
            issuer = baseUrl,

            token_endpoint = $"{baseUrl}/oauth/token",
            authorization_endpoint = $"{baseUrl}/oauth/authorize",
            registration_endpoint = $"{baseUrl}/oauth/register",
            
            grant_types_supported = new string[] { },
            
            response_types_supported = new string[] { },
            
            token_endpoint_auth_methods_supported = new[] { "none" },
            
            code_challenge_methods_supported = new string[] { },
            
            scopes_supported = new string[] { }
        };

        return Ok(metadata);
    }


    [HttpPost("/oauth/token")]
    public IActionResult Token()
    {
        return NotFound(new { error = "token_endpoint_not_implemented", error_description = "This server does not require authentication" });
    }


    [HttpGet("/oauth/authorize")]
    public IActionResult Authorize()
    {
        return NotFound(new { error = "authorization_not_required", error_description = "This server does not require authentication" });
    }


    [HttpPost("/oauth/register")]
    public IActionResult Register()
    {
        return NotFound(new { error = "registration_not_required", error_description = "This server does not require authentication" });
    }
}
