using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Briver.Web.ModelBinding
{
    public class JsonModelBinderProvider : IModelBinderProvider
    {
        private readonly JsonModelBinder _jsonBinder = new JsonModelBinder();

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.IsComplexType)
            {
                return _jsonBinder;
            }
            return null;
        }

        private class JsonModelBinder : IModelBinder
        {
            public Task BindModelAsync(ModelBindingContext context)
            {
                var json = context.ValueProvider.GetValue(context.FieldName).FirstValue;

                object model = null;
                if (!String.IsNullOrEmpty(json))
                {
                    model = JsonConvert.DeserializeObject(json, context.ModelType);
                }
                else if (context.ModelType.IsValueType)
                {
                    model = Activator.CreateInstance(context.ModelType);
                }
                context.Result = ModelBindingResult.Success(model);

                return Task.CompletedTask;
            }
        }

    }

}
