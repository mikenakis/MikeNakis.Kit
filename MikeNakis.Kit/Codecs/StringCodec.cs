namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit.IO;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysText = System.Text;

/// A <see cref="Codec{T}" /> for <see cref="string" />.
public sealed class StringCodec : AbstractCodec<string>
{
	const int stackAllocationLimit = 1024;

	public static readonly StringCodec Instance = new();
	readonly SysText.Encoding encoding = DotNetHelpers.BomlessUtf8;

	StringCodec() { }

	public override void WriteText( string value, TextConsumer textConsumer, Codec.Mode mode )
	{
		switch( mode )
		{
			case Codec.Mode.Verbatim:
				textConsumer.Write( value );
				break;
			case Codec.Mode.Script:
				KitHelpers.ScribeStringLiteral( '"', value, textConsumer );
				break;
			default:
				throw new Sys.ArgumentOutOfRangeException( nameof( mode ), mode, null );
		}
	}

	public override Result<string> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		return mode switch
		{
			Codec.Mode.Verbatim => charSpan.ToString(),
			Codec.Mode.Script => KitHelpers.ParseStringLiteral( '"', charSpan ),
			_ => throw new Sys.ArgumentOutOfRangeException( nameof( mode ), mode, null )
		};
	}

	public override void WriteBinary( string value, BinaryStreamWriter binaryStreamWriter )
	{
		int byteCount = encoding.GetByteCount( value );
		Int32Codec.Instance.WriteBinary( byteCount, binaryStreamWriter ); //TODO: use telescopic integer!
		if( byteCount > stackAllocationLimit )
		{
			byte[] bytes = new byte[byteCount];
			int n = encoding.GetBytes( value, bytes );
			Assert( n == byteCount );
			binaryStreamWriter.WriteBytes( bytes );
		}
		else
		{
			Sys.Span<byte> bytes = stackalloc byte[byteCount];
			int n = encoding.GetBytes( value, bytes );
			Assert( n == byteCount );
			binaryStreamWriter.WriteBytes( bytes );
		}
	}

	public override string ReadBinary( BinaryStreamReader binaryStreamReader )
	{
		int byteCount = Int32Codec.Instance.ReadBinary( binaryStreamReader ); //TODO: use telescopic integer!
		if( byteCount > stackAllocationLimit )
		{
			byte[] bytes = new byte[byteCount];
			binaryStreamReader.ReadBytes( bytes );
			return encoding.GetString( bytes );
		}
		else
		{
			Sys.Span<byte> bytes = stackalloc byte[byteCount];
			binaryStreamReader.ReadBytes( bytes );
			return encoding.GetString( bytes );
		}
	}
}
