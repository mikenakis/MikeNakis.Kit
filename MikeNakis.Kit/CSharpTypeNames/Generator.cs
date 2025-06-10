namespace MikeNakis.Kit.CSharpTypeNames;

[Sys.Flags]
public enum Options
{
	/// <summary>Specifies no options.</summary>
	None = 0,

	/// <summary>Specifies that language keywords for built-in types should not be used.</summary>
	/// <remarks>For example, <b><c>System.Int32</c></b> will be emitted instead of <b><c>int</c></b>.</remarks>
	NoKeywordsForBuiltInTypes = 1 << 0,

	/// <summary>Specifies that shorthand notation for nullable value types should not be used.</summary>
	/// <remarks>For example, <b><c>System.Nullable&lt;System.DateTime&gt;</c></b> will be generated instead of
	/// <b><c>System.DateTime?</c></b>.</remarks>
	NoNullableShorthandNotation = 1 << 1,

	/// <summary>Specifies that generic parameter names should be emitted rather than left blank.</summary>
	/// <remarks>For example, <b><c>System.Collections.Generic.Dictionary&lt;TKey,TValue&gt;</c></b> will be generated
	/// instead of <b><c>System.Collections.Generic.Dictionary&lt;,&gt;</c></b>.<para/>
	/// Note that the name of a generic type parameter will always be generated if it is directly asked for; this option
	/// only applies to the appearance of generic type parameter names within the names of generic type definitions.</remarks>
	EmitGenericParameterNames = 1 << 2,

	/// <summary>Specifies that language keywords for built-in native-sized integer types should not be used.</summary>
	/// <remarks>For example, <b><c>System.IntPtr</c></b> will be generated instead of <b><c>nint</c></b>.<para/></remarks>
	NoKeywordsForNativeSizedIntegers = 1 << 3,

	/// <summary>Specifies that namespaces should not be emitted.</summary>
	/// <remarks>For example, <b><c>DateTime</c></b> will be emitted instead of <b><c>System.DateTime</c></b>.</remarks>
	NoNamespaces = 1 << 4,

	/// <summary>Specifies that tuple notation should not be used.</summary>
	/// <remarks>For example, <b><c>System.ValueTuple&lt;int,int&gt;</c></b> will be generated instead of
	/// <b><c>(int,int)</c></b>.</remarks>
	NoTupleShorthandNotation = 1 << 5,

	/// <summary>Specifies that the keyword of the type definition should be emitted.</summary>
	/// <remarks>For example, <b><c>interface IComparable&lt;int&gt;</c></b> will be generated instead of
	/// <b><c>IComparable&lt;int&gt;</c></b>.</remarks>
	EmitTypeDefinitionKeyword = 1 << 6,
}

