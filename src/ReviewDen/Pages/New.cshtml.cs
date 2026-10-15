using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ReviewDen.Pages;

[RequestFormLimits(MultipartBodyLengthLimit = Images.MaxBytes + 64 * 1024)]
[RequestSizeLimit(Images.MaxBytes + 64 * 1024)]
public class NewModel(Db db, UploadDir uploads, Lang L) : PageModel
{
    [BindProperty, Required, StringLength(40, MinimumLength = 2)] public string AuthorName { get; set; } = "";
    [BindProperty, Required, StringLength(120, MinimumLength = 3)] public string Title { get; set; } = "";
    [BindProperty, Required, StringLength(4000, MinimumLength = 10)] public string Body { get; set; } = "";
    [BindProperty] public int CategoryId { get; set; }
    [BindProperty, Range(1, 5)] public int Mood { get; set; }
    [BindProperty, Required] public IFormFile? Photo { get; set; }

    public List<Category> Categories { get; private set; } = [];

    public async Task OnGetAsync() => Categories = await db.Categories.OrderBy(c => c.Id).ToListAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await db.Categories.AnyAsync(c => c.Id == CategoryId)) ModelState.AddModelError(nameof(CategoryId), L["required"]);

        byte[]? jpeg = null;
        if (Photo is { Length: > 0 and <= Images.MaxBytes })
        {
            using var ms = new MemoryStream();
            await Photo.CopyToAsync(ms);
            jpeg = Images.CleanJpeg(ms.ToArray());
        }
        if (jpeg == null) ModelState.AddModelError(nameof(Photo), L["bad_image"]);

        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var file = $"{Guid.NewGuid():N}.jpg";
        await System.IO.File.WriteAllBytesAsync(Path.Combine(uploads.Path, file), jpeg!);
        var post = new Post { AuthorName = AuthorName.Trim(), Title = Title.Trim(), Body = Body.Trim(), CategoryId = CategoryId, Mood = Mood, ImagePath = file };
        db.Posts.Add(post);
        await db.SaveChangesAsync();
        return Redirect($"/p/{post.Id}?fresh=1");
    }
}
