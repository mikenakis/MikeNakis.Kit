namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using static MikeNakis.Kit.GlobalStatics;

public interface BiDictionary<F, S> where F : notnull where S : notnull
{
	bool Contains( F first );
	bool Contains( S second );
	F? TryGet( S second );
	S? TryGet( F first );
	ICollection<F> Firsts { get; }
	ICollection<S> Seconds { get; }
}

///<summary>Default Extensions for <see cref="BiDictionary{F,S}"/>.</summary>
public static class BiDictionaryDefaultExtensions
{
	public static bool ContainsFirst<F, S>( this BiDictionary<F, S> self, F first ) where F : notnull where S : notnull => self.Contains( first );
	public static bool ContainsSecond<F, S>( this BiDictionary<F, S> self, S second ) where F : notnull where S : notnull => self.Contains( second );
	public static S? TryGetByFirst<F, S>( this BiDictionary<F, S> self, F first ) where F : notnull where S : notnull => self.TryGet( first );
	public static F? TryGetBySecond<F, S>( this BiDictionary<F, S> self, S second ) where F : notnull where S : notnull => self.TryGet( second );

	public static S Get<F, S>( this BiDictionary<F, S> self, F first ) where F : notnull where S : notnull
	{
		S? second = self.TryGetByFirst( first );
		Assert( !DotNetHelpers.Equal( second, default ) );
		return second!;
	}

	public static F Get<F, S>( this BiDictionary<F, S> self, S second ) where F : notnull where S : notnull
	{
		F? first = self.TryGetBySecond( second );
		Assert( !DotNetHelpers.Equal( first, default ) );
		return first!;
	}

	public static S GetByFirst<F, S>( this BiDictionary<F, S> self, F first ) where F : notnull where S : notnull
	{
		S? second = self.TryGetByFirst( first );
		Assert( !DotNetHelpers.Equal( second, default ) );
		return second!;
	}

	public static F GetBySecond<F, S>( this BiDictionary<F, S> self, S second ) where F : notnull where S : notnull
	{
		F? first = self.TryGetBySecond( second );
		Assert( !DotNetHelpers.Equal( first, default ) );
		return first!;
	}
}
