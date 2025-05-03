namespace MikeNakis.Kit.IO;

// PEARL: The System.IO.Stream class of DotNet is so retarded that it does not support checking
//        for end of stream without reading something from the stream.
//        This class corrects this problem.
// TODO: As it turns out, we are not using the EndOfStream property, so maybe the ability to check for end-of-stream without reading something from the stream was not that important, and this class can be simplified.
public sealed class DotNetStreamBinaryStreamReader : BinaryStreamReader
{
	readonly SysIo.Stream dotNetStream;
	byte[]? oneByteBuffer;

	public DotNetStreamBinaryStreamReader( SysIo.Stream dotNetStream )
	{
		this.dotNetStream = dotNetStream;
	}

	public override void ReadBytes( Sys.Span<byte> bytes )
	{
		int n = 0;
		if( oneByteBuffer != null )
		{
			bytes[0] = oneByteBuffer![0];
			oneByteBuffer = null;
			n++;
		}
		while( n < bytes.Length )
		{
			int r = dotNetStream.Read( bytes[n..] );
			if( r == 0 )
				throw new SysIo.EndOfStreamException();
			n += r;
		}
	}

	public override bool EndOfStream
	{
		get
		{
			if( oneByteBuffer != null )
				return false;
			oneByteBuffer = new byte[1];
			int n = dotNetStream.Read( oneByteBuffer, 0, oneByteBuffer.Length );
			Assert( n is 0 or 1 );
			if( n != 0 )
				return false;
			oneByteBuffer = null;
			return true;
		}
	}
}
