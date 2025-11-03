namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;

/// An ordered <see cref="ISet{T}"/>. Allows manipulating the order of elements.
// ReSharper disable once PossibleInterfaceMemberAmbiguity
public interface IOrderedSet<T> : ISet<T>, IReadOnlyOrderedSet<T> where T : notnull
{
	void MoveFirst( T item );
	void MoveLast( T item );
}
