using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace MinNetCoreMvc.MinNetCoreMvc {
    public class MinActionInvoker : IMinActionInvoker {

        private MinActionContext _actionContext { get; }

        public MinActionInvoker(MinActionContext actionContext) {
            this._actionContext = actionContext;
        }

        public async Task InvokeAsync() {
            var requiredServices = _actionContext.HttpContext.RequestServices;
            //create controller
            var controller = ActivatorUtilities.CreateInstance(requiredServices, _actionContext.ActionDescriptor.MethodInfo.DeclaringType!);

            try {
                //bind arguments
                var paremeters = _actionContext.ActionDescriptor.MinParameters;
                var arguments = new object?[paremeters.Length];
                var binder = requiredServices.GetRequiredService<IMinArgumentBinder>();
                for (int index = 0; index < paremeters.Length; index++) {
                    var valueTask = binder.BindAsync(_actionContext, paremeters[index]);
                    if (valueTask.IsCompleted) {
                        arguments[index] = valueTask.Result;
                    }
                    else {
                        arguments[index] = await valueTask;
                    }
                }

                //execute action method
                var executor = requiredServices.GetRequiredService<IMinActionMethodExecutor>();
                var result = executor.Execute(controller, _actionContext.ActionDescriptor, arguments);

                //convert result to IMinActionResult
                var converter = requiredServices.GetRequiredService<IMinActionResultConverter>();
                var convert = converter.ConvertAsync(result);
                var actionResult = convert.IsCompleted ? convert.Result : await convert;

                //execute result
                await actionResult.ExecuteResultAsync(_actionContext);
            }
            catch (Exception ex) {
                throw ex;
            }
            finally {
                (controller as IDisposable)?.Dispose();
            }
        }
    }

    public class MinActionInvokerFactory : IMinActionInvokerFactory {
        public IMinActionInvoker CreateInvoker(MinActionContext actionContext) => new MinActionInvoker(actionContext);
    }
}
