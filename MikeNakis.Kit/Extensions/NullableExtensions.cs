namespace MikeNakis.Kit.Extensions;

using Sys = System;
using SysDiag = System.Diagnostics;

public static class NullableExtensions
{
	[SysDiag.DebuggerHidden] public static T OrThrow<T>( this T? self ) where T : class => self ?? throw Failure();
	[SysDiag.DebuggerHidden] public static T OrThrow<T>( this T? self, Sys.Func<Sys.Exception> exceptionFactory ) where T : class => self ?? throw exceptionFactory.Invoke();
	[SysDiag.DebuggerHidden] public static T OrThrow<T>( this T? self ) where T : struct => self ?? throw Failure();
	[SysDiag.DebuggerHidden] public static T OrThrow<T>( this T? self, Sys.Func<Sys.Exception> exceptionFactory ) where T : struct => self ?? throw exceptionFactory.Invoke();
}
