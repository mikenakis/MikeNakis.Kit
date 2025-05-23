namespace MikeNakis.Kit.Collections;

/// Abstract base class for implementations of <see cref="IReadOnlyList{T}"/>.
public abstract class AbstractReadOnlyList<T> : AbstractReadOnlyCollection<T>, IReadOnlyList<T>
{
	protected AbstractReadOnlyList()
	{ }

	public abstract T this[int index] { get; }
}
