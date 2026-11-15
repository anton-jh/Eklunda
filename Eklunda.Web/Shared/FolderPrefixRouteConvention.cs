using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Eklunda.Web.Shared;

/// <summary>
/// Strips top-level folder names (e.g. "Pages", "Modules") from page routes, so
/// /Pages/Index is served at "/" and /Modules/Heating/X at "/Heating/X".
/// </summary>
public class FolderPrefixRouteConvention(params string[] folders) : IPageRouteModelConvention
{
    public void Apply(PageRouteModel model)
    {
        foreach (var selector in model.Selectors)
        {
            var route = selector.AttributeRouteModel;
            if (route?.Template is not { } template)
                continue;

            foreach (var folder in folders)
            {
                if (template.Equals(folder, StringComparison.OrdinalIgnoreCase))
                {
                    route.Template = "";
                    break;
                }

                if (template.StartsWith(folder + "/", StringComparison.OrdinalIgnoreCase))
                {
                    route.Template = template[(folder.Length + 1)..];
                    break;
                }
            }
        }
    }
}
