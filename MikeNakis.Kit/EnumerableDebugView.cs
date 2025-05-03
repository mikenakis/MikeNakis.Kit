namespace MikeNakis.Kit;

/// A debugger type proxy for enumerables. Use with <see cref="SysDiag.DebuggerTypeProxyAttribute"/>.
// When troubleshooting why this is not working, try the following:
// - Uncheck "Tools" > "Options" > "Debugging" > "General" > "Show raw structure of objects in variables windows".
// - If working with aspDotNet, make sure you are running with full trust. (Partial trust can cause this to fail.)
// - Make sure this class is not nested. (Must be a public or internal class directly under its namespace.)
public class EnumerableDebugView
{
	readonly LegacyCollections.IEnumerable enumerable;

	//This constructor will be invoked by the debugger.
	public EnumerableDebugView( LegacyCollections.IEnumerable enumerable )
	{
		this.enumerable = enumerable;
	}

	[SysDiag.DebuggerBrowsable( SysDiag.DebuggerBrowsableState.RootHidden )]
#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable RS0030 // Do not use banned APIs
	//This property will be invoked by the debugger.
	public object[] Items => enumerable.Cast<object>().ToArray();
#pragma warning restore RS0030 // Do not use banned APIs
#pragma warning restore CA1819 // Properties should not return arrays
}
