namespace MikeNakis.Kit;

using MikeNakis.Kit.Collections;
using MikeNakis.Kit.Extensions;
using MikeNakis.Kit.Logging;

public static class KitHelpers
{
	public static readonly SysText.Encoding Latin1Encoding = SysText.Encoding.GetEncoding( "iso-8859-1" );

	public static string GetCallerFilePath( [SysCompiler.CallerFilePath] string? callerFilePathName = null )
	{
		return callerFilePathName.OrThrow();
	}

	static X defaultExtractor<X>( X a ) => a;
	//static int defaultComparator<T>( T a, T b ) where T : Sys.IComparable<T> => a.CompareTo( b );

	public const double Epsilon = 1e-15;

	public const float FEpsilon = 1.192093E-07f;

	public static readonly SysThread.ThreadLocal<bool> FailureTesting = new( false );

	public static string SafeToString( object? value )
	{
		SysText.StringBuilder stringBuilder = new();
		TextConsumer textConsumer = new StringBuilderTextConsumer( stringBuilder );
		SafeToString( value, textConsumer );
		return stringBuilder.ToString();
	}

	public static void SafeToString( object? value, TextConsumer textConsumer )
	{
		ISet<object> visitedObjects = new HashSet<object>( ReferenceEqualityComparer.Instance );
		recurse( value );
		return;

		void recurse( object? value )
		{
			if( value == null )
			{
				//PEARL: In dotnet, all default conversions of `object` to `string` will convert `null` to the empty
				//       string instead of the string "null". Thus, when we print a string, we can never tell whether it
				//       was `null` or empty, because it always looks empty. We fix this here.
				//       I guess this is happening because dotnet is also used by Visual Basic, and Visual Basic
				//       programmers might have epileptic seizures if they see the word `null`.
				Assert( $"{value}" == "" ); //Ensure that if the behavior of the runtime ever gets fixed, we will notice.
				textConsumer.Write( "null" );
			}
			else if( value is char c )
				EscapeForCSharp( c, textConsumer );
			else if( value is string s )
				EscapeForCSharp( s, textConsumer );
			else if( value is LegacyCollections.IEnumerable enumerable )
				enumerable.Cast<object>().MakeString( recurse, "[ ", ", ", " ]", "[]", textConsumer );
			else if( value.GetType().IsValueType || visitedObjects.Add( value ) )
				textConsumer.Write( value.ToString() ?? "\u2620" ); //U+2620 Skull and Crossbones Unicode Character to indicate that `ToString()` returned `null`.
			else
				textConsumer.Write( $"{value.GetType()}@{SysCompiler.RuntimeHelpers.GetHashCode( value )}" );
		}
	}

	public static string EscapeForCSharp( string content ) => EscapeForCSharp( content, '"' );

	public static string EscapeForCSharp( char content ) => EscapeForCSharp( content.ToString(), '\'' );

	public static void EscapeForCSharp( string content, TextConsumer textConsumer ) => EscapeForCSharp( content, '"', textConsumer );

	public static void EscapeForCSharp( char content, TextConsumer textConsumer ) => EscapeForCSharp( content.ToString(), '\'', textConsumer );

	public static string EscapeForCSharp( string content, char quote )
	{
		SysText.StringBuilder stringBuilder = new();
		EscapeForCSharp( content, quote, stringBuilder );
		return stringBuilder.ToString();
	}

	public static void EscapeForCSharp( string content, char quote, SysText.StringBuilder stringBuilder )
	{
		TextConsumer textConsumer = new StringBuilderTextConsumer( stringBuilder );
		ScribeStringLiteral( quote, content, textConsumer );
	}

	public static void EscapeForCSharp( string content, char quote, TextConsumer textConsumer )
	{
		ScribeStringLiteral( quote, content, textConsumer );
	}

	public static string UnescapeForCSharp( string content )
	{
		Result<string> result = ParseStringLiteral( '\"', content );
		if( !result.IsSuccess )
			throw new Sys.FormatException( result.Expectation.Message );
		return result.Payload;
	}

	public static Sys.Exception NewFormatException( string typeName, string content )
	{
		const int maxLength = 20;
		string fixedContent = content;
		fixedContent = SafeToString( fixedContent );
		if( fixedContent.Length > maxLength )
			fixedContent = fixedContent[..(maxLength - 1)] + "\u2026";
		return new Sys.FormatException( $"Expected a value of type {typeName}, found {fixedContent}" );
	}

