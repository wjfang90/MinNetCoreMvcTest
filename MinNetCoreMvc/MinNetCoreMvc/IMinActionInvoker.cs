using Microsoft.AspNetCore.Mvc;

namespace MinNetCoreMvc.MinNetCoreMvc {
    public interface IMinActionInvoker {
        Task InvokeAsync();
    }

    public interface IMinActionInvokerFactory {
        IMinActionInvoker CreateInvoker(MinActionContext actionContext);
    }
}
