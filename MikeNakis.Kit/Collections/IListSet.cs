namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;

/// A list <see cref="ISet{T}"/>.  Offers indexed access to elements.
// ReSharper disable once PossibleInterfaceMemberAmbiguity
public interface IListSet<T> : IList<T>, ISet<T>, IReadOnlyListSet<T> where T : notnull
{
}