	public static void ScribeStringLiteral( char quoteCharacter, Sys.ReadOnlySpan<char> instance, TextConsumer textConsumer )
	{
		//TODO: cover all escapes, see https://stackoverflow.com/a/323664/773113
		textConsumer.Write( new Sys.ReadOnlySpan<char>( in quoteCharacter ) );
		foreach( char c in instance )
			if( c == quoteCharacter )
				emitEscapedCharacter( textConsumer, c );
			else
				switch( c )
				{
					case '\t':
						emitEscapedCharacter( textConsumer, 't' );
						break;
					case '\r':
						emitEscapedCharacter( textConsumer, 'r' );
						break;
					case '\n':
						emitEscapedCharacter( textConsumer, 'n' );
						break;
					case '\\':
						emitEscapedCharacter( textConsumer, '\\' );
						break;
					default:
						emitOtherCharacter( textConsumer, c );
						break;
				}
		textConsumer.Write( new Sys.ReadOnlySpan<char>( in quoteCharacter ) );
		return;

		static void emitEscapedCharacter( TextConsumer textConsumer, char c )
		{
			Sys.Span<char> buffer = stackalloc char[2];
			buffer[0] = '\\';
			buffer[1] = c;
			textConsumer.Write( buffer );
		}

		static void emitOtherCharacter( TextConsumer textConsumer, char c )
		{
			if( IsPrintable( c ) )
				textConsumer.Write( new Sys.ReadOnlySpan<char>( in c ) );
			else if( c < 256 ) // no need to check for >= 0 because char is unsigned.
			{
				Sys.Span<char> buffer = stackalloc char[4];
				buffer[0] = '\\';
				buffer[1] = 'x';
				buffer[2] = digitFromNibble( c >> 4 );
				buffer[3] = digitFromNibble( c & 0x0f );
				textConsumer.Write( buffer );
			}
			else
			{
				Sys.Span<char> buffer = stackalloc char[6];
				buffer[0] = '\\';
				buffer[1] = 'u';
				buffer[2] = digitFromNibble( c >> 12 & 0x0f );
				buffer[3] = digitFromNibble( c >> 8 & 0x0f );
				buffer[4] = digitFromNibble( c >> 4 & 0x0f );
				buffer[5] = digitFromNibble( c & 0x0f );
				textConsumer.Write( buffer );
			}
			return;

			static char digitFromNibble( int nibble )
			{
				Assert( nibble is >= 0 and < 16 );
				return (char)((nibble >= 10 ? 'a' - 10 : '0') + nibble);
			}
		}
	}

	public static Result<string> ParseStringLiteral( char quoteCharacter, Sys.ReadOnlySpan<char> charSpan )
	{
		int i = 0;
		if( i >= charSpan.Length || charSpan[i] != quoteCharacter )
			return new CustomExpectation( $"expected an opening '{quoteCharacter}', found '{charSpan[i]}'" );
		i++;
		SysText.StringBuilder builder = new();
		while( true )
		{
			if( i >= charSpan.Length )
				return new CustomExpectation( $"expected a closing '{quoteCharacter}'" );
			char c = charSpan[i++];
			if( c == '"' )
				break;
			if( c < 32 )
				return new CustomExpectation( $"escape character in string literal ({c:x2})" );
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
						Result<int> result1 = readNibble( charSpan[i++] );
						if( !result1.IsSuccess )
							return result1.Expectation;
						Result<int> result2 = readNibble( charSpan[i++] );
						if( !result2.IsSuccess )
							return result2.Expectation;
						c = (char)(result1.Payload << 4 | result2.Payload);
						break;
					}
					case 'u':
					{
						Result<int> result1 = readNibble( charSpan[i++] );
						if( !result1.IsSuccess )
							return result1.Expectation;
						Result<int> result2 = readNibble( charSpan[i++] );
						if( !result2.IsSuccess )
							return result2.Expectation;
						Result<int> result3 = readNibble( charSpan[i++] );
						if( !result3.IsSuccess )
							return result3.Expectation;
						Result<int> result4 = readNibble( charSpan[i++] );
						if( !result4.IsSuccess )
							return result4.Expectation;
						c = (char)(result1.Payload << 12 | result2.Payload << 8 | result3.Payload << 4 | result4.Payload);
						break;
					}
					default:
						return new CustomExpectation( $"expected a valid escape sequence, found '{c}'" );
				}

