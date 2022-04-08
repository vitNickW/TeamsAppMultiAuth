using System.Configuration;
using System.Web.Mvc;

namespace TestTeamsAuthSSO.Controllers
{
    public class AuthController : Controller
    {
        // GET: Auth
        public ActionResult msteamsSilent()
        {
            LoadViewBags();
            return PartialView();
        }

        // GET: Auth
        public ActionResult msteamsSilentStart()
        {
            LoadViewBags();
            return PartialView();
        }
        public ActionResult msteamsSilentEnd()
        {
            LoadViewBags();
            return PartialView();
        }

        private void LoadViewBags()
        {
            ViewBag.ClientId = ConfigurationManager.AppSettings["ClientId"];
        }
    }
}