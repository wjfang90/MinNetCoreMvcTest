using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;

namespace MinNetCoreMvc.MinNetCoreMvc {
    public class MinActionEndpointDataSource : EndpointDataSource {

        private readonly List<(
                string RouteName,
                string Template,
                RouteValueDictionary? Defaults,
                IDictionary<string, object?>? Constraints,
                RouteValueDictionary? DataTokens,
                List<Action<EndpointBuilder>> Conventions,
                List<Action<EndpointBuilder>> FinallyConventions)>
        _conventionalRoutes = new();

        private readonly IServiceProvider _serviceProvider;
        private readonly IMinActionDescriptorCollectionProvider _actions;
        private readonly RoutePatternTransformer _transformer;
        private readonly List<Action<EndpointBuilder>> _conventions = new();
        private readonly List<Action<EndpointBuilder>> _finallyConventions = new();
        private int _routeOrder;
        private List<Endpoint>? _endpoints;
        private MinEndpointConventionBuilder DefaultBuilder { get; }

        public override IReadOnlyList<Endpoint> Endpoints => _endpoints ??= _actions.MinActionDescriptors.SelectMany(CreateEndpoints).ToList();

        public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;
        




        public MinActionEndpointDataSource(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
            _actions = serviceProvider.GetRequiredService<IMinActionDescriptorCollectionProvider>();
            _transformer = serviceProvider.GetRequiredService<RoutePatternTransformer>();
            DefaultBuilder = new MinEndpointConventionBuilder(_conventions, _finallyConventions);
        }


        public IEndpointConventionBuilder AddRoute(string routeName, string pattern, RouteValueDictionary? defaults, IDictionary<string, object?>? constraints, RouteValueDictionary? dataTokens) {

            var conventions = new List<Action<EndpointBuilder>>();
            var finallyConvetions = new List<Action<EndpointBuilder>>();

            _conventionalRoutes.Add((routeName, pattern, defaults, constraints, dataTokens, conventions, finallyConvetions));

            return new MinEndpointConventionBuilder(conventions, finallyConvetions);
        }

        private static Task HandleRequestAsync(HttpContext httpContext) {
            var endpoint = httpContext.GetEndpoint() ?? throw new InvalidOperationException("No endpoint is matched to the current request.");
            var actionDescriptor = endpoint.Metadata.GetMetadata<MinActionDescriptor>() ?? throw new InvalidOperationException("No ActionDescriptor is attached to the endpoint as metadata.");

            var actionContenxt = new MinActionContext(httpContext, actionDescriptor);
            return httpContext.RequestServices.GetRequiredService<IMinActionInvokerFactory>()
                    .CreateInvoker(actionContenxt)
                    .InvokeAsync();
        }

        private IEnumerable<Endpoint> CreateEndpoints(MinActionDescriptor actionDescriptor) {
            var routeValues = new RouteValueDictionary
                {
                    {"controller", actionDescriptor.ControllerName },
                    { "action", actionDescriptor.ActionName }
                };
            var attributes = actionDescriptor.MethodInfo.GetCustomAttributes(true)
                            .Union(actionDescriptor.MethodInfo.DeclaringType!.GetCustomAttributes(true));

            var routeTemplateProvider = actionDescriptor.RouteTemplateProvider;
            if (routeTemplateProvider is null) {
                var endpoints = CreateConventionalEndpoints(actionDescriptor, routeValues, attributes);
                foreach (var endpoint in endpoints) {
                    yield return endpoint;
                }
            }
            else {
                yield return CreateAttributeEndpoint(actionDescriptor, routeValues, attributes);
            }
        }

        /// <summary>
        /// 创建约束路由终结点
        /// </summary>
        /// <param name="actionDescriptor"></param>
        /// <param name="routeValues"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        private IEnumerable<Endpoint> CreateConventionalEndpoints(MinActionDescriptor actionDescriptor, RouteValueDictionary routeValues, IEnumerable<object> attributes) {
            foreach (var (routeName, template, defaults, constraints, dataTokens, conventionals, finallyConventionals) in _conventionalRoutes) {
                var pattern = RoutePatternFactory.Parse(template, defaults, constraints);
                pattern = _transformer.SubstituteRequiredValues(pattern, routeValues);

                if (pattern is not null) {
                    var builder = new RouteEndpointBuilder(HandleRequestAsync, pattern, _routeOrder++) {
                        //ApplicationServices = _serviceProvider; 
                    };

                    builder.Metadata.Add(actionDescriptor);

                    foreach (var attribute in attributes) {
                        builder.Metadata.Add(attribute);
                    }

                    yield return builder.Build();
                }
            }
        }

        /// <summary>
        /// 创建特性路由终结点
        /// </summary>
        /// <param name="actionDescriptor"></param>
        /// <param name="routeValues"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        private Endpoint CreateAttributeEndpoint(MinActionDescriptor actionDescriptor, RouteValueDictionary routeValues, IEnumerable<object> attributes) { 
            var routeTemplateProvider = actionDescriptor.RouteTemplateProvider!;
            var pattern = RoutePatternFactory.Parse(routeTemplateProvider.Template!);

            var builder = new RouteEndpointBuilder(HandleRequestAsync, pattern, _routeOrder++) {
                //ApplicationServices = _serviceProvider; 
            };

            builder.Metadata.Add(actionDescriptor);

            foreach (var attribute in attributes) {
                builder.Metadata.Add(attribute);
            }

            if (routeTemplateProvider is IActionHttpMethodProvider httpMethodProvidor) {
                builder.Metadata.Add(new HttpMethodActionConstraint(httpMethodProvidor.HttpMethods));
            }

            return builder.Build();
        }

        /// <summary>
        /// 
        /// </summary>
        public sealed class MinEndpointConventionBuilder : IEndpointConventionBuilder {
            private readonly List<Action<EndpointBuilder>> _conventions;
            private readonly List<Action<EndpointBuilder>> _finallyConventions;

            public MinEndpointConventionBuilder(List<Action<EndpointBuilder>> conventions, List<Action<EndpointBuilder>> finallyConventions) {
                _conventions = conventions;
                _finallyConventions = finallyConventions;
            }

            public void Add(Action<EndpointBuilder> convention) => _conventions.Add(convention);
            public void Finally(Action<EndpointBuilder> finallyConvention) => _finallyConventions.Add(finallyConvention);
        }
    }
}