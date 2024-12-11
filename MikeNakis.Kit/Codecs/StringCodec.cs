namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysText = System.Text;

/// A <see cref="Codec{T}" /> for <see cref="string" />.
public sealed class StringCodec : AbstractCodec<string>
{
	internal static Result<string, Expectation> ParseStringLiteral( char quoteCharacter, Sys.ReadOnlySpan<char> charSpan )
	{
		int i = 0;
		if( i >= charSpan.Length || charSpan[i] != quoteCharacter )
			return Result<string, Expectation>.Failure( new CustomExpectation( $"expected an opening '{quoteCharacter}', found '{charSpan[i]}'" ) );
		i++;
		SysText.StringBuilder builder = new();
		while( true )
		{
			if( i >= charSpan.Length )
				return Result<string, Expectation>.Failure( new CustomExpectation( $"expected a closing '{quoteCharacter}'" ) );
			char c = charSpan[i++];
			if( c == '"' )
				break;
			if( c < 32 )
				return Result<string, Expectation>.Failure( new CustomExpectation( $"escape character in string literal ({c:x2})" ) );
			if( c == '\\' )
			{
				c = charSpan[i++];
				switch( c )
				{
					case '\r':
					case '\n':
						continue;
					case 't':
						c = '\t';
						break;
					case 'n':
						c = '\n';
						break;
					case 'r':
						c = '\r';
						break;
					case '\\':
					case '\'':
					case '\"':
						break;
					case 'x':
					{
						Result<int, Expectation> result1 = readNibble( charSpan[i++] );
						if( !result1.IsSuccess )
							return Result<string, Expectation>.Failure( result1.AsFailure );
						Result<int, Expectation> result2 = readNibble( charSpan[i++] );
						if( !result2.IsSuccess )
							return Result<string, Expectation>.Failure( result2.AsFailure );
						c = (char)(result1.AsSuccess << 4 | result2.AsSuccess);
						break;
					}
					case 'u':
					{
						Result<int, Expectation> result1 = readNibble( charSpan[i++] );
						if( !result1.IsSuccess )
							return Result<string, Expectation>.Failure( result1.AsFailure );
						Result<int, Expectation> result2 = readNibble( charSpan[i++] );
						if( !result2.IsSuccess )
							return Result<string, Expectation>.Failure( result2.AsFailure );
						Result<int, Expectation> result3 = readNibble( charSpan[i++] );
						if( !result3.IsSuccess )
							return Result<string, Expectation>.Failure( result3.AsFailure );
						Result<int, Expectation> result4 = readNibble( charSpan[i++] );
						if( !result4.IsSuccess )
							return Result<string, Expectation>.Failure( result4.AsFailure );
						c = (char)(result1.AsSuccess << 12 | result2.AsSuccess << 8 | result3.AsSuccess << 4 | result4.AsSuccess);
						break;
					}
					default:
						return Result<string, Expectation>.Failure( new CustomExpectation( $"expected a valid escape sequence, found '{c}'" ) );
				}

				static Result<int, Expectation> readNibble( char c )
				{
					return c switch
					{
						>= '0' and <= '9' => Result<int, Expectation>.Success( c - '0' ),
						>= 'a' and <= 'f' => Result<int, Expectation>.Success( c - 'a' + 10 ),
						>= 'A' and <= 'F' => Result<int, Expectation>.Success( c - 'A' + 10 ),
						_ => Result<int, Expectation>.Failure( new CustomExpectation( $"expected a hex digit, found '{c}'" ) )
					};
				}
			}
			builder.Append( c );
		}
		if( i != charSpan.Length )
			return Result<string, Expectation>.Failure( new CustomExpectation( $"expected nothing, found '{charSpan[i..]}'" ) );
		return Result<string, Expectation>.Success( builder.ToString() );
	}

	const int StackAllocationLimit = 1024;

	public static readonly StringCodec Instance = new();

	StringCodec() { }

	public override void WriteText( string value, TextConsumer textConsumer, Codec.Mode mode )
	{
		switch( mode )
		{
			case Codec.Mode.Verbatim:
				textConsumer.Invoke( value );
				break;
			case Codec.Mode.Script:
				KitHelpers.ScribeStringLiteral( '"', value, textConsumer );
				break;
			default:
				throw new Sys.ArgumentOutOfRangeException( nameof( mode ), mode, null );
		}
	}

	public override Result<string, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		return mode switch
		{
			Codec.Mode.Verbatim => Result<string, Expectation>.Success( charSpan.ToString() ),
			Codec.Mode.Script => ParseStringLiteral( '"', charSpan ),
			_ => throw new Sys.ArgumentOutOfRangeException( nameof( mode ), mode, null )
		};
	}

	public override void WriteBinary( string value, BinaryStreamWriter binaryStreamWriter )
	{
		int byteCount = SysText.Encoding.UTF8.GetByteCount( value );
		Int32Codec.Instance.WriteBinary( byteCount, binaryStreamWriter ); //TODO: use telescopic integer!
		if( byteCount > StackAllocationLimit )
		{
			byte[] bytes = new byte[byteCount];
			int n = SysText.Encoding.UTF8.GetBytes( value, bytes );
			Assert( n == byteCount );
			binaryStreamWriter.WriteBytes( bytes );
		}
		else
		{
			Sys.Span<byte> bytes = stackalloc byte[byteCount];
			int n = SysText.Encoding.UTF8.GetBytes( value, bytes );
			Assert( n == byteCount );
			binaryStreamWriter.WriteBytes( bytes );
		}
	}

	public override string ReadBinary( BinaryStreamReader binaryStreamReader )
	{
		int byteCount = Int32Codec.Instance.ReadBinary( binaryStreamReader ); //TODO: use telescopic integer!
		if( byteCount > StackAllocationLimit )
		{
			byte[] bytes = new byte[byteCount];
			binaryStreamReader.ReadBytes( bytes );
			return SysText.Encoding.UTF8.GetString( bytes );
		}
		else
		{
			Sys.Span<byte> bytes = stackalloc byte[byteCount];
			binaryStreamReader.ReadBytes( bytes );
			return SysText.Encoding.UTF8.GetString( bytes );
		}
	}
}
