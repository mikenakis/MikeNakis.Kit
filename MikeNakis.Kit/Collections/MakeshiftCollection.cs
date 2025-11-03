namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using MikeNakis.Kit;
using Sys = System;
using SysDiag = System.Diagnostics;

//TODO: rename all "Makeshift" to "Expedient"
[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public sealed class MakeshiftCollection<T> : MakeshiftReadOnlyCollection<T>, ICollection<T>
{
	readonly Sys.Func<T, bool> containsFunction;
	readonly Sys.Action<T> addFunction;
	readonly Sys.Func<T, bool> removeFunction;
	readonly Sys.Action clearFunction;

	public MakeshiftCollection( IEnumerable<T> enumerable, Sys.Func<int> countFunction, Sys.Func<T, bool>? containsFunction, Sys.Action<T> addFunction, Sys.Func<T, bool> removeFunction, Sys.Action clearFunction )
			: base( enumerable, countFunction )
	{
		this.containsFunction = containsFunction ?? slowContains;
		this.addFunction = addFunction;
		this.removeFunction = removeFunction;
		this.clearFunction = clearFunction;
	}

	public void Add( T item ) => addFunction.Invoke( item );
	public bool Remove( T item ) => removeFunction.Invoke( item );
	public void Clear() => clearFunction.Invoke();
	public bool Contains( T item ) => containsFunction.Invoke( item );
	public void CopyTo( T[] array, int arrayIndex ) => DotNetHelpers.CopyTo( this, array, arrayIndex );
	public bool IsReadOnly => false;

	bool slowContains( T item )
	{
		foreach( T existingItem in this )
			if( EqualityComparer<T>.Default.Equals( existingItem, item ) )
				return true;
		return false;
	}
}