				static Result<int> readNibble( char c )
				{
					return c switch
					{
						>= '0' and <= '9' => c - '0',
						>= 'a' and <= 'f' => c - 'a' + 10,
						>= 'A' and <= 'F' => c - 'A' + 10,
						_ => new CustomExpectation( $"expected a hex digit, found '{c}'" )
					};
				}
			}
			builder.Append( c );
		}
		if( i != charSpan.Length )
			return new CustomExpectation( $"expected nothing, found '{charSpan[i..]}'" );
		return builder.ToString();
	}

	public static bool IsPrintable( char c )
	{
		// see https://www.johndcook.com/blog/2013/04/11/which-unicode-characters-can-you-depend-on/
		// see https://en.wikipedia.org/wiki/Windows-1252
		return (int)c switch
		{
			< 32 => false,
			< 127 => true,
			< 160 => false,
			173 => false,
			< 256 => true,
			338 => true, // Œ
			339 => true, // œ
			352 => true, // Š
			353 => true, // š
			376 => true, // Ÿ
			381 => true, // Ž
			382 => true, // ž
			402 => true, // ƒ
			710 => true, // ˆ
			732 => true, // ˜
			8211 => true, // –
			8212 => true, // —
			8216 => true, // ‘
			8217 => true, // ’
			8218 => true, // ‚
			8220 => true, // “
			8221 => true, // ”
			8222 => true, // „
			8224 => true, // †
			8225 => true, // ‡
			8226 => true, // •
			8230 => true, // …
			8240 => true, // ‰
			8249 => true, // ‹
			8250 => true, // ›
			8364 => true, // €
			8482 => true, // ™
			_ => false
		};
	}

	public static int RoundUpPowerOf2( int v )
	{
		//from http://graphics.stanford.edu/%7Eseander/bithacks.html#RoundUpPowerOf2, also in "Hacker's Delight"
		Assert( v < 0x40000000 ); //because the number 0x80000000 is not representable as a 32-bit signed integer.
		unchecked
		{
			v--;
			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			v |= v >> 8;
			v |= v >> 16;
			v++;
		}
		return v;
	}

	public static T EnumFromByte<T>( byte byteValue ) where T : struct, Sys.Enum => SysCompiler.Unsafe.BitCast<byte, T>( byteValue );

	public static byte ByteFromEnum<T>( T enumValue ) where T : struct, Sys.Enum => SysCompiler.Unsafe.BitCast<T, byte>( enumValue );

	public static T EnumFromInt<T>( int intValue ) where T : struct, Sys.Enum => SysCompiler.Unsafe.BitCast<int, T>( intValue );

	public static int IntFromEnum<T>( T enumValue ) where T : unmanaged, Sys.Enum => SysCompiler.Unsafe.BitCast<T, int>( enumValue );

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Legacy IEnumerable

	public static IEnumerable<T> LegacyAsEnumerable<T>( LegacyCollections.IEnumerable self )
	{
		foreach( T element in self )
			yield return element;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// IReadOnlyList

	public static int BinarySearch<E, T>( IReadOnlyList<E> list, T element, Sys.Func<E, T> extractor, IComparer<T> comparer, bool skipDuplicates )
	{
		Assert( IsSortedAssertion( list, extractor, comparer, skipDuplicates, false ) );
		int low = 0;
		int hi = list.Count - 1;
		int median;
		while( true )
		{
			if( low > hi )
				return ~low;
			median = low + ((hi - low) >> 1);
			int num = compare( median );
			if( num < 0 )
				low = median + 1;
			else if( num > 0 )
				hi = median - 1;
			else
				break;
		}
		if( skipDuplicates )
			while( median > 0 && compare( median - 1 ) == 0 )
				median--;
		return median;

		int compare( int index )
		{
			T extractedElement = extractor.Invoke( list[index] );
			return comparer.Compare( extractedElement, element );
		}
	}

	public static int BinarySearch<E, T>( IReadOnlyList<E> list, T element, Sys.Func<E, T> extractor, Sys.Comparison<T> comparison, bool skipDuplicates ) //
		=> BinarySearch( list, element, extractor, comparerFromComparison( comparison ), skipDuplicates );

	public static int BinarySearch<E>( IReadOnlyList<E> list, E item, Sys.Comparison<E> comparison, bool skipDuplicates ) //
		=> BinarySearch( list, item, defaultExtractor, comparison, skipDuplicates );

	public static int BinarySearch<E>( IReadOnlyList<E> list, E item, IComparer<E> comparer, bool skipDuplicates ) //
		=> BinarySearch( list, item, defaultExtractor, comparer, skipDuplicates );

	static IComparer<T> comparerFromComparison<T>( Sys.Comparison<T> comparison )
	{
		if( True )
			return Comparer<T>.Create( comparison ); //this is built-in, but it does not seem to handle `null`. If null-handling is ever needed, use the code below.
		else
			return new ComparisonComparer<T>( comparison );
	}

	public static bool IsSortedAssertion<E, T>( IReadOnlyList<E> self, Sys.Func<E, T> extractor, IComparer<T> comparer, bool allowDuplicates, bool descending )
	{
		int n = sortedCount( self, extractor, comparer, allowDuplicates, descending );
		Assert( n == self.Count );
		return true;
	}

	public static int IndexOf<T>( IReadOnlyList<T> self, T elementToFind )
	{
		int i = 0;
		foreach( T element in self )
		{
			if( DotNetHelpers.Equal( element, elementToFind ) )
				return i;
			i++;
		}
		return -1;
	}

	///<summary>Returns the number of elements from the start of the enumerable that are sorted. Essentially, it finds
	///the index of the first out-of-order element.</summary>
	static int sortedCount<E, T>( IEnumerable<E> enumerable, Sys.Func<E, T> extractor, IComparer<T> comparer, //
			bool allowDuplicates = true, bool descending = false )
	{
		using( IEnumerator<E> enumerator = enumerable.GetEnumerator() )
		{
			if( !enumerator.MoveNext() )
				return 0;
			T left = extractor.Invoke( enumerator.Current );
			int i;
			for( i = 1; enumerator.MoveNext(); i++ )
			{
				T right = extractor.Invoke( enumerator.Current );
				int d = comparer.Compare( left, right );
				if( descending )
					d = -d;
				if( d >= 0 )
				{
					if( d > 0 )
						break;
					if( !allowDuplicates )
						break;
				}
				left = right;
			}
			return i;
		}
	}

	public static IEnumerable<T> Concat<T>( IEnumerable<T> self, IEnumerable<T> other )
	{
		// PEARL: ReSharper suggests to convert this call to an extension method call;
		// if you do, it converts it and it thinks that it is invoking the extension method in Linq.Enumerable;
		// however, the C# compiler thinks otherwise: it invokes this same method instead,
		// which of course miserably fails with a stack overflow.
		// the only solution I have been able to come up with is to disable the inspection.
		// ReSharper disable once InvokeAsExtensionMethod
		return Enumerable.Concat( self, other );
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// ICollection

	public static void AddRange<T>( ICollection<T> self, IEnumerable<T> other )
	{
		foreach( T element in other )
			self.Add( element );
	}

	public static bool AddRange<T>( ISet<T> self, IEnumerable<T> other )
	{
		bool added = false;
		foreach( T element in other )
			if( self.Add( element ) )
				added = true;
		return added;
	}

	//PEARL: the add-item-to-set method of DotNet is not really an "add" method, it is actually a "try-add" method,
	//       because it returns a boolean to indicate success or failure. So, if we want a real "add" function which
	//       will actually fail on failure, (duh!) we have to introduce it ourselves.  Unfortunately, since the name
	//       `Add` is taken, we have to give the new function a different name.
	public static void DoAdd<T>( ISet<T> self, T element )
	{
		bool ok = self.Add( element );
		Assert( ok );
	}

	//PEARL: the remove-item-from-collection method of DotNet is not really a "remove" method, it is actually a
	//       "try-remove" method, because it returns a boolean to indicate success or failure. So, if we want a real
	//       "remove" function which will actually fail on failure, (duh!) we have to introduce it ourselves.
	//       Unfortunately, since the name `Remove` is taken, we have to give the new function a different name.
	public static void DoRemove<T>( ICollection<T> self, T element )
	{
		bool ok = self.Remove( element );
		Assert( ok );
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Legacy IList

	public static bool LegacyContains( LegacyCollections.IList self, object value )
	{
		foreach( object v in self )
			if( Equals( v, value ) )
				return true;
		return false;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// IList

	public static T ExtractAt<T>( IList<T> self, int index )
	{
		T result = self[index];
		self.RemoveAt( index );
		return result;
	}

	public static void Move<T>( IList<T> self, int oldIndex, int newIndex )
	{
		if( oldIndex == newIndex )
			return;
		T item = self[oldIndex];
		self.RemoveAt( oldIndex );
		self.Insert( newIndex, item );
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Exception

	public static string BuildShortExceptionMessage( string prefix, Sys.Exception exception )
	{
		return $"{prefix}: {exception.GetType().FullName}: {fixMessage( exception.Message )}";
	}

	public static IEnumerable<string> BuildMediumExceptionMessage( string prefix, Sys.Exception exception )
	{
		MutableList<string> mutableLines = new();
		while( true )
		{
			string message = BuildShortExceptionMessage( prefix, exception );
			mutableLines.Add( message );
			if( exception.InnerException != null )
			{
				prefix = "Caused by";
				exception = exception.InnerException;
				continue;
			}
			break;
		}
		return mutableLines.AsEnumerable;
	}

	public static IEnumerable<string> BuildLongExceptionMessage( string prefix, Sys.Exception exception ) => BuildLongExceptionMessage( prefix, exception, Identity );

	public static IEnumerable<string> BuildLongExceptionMessage( string prefix, Sys.Exception exception, Sys.Func<string, string> sourceFileNameFixer )
	{
		MutableList<string> lines = new();
		recurse( prefix, lines, exception, sourceFileNameFixer );
		return lines;

		static void recurse( string prefix, MutableList<string> lines, Sys.Exception exception, Sys.Func<string, string> sourceFileNameFixer )
		{
			for( ; true; exception = exception.InnerException )
			{
				lines.Add( BuildShortExceptionMessage( prefix, exception ) );
				SysDiag.StackFrame[] stackFrames = new SysDiag.StackTrace( exception, true ).GetFrames();
				lines.AddRange( stackFrames.Select( stackFrame => stringFromStackFrame( stackFrame, sourceFileNameFixer ) ) );
				if( exception is Sys.AggregateException aggregateException )
				{
					Assert( ReferenceEquals( exception.InnerException, aggregateException.InnerExceptions[0] ) );
					foreach( Sys.Exception innerException in aggregateException.InnerExceptions )
						recurse( "Aggregates", lines, innerException, sourceFileNameFixer );
					break;
				}
				if( exception.InnerException == null )
					break;
				prefix = "Caused by";
			}
		}

		static string stringFromStackFrame( SysDiag.StackFrame stackFrame, Sys.Func<string, string> sourceFileNameFixer )
		{
			SysText.StringBuilder stringBuilder = new();
			stringBuilder.Append( "    " );
			string? sourceFileName = stackFrame.GetFileName();
			stringBuilder.Append( string.IsNullOrEmpty( sourceFileName ) ? "<unknown-source>: " : $"{sourceFileNameFixer.Invoke( sourceFileName )}({stackFrame.GetFileLineNumber()}): " );
			SysReflect.MethodBase? method = stackFrame.GetMethod();
			if( method != null )
			{
				stringBuilder.Append( "method " );
				Sys.Type? declaringType = method.DeclaringType;
				if( declaringType != null )
					stringBuilder.Append( GetCSharpTypeName( declaringType ).Replace( '+', '.' ) ).Append( '.' );
				stringBuilder.Append( method.Name );
				if( method is SysReflect.MethodInfo && method.IsGenericMethod )
					stringBuilder.Append( '<' ).Append( method.GetGenericArguments().Select( a => a.Name ).MakeString( "," ) ).Append( '>' );
				stringBuilder.Append( '(' ).Append( method.GetParameters().Select( p => p.ParameterType.Name + " " + p.Name ).MakeString( ", " ) ).Append( ')' );
			}
			return stringBuilder.ToString();
		}
	}

	static string fixMessage( string message )
	{
		message = message.Replace2( "|", "¦" );
		message = message.Replace2( "\r\n", " ¦ " );
		message = message.Replace2( "\r", " ¦ " );
		message = message.Replace2( "\n", " ¦ " );
		message = message.Replace2( "\t", "    " );
		return message;
	}

	public static void SwallowException( LogLevel logLevel, string operationName, Sys.Action procedure )
	{
		object? result = SwallowException<object?>( logLevel, operationName, () =>
		{
			procedure.Invoke();
			return null;
		} );
		Assert( result == null );
	}

	public static T SwallowException<T>( LogLevel logLevel, string operationName, Sys.Func<T> function )
	{
		try
		{
			return function.Invoke();
		}
		catch( Sys.Exception exception )
		{
			IEnumerable<string> lines = BuildMediumExceptionMessage( $"{operationName} failed with ", exception );
			string message = lines.MakeString( "; " );
			Log.MessageWithGivenLevel( logLevel, message );
			return default!;
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// System.Type

	// Obtains the full name of a type using C# notation.
	// PEARL: DotNet represents the full names of types in a cryptic way which does not correspond to any language in particular:
	//        - Generic types are suffixed with a back-quote character, followed by the number of generic parameters.
	//        - Constructed generic types are further suffixed with a list of assembly-qualified type names, one for each generic parameter.
	//        Plus, a nested class is denoted with the '+' sign. (Handling of which is TODO.)
	//        This method returns the full name of a type using C#-specific notation instead of DotNet's cryptic notation.
	public static string GetCSharpTypeName( Sys.Type type )
	{
		if( type.IsArray )
		{
			SysText.StringBuilder stringBuilder = new();
			stringBuilder.Append( GetCSharpTypeName( NotNull( type.GetElementType() ) ) );
			stringBuilder.Append( '[' );
			int rank = type.GetArrayRank();
			Assert( rank >= 1 );
			for( int i = 0; i < rank - 1; i++ )
				stringBuilder.Append( ',' );
			stringBuilder.Append( ']' );
			return stringBuilder.ToString();
		}
		if( type.IsGenericType )
		{
			SysText.StringBuilder stringBuilder = new();
			stringBuilder.Append( getBaseTypeName( type ) );
			stringBuilder.Append( '<' );
			stringBuilder.Append( type.GenericTypeArguments.Select( GetCSharpTypeName ).MakeString( "," ) );
			stringBuilder.Append( '>' );
			return stringBuilder.ToString();
		}
		return type.Namespace + '.' + type.Name.Replace( '+', '.' );

		static string getBaseTypeName( Sys.Type type )
		{
			string typeName = NotNull( type.GetGenericTypeDefinition().FullName );
			int indexOfTick = typeName.LastIndexOf( '`' );
			//Assert( indexOfTick == typeName.IndexOf2( '`' ) );
			return typeName[..indexOfTick];
		}
	}

	public static ListDictionary<K, V> ToListDictionary<T, K, V>( this IEnumerable<T> self, Sys.Func<T, K> keySelector, Sys.Func<T, V> elementSelector ) where K : notnull
	{
		if( self is ICollection<T> collection )
		{
			if( collection is T[] array )
				return fromArray( array );
			if( collection is List<T> list )
				return fromList( list );
		}
		return fromEnumerable( self );

		ListDictionary<K, V> fromEnumerable( IEnumerable<T> enumerable )
		{
			ListDictionary<K, V> d = new();
			foreach( T element in enumerable )
				d.Add( keySelector( element ), elementSelector( element ) );
			return d;
		}

		ListDictionary<K, V> fromArray( T[] array )
		{
			ListDictionary<K, V> d = new();
			foreach( T element in array )
				d.Add( keySelector( element ), elementSelector( element ) );
			return d;
		}

		ListDictionary<K, V> fromList( List<T> list )
		{
			ListDictionary<K, V> d = new();
			foreach( T element in list )
				d.Add( keySelector( element ), elementSelector( element ) );
			return d;
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// double

	public static double Clamped( this double self, double min, double max )
	{
		Assert( min <= max );
		if( self < min )
			return min;
		if( self > max )
			return max;
		return self;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// float

	public static float Clamped( this float self, float min, float max )
	{
		Assert( min < max );
		if( self < min )
			return min;
		if( self > max )
			return max;
		return self;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Math

	// From https://stackoverflow.com/a/44203452/773113, with a few improvements
	public static bool IsPrime( long number )
	{
		if( number <= 1 )
			return false;
		if( number is 2 or 3 or 5 )
			return true;
		if( number % 2 == 0 || number % 3 == 0 || number % 5 == 0 )
			return false;
		long boundary = (long)Math.Sqrt( number );
		for( long i = 6; i <= boundary; i += 6 )
			if( number % (i + 1) == 0 || number % (i + 5) == 0 )
				return false;
		return true;
	}
}
