using Microsoft.AspNetCore.Mvc.Abstractions;

namespace MinNetCoreMvc.MinNetCoreMvc {
    public interface IMinActionDescriptorCollectionProvider {
        IReadOnlyList<MinActionDescriptor> MinActionDescriptors { get; }
    }
}
