using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace Fieldnotes.Services;

public sealed class LearnerMiddleware(RequestDelegate next, IDataProtectionProvider provider)
{
    public const string ContextKey = "Fieldnotes.LearnerId";
    public const string CookieName = "fieldnotes.learner";
    private readonly IDataProtector protector = provider.CreateProtector("Fieldnotes.Learner.v1");

    public async Task InvokeAsync(HttpContext context)
    {
        string? learnerId = null;
        if (context.Request.Cookies.TryGetValue(CookieName, out var cookie))
        {
            try
            {
                var candidate = protector.Unprotect(cookie);
                if (Guid.TryParseExact(candidate, "N", out _))
                {
                    learnerId = candidate;
                }
            }
            catch (CryptographicException)
            {
                learnerId = null;
            }
        }

        if (learnerId is null)
        {
            learnerId = Guid.NewGuid().ToString("N");
            context.Response.Cookies.Append(CookieName, protector.Protect(learnerId), new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                MaxAge = TimeSpan.FromDays(365),
                Path = "/"
            });
        }

        context.Items[ContextKey] = learnerId;
        context.Response.Headers.CacheControl = "no-store";
        await next(context);
    }
}