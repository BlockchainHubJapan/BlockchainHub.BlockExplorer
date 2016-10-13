using NBitcoin;
using System;
using System.Reflection;
using System.Web.Mvc;

namespace BlockchainHub.BlockExplorer.ModelBinders
{
    public class UInt256ModelBinder : IModelBinder
    {
        #region IModelBinder Members

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
            if(!typeof(uint256).IsAssignableFrom(bindingContext.ModelType))
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
            return uint256.Parse(key);
        }

        #endregion
    }

    public class UInt160ModelBinder : IModelBinder
    {
        #region IModelBinder Members

		
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
            if(!typeof(uint160).IsAssignableFrom(bindingContext.ModelType))
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
                bindingContext.Model = null;
                return true;
            }
            var r = uint160.Parse(key);
            return r;
        }

        #endregion
    }
}
