namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using System.Linq;
using MikeNakis.Kit;
using static System.MemoryExtensions;
using SysDiag = System.Diagnostics;

[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public sealed class MakeshiftReadOnlyDictionary<K, V> : AbstractReadOnlyDictionary<K, V> where K : notnull
{
	readonly IReadOnlyCollection<K> keys;
	readonly System.Func<K, V> resolver;

	public MakeshiftReadOnlyDictionary( IReadOnlyCollection<K> keys, System.Func<K, V> resolver )
	{
		this.keys = keys;
		this.resolver = resolver;
	}

	public override int Count => keys.Count;
	public override bool ContainsKey( K key ) => keys.Contains( key );
	public override IEnumerable<K> Keys => keys;
	public override IEnumerable<V> Values => keys.Select( resolver );

	public override bool TryGetValue( K key, out V value )
	{
		if( !keys.Contains( key ) )
		{
			value = default!;
			return false;
		}
		value = resolver.Invoke( key );
		return true;
	}
}
