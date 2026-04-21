using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace BVN.WinForms
{
    public static class VersionHelper
    {
        public static string GetAppVersion()
        {
            // Try to get version from .csproj
            var csprojPath = "BVN.WinForms.csproj";
            if (File.Exists(csprojPath))
            {
                var doc = XDocument.Load(csprojPath);
                var versionElement = doc.Descendants("version").FirstOrDefault();
                if (versionElement != null)
                {
                    return versionElement.Value;
                }
            }
            // Fallback to assembly version
            return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "?";
        }
    }
}
