namespace MikeNakis.Kit;

using MikeNakis.Kit.Extensions;

public abstract class ThreadGuard
{
	public static ThreadGuard Create()
	{
		if( DebugMode )
			return new DebugThreadGuard();
		return ProductionThreadGuard.Instance;
	}

	public abstract bool InThreadAssertion();

	public abstract bool OutOfThreadAssertion();

	sealed class ProductionThreadGuard : ThreadGuard
	{
		public static readonly ProductionThreadGuard Instance = new();

		ProductionThreadGuard()
		{ }

		public override bool InThreadAssertion() => throw Failure(); //never invoke on a release build

		public override bool OutOfThreadAssertion() => throw Failure(); //never invoke on a release build
	}

	sealed class DebugThreadGuard : ThreadGuard
	{
		readonly SysThread.Thread thread;

		public DebugThreadGuard()
		{
			thread = SysThread.Thread.CurrentThread;
		}

		public override bool InThreadAssertion() => SysThread.Thread.CurrentThread.ReferenceEquals( thread );

		public override bool OutOfThreadAssertion() => !SysThread.Thread.CurrentThread.ReferenceEquals( thread );

		public override string ToString() => $"thread={thread}";
	}
}
