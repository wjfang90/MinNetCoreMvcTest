using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;

namespace MinNetCoreMvc.MinNetCoreMvc {
    public class MinActionDescriptorCollectionProvider : IMinActionDescriptorCollectionProvider {
        private readonly Assembly _assembly;
        private List<MinActionDescriptor>? _actionDescriptors;
        public IReadOnlyList<MinActionDescriptor> MinActionDescriptors => _actionDescriptors ??= Resolve(_assembly.GetExportedTypes()).ToList();

        public MinActionDescriptorCollectionProvider(IHostEnvironment environment) {
            var assemblyName = new AssemblyName(environment.ApplicationName);
            _assembly = Assembly.Load(assemblyName);
        }

        private IEnumerable<MinActionDescriptor> Resolve(IEnumerable<Type> types) {
            var methods = types.Where(t => IsValidateController(t))
                            .SelectMany(type => type.GetMethods()
                                                    .Where(m => m.DeclaringType == type && IsValidateAction(m))
                            );

            foreach (var method in methods) {
                var providors = method.GetCustomAttributes().OfType<IRouteTemplateProvider>();

                if (providors.Any()) {
                    foreach (var item in providors) {
                        yield return new MinActionDescriptor(method, item);
                    }
                }
                else {
                    yield return new MinActionDescriptor(method, null);
                }
            }

        }

        private bool IsValidateController(Type type) => type.IsPublic && !type.IsAbstract && type.Name.EndsWith("Controller");
        private bool IsValidateAction(MethodInfo methodInfo) => methodInfo.IsPublic | !methodInfo.IsAbstract;
    }
}
