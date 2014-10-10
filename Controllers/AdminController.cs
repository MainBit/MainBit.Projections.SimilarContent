/************************************
 * 
 * 
This is copy of Orchard.Projections.Controllers.FilterController
With no changes
 * 
 * 
************************************/

using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Projections.ViewModels;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using MainBit.Projections.SimilarContent.Services;
using System.Collections.Generic;
using Orchard;

namespace MainBit.Projections.SimilarContent
{
    [ValidateInput(false), Admin]
    public class AdminController : Controller
    {
        private readonly ISimilarContentService _similarContentService;
        public AdminController(
            IOrchardServices services,
            IShapeFactory shapeFactory,
            ISimilarContentService similarContentService)
        {
            _similarContentService = similarContentService;
            Services = services;
            Shape = shapeFactory;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public dynamic Shape { get; set; }

        public ActionResult Index()
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.AccessAdminPanel, T("Not authorized to manage similar content")))
                return new HttpUnauthorizedResult();

            return View();
        }

        [HttpPost]
        public ActionResult ClearCache(IEnumerable<string> contentTypes = null)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.AccessAdminPanel, T("Not authorized to manage similar content")))
                return new HttpUnauthorizedResult();

            _similarContentService.ClearCache(contentTypes);
            return RedirectToAction("Index");
        }
    }
}