namespace MikeNakis.Kit.Collections;

using MikeNakis.Kit;

[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public class WeakCollection<T> : ICollection<T> where T : class
{
	readonly MutableList<Sys.WeakReference<T>> list = new();

	public void Add( T item ) => list.Add( new Sys.WeakReference<T>( item ) );
	public void Clear() => list.Clear();
	public int Count => list.Count;
	public bool IsReadOnly => false;
	LegacyCollections.IEnumerator LegacyCollections.IEnumerable.GetEnumerator() => GetEnumerator();

	public bool Contains( T item )
	{
		foreach( T element in this )
			if( Equals( element, item ) )
				return true;
		return false;
	}

	public void CopyTo( T[] array, int arrayIndex ) => DotNetHelpers.CopyTo( this, array, arrayIndex );

	public bool Remove( T item )
	{
		for( int i = 0; i < list.Count; i++ )
		{
			if( !list[i].TryGetTarget( out T? target ) )
				continue;
			if( Equals( target, item ) )
			{
				list.RemoveAt( i );
				return true;
			}
		}
		return false;
	}

	public IEnumerator<T> GetEnumerator()
	{
		for( int i = list.Count - 1; i >= 0; i-- )
		{
			if( !list[i].TryGetTarget( out T? element ) )
			{
				list.RemoveAt( i );
				continue;
			}
			yield return element;
		}
	}
}
