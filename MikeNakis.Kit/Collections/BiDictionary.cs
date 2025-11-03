namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;

public interface BiDictionary<F, S> where F : notnull where S : notnull
{
	IReadOnlyDictionary<F, S> Forward { get; }
	IReadOnlyDictionary<S, F> Reverse { get; }
	int Count { get; }
}
