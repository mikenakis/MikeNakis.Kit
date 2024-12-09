namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using MikeNakis.Kit;
using static MikeNakis.Kit.GlobalStatics;
using SysDiag = System.Diagnostics;

[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public class MutableBiDictionary<F, S> : BiDictionary<F, S> where F : notnull where S : notnull
{
	readonly Dictionary<F, S> firstToSecond;
	readonly Dictionary<S, F> secondToFirst;

	public MutableBiDictionary( IEqualityComparer<F>? firstEqualityComparer = null, IEqualityComparer<S>? secondEqualityComparer = null )
	{
		firstToSecond = new Dictionary<F, S>( firstEqualityComparer );
		secondToFirst = new Dictionary<S, F>( secondEqualityComparer );
	}

	public bool Contains( F first ) => firstToSecond.ContainsKey( first );
	public bool Contains( S second ) => secondToFirst.ContainsKey( second );
	public S this[F first] => this.Get( first );
	public F this[S second] => this.Get( second );
	public S? TryGet( F first ) => firstToSecond.GetValueOrDefault( first );
	public F? TryGet( S second ) => secondToFirst.GetValueOrDefault( second );
	public ICollection<F> Firsts => firstToSecond.Keys;
	public ICollection<S> Seconds => secondToFirst.Keys;
	public void Remove( F first ) => remove( first, this.GetByFirst( first ) );
	public void Remove( S second ) => remove( this.GetBySecond( second ), second );
	public void RemoveByFirst( F first ) => Remove( first );
	public void RemoveBySecond( S second ) => Remove( second );

	public int Count
	{
		get
		{
			int result = firstToSecond.Count;
			Assert( secondToFirst.Count == result );
			return result;
		}
	}

	public void Add( F first, S second )
	{
		Assert( !secondToFirst.ContainsKey( second ) ); //prevent modifying firstToSecond if modification of secondToFirst is going to fail
		firstToSecond.Add( first, second );
		secondToFirst.Add( second, first );
	}

	void remove( F first, S second )
	{
		Assert( secondToFirst.ContainsKey( second ) ); //prevent modifying firstToSecond if modification of secondToFirst is going to fail
		firstToSecond.Remove( first );
		secondToFirst.Remove( second );
	}

	public void Clear()
	{
		firstToSecond.Clear();
		secondToFirst.Clear();
	}
}
