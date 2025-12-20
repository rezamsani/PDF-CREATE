namespace PDF_CREATE;

using QuestPDF.Drawing;
using QuestPDF.Infrastructure;

public static class RenderingExtensions
{
    public static void AddQuestPdfWithFonts(this WebApplicationBuilder builder)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var configuration = builder.Configuration;
        var contentRoot = builder.Environment.ContentRootPath;

        // --------------------
        // Resolve Assets Root
        // --------------------
        var assetsRootConfig = configuration["Rendering:Assets:Root"];

        if (string.IsNullOrWhiteSpace(assetsRootConfig))
        {
            throw new InvalidOperationException("Rendering:Assets:Root is not configured.");
        }

        var assetsRoot = Path.GetFullPath(
            Path.Combine(contentRoot, assetsRootConfig)
        );

        if (!Directory.Exists(assetsRoot))
        {
            throw new DirectoryNotFoundException($"Assets root not found: {assetsRoot}");
        }

        // --------------------
        // Resolve Fonts Root
        // --------------------
        var fontsRootConfig = configuration["Rendering:Assets:Fonts:Root"];

        if (string.IsNullOrWhiteSpace(fontsRootConfig))
        {
            throw new InvalidOperationException("Rendering:Assets:Fonts:Root is not configured.");
        }

        var fontsRoot = Path.Combine(assetsRoot, fontsRootConfig);

        if (!Directory.Exists(fontsRoot))
        {
            throw new DirectoryNotFoundException($"Fonts directory not found: {fontsRoot}");
        }

        // --------------------
        // Register Fonts
        // --------------------
        var familiesSection = configuration.GetSection("Rendering:Assets:Fonts:Families");

        if (!familiesSection.Exists())
        {
            throw new InvalidOperationException("Rendering:Assets:Fonts:Families is not configured.");
        }

        var registeredFonts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var family in familiesSection.GetChildren())
        {
            foreach (var fontNode in family.GetChildren())
            {
                var relativePath = fontNode.Value;

                if (string.IsNullOrWhiteSpace(relativePath))
                {
                    continue;
                }

                var fontPath = Path.GetFullPath(
                    Path.Combine(fontsRoot, relativePath)
                );

                if (!File.Exists(fontPath))
                {
                    throw new FileNotFoundException(
                        $"Font file not found (Family: {family.Key}): {fontPath}"
                    );
                }

                if (registeredFonts.Add(fontPath))
                {
                    FontManager.RegisterFont(File.OpenRead(fontPath));
                }
            }
        }
    }
}
