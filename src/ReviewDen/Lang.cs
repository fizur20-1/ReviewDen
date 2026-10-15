namespace ReviewDen;

// ponytail: in-code dictionary, move to .resx if a third language or translators show up
public class Lang(IHttpContextAccessor http)
{
    public const string Cookie = "lang";

    static readonly Dictionary<string, (string En, string Bn)> S = new()
    {
        ["hero"] = ("Real reviews of", "আসল রিভিউ —"),
        ["mood"] = ("How was it?", "কেমন লাগলো?"),
        ["tagline"] = ("Real people. Real products. Real photos.", "আসল মানুষ। আসল পণ্য। আসল ছবি।"),
        ["trending"] = ("Trending", "ট্রেন্ডিং"),
        ["new"] = ("New", "নতুন"),
        ["all"] = ("All", "সব"),
        ["search"] = ("Search products…", "পণ্য খুঁজুন…"),
        ["post"] = ("Post a review", "রিভিউ দিন"),
        ["empty"] = ("Nothing here yet. Be the first!", "এখনো কিছু নেই। আপনিই প্রথম হোন!"),
        ["more"] = ("Load more", "আরও দেখুন"),
        ["name"] = ("Your name", "আপনার নাম"),
        ["title"] = ("Product name", "পণ্যের নাম"),
        ["category"] = ("Category", "ক্যাটাগরি"),
        ["details"] = ("Details — price, where you bought it, good & bad", "বিস্তারিত — দাম, কোথা থেকে কিনেছেন, ভালো-খারাপ"),
        ["photo"] = ("Photo", "ছবি"),
        ["submit"] = ("Publish", "প্রকাশ করুন"),
        ["by"] = ("by", "লিখেছেন"),
        ["like"] = ("Like", "লাইক"),
        ["report"] = ("Report", "রিপোর্ট"),
        ["comments"] = ("Comments", "মন্তব্য"),
        ["comment"] = ("Write a comment…", "মন্তব্য লিখুন…"),
        ["send"] = ("Send", "পাঠান"),
        ["share"] = ("Share", "শেয়ার"),
        ["card"] = ("Save image", "ছবি সেভ"),
        ["live"] = ("Your review is live! Save the image and share it.", "আপনার রিভিউ প্রকাশ হয়েছে! ছবি সেভ করে শেয়ার করুন।"),
        ["read_more"] = ("Read the full review", "পুরো রিভিউ পড়ুন"),
        ["copied"] = ("Copied!", "কপি হয়েছে!"),
        ["reported"] = ("Thanks, we'll take a look.", "ধন্যবাদ, আমরা দেখে নেব।"),
        ["bad_image"] = ("Please upload a JPG/PNG photo under 5 MB.", "৫ MB এর কম JPG/PNG ছবি দিন।"),
        ["required"] = ("Required", "আবশ্যক"),
        ["share_text"] = ("Check out this review on ReviewDen", "ReviewDen এ এই রিভিউটা দেখুন"),
        ["post_sub"] = ("Takes 1 minute. Help others buy smart.", "১ মিনিট লাগবে। অন্যদের সঠিক জিনিস কিনতে সাহায্য করুন।"),
        ["photo_hint"] = ("Tap to add a real photo of the product", "পণ্যের আসল ছবি দিতে ট্যাপ করুন"),
        ["title_ph"] = ("e.g. Redmi Note 14, Aarong panjabi…", "যেমন: Redmi Note 14, আড়ং পাঞ্জাবি…"),
        ["details_ph"] = ("Price, shop/page name, delivery, quality, would you buy again?", "দাম, দোকান/পেজের নাম, ডেলিভারি, কোয়ালিটি, আবার কিনবেন?"),
        ["name_ph"] = ("Name or nickname", "নাম বা ডাকনাম"),
        ["comment_first"] = ("No comments yet. Ask something or share your experience!", "এখনো কোনো মন্তব্য নেই। প্রশ্ন করুন বা অভিজ্ঞতা জানান!"),
        ["as"] = ("Commenting as", "মন্তব্য করছেন"),
        ["quick"] = ("Quick replies", "দ্রুত উত্তর"),
        ["scam_tab"] = ("Scam alerts", "প্রতারণা সতর্কতা"),
        ["reviews"] = ("reviews", "রিভিউ"),
        ["scams"] = ("scam alerts", "প্রতারণা সতর্কতা"),
        ["ticker"] = ("Beware", "সাবধান"),
        ["hero_sub"] = ("Before you buy from a Facebook page or shop, see what real buyers say — with real photos.", "ফেসবুক পেজ বা দোকান থেকে কেনার আগে দেখুন আসল ক্রেতারা কী বলছে — আসল ছবিসহ।"),
    };

    public bool Bn => http.HttpContext?.Request.Cookies[Cookie] != "en";
    public string Code => Bn ? "bn" : "en";
    public string this[string key] => S.TryGetValue(key, out var s) ? (Bn ? s.Bn : s.En) : key;
    public string Cat(Category? c) => c == null ? "" : Bn ? c.NameBn : c.NameEn;

    public string Ago(DateTime utc)
    {
        var t = DateTime.UtcNow - utc;
        return t.TotalMinutes < 1 ? (Bn ? "এইমাত্র" : "just now")
            : t.TotalHours < 1 ? (Bn ? $"{(int)t.TotalMinutes} মিনিট আগে" : $"{(int)t.TotalMinutes}m ago")
            : t.TotalDays < 1 ? (Bn ? $"{(int)t.TotalHours} ঘণ্টা আগে" : $"{(int)t.TotalHours}h ago")
            : t.TotalDays < 30 ? (Bn ? $"{(int)t.TotalDays} দিন আগে" : $"{(int)t.TotalDays}d ago")
            : utc.ToString("dd MMM yyyy");
    }

    // Tap-to-fill comment starters, tuned to the review's mood.
    public string[] QuickReplies(string mood) => (mood, Bn) switch
    {
        ("scam", true) => ["পেজের লিংক দিন", "আমিও প্রতারিত হয়েছি 😡", "সাবধান করার জন্য ধন্যবাদ!"],
        ("scam", false) => ["Share the page link", "Same happened to me 😡", "Thanks for the warning!"],
        ("bad", true) => ["রিফান্ড পেয়েছেন?", "আমারও একই অভিজ্ঞতা", "ভালো বিকল্প কী?"],
        ("bad", false) => ["Did you get a refund?", "Same experience here", "What's a better option?"],
        (_, true) => ["দাম কত?", "কোথা থেকে কিনলেন?", "আমিও কিনেছি! 👍"],
        (_, false) => ["How much was it?", "Where did you buy it?", "I bought it too! 👍"],
    };
}
