namespace MikeNakis.Kit.Extensions;

using System.Collections.Generic;
using MikeNakis.Kit.Collections;
using SysDiag = System.Diagnostics;
using Sys = System;

///<remarks>NOTE: This class must be kept AS SMALL AS POSSIBLE.</remarks>
public static class DictionaryExtensions
{
	public static V ComputeIfAbsent<K, V>( this IDictionary<K, V> self, K key, Sys.Func<K, V> factory ) => KitHelpers.ComputeIfAbsent( self, key, factory );
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
