namespace MikeNakis.Kit.Collections;

public sealed class ExtractingComparer<T, E> : IComparer<T>
{
	readonly System.Func<T, E> extractor;
	readonly System.Func<E, E, int> comparator;
	readonly bool descending;

	public ExtractingComparer( System.Func<T, E> extractor, System.Func<E, E, int> comparator, bool descending = false )
	{
		this.extractor = extractor;
		this.comparator = comparator;
		this.descending = descending;
	}

	public int Compare( T? x, T? y )
	{
		if( x is null && y is null )
			return 0;
		if( x is null )
			return 1;
		if( y is null )
			return -1;
		E xe = extractor.Invoke( x );
		E ye = extractor.Invoke( y );
		int result = comparator.Invoke( xe, ye );
		return descending ? -result : result;
	}
}
