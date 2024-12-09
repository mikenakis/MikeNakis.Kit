namespace MikeNakis.Kit;

using ConcurrentCollections = System.Collections.Concurrent;
using Sys = System;
using SysConsole = System.Console;

public sealed class MessagePumpSynchronizer : Sys.IDisposable
{
	readonly LifeGuard lifeGuard = LifeGuard.Create();
	public override string ToString() => lifeGuard.ToStringFor( this );
	readonly ConcurrentCollections.BlockingCollection<Sys.Action> outOfThreadProcedures = new( new ConcurrentCollections.ConcurrentQueue<Sys.Action>() ); // use a concurrent queue to enforce FIFO behavior (tasks being executed in the order in which they were received)
	public bool TerminationRequested { get; private set; }
	public bool HasProcedureWaiting => outOfThreadProcedures.Count > 0;

	public MessagePumpSynchronizer()
	{ }

	public void Dispose()
	{
		Assert( lifeGuard.IsAliveAssertion() );
		outOfThreadProcedures.Dispose();
		lifeGuard.Dispose();
	}

	void requestTermination()
	{
		TerminationRequested = true;
	}

	public void Post( Sys.Action procedure )
	{
		Assert( procedure != null );
		outOfThreadProcedures.Add( procedure );
	}

	// public void RunTask<T>( string producerName, Function<T> producer, string consumerName, Procedure<T> consumer )
	// {
	// 	T result = producer.Invoke();
	// 	consumer.Invoke( result );
	// }

	public void Run()
	{
		SysConsole.WriteLine( "Starting message loop" );
		for( TerminationRequested = false; !TerminationRequested; )
		{
			Sys.Action procedure = outOfThreadProcedures.Take(); //this blocks waiting for a procedure to be placed in the queue.
			dispatch( procedure );
		}
	}

	public void TakeOneAndDispatch()
	{
		Sys.Action procedure = outOfThreadProcedures.Take(); //this blocks waiting for a procedure to be placed in the queue.
		dispatch( procedure );
	}

	static void dispatch( Sys.Action procedure )
	{
		try
		{
			procedure.Invoke();
		}
		catch( Sys.Exception exception )
		{
			Log.Error( "Message Pump Exception: ", exception );
		}
	}

	public void Stop()
	{
		Assert( !TerminationRequested ); //we are already stopped, or we have not even started.
		outOfThreadProcedures.Add( requestTermination );
	}
}
