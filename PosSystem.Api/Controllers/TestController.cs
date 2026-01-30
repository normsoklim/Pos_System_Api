using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PosSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult PublicEndpoint()
    {
        return Ok(new { message = "This is a public endpoint" });
    }

    [Authorize]
    [HttpGet("protected")]
    public IActionResult ProtectedEndpoint()
    {
        var userId = User.FindFirst("nameid")?.Value;
        var username = User.FindFirst("unique_name")?.Value;
        var email = User.FindFirst("email")?.Value;
        
        return Ok(new 
        { 
            message = "This is a protected endpoint", 
            userId = userId,
            username = username,
            email = email
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult AdminEndpoint()
    {
        return Ok(new { message = "This is an admin-only endpoint" });
    }
}