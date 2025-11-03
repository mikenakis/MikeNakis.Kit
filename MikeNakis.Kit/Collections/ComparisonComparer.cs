namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using Sys = System;

//This class exists only because `Comparer<T>.Default` does not seem to handle nulls.
public sealed class ComparisonComparer<T> : IComparer<T>
{
	readonly Sys.Comparison<T> comparison;

	public ComparisonComparer( Sys.Comparison<T> comparison )
	{
		this.comparison = comparison;
	}

	public int Compare( T? x, T? y )
	{
		if( x is null && y is null )
			return 0;
		if( x is null )
			return 1;
		if( y is null )
			return -1;
		return comparison.Invoke( x, y );
	}
}
