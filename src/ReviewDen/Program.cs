using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.RateLimiting;
using Microsoft.Extensions.WebEncoders;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ReviewDen;

if (args.Contains("--selftest")) { Images.SelfTest(); return; }

var builder = WebApplication.CreateBuilder(args);

var dataDir = Path.GetFullPath(builder.Configuration["DataDir"] ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data"));
var uploadDir = Path.Combine(dataDir, "uploads");
Directory.CreateDirectory(uploadDir);

builder.Services.AddDbContext<Db>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.Configure<WebEncoderOptions>(o => o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Lang>();
builder.Services.AddSingleton(new UploadDir(uploadDir));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => o.LoginPath = "/admin/login");
builder.Services.AddRazorPages(o => o.Conventions.AuthorizeFolder("/Admin").AllowAnonymousToPage("/Admin/Login"));

// ponytail: per-IP fixed window on POSTs only; behind a proxy add UseForwardedHeaders or every user shares one IP
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        HttpMethods.IsPost(ctx.Request.Method)
            ? RateLimitPartition.GetFixedWindowLimiter(ctx.Connection.RemoteIpAddress?.ToString() ?? "?",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1) })
            : RateLimitPartition.GetNoLimiter("get"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<Db>().Database.Migrate();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.Use((ctx, next) =>
{
    ctx.Response.Headers.XContentTypeOptions = "nosniff";
    return next();
});

app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(uploadDir), RequestPath = "/uploads" });
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/lang/{code}", (string code, string? r, HttpContext ctx) =>
{
    ctx.Response.Cookies.Append(Lang.Cookie, code == "en" ? "en" : "bn", new CookieOptions { MaxAge = TimeSpan.FromDays(365), IsEssential = true });
    return Results.LocalRedirect(r is { Length: > 0 } && r.StartsWith('/') && !r.StartsWith("//") ? r : "/");
});

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();

public record UploadDir(string Path);
