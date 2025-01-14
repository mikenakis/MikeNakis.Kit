namespace MikeNakis.Kit;

using System.Linq;
using static System.MemoryExtensions;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysGlob = System.Globalization;
using SysReflect = System.Reflection;

public static class ReflectionHelpers
{
	public static SysReflect.PropertyInfo? TryGetPropertyInfo( Sys.Type containingType, string propertyName ) //
		=> containingType.GetProperty( propertyName, SysReflect.BindingFlags.Instance | SysReflect.BindingFlags.Public | SysReflect.BindingFlags.NonPublic );

	public static SysReflect.MethodInfo? TryGetMethodInfo( Sys.Type containingType, string methodName ) => containingType.GetMethod( methodName, SysReflect.BindingFlags.Instance | SysReflect.BindingFlags.Public | SysReflect.BindingFlags.NonPublic );

	public static SysReflect.PropertyInfo GetPropertyInfo( Sys.Type containingType, string propertyName ) => NotNull( TryGetPropertyInfo( containingType, propertyName ) );

	public static Sys.Type GetPropertyType( Sys.Type containingType, string propertyName )
	{
		SysReflect.PropertyInfo propertyInfo = GetPropertyInfo( containingType, propertyName );
		return propertyInfo.PropertyType;
	}

	public static object? GetPropertyValue( object containingInstance, string propertyName ) //
		=> GetPropertyInfo( containingInstance.GetType(), propertyName ).GetValue( containingInstance );

	public static void SetPropertyValue( object containingInstance, string propertyName, object? value ) //
		=> GetPropertyInfo( containingInstance.GetType(), propertyName ).SetValue( containingInstance, value );

	public static T CreateInstanceInAppDomain<T>( Sys.AppDomain newAppDomain, object?[] arguments ) where T : Sys.MarshalByRefObject
	{
		Sys.Type type = typeof( T );
		string assemblyName = NotNull( type.Assembly.FullName );
		string typeName = NotNull( type.FullName );
		Assert( type.GetConstructors().Any( constructorInfo => constructorMatchesArguments( constructorInfo, arguments ) ) );
		object? instance = newAppDomain.CreateInstanceAndUnwrap( assemblyName: assemblyName, typeName: typeName, ignoreCase: false, //
				bindingAttr: SysReflect.BindingFlags.Instance | SysReflect.BindingFlags.Public | SysReflect.BindingFlags.CreateInstance, binder: null, args: arguments, //
				culture: SysGlob.CultureInfo.InvariantCulture, activationAttributes: null );
		return (T)NotNull( instance );
	}

	static bool constructorMatchesArguments( SysReflect.ConstructorInfo constructorInfo, object?[] arguments )
	{
		SysReflect.ParameterInfo[] parameters = constructorInfo.GetParameters();
		if( parameters.Length != arguments.Length )
			return false;
		for( int i = 0; i < parameters.Length; i++ )
		{
			if( arguments[i] == null )
				continue;
			if( !parameters[i].ParameterType.IsInstanceOfType( arguments[i] ) )
				return false;
		}
		return true;
	}

	public static bool MemberwiseEquals<T>( T a, object? b, params string[] excludedFieldNames ) where T : notnull
	{
		if( b == null )
			return false;
		if( ReferenceEquals( a, b ) )
			return true;
		Sys.Type type = typeof( T );
		Assert( a.GetType() == type );
		Assert( b.GetType() == type );
		foreach( SysReflect.FieldInfo fieldInfo in type.GetFields( SysReflect.BindingFlags.Instance | SysReflect.BindingFlags.Public | SysReflect.BindingFlags.NonPublic ) )
		{
			if( excludedFieldNames.Contains( fieldInfo.Name ) )
				continue;
			object? value1 = fieldInfo.GetValue( a );
			object? value2 = fieldInfo.GetValue( b );
			if( fieldInfo.FieldType.IsPrimitive )
			{
				if( !value1!.Equals( value2 ) )
					return false;
			}
			else
			{
				if( !DotNetHelpers.Equals( value1, value2 ) )
					return false;
			}
		}
		return true;
	}

	public static int MemberwiseHashCode<T>( T obj ) where T : notnull
	{
		Sys.Type type = typeof( T );
		Assert( obj.GetType() == type );
#if NETCOREAPP
		Sys.HashCode hashCodeBuilder = new();
		foreach( SysReflect.FieldInfo fieldInfo in type.GetFields( SysReflect.BindingFlags.Instance | SysReflect.BindingFlags.Public | SysReflect.BindingFlags.NonPublic ) )
		{
			Assert( fieldInfo.IsInitOnly );
			object? fieldValue = fieldInfo.GetValue( obj );
			hashCodeBuilder.Add( fieldValue );
		}
		return hashCodeBuilder.ToHashCode();
#else
		uint accumulatedHashCode = 1;
		foreach( SysReflect.FieldInfo fieldInfo in type.GetFields( SysReflect.BindingFlags.Instance | SysReflect.BindingFlags.Public | SysReflect.BindingFlags.NonPublic ) )
		{
			Assert( fieldInfo.IsInitOnly );
			object? fieldValue = fieldInfo.GetValue( obj );
			accumulatedHashCode *= 31;
			accumulatedHashCode += (uint)(fieldValue?.GetHashCode() ?? 0);
		}
		return (int)accumulatedHashCode;
#endif
	}

	public static bool OverridesEqualsMethod( Sys.Type type )
	{
		SysReflect.MethodInfo equalsMethod = type.GetMethods( SysReflect.BindingFlags.Instance | SysReflect.BindingFlags.Public ) //
				.Single( m => m.Name == "Equals" && m.GetBaseDefinition().DeclaringType == typeof( object ) );
		return equalsMethod.DeclaringType != typeof( object );
	}
}
