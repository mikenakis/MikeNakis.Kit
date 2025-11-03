namespace MikeNakis.Kit;

using Sys = System;
using SysText = System.Text;

public sealed class StringBuilderTextConsumer : TextConsumer
{
	readonly SysText.StringBuilder stringBuilder;

	public StringBuilderTextConsumer( SysText.StringBuilder stringBuilder )
	{
		this.stringBuilder = stringBuilder;
	}

	public void Write( Sys.ReadOnlySpan<char> s )
	{
		stringBuilder.Append( s );
	}

	[Sys.Obsolete] public override string ToString() => stringBuilder.ToString();
}
