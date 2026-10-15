using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ReviewDen.Pages.Admin;

public class IndexModel(Db db) : PageModel
{
    public List<Post> Posts { get; private set; } = [];
    public List<Comment> Comments { get; private set; } = [];
    public bool All { get; private set; }

    public async Task OnGetAsync(bool all = false)
    {
        All = all;
        var posts = db.Posts.AsQueryable();
        if (!all) posts = posts.Where(p => p.Reports > 0 || p.Hidden);
        Posts = await posts.OrderByDescending(p => p.Id).Take(100).ToListAsync();
        Comments = await db.Comments.Where(c => c.Reports > 0 || c.Hidden).OrderByDescending(c => c.Id).Take(100).ToListAsync();
    }

    public async Task<IActionResult> OnPostPostAsync(int id, bool hide)
    {
        await db.Posts.Where(p => p.Id == id).ExecuteUpdateAsync(s => s.SetProperty(p => p.Hidden, hide).SetProperty(p => p.Reports, 0));
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCommentAsync(int id, bool hide)
    {
        await db.Comments.Where(c => c.Id == id).ExecuteUpdateAsync(s => s.SetProperty(c => c.Hidden, hide).SetProperty(c => c.Reports, 0));
        return RedirectToPage();
    }
}
