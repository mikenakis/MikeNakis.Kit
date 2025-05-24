#pragma warning disable CA1812 // Avoid uninstantiated internal classes
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
#pragma warning disable CA1852 // Seal internal types

namespace MikeNakis.Kit;

using MikeNakis.Kit.Extensions;

///<summary>Obtains the full name of a type in C# notation.</summary>
///<remarks>
/// PEARL: DotNet internally represents the names of types in a cryptic way which greatly differs from the way they are
/// specified in C# source code:<para/>
/// <list type="bullet">
/// <item>Generic types are suffixed with a back-quote character, followed by the number of generic parameters.</item>
/// <item>Constructed generic types are further suffixed with a list of assembly-qualified type names, one for each generic parameter.</item>
/// <item>Nested class names are delimited with '+' instead of '.'.</item>
/// </list>
/// PEARL-ON-PEARL: Dotnet does not provide any means of converting such cryptic type names to C# notation.<para/>
/// This method fixes all this insanity.<para/>
/// TODO: add the ability to specify whether generic parameter names will be emitted. (Default is to leave blank.)<para/>
/// TODO: add the ability to specify whether <see cref="Sys.Nullable{T}" /> should be replaced with <c>T?</c> (separately from 'useAliases')<para/>
/// TODO: see how the code can be further simplified. (<c>recurse2()</c> can probably be merged into <c>recurse()</c>)<para/>
///</remarks>
static class CSharpTypeNameGenerator
{
	public static string GetCSharpTypeName( Sys.Type type, bool useAliases = true )
	{
		if( type.IsGenericParameter )
			return type.Name;
		SysText.StringBuilder stringBuilder = new();
		recurse( stringBuilder, type, useAliases );
		return stringBuilder.ToString();

		static void recurse( SysText.StringBuilder stringBuilder, Sys.Type type, bool useAliases )
		{
			if( type.IsGenericParameter )
				stringBuilder.Append( "" ); //TODO: append actual parameter names if requested
			else if( type.IsArray )
			{
				Sys.Type elementType = type.GetElementType().OrThrow();
				recurse( stringBuilder, elementType, useAliases );
				appendArraySuffix( stringBuilder, type.GetArrayRank() );
			}
			else
			{
				if( useAliases && appendTypeNameAlias( stringBuilder, type ) )
					return;
				Sys.Type[] allGenericArguments = type.GetGenericArguments();
				recurse2( stringBuilder, type, allGenericArguments, useAliases );
			}
			return;

			static void recurse2( SysText.StringBuilder stringBuilder, Sys.Type type, Sys.Type[] allGenericArguments, bool useAliases )
			{
				if( type.IsNested )
				{
					Sys.Type declaringType = type.DeclaringType.OrThrow();
					recurse2( stringBuilder, declaringType, allGenericArguments, useAliases );
					stringBuilder.Append( '.' );
				}
				else
					appendNamespaceAndDot( stringBuilder, type );

				if( type.IsGenericType )
				{
					string typeName = type.Name;
					int indexOfTick = typeName.LastIndexOf( '`' );
					Assert( indexOfTick == typeName.IndexOf2( '`' ) );
					if( indexOfTick == -1 )
						stringBuilder.Append( typeName );
					else
					{
						stringBuilder.Append( typeName[..indexOfTick] );
						stringBuilder.Append( '<' );
						Sys.Type[] arguments = type.GetGenericArguments();
						int start = numberOfArgumentsToSkip( type );
						bool first = true;
						for( int i = start; i < arguments.Length; i++ )
						{
							if( first )
								first = false;
							else
								stringBuilder.Append( ',' );
							Sys.Type argument = arguments[i];
							if( argument.IsGenericParameter )
							{
								int position = argument.GenericParameterPosition;
								argument = allGenericArguments[position];
							}
							recurse( stringBuilder, argument, useAliases );
						}
						stringBuilder.Append( '>' );
					}
					return;
				}

				stringBuilder.Append( type.Name.Replace( '+', '.' ) );
				return;
			}

			static bool appendTypeNameAlias( SysText.StringBuilder stringBuilder, Sys.Type type )
			{
				string? alias = getTypeNameAlias( type );
				if( alias == null )
					return false;
				stringBuilder.Append( alias );
				return true;

				static string? getTypeNameAlias( Sys.Type type )
				{
					if( type == typeof( sbyte ) )
						return "sbyte";
					if( type == typeof( byte ) )
						return "byte";
					if( type == typeof( short ) )
						return "short";
					if( type == typeof( ushort ) )
						return "ushort";
					if( type == typeof( int ) )
						return "int";
					if( type == typeof( uint ) )
						return "uint";
					if( type == typeof( long ) )
						return "long";
					if( type == typeof( ulong ) )
						return "ulong";
					if( type == typeof( char ) )
						return "char";
					if( type == typeof( float ) )
						return "float";
					if( type == typeof( double ) )
						return "double";
					if( type == typeof( bool ) )
						return "bool";
					if( type == typeof( decimal ) )
						return "decimal";
					if( type == typeof( object ) )
						return "object";
					if( type == typeof( string ) )
						return "string";
					return null;
				}
			}

			static void appendNamespaceAndDot( SysText.StringBuilder stringBuilder, Sys.Type type )
			{
				string? namespaceName = type.Namespace;
				if( namespaceName != null )
				{
					stringBuilder.Append( namespaceName );
					stringBuilder.Append( '.' );
				}
			}

			static void appendArraySuffix( SysText.StringBuilder stringBuilder, int rank )
			{
				stringBuilder.Append( '[' );
				Assert( rank >= 1 );
				for( int i = 0; i < rank - 1; i++ )
					stringBuilder.Append( ',' );
				stringBuilder.Append( ']' );
			}

			static int numberOfArgumentsToSkip( Sys.Type type )
			{
				if( !type.IsNested )
					return 0;
				type = type.DeclaringType.OrThrow();
				return type.GetGenericArguments().Length;
			}
		}
	}
}
