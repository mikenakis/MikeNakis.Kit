namespace MikeNakis.Kit.Collections;

/// An ordered <see cref="ISet{T}"/>. Allows manipulating the order of elements.
// ReSharper disable once PossibleInterfaceMemberAmbiguity
public interface IOrderedSet<T> : ISet<T>, IReadOnlyOrderedSet<T> where T : notnull
{
	void MoveFirst( T item );
	void MoveLast( T item );
}
