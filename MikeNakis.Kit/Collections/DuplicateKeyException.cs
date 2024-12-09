namespace MikeNakis.Kit.Collections;

using System;

public abstract class DuplicateKeyException : ArgumentException
{
	public object Key => OnGetKey();

	protected DuplicateKeyException()
	{ }

	protected abstract object OnGetKey();
}

public class DuplicateKeyException<K> : DuplicateKeyException where K : notnull
{
	public new K Key { get; }
	protected override object OnGetKey() => Key;

	public DuplicateKeyException( K key )
	{
		Key = key;
	}
}