public static class Generator
{
	/// <summary>Generates the human-readable name of a <see cref="Sys.Type"/> using C# notation.</summary>
	/// <param name="type">The <see cref="Sys.Type"/> whose name is to be generated.</param>
	/// <param name="options">Specifies how the name will be generated.</param>
	/// <returns>The human-readable name of the given <see cref="Sys.Type"/> in C# notation.</returns>
	public static string GetCSharpTypeName( Sys.Type type, Options options )
	{
		SysText.StringBuilder stringBuilder = new();
		emitTypeKeyword( type, options );

		if( type.IsGenericParameter ) //if a generic parameter is directly passed, always yield its name.
			stringBuilder.Append( type.Name );
		else
			recurse( type );

		return stringBuilder.ToString();

		void emitTypeKeyword( Sys.Type type, Options options )
		{
			if( !options.HasFlag( Options.EmitTypeDefinitionKeyword ) )
				return;
			if( getLanguageKeywordIfBuiltInType( type, options ) != null )
				return;

			if( type.IsGenericTypeDefinition )
				stringBuilder.Append( "generic " );

			if( type.IsGenericParameter )
				stringBuilder.Append( "generic type parameter " );
			else if( type.IsPointer )
				stringBuilder.Append( "pointer " );
			else if( typeof( Sys.Delegate ).IsAssignableFrom( type ) )
				stringBuilder.Append( "delegate " );
			else if( type.IsEnum )
				stringBuilder.Append( "enum " );
			else if( type.IsArray )
				stringBuilder.Append( "array " );

			else if( type.IsInterface )
				stringBuilder.Append( "interface " );
			else if( type.IsValueType )
			{
				if( !isValueTuple( type ) )
					stringBuilder.Append( "struct " );
			}
			else if( type.IsClass )
				stringBuilder.Append( "class " );
			else
				SysDiag.Debug.Assert( false );

			if( type.IsGenericTypeDefinition )
				stringBuilder.Append( "definition " );
		}

		void recurse( Sys.Type type )
		{
			if( type.IsGenericParameter )
			{
				if( options.HasFlag( Options.EmitGenericParameterNames ) )
					stringBuilder.Append( type.Name );
				return;
			}

			if( type.IsArray )
			{
				recurse( type.GetElementType()! );
				stringBuilder.Append( '[' );
				int rank = type.GetArrayRank();
				SysDiag.Debug.Assert( rank >= 1 );
				stringBuilder.Append( ',', rank - 1 );
				stringBuilder.Append( ']' );
				return;
			}

			if( !options.HasFlag( Options.NoKeywordsForBuiltInTypes ) )
			{
				string? languageKeyword = getLanguageKeywordIfBuiltInType( type, options );
				if( languageKeyword != null )
				{
					stringBuilder.Append( languageKeyword );
					return;
				}
			}

			if( !options.HasFlag( Options.NoNullableShorthandNotation ) )
			{
				Sys.Type? underlyingType = Sys.Nullable.GetUnderlyingType( type );
				if( underlyingType != null )
				{
					recurse( underlyingType );
					stringBuilder.Append( '?' );
					return;
				}
			}

			if( !options.HasFlag( Options.NoTupleShorthandNotation ) && isValueTuple( type ) )
			{
				stringBuilder.Append( '(' );
				Sys.Type[] genericArguments = type.GetGenericArguments();
				for( int i = 0; i < genericArguments.Length; i++ )
				{
					recurse( genericArguments[i] );
					if( i + 1 < genericArguments.Length )
						stringBuilder.Append( ',' );
				}
				stringBuilder.Append( ')' );
				return;
			}

			Sys.Type[] allGenericArguments = getAllGenericArguments( type );
			recurseNested( type, allGenericArguments );
			return;

			static Sys.Type[] getAllGenericArguments( Sys.Type type )
			{
				//if( type.IsNested )
				//	return getAllGenericArguments( type.DeclaringType );
				return type.GetGenericArguments();
			}

			void recurseNested( Sys.Type type, Sys.Type[] allGenericArguments )
			{
				if( type.IsNested )
				{
					recurseNested( type.DeclaringType!, allGenericArguments );
					stringBuilder.Append( '.' );
				}
				else
				{
					if( type.Namespace != null && !options.HasFlag( Options.NoNamespaces ) )
					{
						stringBuilder.Append( type.Namespace );
						stringBuilder.Append( '.' );
					}
				}

				string typeName = type.Name;

				// We do this weird thing to emulate the behavior of CSharpCodeProvider, CodeTypeReference
				if( typeName.StartsWith( "__", Sys.StringComparison.Ordinal ) )
					stringBuilder.Append( '@' );

				if( type.IsGenericType )
				{
					//PEARL: DotNet offers no means of obtaining the plain, unadulterated name of a generic type.
					//    The only way to get the name of a type seems to be its `Name` property, but this property
					//    returns a name that is polluted by a back-quote followed by a number.
					//    So, in order to get the _actual_ name of the type we have no option but to engage in string
					//    manipulation to extract the part before the back-quote.
					//PEARL: DotNet does not even offer any proper means of detecting whether a type has generic
					//    parameters, so as to know beforehand whether we should expect a back-quote in the name.
					//    The ContainsGenericParameters property, the GenericTypeArguments property, and the
					//    GetGenericArguments() method all return different results under different circumstances, which
					//    do not reliably correspond to the presence or absence of a back-quote in the name.
					//    So, the only way to find out if there is a back-quote in the name is to engage in string
					//    search to try and find it.
					int indexOfBackQuote = typeName.LastIndexOf( '`' );
					SysDiag.Debug.Assert( indexOfBackQuote == typeName.IndexOf( '`', Sys.StringComparison.Ordinal ) );
					if( indexOfBackQuote != -1 )
					{
						stringBuilder.Append( typeName.Substring( 0, indexOfBackQuote ) );
						stringBuilder.Append( '<' );
						Sys.Type[] arguments = type.GetGenericArguments();
						int start = type.IsNested ? type.DeclaringType!.GetGenericArguments().Length : 0;
						SysDiag.Debug.Assert( arguments.Length - start == int.Parse( typeName.Substring( indexOfBackQuote + 1 ), SysGlob.CultureInfo.InvariantCulture ) );
						for( int i = start; i < arguments.Length; i++ )
						{
							if( !(type.ContainsGenericParameters && !type.IsGenericTypeDefinition) ) // very special case for the type of the field in `TypeA<T1,T2> { TypeB<T2> Field; }`
							{
								Sys.Type argument = arguments[i];
								if( argument.IsGenericParameter )
								{
									int position = argument.GenericParameterPosition;
									argument = allGenericArguments[position];
								}
								recurse( argument );
							}
							if( i + 1 < arguments.Length )
								stringBuilder.Append( ',' );
						}
						stringBuilder.Append( '>' );
						return;
					}
				}

				stringBuilder.Append( typeName );
			}
		}
	}

