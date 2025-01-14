namespace MikeNakis.Kit.Collections;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// Abstract base class for implementations of <see cref="IEnumerable{T}"/>.
public abstract class AbstractEnumerable<T> : IEnumerable<T>
{
	protected AbstractEnumerable()
	{ }

	public abstract IEnumerator<T> GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	public override bool Equals( object? other ) => other is IEnumerable<T> kin && Equals( kin );
	public virtual bool Equals( IEnumerable<T> other ) => this.SequenceEqual( other );

	public override int GetHashCode()
	{
#if NETCOREAPP
		System.HashCode hashCode = new();
		foreach( T item in this )
			hashCode.Add( item?.GetHashCode() ?? 0 );
		return hashCode.ToHashCode();
#else
		uint accumulatedHashCode = 1;
		foreach( T item in this )
		{
			accumulatedHashCode *= 31;
			accumulatedHashCode += (uint)(item?.GetHashCode() ?? 0);
		}
		return (int)accumulatedHashCode;
#endif
	}
}
