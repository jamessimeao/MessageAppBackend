using MessageREST.Data;
using Microsoft.AspNetCore.Mvc;

namespace MessageREST.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class MessageController(IDataAccess dataAccess) : ControllerBase
    {
    }
}
