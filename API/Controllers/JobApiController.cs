using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobApiController : ControllerBase
    {
        // Endpoint protected with JWT authentication (requires a user token)
        [HttpGet("jwt-protected")]
        [Authorize]
        public IActionResult GetJwtProtected()
        {
            return Ok(new { message = "This endpoint is protected with JWT authentication" });
        }
        
        // Endpoint protected with API key authentication (accessible by jobs)
        [HttpGet("apikey-protected")]
        [Authorize(Policy = "JobApiKey")]
        public IActionResult GetApiKeyProtected()
        {
            return Ok(new { message = "This endpoint is protected with API key authentication" });
        }
        
        // Endpoint accessible by both JWT and API key
        [HttpGet("both-protected")]
        [Authorize(AuthenticationSchemes = "Bearer,ApiKey")]
        public IActionResult GetBothProtected()
        {
            return Ok(new { message = "This endpoint is accessible with either JWT or API key authentication" });
        }
    }
}