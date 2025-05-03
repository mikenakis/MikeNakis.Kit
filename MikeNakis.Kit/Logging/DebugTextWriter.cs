namespace MikeNakis.Kit.Logging;

//PEARL: Excessive use of this class slows down debug runs, because when the Visual Studio Debugger is active, it
//       intercepts DotNet debug output and does extremely time-consuming stuff with it. For a full explanation, and
//       also for an explanation as to why this cannot be avoided, see https://stackoverflow.com/a/72894816/773113
//PEARL: `System.IO.TextWriter` is an abstract class instead of an interface!
//PEARL: `System.IO.TextWriter` contains methods that have an empty implementation instead of being abstract, for
//       example `Write( char )`.
//PEARL: `System.IO.TextWriter` exposes an insanely rich set of functionality, and it is impossible to know which
//       methods must be overridden and which ones can be omitted (inherited) unless you look at its implementation,
//       which in turn means that they better not change the implementation!
//PEARL: Class `System.Diagnostics.Debug` does not extend `System.IO.TextWriter` even though it exposes a set of
//       methods that are very similar to the methods of `System.IO.TextWriter`. (But not exactly the same.)
public class DebugTextWriter : SysIo.TextWriter
{
	public override SysText.Encoding Encoding => SysText.Encoding.Default;

	public override void Flush()
	{
		SysDiag.Debug.Flush();
		base.Flush();
	}

	// Must override:
	public override void Write( char value ) => SysDiag.Debug.Write( value );

	// May override for better performance:
	public override void Write( string? value )
	{
		if( False )
			base.Write( value );
		else
			SysDiag.Debug.Write( value );
	}

	// May override for better performance:
	public override void Write( char[] buffer, int index, int count )
	{
		if( False )
			base.Write( buffer, index, count );
		else
			Write( new string( buffer, index, count ) );
	}

	// Not necessary to override:
	// public override void WriteLine( string value ) => base.WriteLine( value );
	// public override void Write( bool value ) => base.Write( value );
	// public override void Write( char[] buffer ) => base.Write( buffer );
	// public override void Write( decimal value ) => base.Write( value );
	// public override void Write( double value ) => base.Write( value );
	// public override void Write( float value ) => base.Write( value );
	// public override void Write( int value ) => base.Write( value );
	// public override void Write( long value ) => base.Write( value );
	// public override void Write( object value ) => base.Write( value );
	// public override void Write( uint value ) => base.Write( value );
	// public override void Write( ulong value ) => base.Write( value );
	// public override void Write( string format, object arg0 ) => base.Write( format, arg0 );
	// public override void Write( string format, params object[] arg ) => base.Write( format, arg );
	// public override void Write( string format, object arg0, object arg1 ) => base.Write( format, arg0, arg1 );
	// public override void Write( string format, object arg0, object arg1, object arg2 ) => base.Write( format, arg0, arg1, arg2 );
	// public override void WriteLine() => base.WriteLine();
	// public override void WriteLine( bool value ) => base.WriteLine( value );
	// public override void WriteLine( char value ) => base.WriteLine( value );
	// public override void WriteLine( char[] buffer ) => base.WriteLine( buffer );
	// public override void WriteLine( decimal value ) => base.WriteLine( value );
	// public override void WriteLine( double value ) => base.WriteLine( value );
	// public override void WriteLine( float value ) => base.WriteLine( value );
	// public override void WriteLine( int value ) => base.WriteLine( value );
	// public override void WriteLine( long value ) => base.WriteLine( value );
	// public override void WriteLine( object value ) => base.WriteLine( value );
	// public override void WriteLine( uint value ) => base.WriteLine( value );
	// public override void WriteLine( ulong value ) => base.WriteLine( value );
	// public override void WriteLine( string format, object arg0 ) => base.WriteLine( format, arg0 );
	// public override void WriteLine( string format, params object[] arg ) => base.WriteLine( format, arg );
	// public override void WriteLine( char[] buffer, int index, int count ) => base.WriteLine( buffer, index, count );
	// public override void WriteLine( string format, object arg0, object arg1 ) => base.WriteLine( format, arg0, arg1 );
	// public override void WriteLine( string format, object arg0, object arg1, object arg2 ) => base.WriteLine( format, arg0, arg1, arg2 );
}
