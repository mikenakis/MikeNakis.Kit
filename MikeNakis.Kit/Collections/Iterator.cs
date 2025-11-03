namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using SysCompiler = System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;

//TODO maybe delete this file?

public interface Iterator<T>
{
	bool HasNext { get; }
	T Current { get; }
	void MoveNext();
}

[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public sealed class ReadOnlyListIterator<T> : Iterator<T>
{
	readonly IReadOnlyList<T> list;
	int index;

	public ReadOnlyListIterator( IReadOnlyList<T> list )
	{
		this.list = list;
		index = 0;
	}

	public bool HasNext => index < list.Count;

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	public void MoveNext()
	{
		Assert( HasNext );
		index++;
	}

	public T Current
	{
		get
		{
			Assert( HasNext );
			return list[index];
		}
	}

	public override string ToString() => HasNext ? $"{Current}" : "<at end>";
}
