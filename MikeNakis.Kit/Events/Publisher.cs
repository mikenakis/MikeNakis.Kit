namespace MikeNakis.Kit.Events;

using Sys = System;

public interface Publisher<out T>
{
	Subscription NewSubscription( Sys.Action<Subscription, T> subscriber );
}
