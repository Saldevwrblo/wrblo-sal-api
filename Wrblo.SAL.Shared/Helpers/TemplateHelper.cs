namespace Wrblo.SAL.Shared.Helpers;

public static class TemplateHelper
{
    public static string GetTemplatePath(string templateFileName)
    {
        string prodPath = Path.Combine(AppContext.BaseDirectory, "Templates", templateFileName);

        if (File.Exists(prodPath))
            return prodPath;

        // Optional: fallback for local development
        string devPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", templateFileName);

        if (File.Exists(devPath))
            return devPath;

        throw new FileNotFoundException($"{templateFileName} file not found");
    }
}
