namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;

public interface BiDictionary<F, S> where F : notnull where S : notnull
{
	public IReadOnlyDictionary<F, S> Forward { get; }
	public IReadOnlyDictionary<S, F> Reverse { get; }
	public int Count { get; }
}
