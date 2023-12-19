using System.Reflection;

namespace MinNetCoreMvc.MinNetCoreMvc {
    public class MinParameterDescriptor {
        public ParameterInfo ParameterInfo { get; }

        public MinParameterDescriptor(ParameterInfo parameterInfo) {
            this.ParameterInfo = parameterInfo;
        }
    }
}
