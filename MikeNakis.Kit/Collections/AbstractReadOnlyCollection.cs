namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;

/// Abstract base class for implementations of <see cref="IReadOnlyCollection{T}"/>.
public abstract class AbstractReadOnlyCollection<T> : AbstractEnumerable<T>, IReadOnlyCollection<T>
{
	protected AbstractReadOnlyCollection()
	{ }

	public abstract int Count { get; }
}
