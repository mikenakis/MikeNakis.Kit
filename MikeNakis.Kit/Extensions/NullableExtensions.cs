namespace MikeNakis.Kit.Extensions;

public static class NullableExtensions
{
	const string nullMessage = "value is null";

	[SysDiag.DebuggerHidden] public static T OrThrow<T>( this T? self ) where T : class => self ?? throw Failure( nullMessage );
	[SysDiag.DebuggerHidden] public static T OrThrow<T>( this T? self, Sys.Func<Sys.Exception> exceptionFactory ) where T : class => self ?? throw exceptionFactory.Invoke();
	[SysDiag.DebuggerHidden] public static T OrThrow<T>( this T? self ) where T : struct => self ?? throw Failure( nullMessage );
	[SysDiag.DebuggerHidden] public static T OrThrow<T>( this T? self, Sys.Func<Sys.Exception> exceptionFactory ) where T : struct => self ?? throw exceptionFactory.Invoke();
}
