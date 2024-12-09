namespace MikeNakis.Kit;

using Sys = System;
using SysText = System.Text;
using SysThreading = System.Threading;
using LegacyCollections = System.Collections;
using SysCompiler = System.Runtime.CompilerServices;
using static GlobalStatics;
using Math = System.Math;
using MikeNakis.Kit.Collections;
using SysDiag = System.Diagnostics;
using SysReflect = System.Reflection;
using MikeNakis.Kit.Logging;

public static class KitHelpers
{
	static X defaultExtractor<X>( X a ) => a;

	public const double Epsilon = 1e-15;

	///<summary>Compares two <c>double</c> values for approximate equality, using a given optional tolerance.</summary>
	//TODO: perhaps replace with something more sophisticated, like this: https://stackoverflow.com/a/3875619/773113
	public static bool DoubleEquals( double a, double b, double? maybeTolerance )
	{
		if( double.IsNaN( a ) && double.IsNaN( b ) )
			return true;
		double difference = Math.Abs( a - b );
		double tolerance = maybeTolerance ?? Epsilon;
		return difference < tolerance;
	}

	///<summary>Compares two <c>double</c> values for exact equality, avoiding the "equality comparison of floating point numbers" inspection.</summary>
	public static bool DoubleExactlyEquals( double a, double b )
	{
		return a.Equals( b );
	}

	public const float FEpsilon = 1.192093E-07f;

	///<summary>Compares two <c>float</c> values for approximate equality, using a given optional tolerance.</summary>
	//TODO: perhaps replace with something more sophisticated, like this: https://stackoverflow.com/a/3875619/773113
	public static bool FloatEquals( float a, float b, float? maybeTolerance = null )
	{
		if( float.IsNaN( a ) && float.IsNaN( b ) )
			return true;
		float difference = Math.Abs( a - b );
		float tolerance = maybeTolerance ?? FEpsilon;
		return difference < tolerance;
	}

	///<summary>Compares two <c>float</c> values for exact equality, avoiding the "equality comparison of floating point numbers" inspection.</summary>
	public static bool FloatExactlyEquals( float a, float b )
	{
		return a.Equals( b );
	}

	public static string MakeString( this IEnumerable<string> self, string delimiter ) => self.MakeString( "", delimiter, "", "" );

	public static string MakeString( this IEnumerable<string> self, string prefix, string delimiter, string suffix, string ifEmpty )
	{
		SysText.StringBuilder stringBuilder = new();
		TextConsumer textConsumer = s => stringBuilder.Append( s );
		Sys.Action<string> elementConsumer = s => stringBuilder.Append( s );
		self.MakeString( elementConsumer, prefix, delimiter, suffix, ifEmpty, textConsumer );
		return stringBuilder.ToString();
	}

	public static void MakeString<T>( this IEnumerable<T> self, Sys.Action<T> elementConsumer, string prefix, string delimiter, string suffix, string ifEmpty, TextConsumer textConsumer )
	{
		bool first = true;
		foreach( T element in self )
		{
			if( first )
			{
				textConsumer.Invoke( prefix );
				first = false;
			}
			else
				textConsumer.Invoke( delimiter );
			elementConsumer.Invoke( element );
		}
		textConsumer.Invoke( first ? ifEmpty : suffix );
	}

	public delegate void TextConsumer( Sys.ReadOnlySpan<char> text );

	public static readonly SysThreading.ThreadLocal<bool> FailureTesting = new( false );

