using Xunit;

namespace Vello.Tests.Interop;

[CollectionDefinition(CollectionName, DisableParallelization = true)]
public sealed class NativeInteropCollection
{
    public const string CollectionName = "NativeInteropSequential";
}
