# ReviewDen

Anonymous product reviews with photos: post, like, comment, share. Bangla + English.

Stack: ASP.NET Core 10 Razor Pages, EF Core, SQL Server. Uploads are stored in `src/ReviewDen/App_Data/uploads`.

## Run

```bash
cd src/ReviewDen
dotnet user-secrets init
dotnet user-secrets set AdminPassword "<at least 8 chars>"
dotnet run
```

Open http://localhost:5103. The database `ReviewDen` is created on the local SQL Server (`Server=.`) on first start. Change `ConnectionStrings:Default` in `appsettings.json` to use another server.

- `/new` — post a review
- `/admin` — moderation (hide or show reported posts and comments)
- `dotnet run -- --selftest` — checks EXIF stripping and trending order

## Notes

- Every photo is re-encoded to JPEG (max 1600px) in the browser. The server accepts only JPEG and strips EXIF (including GPS) before saving.
- Posts and comments auto-hide after 5 reports.
- POSTs are rate limited to 20 per minute per IP. Behind a reverse proxy, add `UseForwardedHeaders`.
- Facebook link previews only work on a public HTTPS domain, not localhost.
