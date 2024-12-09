namespace MikeNakis.Kit;

using System.Collections.Generic;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;

public interface Traverser<out T> : Sys.IDisposable
{
	IEnumerator<T> AsEnumerator => new TraverserEnumerator<T>( this );
	T Current { get; }
	bool HasCurrent { get; }
	void MoveNext();
}

public interface Traversable<out T>
{
	IEnumerable<T> AsEnumerable => new TraversableEnumerable<T>( this );
	Traverser<T> GetTraverser();
}

public sealed class TraversableEnumerable<T> : IEnumerable<T>
{
	readonly Traversable<T> traversable;

	public TraversableEnumerable( Traversable<T> traversable )
	{
		this.traversable = traversable;
	}

	public IEnumerator<T> GetEnumerator() => new TraverserEnumerator<T>( traversable.GetTraverser() );
	Sys.Collections.IEnumerator Sys.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

public class TraverserEnumerator<T> : IEnumerator<T>
{
	readonly Traverser<T> traverser;
	bool first;

	public TraverserEnumerator( Traverser<T> traverser )
	{
		this.traverser = traverser;
		first = true;
	}

	public bool MoveNext()
	{
		if( first )
			first = false;
		else
			traverser.MoveNext();
		return traverser.HasCurrent;
	}

	public void Reset() => throw new Sys.NotImplementedException();

	public T Current => traverser.Current;

	object? Sys.Collections.IEnumerator.Current => Current;

	public void Dispose() => traverser.Dispose();
}

public class EnumerableTraversable<T> : Traversable<T>
{
	readonly IEnumerable<T> enumerable;

	public EnumerableTraversable( IEnumerable<T> enumerable )
	{
		this.enumerable = enumerable;
	}

	public Traverser<T> GetTraverser() => new EnumeratorTraverser<T>( enumerable.GetEnumerator() );
}

public class EnumeratorTraverser<T> : Traverser<T>
{
	readonly IEnumerator<T> enumerator;

	public EnumeratorTraverser( IEnumerator<T> enumerator )
	{
		this.enumerator = enumerator;
		HasCurrent = enumerator.MoveNext();
	}

	public void Dispose() => enumerator.Dispose();

	public T Current => enumerator.Current;

	public bool HasCurrent { get; private set; }

	public void MoveNext()
	{
		Assert( HasCurrent );
		HasCurrent = enumerator.MoveNext();
	}
}
