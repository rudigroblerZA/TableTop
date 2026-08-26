using System.Text;

namespace TableTop.Hosting;

/// <summary>
/// Text utilities for rendering card content in UIs that have no rich-text
/// support (MAUI Labels). Card banks use light HTML
/// (&lt;b&gt;, &lt;i&gt;, &lt;br&gt;) that WPF's HtmlTextBlock understands —
/// everywhere else, strip it or the player sees literal tags.
/// </summary>
public static class CardText
{
    /// <summary>Removes all &lt;tag&gt; markup, leaving plain text.</summary>
    public static string StripHtml(string? text)
    {
        if (string.IsNullOrEmpty(text)) return text ?? string.Empty;
        var sb = new StringBuilder(text.Length);
        var inTag = false;
        foreach (var ch in text)
        {
            if (ch == '<') { inTag = true; continue; }
            if (ch == '>') { inTag = false; continue; }
            if (!inTag) sb.Append(ch);
        }
        return sb.ToString();
    }
}
