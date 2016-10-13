using BlockchainHub.BlockExplorer.ModelBinders;
using NBitcoin;
using QBitNinja.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BlockchainHub.BlockExplorer
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Main", action = "Index", id = UrlParameter.Optional }
            //);

            routes.MapMvcAttributeRoutes();

			System.Web.Mvc.ModelBinders.Binders.Add(typeof(uint160), new UInt160ModelBinder());
			System.Web.Mvc.ModelBinders.Binders.Add(typeof(uint256), new UInt256ModelBinder());
			System.Web.Mvc.ModelBinders.Binders.Add(typeof(BlockFeature), new BlockFeatureModelBinder());
		}
    }
}
