namespace LastGrind.WebApi.Extensions
{
    public static class GeneralExtensions
    {
        public static string GetUserId(this HttpContext httpContext)
        {
            if (httpContext.User == null) return string.Empty;

            return httpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;
        }

    }
}
