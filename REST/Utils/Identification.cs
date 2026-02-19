using System.Security.Claims;

namespace REST.Utils
{
    public static class Identification
    {
        public static int? GetUserId(ClaimsPrincipal user)
        {
            string? idString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (idString == null)
            {
                return null;
            }
            int id;
            bool parsed = Int32.TryParse(idString, out id);
            if (parsed)
            {
                return id;
            }
            else
            {
                return null;
            }
        }
    }
}
