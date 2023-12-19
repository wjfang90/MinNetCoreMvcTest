using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace MinNetCoreMvc.MinNetCoreMvc {
    public class MinActionContext {
        public HttpContext HttpContext { get; }
        public MinActionDescriptor ActionDescriptor { get; }

        public MinActionContext(HttpContext httpContext, MinActionDescriptor actionDescriptor) {
            this.HttpContext = httpContext;
            this.ActionDescriptor = actionDescriptor;
        }

    }
}
