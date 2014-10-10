using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace MainBit.Projections.SimilarContent
{
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Settings"),
                menu => menu.Add(T("Similar Content"), "1.1", item => item.Action("Index", "Admin", new { area = "MainBit.Projections.SimilarContent" })
                    .Permission(StandardPermissions.AccessAdminPanel))
            );
        }
    }
}
