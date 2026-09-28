using Microsoft.AspNetCore.Mvc;

namespace CampusConnect.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class ApiBaseController : ControllerBase
    {
        protected IActionResult ApiOk(object? data, string message = "OK") =>
            Ok(new { success = true, message, data });

        protected IActionResult ApiError(string message, int statusCode = 400)
        {
            return StatusCode(statusCode, new { success = false, message });
        }

        protected int GetCurrentUserId()
        {
            var claim = User.FindFirst("UserId");
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}
