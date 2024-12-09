namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using MikeNakis.Kit;
using Sys = System;
using SysDiag = System.Diagnostics;

[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public class MakeshiftReadOnlyCollection<T> : AbstractReadOnlyCollection<T>
{
	readonly IEnumerable<T> enumerable;
	readonly Sys.Func<int> countFunction;

	public MakeshiftReadOnlyCollection( IEnumerable<T> enumerable, Sys.Func<int>? countFunction )
	{
		this.enumerable = enumerable;
		this.countFunction = countFunction ?? slowCount;
	}

	int slowCount()
	{
		int count = 0;
		foreach( T _ in this )
			count++;
		return count;
	}

	public override IEnumerator<T> GetEnumerator() => enumerable.GetEnumerator();
	public override int Count => countFunction.Invoke();
}
