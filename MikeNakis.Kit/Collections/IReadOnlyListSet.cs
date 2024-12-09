namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;

/// A list <see cref="IReadOnlySet{T}"/>.  Allows indexed access to elements.
// ReSharper disable once PossibleInterfaceMemberAmbiguity
public interface IReadOnlyListSet<T> : IList<T>, IReadOnlySet<T> where T : notnull
{
}
