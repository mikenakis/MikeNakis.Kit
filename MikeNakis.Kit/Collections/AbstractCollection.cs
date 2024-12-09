namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;

/// Abstract base class for implementations of <see cref="ICollection{T}"/>.
public abstract class AbstractCollection<T> : AbstractReadOnlyCollection<T>, ICollection<T>
{
	protected AbstractCollection()
	{ }

	public abstract override IEnumerator<T> GetEnumerator();
	public abstract void Add( T item );
	public abstract bool Contains( T item );
	public abstract bool Remove( T item );
	public abstract override int Count { get; }
	public bool IsReadOnly => false;
	public void CopyTo( T[] array, int arrayIndex ) => DotNetHelpers.CopyTo( this, array, arrayIndex );

	public virtual void Clear()
	{
		foreach( T item in this.Collect() )
			Remove( item );
	}
}
