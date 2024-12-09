namespace MikeNakis.Kit.Events;

using MikeNakis.Kit.Collections;
using Sys = System;

public sealed class MutablePublisher<T> : Publisher<T>, Sys.IDisposable
{
	sealed class MySubscription : Subscription
	{
		readonly MutablePublisher<T> eventPublisher;
		readonly Sys.Action<Subscription, T> subscriber;

		public MySubscription( MutablePublisher<T> eventPublisher, Sys.Action<Subscription, T> subscriber )
		{
			this.eventPublisher = eventPublisher;
			this.subscriber = subscriber;
		}

		public void Send( T issue )
		{
			subscriber.Invoke( this, issue );
		}

		public override void Dispose()
		{
			eventPublisher.subscriptions.DoRemove( this );
			base.Dispose();
		}
	}

	readonly LifeGuard lifeGuard = LifeGuard.Create( true );
	public override string ToString() => lifeGuard.ToStringFor( this );
	readonly MutableList<MySubscription> subscriptions = new();

	public MutablePublisher()
	{ }

	public void Publish( T issue )
	{
		foreach( MySubscription subscription in subscriptions.AsReadOnlyList.Collect() )
			subscription.Send( issue );
	}

	public Subscription NewSubscription( Sys.Action<Subscription, T> subscriber )
	{
		MySubscription subscription = new( this, subscriber );
		subscriptions.Add( subscription );
		return subscription;
	}

	public void Dispose()
	{
		Assert( lifeGuard.IsAliveAssertion() );
		Assert( subscriptions.IsEmpty() );
		lifeGuard.Dispose();
	}
}
