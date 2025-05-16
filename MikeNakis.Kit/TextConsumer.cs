namespace MikeNakis.Kit;

public interface TextConsumer
{
	void Write( Sys.ReadOnlySpan<char> text );

	sealed void Write( char c )
	{
		Write( new Sys.ReadOnlySpan<char>( in c ) );
	}

	sealed void WriteLine( Sys.ReadOnlySpan<char> text )
	{
		Write( text );
		Write( '\n' );
	}

	sealed void WriteLine()
	{
		Write( '\n' );
	}
}
