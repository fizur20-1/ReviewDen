using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ReviewDen;

public class Category
{
    public int Id { get; set; }
    [MaxLength(40)] public string NameEn { get; set; } = "";
    [MaxLength(40)] public string NameBn { get; set; } = "";
}

public class Post
{
    public int Id { get; set; }
    [MaxLength(120)] public string Title { get; set; } = "";
    [MaxLength(4000)] public string Body { get; set; } = "";
    [MaxLength(60)] public string ImagePath { get; set; } = "";
    [MaxLength(40)] public string AuthorName { get; set; } = "";
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int Mood { get; set; }
    public int Likes { get; set; }
    public int Reports { get; set; }
    public bool Hidden { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Comment> Comments { get; set; } = [];

    // HN-style decay: newer posts with fewer likes can beat old popular ones
    public double Score(DateTime now) => (Likes + Comments.Count(c => !c.Hidden) * 2 + 1) / Math.Pow((now - CreatedAt).TotalHours + 2, 1.5);
}

public record Mood(int Id, string Key, string Emoji, string Char, string En, string Bn, string Color);

// The reviewer's verdict; drives the review page's full-page animation and the share image theme.
public static class Moods
{
    public static readonly Mood[] All =
    [
        new(1, "love", "1f60d", "😍", "Loved it!", "দারুণ!", "#ff3d7f"),
        new(2, "good", "1f44d", "👍", "Good", "ভালো", "#16a34a"),
        new(3, "okay", "1f610", "😐", "It's okay", "মোটামুটি", "#d97706"),
        new(4, "bad", "1f44e", "👎", "Bad", "খারাপ", "#475569"),
        new(5, "scam", "1f6a8", "🚨", "Scam alert!", "প্রতারণা!", "#dc2626"),
    ];

    public static Mood Of(int id) => All.FirstOrDefault(m => m.Id == id) ?? All[2];

    static readonly Dictionary<int, string> CategoryEmoji = new()
    {
        [1] = "💻 🎧 📺 🔌", [2] = "📱 🔋 📸 📶", [3] = "👗 👟 👜 🕶️", [4] = "🍔 🍕 🍜 🧋",
        [5] = "💄 💅 🧴 🌸", [6] = "🛋️ 🪴 🧹 🛏️", [7] = "📦 🛒 🚚 💳", [8] = "🛍️ 🎁 ⭐ 🏷️",
    };

    public static string ForCategory(int id) => CategoryEmoji.GetValueOrDefault(id, "🛍️ 🎁 ⭐");
}

public class Comment
{
    public int Id { get; set; }
    public int PostId { get; set; }
    [MaxLength(40)] public string AuthorName { get; set; } = "";
    [MaxLength(1000)] public string Body { get; set; } = "";
    public int Reports { get; set; }
    public bool Hidden { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Db(DbContextOptions<Db> options) : DbContext(options)
{
    public const int AutoHideReports = 5;

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Post>().HasIndex(p => p.CreatedAt);
        b.Entity<Category>().HasData(
            new Category { Id = 1, NameEn = "Electronics", NameBn = "ইলেকট্রনিক্স" },
            new Category { Id = 2, NameEn = "Mobile", NameBn = "মোবাইল" },
            new Category { Id = 3, NameEn = "Fashion", NameBn = "ফ্যাশন" },
            new Category { Id = 4, NameEn = "Food", NameBn = "খাবার" },
            new Category { Id = 5, NameEn = "Beauty", NameBn = "বিউটি" },
            new Category { Id = 6, NameEn = "Home", NameBn = "ঘর-গৃহস্থালি" },
            new Category { Id = 7, NameEn = "Online Shop", NameBn = "অনলাইন শপ" },
            new Category { Id = 8, NameEn = "Other", NameBn = "অন্যান্য" });
    }
}
