namespace MikeNakis.Kit;

using MikeNakis.Kit.Extensions;

/// <summary>Guard for overridable methods that need to make sure that descendants will never forget to invoke base.</summary>
public class OverridableGuard
{
	readonly Stack<Invocation> invocationStack = new();
	readonly string methodName;

	/// <summary>Constructor.</summary>
	public OverridableGuard( string methodName )
	{
		this.methodName = methodName;
	}

	public T Invoke<T>( Sys.Func<T> function, [SysCompiler.CallerFilePath] string callerFilePath = null!, [SysCompiler.CallerLineNumber] int callerLineNumber = 0 )
	{
		Invocation invocation = new( this, callerFilePath, callerLineNumber );
		T result = function.Invoke();
		invocation.Dispose();
		return result;
	}

	public void Invoke( Sys.Action procedure, [SysCompiler.CallerFilePath] string callerFilePath = null!, [SysCompiler.CallerLineNumber] int callerLineNumber = 0 )
	{
		// NOTE: we do not want to use `using` here, because the checks must not run if the procedure throws, otherwise the errors reported by the failed
		//       checks will be hiding the fact that an exception was thrown.
		Invocation invocation = new( this, callerFilePath, callerLineNumber );
		procedure.Invoke();
		invocation.Dispose();
	}

	public void InvocationComplete( [SysCompiler.CallerMemberName] string methodName = null! )
	{
		Assert( methodName == this.methodName );
		Assert( invocationStack.Count > 0 ); //the overridable was invoked from some method other than the method which is supposed to invoke it.
		Invocation invocation = invocationStack.Peek();
		Assert( !invocation.Invoked );
		invocation.Invoked = true;
	}

	/// <remarks>NOTE: This class has a Dispose() method but it does not implement IDisposable!
	/// This is done on purpose, in order to avoid the use of the 'using()' keyword for automatic disposal of this class.
	/// The 'using()' construct must not be used here because the overridable being guarded may fail due to an exception before it has had the chance to
	/// invoke the base-most overridable. This means that if an exception occurs, <see cref="InvocationComplete(string)"/> might not be invoked.
	/// However, the 'using()' construct would cause the <see cref="Invocation"/> to be disposed while the exception unwinds,
	/// at which point the <see cref="Invocation"/>  would see that <see cref="InvocationComplete(string)"/> has not been invoked, and throw an exception
	/// as a result, which would then mask the original exception.</remarks>
	sealed class Invocation : Sys.IDisposable
	{
		readonly OverridableGuard overridableGuard;
		readonly string callerFilePath;
		readonly int callerLineNumber;
		string methodName => overridableGuard.methodName;
		public bool Invoked { get; set; }

		public Invocation( OverridableGuard overridableGuard, string callerFilePath, int callerLineNumber )
		{
			this.overridableGuard = overridableGuard;
			this.callerFilePath = callerFilePath;
			this.callerLineNumber = callerLineNumber;
			overridableGuard.invocationStack.Push( this );
		}

		public void Dispose()
		{
			Assert( overridableGuard.invocationStack.Peek().ReferenceEquals( this ) );
			overridableGuard.invocationStack.Pop();
			if( !Invoked )
			{
				Log.Error( $"Overridable did not invoke base.{methodName}()" );
				Log.Info( "Invocation started on this line.", callerFilePath, callerLineNumber );
				Assert( false );
			}
		}

		public override string ToString() => $"{typeof( Invocation ).Name} for {methodName}";
	}
}
