namespace MikeNakis.Kit.Extensions;

using MikeNakis.Kit.Collections;

public static class DictionaryExtensions
{
	public static V ComputeIfAbsent<K, V>( this IDictionary<K, V> self, K key, Sys.Func<K, V> factory )
	{
		if( self.TryGetValue( key, out V? existingValue ) )
			return existingValue;
		V newValue = factory.Invoke( key );
		self.Add( key, newValue );
		return newValue;
	}

	public static V Extract<K, V>( this IDictionary<K, V> self, K key )
	{
		V value = self[key];
		self.DoRemove( key );
		return value;
	}

	public static V? TryExtract<K, V>( this IDictionary<K, V> self, K key ) where K : notnull where V : class
	{
		if( self.TryGetValue( key, out V? value ) )
			self.DoRemove( key );
		return value;
	}

	public static V? TryGet<V, K>( this IDictionary<K, V> self, K key ) where K : notnull where V : notnull
	{
		if( self.TryGetValue( key, out V? value ) )
			return value;
		return default;
	}

	public static bool AddOrReplace<K, V>( this IDictionary<K, V> self, K key, V value )
	{
		if( !self.TryAdd( key, value ) )
		{
			self[key] = value;
			return false;
		}
		return true;
	}

	public static IReadOnlyDictionary<K, V> AsReadOnly<K, V>( this IDictionary<K, V> self ) where K : notnull => new ReadOnlyDictionaryOnDictionary<K, V>( self );

	[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
	sealed class ReadOnlyDictionaryOnDictionary<K, V> : AbstractReadOnlyDictionary<K, V> where K : notnull
	{
		readonly IDictionary<K, V> dictionary;
		public ReadOnlyDictionaryOnDictionary( IDictionary<K, V> dictionary ) => this.dictionary = dictionary;
		public override IEnumerable<K> Keys => dictionary.Keys;
		public override IEnumerable<V> Values => dictionary.Values;
		public override int Count => dictionary.Count;
		public override bool ContainsKey( K key ) => dictionary.ContainsKey( key );
		public override bool TryGetValue( K key, out V value ) => dictionary.TryGetValue( key, out value! );
		public override string? ToString() => dictionary.ToString();
	}
}
