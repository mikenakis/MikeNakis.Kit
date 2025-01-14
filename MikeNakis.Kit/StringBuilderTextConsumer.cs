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

	public TextConsumer Write( Sys.ReadOnlySpan<char> s )
	{
		stringBuilder.Append( s );
		return this;
	}

	[Sys.Obsolete] public override string ToString() => stringBuilder.ToString();
}
