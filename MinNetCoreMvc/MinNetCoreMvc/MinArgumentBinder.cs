using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinNetCoreMvc.MinNetCoreMvc {
    /// <summary>
    /// 绑定Action方法参数
    /// </summary>
    public class MinArgumentBinder : IMinArgumentBinder {
        private readonly ConcurrentDictionary<Type, object?> _defaults = new();
        private readonly MethodInfo _method = typeof(MinArgumentBinder).GetMethod(nameof(GetDefaultValue))!;

        public static T GetDefaultValue<T>() => default!;

        public ValueTask<object?> BindAsync(MinActionContext actionContext, MinParameterDescriptor parameterDescriptor) {
            var requredServices = actionContext.HttpContext.RequestServices;
            var parameterInfo = parameterDescriptor.ParameterInfo;
            var parameterName = parameterInfo.Name!;
            var parameterType = parameterInfo.ParameterType;

            //from required service
            var result = requredServices.GetService(parameterType);
            if (result is not null) {
                return ValueTask.FromResult(result)!;
            }

            //from query
            var request = actionContext.HttpContext.Request;
            if (request.Query.TryGetValue(parameterName, out var value1)) {
                return ValueTask.FromResult(Convert.ChangeType((string)value1!, parameterType))!;
            }
            //from route
            if (request.RouteValues.TryGetValue(parameterName, out var value2)) {
                return ValueTask.FromResult(Convert.ChangeType((string)value2!, parameterType)!)!;
            }
            //from body
            if (request.ContentLength > 0) {
                var jsonOption = new JsonSerializerOptions() {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };
                return JsonSerializer.DeserializeAsync(request.Body, parameterType, jsonOption);
            }

            //from default value
            var defaultValue = _defaults.GetOrAdd(parameterType, type => _method.MakeGenericMethod(parameterType).Invoke(null, null));
            return ValueTask.FromResult(defaultValue);

        }
    }

    public interface IMinArgumentBinder {
        public ValueTask<object?> BindAsync(MinActionContext actionContext, MinParameterDescriptor parameterDescriptor);
    }
}
