namespace MikeNakis.Kit;

using Sys = System;

public interface TextConsumer
{
	public TextConsumer Write( Sys.ReadOnlySpan<char> text );

	public TextConsumer Write( char c ) => Write( new Sys.ReadOnlySpan<char>( in c ) );
}
