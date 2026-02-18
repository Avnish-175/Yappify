using System.Net.Mail;

namespace ChatApp2.Helper
{
    public static class CommonHelper
    {
        /// <summary>
        /// To validate Email ID
        /// </summary>
        /// <param name="emailaddress"></param>
        /// <returns></returns>
        public static bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        /// <summary>
        /// To get client IP Address
        /// </summary>
        /// <returns></returns>
        public static string GetIPAddress(this HttpContext context)
        {
            string ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Connection.RemoteIpAddress?.ToString();
            }
            return ipAddress;
        }
    }
}