	static bool isValueTuple( Sys.Type type )
	{
		// See https://stackoverflow.com/a/75852077/773113
		if( !type.IsValueType )
			return false;
		if( type.IsGenericTypeDefinition )
			return false;
		//In netcore all ValueTuple structs implement ITuple, so they can be easily detected as follows:
		//    return typeof( SysCompiler.ITuple ).IsAssignableFrom( type );
		//Unfortunately, in netstandard2.0, ITuple does not seem to exist, so we have to resort to string comparison.
		if( type.FullName == null )
			return false;
		return type.FullName.StartsWith( "System.ValueTuple`", Sys.StringComparison.Ordinal );
		//Note that the above would also match "System.ValueTuple`1[]", but it cannot happen here because arrays
		//     are not value types, and we have already checked to make sure that this is a value type.
	}

	static string? getLanguageKeywordIfBuiltInType( Sys.Type type, Options options )
	{
		if( type.IsEnum )
			return null;
		return Sys.Type.GetTypeCode( type ) switch
		{
			Sys.TypeCode.SByte => "sbyte",
			Sys.TypeCode.Byte => "byte",
			Sys.TypeCode.Int16 => "short",
			Sys.TypeCode.UInt16 => "ushort",
			Sys.TypeCode.Int32 => "int",
			Sys.TypeCode.UInt32 => "uint",
			Sys.TypeCode.Int64 => "long",
			Sys.TypeCode.UInt64 => "ulong",
			Sys.TypeCode.Char => "char",
			Sys.TypeCode.Single => "float",
			Sys.TypeCode.Double => "double",
			Sys.TypeCode.Boolean => "bool",
			Sys.TypeCode.Decimal => "decimal",
			Sys.TypeCode.Object => otherType( type, options ),
			Sys.TypeCode.String => "string",
			_ => null,
		};

		static string? otherType( Sys.Type type, Options options )
		{
			if( type == typeof( object ) )
				return "object";
			if( type == typeof( void ) )
				return "void";
			if( !options.HasFlag( Options.NoKeywordsForNativeSizedIntegers ) )
			{
				if( type == typeof( nint ) )
					return "nint";
				if( type == typeof( nuint ) )
					return "nuint";
			}
			return null;
		}
	}
}
