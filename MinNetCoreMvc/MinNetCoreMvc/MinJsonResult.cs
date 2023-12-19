using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MinNetCoreMvc.MinNetCoreMvc {
    public class MinJsonResult : IMinActionResult {

        private object data;
        public MinJsonResult(object data) { 
            this.data = data;
        }
        public Task ExecuteResultAsync(MinActionContext actionContext) {
            var response = actionContext.HttpContext.Response;
            response.ContentType = "application/json";
            return JsonSerializer.SerializeAsync(response.Body, data);
        }
    }

    public sealed class MinObjectActionResult : IMinActionResult {

        private object data;
        public MinObjectActionResult(object data) {
            this.data = data;
        }

        public Task ExecuteResultAsync(MinActionContext actionContext) {
            var response = actionContext.HttpContext.Response;
            response.ContentType = "text/plain";
            return response.WriteAsync(data.ToString()!);
        }
    }

    public sealed class MinVoidActionResult : IMinActionResult {
        public static readonly IMinActionResult Instance = new MinVoidActionResult();
        public Task ExecuteResultAsync(MinActionContext actionContext)
          => Task.CompletedTask;
    }

    public interface IMinActionResult {
        Task ExecuteResultAsync(MinActionContext actionContext);
    }

    
}
