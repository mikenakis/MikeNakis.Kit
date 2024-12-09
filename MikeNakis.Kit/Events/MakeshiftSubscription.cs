namespace MikeNakis.Kit.Events;

public sealed class MakeshiftSubscription : Subscription
{
	readonly System.Action deregister;

	public MakeshiftSubscription( System.Action deregister )
	{
		this.deregister = deregister;
	}

	public override void Dispose()
	{
		deregister.Invoke();
		base.Dispose();
	}
}
