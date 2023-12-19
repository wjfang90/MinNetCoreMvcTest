using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace MinNetCoreMvc.MinNetCoreMvc {

    /// <summary>
    /// 
    /// </summary>
    public class MinActionResultConverter : IMinActionResultConverter {

        private readonly MethodInfo _valueTaskConvertMethod = typeof(MinActionResultConverter).GetMethod(nameof(ConvertFromValueTask))!;
        private readonly MethodInfo _taskConvertMethod = typeof(MinActionResultConverter).GetMethod(nameof(ConvertFromTask))!;
        private readonly ConcurrentDictionary<Type, Func<object, ValueTask<IMinActionResult>>> _converters = new();
        public ValueTask<IMinActionResult> ConvertAsync(object? result) {
            if (result is null) {
                return ValueTask.FromResult(MinVoidActionResult.Instance);
            }

            // Task<IMinActionResult>
            if (result is Task<IMinActionResult> taskActionResult) {
                return new ValueTask<IMinActionResult>(taskActionResult);
            }

            //ValueTask<IMinActionResult>
            if (result is ValueTask<IMinActionResult> valueTaskActionResult) {
                return valueTaskActionResult;
            }

            //IMinActionResult
            if (result is IMinActionResult actionResult) {
                return ValueTask.FromResult(actionResult);
            }

            //ValueTask
            if (result is ValueTask valueTask) {
                return Convert(valueTask);
            }

            var resultType = result.GetType();
            //Task
            if (resultType == typeof(Task)) {
                return Convert((Task)result);
            }

            //ValueTask<T>
            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(ValueTask<>)) {
                return _converters.GetOrAdd(resultType, t => CreateValueTaskConverter(t, _valueTaskConvertMethod))
                                  .Invoke(result);
            }

            //Task<T>
            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>)) {
                return _converters.GetOrAdd(resultType, t => CreateValueTaskConverter(t, _taskConvertMethod))
                                  .Invoke(result);
            }

            //object
            return ValueTask.FromResult<IMinActionResult>(new MinObjectActionResult(result));

        }

        public static async ValueTask<IMinActionResult> ConvertFromValueTask<T>(ValueTask<T> valueTask) {
            var result = valueTask.IsCompleted ? valueTask.Result : await valueTask;
            return result is IMinActionResult actionResult ? actionResult : new MinObjectActionResult(result!);
        }

        public static async ValueTask<IMinActionResult> ConvertFromTask<T>(Task<T> task) {
            var result = await task;
            return result is IMinActionResult actionResult ? actionResult : new MinObjectActionResult(result!);
        }

        private static async ValueTask<IMinActionResult> Convert(ValueTask valueTask) {
            if (!valueTask.IsCompleted) await valueTask;
            return MinVoidActionResult.Instance;
        }

        private static async ValueTask<IMinActionResult> Convert(Task task) {
            await task;
            return MinVoidActionResult.Instance;
        }

        private static Func<object, ValueTask<IMinActionResult>> CreateValueTaskConverter(Type valueTaskType, MethodInfo convertMethod) {
            var parameter = Expression.Parameter(typeof(object));
            var convert = Expression.Convert(parameter, valueTaskType);
            var method = convertMethod.MakeGenericMethod(valueTaskType.GetGenericArguments()[0]);
            var call = Expression.Call(method, convert);

            return Expression.Lambda<Func<object, ValueTask<IMinActionResult>>>(call, parameter).Compile();

        }


    }



    public interface IMinActionResultConverter {
        ValueTask<IMinActionResult> ConvertAsync(object? result);
    }
}
