using System.Web.Optimization;

namespace FileUpload
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                "~/scripts/jquery-{version}.js",
                "~/scripts/bootstrap.js",
                "~/scripts/app.js"));

            bundles.Add(new StyleBundle("~/content/css").Include(
                "~/content/bootstrap.css",
                "~/content/site.css"));
        }
    }
}
