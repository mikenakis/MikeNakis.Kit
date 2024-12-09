namespace MikeNakis.Kit.IO;

using Sys = System;

// PEARL: DotNet does not make a distinction between binary input streams and binary output streams.
//        Instead, it only has one System.IO.Stream class for binary streams, with a lame pair of `CanRead` and `CanWrite` properties.
//        We correct this deficiency by providing our own classes for this purpose.
// TODO: once we start using DotNet 5, this abstract class should be converted to an interface with default methods.
public abstract class BinaryStreamReader
{
	public abstract void ReadBytes( Sys.Span<byte> bytes );
	public abstract bool EndOfStream { get; }

	public byte[] ReadBytes( int count )
	{
		byte[] bytes = new byte[count];
		ReadBytes( bytes );
		return bytes;
	}
}
