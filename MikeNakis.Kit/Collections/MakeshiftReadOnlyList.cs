namespace MikeNakis.Kit.Collections;

using MikeNakis.Kit;

[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public class MakeshiftReadOnlyList<T> : AbstractReadOnlyList<T>
{
	readonly IEnumerable<T> enumerable;
	readonly Sys.Func<int> countFunction;
	readonly Sys.Func<int, T> indexFunction;

	public MakeshiftReadOnlyList( IEnumerable<T> enumerable, Sys.Func<int> countFunction, Sys.Func<int, T> indexFunction )
	{
		this.enumerable = enumerable;
		this.countFunction = countFunction;
		this.indexFunction = indexFunction;
	}

	public override IEnumerator<T> GetEnumerator() => enumerable.GetEnumerator();
	public override int Count => countFunction.Invoke();
	public override T this[int index] => indexFunction.Invoke( index );
}
