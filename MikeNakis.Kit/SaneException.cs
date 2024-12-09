namespace MikeNakis.Kit;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikeNakis.Kit.Collections;
using MikeNakis.Kit.Extensions;
using Sys = System;
using SysDiag = System.Diagnostics;
using SysReflect = System.Reflection;

[SysDiag.DebuggerDisplay( "{GetType().Name,nq}: {Message,nq}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public abstract class SaneException : Sys.Exception, IEnumerable<(string, object?)>
{
	public override string Message => buildMessage();

	string buildMessage()
	{
		return getMemberRepresentations( this );

		static string getMemberRepresentations( object self )
		{
			return getMemberTuples( self ) //
					.Select( getMemberRepresentation )
					.MakeString( "; " );
		}
	}

	static IEnumerable<(string name, object? value)> getMemberTuples( object self )
	{
		return getMembers( self ) //
				.Where( memberInfo => memberInfo.Name != nameof( Message ) )
				.Where( memberInfo => self.GetType().IsAssignableFrom( memberInfo.DeclaringType ) )
				.Select( memberInfo => getMemberTuple( self, memberInfo ) )
				.WhereNonNull();
	}

	static SysReflect.MemberInfo[] getMembers( object self ) //
		=> self.GetType().GetMembers( SysReflect.BindingFlags.Public | SysReflect.BindingFlags.Instance );

	static string getMemberRepresentation( (string name, object? value) tuple ) //
		=> $"{tuple.name} = {KitHelpers.SafeToString( tuple.value )}";

	static (string name, object? value)? getMemberTuple( object self, SysReflect.MemberInfo memberInfo ) //
		=> memberInfo switch
		{
			SysReflect.FieldInfo fieldInfo => (fieldInfo.Name, fieldInfo.GetValue( self )),
			SysReflect.PropertyInfo propertyInfo => (propertyInfo.Name, propertyInfo.GetValue( self )),
			_ => null
		};

	protected SaneException( Sys.Exception? cause )
			: base( null, cause )
	{ }

	protected SaneException()
			: base( null, null )
	{ }

	public IEnumerator<(string, object?)> GetEnumerator() => getMemberTuples( this ).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
