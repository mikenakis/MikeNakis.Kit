namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using MikeNakis.Kit;
using static MikeNakis.Kit.GlobalStatics;
using SysDiag = System.Diagnostics;

[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public class MutableBiDictionary<F, S> where F : notnull where S : notnull
{
	readonly Dictionary<F, S> forward;
	readonly Dictionary<S, F> reverse;

	public MutableBiDictionary( IEqualityComparer<F>? forwardEqualityComparer = null, IEqualityComparer<S>? reverseEqualityComparer = null )
	{
		forward = new Dictionary<F, S>( forwardEqualityComparer );
		reverse = new Dictionary<S, F>( reverseEqualityComparer );
	}

	public BiDictionary<F, S> AsReadOnly => new ReadOnlyBiDictionaryOnMutableBiDictionary( this );
	public IDictionary<F, S> Forward => forward;
	public IDictionary<S, F> Reverse => reverse;

	class ReadOnlyBiDictionaryOnMutableBiDictionary( MutableBiDictionary<F, S> mutableBiDictionary ) : BiDictionary<F, S>
	{
		public IReadOnlyDictionary<F, S> Forward => mutableBiDictionary.forward;
		public IReadOnlyDictionary<S, F> Reverse => mutableBiDictionary.reverse;
		public int Count => mutableBiDictionary.Count;
	}

	class ForwardDictionary : AbstractDictionary<F, S>
	{
		readonly MutableBiDictionary<F, S> mutableBiDictionary;
		public ForwardDictionary( MutableBiDictionary<F, S> mutableBiDictionary ) => this.mutableBiDictionary = mutableBiDictionary;
		public override int Count => mutableBiDictionary.Count;
		public override IEnumerable<F> Keys => mutableBiDictionary.forward.Keys;
		public override IEnumerable<S> Values => mutableBiDictionary.forward.Values;
		public override bool ContainsKey( F key ) => mutableBiDictionary.forward.ContainsKey( key );
		public override bool TryGetValue( F key, out S value ) => mutableBiDictionary.forward.TryGetValue( key, out value! );
		public override void Clear() => mutableBiDictionary.Clear();
		public override void Add( F key, S value ) => mutableBiDictionary.add( key, value );

		public override bool Remove( F key )
		{
			if( !mutableBiDictionary.forward.TryGetValue( key, out S? other ) )
				return false;
			mutableBiDictionary.remove( key, other );
			return true;
		}

		protected override bool Replace( F key, S value )
		{
			if( !mutableBiDictionary.forward.TryGetValue( key, out S? existingOther ) )
				return false;
			if( Equals( value, existingOther ) )
				return false;
			if( mutableBiDictionary.reverse.TryGetValue( value, out F? _ ) )
				return false;
			mutableBiDictionary.remove( key, existingOther );
			mutableBiDictionary.add( key, value );
			return true;
		}
	}

	class ReverseDictionary : AbstractDictionary<S, F>
	{
		readonly MutableBiDictionary<F, S> biDictionary;
		public ReverseDictionary( MutableBiDictionary<F, S> biDictionary ) => this.biDictionary = biDictionary;
		public override int Count => biDictionary.Count;
		public override IEnumerable<S> Keys => biDictionary.reverse.Keys;
		public override IEnumerable<F> Values => biDictionary.reverse.Values;
		public override bool ContainsKey( S key ) => biDictionary.reverse.ContainsKey( key );
		public override bool TryGetValue( S key, out F value ) => biDictionary.reverse.TryGetValue( key, out value! );
		public override void Clear() => biDictionary.Clear();
		public override void Add( S key, F value ) => biDictionary.add( value, key );

		public override bool Remove( S second )
		{
			if( !biDictionary.reverse.TryGetValue( second, out F? other ) )
				return false;
			biDictionary.remove( other, second );
			return true;
		}

		protected override bool Replace( S key, F value )
		{
			if( !biDictionary.reverse.TryGetValue( key, out F? existingOther ) )
				return false;
			if( Equals( value, existingOther ) )
				return false;
			if( biDictionary.forward.TryGetValue( value, out S? _ ) )
				return false;
			biDictionary.remove( existingOther, key );
			biDictionary.add( value, key );
			return true;
		}
	}

	public int Count
	{
		get
		{
			int result = forward.Count;
			Assert( reverse.Count == result );
			return result;
		}
	}

	void add( F first, S second )
	{
		Assert( !reverse.ContainsKey( second ) ); //prevent modifying firstToSecond if modification of secondToFirst is going to fail
		forward.Add( first, second );
		reverse.Add( second, first );
	}

	void remove( F first, S second )
	{
		bool ok = forward.Remove( first );
		Assert( ok );
		ok = reverse.Remove( second );
		Assert( ok );
	}

	public void Clear()
	{
		forward.Clear();
		reverse.Clear();
	}
}
