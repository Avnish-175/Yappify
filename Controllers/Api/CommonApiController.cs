using ChatApp2.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ChatApp2.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommonApiController : ControllerBase
    {
        //private readonly IHttpContextAccessor _httpContextAccessor;

        //public CommonApiController(IHttpContextAccessor httpContextAccessor)
        //{
        //    _httpContextAccessor = httpContextAccessor;
        //}

        [HttpGet]
        [ActionName("getMasterDataByCode")]
        public IActionResult GetMasterDataByCode(string Code)
        {
            ManageData ObjManageData = new ManageData();
            if (Code == "6") //Location Code
            {
                var Data = ObjManageData.GetGetUserAreaByUserID(HttpContext.Session.GetString("UserID"));
                if (Data.Rows.Count > 0)
                    return Ok(JsonConvert.SerializeObject(Data));
                else
                    return Ok(JsonConvert.SerializeObject(ObjManageData.GetMasterDataByCode(Code, HttpContext.Session.GetString("UserID"))));
            }
            else
                return Ok(JsonConvert.SerializeObject(ObjManageData.GetMasterDataByCode(Code, HttpContext.Session.GetString("UserID"))));
        }

        //public string GetClientIP()
        //{
        //    // Check if HttpContext and its properties are available
        //    if (HttpContext?.Request?.Headers == null)
        //        return "Unable to determine client IP.";

        //    // Attempt to get the IP from the X-Forwarded-For header or RemoteIpAddress
        //    var clientIp = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
        //                   ?? HttpContext.Connection?.RemoteIpAddress?.ToString();

        //    // Fallback for localhost
        //    if (string.IsNullOrEmpty(clientIp) || clientIp == "::1")
        //        clientIp = "127.0.0.1"; // Default for localhost

        //    return clientIp;
        //}


    }
}
