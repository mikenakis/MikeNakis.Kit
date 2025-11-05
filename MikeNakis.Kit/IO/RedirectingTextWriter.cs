namespace MikeNakis.Kit.IO;

using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysIo = System.IO;
using SysText = System.Text;

///<summary>A <see cref="SysIo.TextWriter"/> which sends each line to a <see cref="Sys.Action{T}"/> of <c>string</c>.</summary>
//PEARL: `System.IO.TextWriter` is an abstract class instead of an interface!
//PEARL: `System.IO.TextWriter` contains methods that have an empty implementation instead of being abstract, for
//       example `Write( char )`.
//PEARL: `System.IO.TextWriter` exposes an insanely rich set of functionality, and it is impossible to know which
//       methods must be overridden and which ones can be omitted (inherited) unless you look at its implementation,
//       which in turn means that they better not change the implementation!
public class RedirectingTextWriter : SysIo.TextWriter
{
	readonly LifeGuard lifeGuard = LifeGuard.Create();
	public override SysText.Encoding Encoding => SysText.Encoding.Default;
	readonly SysText.StringBuilder stringBuilder = new();
	readonly Sys.Action<string> lineConsumer;

	public RedirectingTextWriter( Sys.Action<string> lineConsumer )
	{
		this.lineConsumer = lineConsumer;
	}

	protected override void Dispose( bool disposing )
	{
		if( disposing )
		{
			Assert( lifeGuard.IsAliveAssertion() );
			if( stringBuilder.Length > 0 )
				lineConsumer.Invoke( stringBuilder.ToString() );
			lifeGuard.Dispose();
		}
		base.Dispose( disposing );
	}

	// Must override:
	public override void Write( char value )
	{
		Assert( lifeGuard.IsAliveAssertion() );
		Write( new string( value, 1 ) );
	}

	// May override for better performance:
	public override void Write( string? value )
	{
		Assert( lifeGuard.IsAliveAssertion() );
		if( value == null )
			return;
		lock( stringBuilder )
		{
			string[] parts = value.Replace( "\r\n", "\n", Sys.StringComparison.Ordinal ) //
				.Split( '\n', Sys.StringSplitOptions.None );
			if( parts.Length == 0 )
				return;
			if( parts.Length == 1 )
			{
				stringBuilder.Append( value );
				return;
			}
			stringBuilder.Append( parts[0] );
			for( int i = 1; i < parts.Length; i++ )
			{
				writeLine();
				stringBuilder.Append( parts[i] );
			}
		}
	}

	// May override for better performance:
	public override void Write( char[] buffer, int index, int count )
	{
		Assert( lifeGuard.IsAliveAssertion() );
		Write( new string( buffer, index, count ) );
	}

	void writeLine()
	{
		lineConsumer.Invoke( stringBuilder.ToString() );
		stringBuilder.Clear();
	}
}
