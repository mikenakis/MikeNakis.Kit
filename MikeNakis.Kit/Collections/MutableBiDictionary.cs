namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using MikeNakis.Kit;
using static MikeNakis.Kit.GlobalStatics;
using SysDiag = System.Diagnostics;

[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public class MutableBiDictionary<F, S> where F : notnull where S : notnull
{
	readonly ForwardDictionary forwardDictionary;
	readonly ReverseDictionary reverseDictionary;

	public MutableBiDictionary( IEqualityComparer<F>? forwardEqualityComparer = null, IEqualityComparer<S>? reverseEqualityComparer = null )
	{
		forwardDictionary = new ForwardDictionary( this, forwardEqualityComparer );
		reverseDictionary = new ReverseDictionary( this, reverseEqualityComparer );
	}

	public BiDictionary<F, S> AsReadOnly => new ReadOnlyBiDictionaryOnMutableBiDictionary( this );
	public IDictionary<F, S> Forward => forwardDictionary;
	public IDictionary<S, F> Reverse => reverseDictionary;

	class ReadOnlyBiDictionaryOnMutableBiDictionary( MutableBiDictionary<F, S> mutableBiDictionary ) : BiDictionary<F, S>
	{
		public IReadOnlyDictionary<F, S> Forward => mutableBiDictionary.forwardDictionary;
		public IReadOnlyDictionary<S, F> Reverse => mutableBiDictionary.reverseDictionary;
		public int Count => mutableBiDictionary.Count;
	}

	class ForwardDictionary : AbstractDictionary<F, S>
	{
		readonly MutableBiDictionary<F, S> mutableBiDictionary;
		internal readonly Dictionary<F, S> Dictionary;

		public ForwardDictionary( MutableBiDictionary<F, S> mutableBiDictionary, IEqualityComparer<F>? forwardEqualityComparer = null )
		{
			this.mutableBiDictionary = mutableBiDictionary;
			Dictionary = new Dictionary<F, S>( forwardEqualityComparer );
		}

		public override int Count => mutableBiDictionary.Count;
		public override IEnumerable<F> Keys => Dictionary.Keys;
		public override IEnumerable<S> Values => Dictionary.Values;
		public override bool ContainsKey( F key ) => Dictionary.ContainsKey( key );
		public override bool TryGetValue( F key, out S value ) => Dictionary.TryGetValue( key, out value! );
		public override void Clear() => mutableBiDictionary.Clear();
		public override void Add( F key, S value ) => mutableBiDictionary.add( key, value );

		public override bool Remove( F key )
		{
			if( !Dictionary.TryGetValue( key, out S? other ) )
				return false;
			mutableBiDictionary.remove( key, other );
			return true;
		}

		protected override bool Replace( F key, S value )
		{
			if( !Dictionary.TryGetValue( key, out S? existingOther ) )
				return false;
			if( Equals( value, existingOther ) )
				return false;
			if( mutableBiDictionary.reverseDictionary.Dictionary.TryGetValue( value, out F? _ ) )
				return false;
			mutableBiDictionary.remove( key, existingOther );
			mutableBiDictionary.add( key, value );
			return true;
		}
	}

	class ReverseDictionary : AbstractDictionary<S, F>
	{
		readonly MutableBiDictionary<F, S> biDictionary;
		internal readonly Dictionary<S, F> Dictionary;

		public ReverseDictionary( MutableBiDictionary<F, S> biDictionary, IEqualityComparer<S>? reverseEqualityComparer = null )
		{
			this.biDictionary = biDictionary;
			Dictionary = new Dictionary<S, F>( reverseEqualityComparer );
		}

		public override int Count => biDictionary.Count;
		public override IEnumerable<S> Keys => Dictionary.Keys;
		public override IEnumerable<F> Values => Dictionary.Values;
		public override bool ContainsKey( S key ) => Dictionary.ContainsKey( key );
		public override bool TryGetValue( S key, out F value ) => Dictionary.TryGetValue( key, out value! );
		public override void Clear() => biDictionary.Clear();
		public override void Add( S key, F value ) => biDictionary.add( value, key );

		public override bool Remove( S second )
		{
			if( !Dictionary.TryGetValue( second, out F? other ) )
				return false;
			biDictionary.remove( other, second );
			return true;
		}

		protected override bool Replace( S key, F value )
		{
			if( !Dictionary.TryGetValue( key, out F? existingOther ) )
				return false;
			if( Equals( value, existingOther ) )
				return false;
			if( biDictionary.forwardDictionary.Dictionary.TryGetValue( value, out S? _ ) )
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
			int result = forwardDictionary.Dictionary.Count;
			Assert( reverseDictionary.Dictionary.Count == result );
			return result;
		}
	}

	void add( F first, S second )
	{
		Assert( !reverseDictionary.Dictionary.ContainsKey( second ) ); //prevent modifying firstToSecond if modification of secondToFirst is going to fail
		forwardDictionary.Dictionary.Add( first, second );
		reverseDictionary.Dictionary.Add( second, first );
	}

	void remove( F first, S second )
	{
		bool ok = forwardDictionary.Dictionary.Remove( first );
		Assert( ok );
		ok = reverseDictionary.Dictionary.Remove( second );
		Assert( ok );
	}

	public void Clear()
	{
		forwardDictionary.Dictionary.Clear();
		reverseDictionary.Dictionary.Clear();
	}
}
