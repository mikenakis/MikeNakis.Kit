namespace MikeNakis.Kit.IO;

using System;
using SysIo = System.IO;

public sealed class DotNetStreamBinaryStreamWriter : BinaryStreamWriter
{
	readonly SysIo.Stream dotNetStream;

	public DotNetStreamBinaryStreamWriter( SysIo.Stream dotNetStream )
	{
		this.dotNetStream = dotNetStream;
	}

	public override void WriteBytes( ReadOnlySpan<byte> bytes )
	{
		dotNetStream.Write( bytes );
	}

	public override void Flush()
	{
		dotNetStream.Flush();
	}
}
