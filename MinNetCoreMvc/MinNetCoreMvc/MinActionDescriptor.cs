using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MinNetCoreMvc.MinNetCoreMvc {
    public class MinActionDescriptor {
        public MethodInfo MethodInfo { get; }
        /// <summary>
        /// 特性路由模板
        /// </summary>
        public IRouteTemplateProvider? RouteTemplateProvider { get; }
        public string ControllerName { get; }
        public string ActionName { get; }
        public MinParameterDescriptor[] MinParameters { get; }
        //public ParameterDescriptor[] Parameters { get; }

        public MinActionDescriptor(MethodInfo methodInfo, IRouteTemplateProvider? routeTemplateProvider) {
            this.MethodInfo = methodInfo;
            this.RouteTemplateProvider = routeTemplateProvider;
            this.ControllerName = methodInfo.DeclaringType!.Name[..^"Controller".Length];
            this.ActionName = methodInfo.Name;
            //this.Parameters = methodInfo.GetParameters()
            //                            .Select(t => new ParameterDescriptor() {
            //                                Name = t.Name!,
            //                                ParameterType = t.ParameterType,
            //                                BindingInfo = BindingInfo.GetBindingInfo(t.GetCustomAttributes())
            //                            })
            //                            .ToArray();

            this.MinParameters = methodInfo.GetParameters()
                                        .Select(t => new MinParameterDescriptor(t))
                                        .ToArray();

        }
    }
}
