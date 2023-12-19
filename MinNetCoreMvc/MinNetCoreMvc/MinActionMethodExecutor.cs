using Microsoft.AspNetCore.Mvc.Abstractions;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace MinNetCoreMvc.MinNetCoreMvc {
    public class MinActionMethodExecutor : IMinActionMethodExecutor { 

        private readonly ConcurrentDictionary<MethodInfo, Func<object, object?[], object?>> _executors = new();
        public object? Execute(object controller, MinActionDescriptor actionDescriptor, object?[] arguments) => _executors.GetOrAdd(actionDescriptor.MethodInfo, CreateExecutor).Invoke(controller, arguments);

        private Func<object, object?[], object?> CreateExecutor(MethodInfo methodInfo) {
            var controller = Expression.Parameter(typeof(object));
            var arguments = Expression.Parameter(typeof(object?[]));

            var parameters = methodInfo.GetParameters();
            var convertedArguments = new Expression[parameters.Length];
            for (int index = 0; index < parameters.Length; index++) {
                var exp = Expression.ArrayIndex(arguments, Expression.Constant(index));
                convertedArguments[index] = Expression.Convert(exp, parameters[index].ParameterType);
            }

            var convertedController = Expression.Convert(controller, methodInfo.DeclaringType!);

            var call = Expression.Call(convertedController, methodInfo, convertedArguments);

            //return Expression.Lambda<Func<object, object?[], object?>>(call, controller, arguments).Compile();
            //fang add  Expression of type 'System.Threading.Tasks.ValueTask`1[Result]' cannot be used for return type 'System.Object'
            var rerutyType = methodInfo.ReturnType;
            if (rerutyType.IsGenericType && rerutyType.BaseType == typeof(ValueType)) {

                var awaitedCall = Expression.Call(typeof(ValueTaskExtensions), nameof(ValueTaskExtensions.AsTask), new[] { methodInfo.ReturnType.GetGenericArguments()[0] }, call);
                var convertResult = Expression.Convert(awaitedCall, typeof(object));
                return Expression.Lambda<Func<object, object?[], object?>>(convertResult, controller, arguments).Compile();
            }
            else {

                return Expression.Lambda<Func<object, object?[], object?>>(call, controller, arguments).Compile();
            }
        }
    }

    public interface IMinActionMethodExecutor {
        object? Execute(object controller, MinActionDescriptor actionDescriptor, object?[] arguments);
    }

    /// <summary>
    /// fang add
    /// </summary>
    public static class ValueTaskExtensions {
        public static Task<T> AsTask<T>(this ValueTask<T> valueTask) {
            return valueTask.AsTask();
        }
    }
}
