using QBitNinja.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BlockchainHub.BlockExplorer.ModelBinders
{
	public class BlockFeatureModelBinder : IModelBinder
	{
		#region IModelBinder Members

		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			if(!typeof(BlockFeature).IsAssignableFrom(bindingContext.ModelType))
			{
				return null;
			}

			ValueProviderResult val = bindingContext.ValueProvider.GetValue(
				bindingContext.ModelName);
			if(val == null)
			{
				return null;
			}

			string key = val.RawValue as string;
			if(key == null)
			{
				return null;
			}

			BlockFeature feature = BlockFeature.Parse(key);
			return feature;
		}

		#endregion
	}
}