	public static string SafeToString( object? value )
	{
		SysText.StringBuilder stringBuilder = new();
		SafeToString( value, s => stringBuilder.Append( s ) );
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
				textConsumer.Invoke( "null" );
			else if( value is char c )
				EscapeForCSharp( c, textConsumer );
			else if( value is string s )
				EscapeForCSharp( s, textConsumer );
			else if( value is LegacyCollections.IEnumerable enumerable )
				enumerable.Cast<object>().MakeString( recurse, "[ ", ", ", " ]", "[]", textConsumer );
			else if( value.GetType().IsValueType || visitedObjects.Add( value ) )
				textConsumer.Invoke( value.ToString() ?? "\u2620" ); //U+2620 Skull and Crossbones Unicode Character
			else
				textConsumer.Invoke( $"{value.GetType()}@{SysCompiler.RuntimeHelpers.GetHashCode( value )}" );
		}
	}

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
		ScribeStringLiteral( quote, content, s => stringBuilder.Append( s ) );
	}

	public static void EscapeForCSharp( string content, char quote, TextConsumer textConsumer )
	{
		ScribeStringLiteral( quote, content, textConsumer );
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
		textConsumer.Invoke( new Sys.ReadOnlySpan<char>( in quoteCharacter ) );
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
		textConsumer.Invoke( new Sys.ReadOnlySpan<char>( in quoteCharacter ) );
		return;

		static void emitEscapedCharacter( TextConsumer textConsumer, char c )
		{
			Sys.Span<char> buffer = stackalloc char[2];
			buffer[0] = '\\';
			buffer[1] = c;
			textConsumer.Invoke( buffer );
		}

		static void emitOtherCharacter( TextConsumer textConsumer, char c )
		{
			if( isPrintable( c ) )
				textConsumer.Invoke( new Sys.ReadOnlySpan<char>( in c ) );
			else if( c < 256 ) // no need to check for >= 0 because char is unsigned.
			{
				Sys.Span<char> buffer = stackalloc char[4];
				buffer[0] = '\\';
				buffer[1] = 'x';
				buffer[2] = digitFromNibble( c >> 4 );
				buffer[3] = digitFromNibble( c & 0x0f );
				textConsumer.Invoke( buffer );
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
				textConsumer.Invoke( buffer );
			}
			return;

			static bool isPrintable( char c ) => c is >= (char)32 and < (char)127;

			static char digitFromNibble( int nibble )
			{
				Assert( nibble is >= 0 and < 16 );
				return (char)((nibble >= 10 ? 'a' - 10 : '0') + nibble);
			}
		}
	}

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

	///<summary>Returns the number of elements from the start of the enumerable that are sorted. Essentially, it finds the index of the first out-of-order element.</summary>
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
	// IDictionary

	public static V Extract<K, V>( IDictionary<K, V> self, K key )
	{
		V value = self[key];
		self.DoRemove( key );
		return value;
	}

	public static V? TryExtract<K, V>( IDictionary<K, V> self, K key ) where K : notnull where V : class
	{
		if( self.TryGetValue( key, out V? value ) )
			self.DoRemove( key );
		return value;
	}

	//PEARL: the remove-item-from-dictionary method of DotNet is not really a "remove" method, it is actually a "try-remove" method, because it returns a
	//boolean to indicate success or failure. So, if we want a real "remove" function which will actually fail on failure, (duh!) we have to introduce it
	//ourselves.  Unfortunately, since the name `Remove` is taken, we have to give the new function a different name, (I chose `DoRemove`,) so we still
	//have to remember to invoke `DoRemove()` instead of `Remove()`.
	public static void DoRemove<K, V>( IDictionary<K, V> self, K key )
	{
		bool ok = self.Remove( key );
		Assert( ok );
	}

	public static V ComputeIfAbsent<K, V>( IDictionary<K, V> self, K key, Sys.Func<K, V> factory )
	{
		if( self.TryGetValue( key, out V? existingValue ) )
			return existingValue;
		V newValue = factory.Invoke( key );
		self.Add( key, newValue );
		return newValue;
	}

	public static V? TryGet<V, K>( IReadOnlyDictionary<K, V> self, K key ) where K : notnull where V : notnull
	{
		return self.GetValueOrDefault( key );
	}

	public static V? TryGet<V, K>( IDictionary<K, V> self, K key ) where K : notnull where V : notnull
	{
		if( self.TryGetValue( key, out V? value ) )
			return value;
		return default;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Exception

	public static IEnumerable<string> BuildMediumExceptionMessage( string prefix, Sys.Exception exception )
	{
		MutableList<string> mutableLines = new();
		while( true )
		{
			mutableLines.Add( $"{prefix}: {exception.GetType().FullName}: {fixMessage( exception.Message )}" );
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
				lines.Add( $"{prefix}: {exception.GetType().FullName}: {fixMessage( exception.Message )}" );
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
			string message = MakeString( lines, "; " );
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
			stringBuilder.Append( MakeString( type.GenericTypeArguments.Select( GetCSharpTypeName ), "," ) );
			stringBuilder.Append( '>' );
			return stringBuilder.ToString();
		}
		return type.Namespace + '.' + type.Name.Replace( '+', '.' );

		static string getBaseTypeName( Sys.Type type )
		{
			string typeName = NotNull( type.GetGenericTypeDefinition().FullName );
			int indexOfTick = typeName.LastIndexOf( '`' );
			Assert( indexOfTick == typeName.IndexOf2( '`' ) );
			return typeName[..indexOfTick];
		}
	}
}
