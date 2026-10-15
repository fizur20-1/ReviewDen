using Microsoft.AspNetCore.Html;

namespace ReviewDen;

public static class Ui
{
    // Google Noto animated emoji; site.js swaps to the static .svg when motion is paused.
    public static IHtmlContent Emoji(string code, int size = 22) =>
        new HtmlString($"<img class=\"emo\" data-emoji=\"{code}\" src=\"https://fonts.gstatic.com/s/e/notoemoji/latest/{code}/512.webp\" width=\"{size}\" height=\"{size}\" alt=\"\" aria-hidden=\"true\" loading=\"lazy\" />");

    public static IHtmlContent Icon(string name) => new HtmlString($"<i class=\"bi bi-{name}\" aria-hidden=\"true\"></i>");
}
