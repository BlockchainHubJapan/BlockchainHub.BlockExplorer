using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BlockchainHub.BlockExplorer.Controllers
{
    public class MainController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("addresses")]
        public ActionResult Address()
        {
            return View();
        }

        [Route("blocks")]
        public ActionResult Block()
        {
            return View();
        }

        [Route("transactions")]
        public ActionResult Transaction()
        {
            return View();
        }
    }
}