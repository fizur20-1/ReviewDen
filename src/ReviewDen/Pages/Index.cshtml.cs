using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ReviewDen.Pages;

public class IndexModel(Db db) : PageModel
{
    public const int PageSize = 24;

    public List<Category> Categories { get; private set; } = [];
    public List<Post> Posts { get; private set; } = [];
    public bool HasMore { get; private set; }
    public string Sort { get; private set; } = "trending";
    public int? Cat { get; private set; }
    public string? Q { get; private set; }
    public int PageNo { get; private set; }
    public int TotalReviews { get; private set; }
    public int TotalScams { get; private set; }
    public List<Post> ScamTicker { get; private set; } = [];

    public async Task OnGetAsync(string? sort, int? cat, string? q, int p = 0)
    {
        Sort = sort is "new" or "scam" ? sort : "trending";
        Cat = cat;
        Q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        PageNo = Math.Max(0, p);
        Categories = await db.Categories.OrderBy(c => c.Id).ToListAsync();
        TotalReviews = await db.Posts.CountAsync(x => !x.Hidden);
        TotalScams = await db.Posts.CountAsync(x => !x.Hidden && x.Mood == 5);
        ScamTicker = await db.Posts.Where(x => !x.Hidden && x.Mood == 5).OrderByDescending(x => x.Id).Take(8).ToListAsync();

        var query = db.Posts.Where(x => !x.Hidden);
        if (Sort == "scam") query = query.Where(x => x.Mood == 5);
        if (Cat is int c) query = query.Where(x => x.CategoryId == c);
        if (Q != null) query = query.Where(x => x.Title.Contains(Q) || x.Body.Contains(Q));
        query = query.Include(x => x.Category).Include(x => x.Comments);

        if (Sort != "trending")
        {
            var page = await query.OrderByDescending(x => x.Id).Skip(PageNo * PageSize).Take(PageSize + 1).AsSplitQuery().ToListAsync();
            HasMore = page.Count > PageSize;
            Posts = page.Take(PageSize).ToList();
            return;
        }

        // ponytail: scores the newest 500 in memory, move Score into a SQL computed column when the feed outgrows that
        var now = DateTime.UtcNow;
        var recent = await query.OrderByDescending(x => x.Id).Take(500).AsSplitQuery().ToListAsync();
        var ranked = recent.OrderByDescending(x => x.Score(now)).Skip(PageNo * PageSize).ToList();
        HasMore = ranked.Count > PageSize;
        Posts = ranked.Take(PageSize).ToList();
    }

    public string Link(string? sort = null, int? cat = -1, int? p = null) =>
        Url.Page("/Index", new { sort = sort ?? Sort, cat = cat == -1 ? Cat : cat, q = Q, p })!;
}
