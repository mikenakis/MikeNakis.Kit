namespace MikeNakis.Kit;

using Sys = System;

public interface TextConsumer
{
	TextConsumer Write( Sys.ReadOnlySpan<char> text );

	TextConsumer Write( char c ) => Write( new Sys.ReadOnlySpan<char>( in c ) );
}
