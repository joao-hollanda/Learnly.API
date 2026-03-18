using Microsoft.AspNetCore.Mvc;

namespace Learnly.API.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected int? GetUserId()
        {
            var value = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(value)) return null;
            return int.TryParse(value, out var id) ? id : null;
        }
    }
}