using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ReviewDen.Pages;

public class PModel(Db db) : PageModel
{
    public Post Post { get; private set; } = null!;
    public bool Liked { get; private set; }

    [BindProperty, Required, StringLength(40, MinimumLength = 2)] public string AuthorName { get; set; } = "";
    [BindProperty, Required, StringLength(1000, MinimumLength = 1)] public string Body { get; set; } = "";

    async Task<bool> Load(int id)
    {
        var post = await db.Posts.Include(p => p.Category).Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == id && !p.Hidden);
        if (post == null) return false;
        Post = post;
        Liked = Seen("liked").Contains(id);
        return true;
    }

    public async Task<IActionResult> OnGetAsync(int id) => await Load(id) ? Page() : NotFound();

    public async Task<IActionResult> OnPostCommentAsync(int id)
    {
        if (!await Load(id)) return NotFound();
        if (!ModelState.IsValid) return Page();
        db.Comments.Add(new Comment { PostId = id, AuthorName = AuthorName.Trim(), Body = Body.Trim() });
        await db.SaveChangesAsync();
        return Redirect($"/p/{id}#comments");
    }

    // ponytail: one-like-per-browser via cookie, clearing cookies allows re-likes; the POST rate limit bounds the damage
    public async Task<IActionResult> OnPostLikeAsync(int id)
    {
        if (Remember("liked", id)) await db.Posts.Where(p => p.Id == id).ExecuteUpdateAsync(s => s.SetProperty(p => p.Likes, p => p.Likes + 1));
        return Redirect($"/p/{id}");
    }

    public async Task<IActionResult> OnPostReportAsync(int id, int? commentId)
    {
        if (Remember("reported", commentId is int c ? -c : id))
        {
            if (commentId != null)
                await db.Comments.Where(x => x.Id == commentId && x.PostId == id).ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Reports, x => x.Reports + 1)
                    .SetProperty(x => x.Hidden, x => x.Hidden || x.Reports + 1 >= Db.AutoHideReports));
            else
                await db.Posts.Where(x => x.Id == id).ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Reports, x => x.Reports + 1)
                    .SetProperty(x => x.Hidden, x => x.Hidden || x.Reports + 1 >= Db.AutoHideReports));
        }
        TempData["msg"] = "reported";
        return Redirect($"/p/{id}");
    }

    List<int> Seen(string cookie) =>
        (Request.Cookies[cookie] ?? "").Split('.', StringSplitOptions.RemoveEmptyEntries).Select(s => int.TryParse(s, out var n) ? n : 0).ToList();

    bool Remember(string cookie, int id)
    {
        var ids = Seen(cookie);
        if (ids.Contains(id)) return false;
        ids.Add(id);
        Response.Cookies.Append(cookie, string.Join('.', ids.TakeLast(300)), new CookieOptions { MaxAge = TimeSpan.FromDays(365), HttpOnly = true, IsEssential = true });
        return true;
    }
}
