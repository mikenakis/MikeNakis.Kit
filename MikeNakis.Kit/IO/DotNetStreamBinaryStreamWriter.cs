namespace MikeNakis.Kit.IO;

public sealed class DotNetStreamBinaryStreamWriter : BinaryStreamWriter
{
	readonly SysIo.Stream dotNetStream;

	public DotNetStreamBinaryStreamWriter( SysIo.Stream dotNetStream )
	{
		this.dotNetStream = dotNetStream;
	}

	public override void WriteBytes( Sys.ReadOnlySpan<byte> bytes )
	{
		dotNetStream.Write( bytes );
	}

	public override void Flush()
	{
		dotNetStream.Flush();
	}
}
