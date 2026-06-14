using System.Collections.Concurrent;

namespace KenmanDesignStudio.Web.Services;

/// <summary>
/// Resolves a project-media path to the best available file. Plates are written as .svg, but if a
/// real photograph with the same base name (e.g. va-24012-1.jpg/.webp/.png) is dropped into the
/// same folder, it is preferred automatically. When neither a raster nor the .svg plate exists on
/// disk — or when no path is supplied at all — a generic branded placeholder is returned instead of
/// a broken image. Results are cached for the app lifetime.
/// </summary>
public class ImageResolver
{
    /// <summary>Generic branded plate shown wherever a project image is missing.</summary>
    public const string Placeholder = "/images/projects/_placeholder.svg";

    private static readonly string[] RasterExtensions = { ".jpg", ".jpeg", ".webp", ".png", ".avif" };
    private static readonly ConcurrentDictionary<string, string> Cache = new();

    private readonly string _webRoot;

    public ImageResolver(IWebHostEnvironment env)
    {
        _webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
    }

    public string Resolve(string? mediaPath)
    {
        if (string.IsNullOrWhiteSpace(mediaPath)) return Placeholder;

        return Cache.GetOrAdd(mediaPath, ResolveUncached);
    }

    private string ResolveUncached(string path)
    {
        var rel = path.TrimStart('/');

        // Prefer a real photograph dropped in beside the generated .svg plate.
        if (path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
        {
            var baseRel = rel[..^4]; // strip ".svg"
            foreach (var ext in RasterExtensions)
            {
                var candidate = baseRel + ext;
                if (FileExists(candidate))
                    return "/" + candidate;
            }
        }

        // Fall back to the placeholder if the requested file isn't actually on disk.
        return FileExists(rel) ? path : Placeholder;
    }

    private bool FileExists(string relPath) =>
        File.Exists(Path.Combine(_webRoot, relPath.Replace('/', Path.DirectorySeparatorChar)));
}
