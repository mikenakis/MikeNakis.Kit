namespace MikeNakis.Kit;

using System.Collections.Generic;
using MikeNakis.Kit.Collections;
using MikeNakis.Kit.Extensions;
using MikeNakis.Kit.FileSystem;
using static MikeNakis.Kit.GlobalStatics;
using LegacyCollections = System.Collections;
using Math = System.Math;
using Sys = System;
using SysCompiler = System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;
using SysGlob = System.Globalization;
using SysInterop = System.Runtime.InteropServices;
using SysIo = System.IO;
using SysReflect = System.Reflection;
using SysText = System.Text;
using SysThread = System.Threading;

public static class DotNetHelpers
{
	public static readonly SysText.Encoding BomlessUtf8 = new SysText.UTF8Encoding( false );

	//See https://stackoverflow.com/q/616584/773113
	public static string GetProductName()
	{
		string name = Sys.AppDomain.CurrentDomain.FriendlyName;
#pragma warning disable RS0030 // Do not use banned APIs
		return SysIo.Path.GetFileNameWithoutExtension( name );
#pragma warning restore RS0030 // Do not use banned APIs
	}

	public static long GetProcessPrivateMemory()
	{
		SysDiag.Process currentProcess = SysDiag.Process.GetCurrentProcess();
		return currentProcess.PrivateMemorySize64;
	}

	static object garbageCollectedObject = new();

#pragma warning disable RS0030 // Do not use banned APIs
	static Sys.DateTime currentTime() => Sys.DateTime.Now;
#pragma warning restore RS0030 // Do not use banned APIs

	[SysDiag.Conditional( "DEBUG" )]
	public static void PerformGarbageCollectionAndWait()
	{
		Sys.DateTime startTime = currentTime();
		int numberOfVainCollectionsInARow = 0;
		long startMemory = GetProcessPrivateMemory();
		long currentMemory = startMemory;
		while( true )
		{
			long previousMemory = currentMemory;
			performGarbageCollection();
			currentMemory = GetProcessPrivateMemory();
			bool vain = currentMemory >= previousMemory;
			if( vain )
			{
				numberOfVainCollectionsInARow++;
				if( numberOfVainCollectionsInARow > 3 )
					break;
			}
			else
				numberOfVainCollectionsInARow = 0;
			SysThread.Thread.Sleep( 100 );
		}
		Sys.TimeSpan timeSpan = currentTime() - startTime;
		Log.Debug( $"Garbage collection: {message( currentMemory - startMemory )} in {timeSpan.TotalMilliseconds} ms." );
		return;

		static string message( long memoryDifference ) //
			=> memoryDifference switch
			{
				<= 0 => $"{-memoryDifference:N0} bytes reclaimed",
				_ => $"{memoryDifference:N0} bytes lost"
			};

		static void performGarbageCollection()
		{
			Sys.WeakReference reference = createWeakReference();
			do
			{
				Sys.GC.Collect( Sys.GC.MaxGeneration, Sys.GCCollectionMode.Aggressive, blocking: true, compacting: true );
				Sys.GC.WaitForPendingFinalizers();
				//Sys.GC.Collect( Sys.GC.MaxGeneration, Sys.GCCollectionMode.Aggressive, blocking: true );
				Assert( !reference.IsAlive ); //this has been observed to fail on a rare occasion
			} while( reference.IsAlive );
		}

		// PEARL: the allocation of a dummy weak reference needs to be done in a separate method because the C# compiler
		// anchors `new object()` to the executing method's stack frame even though no local variable is declared.
		static Sys.WeakReference createWeakReference()
		{
			object obj = garbageCollectedObject;
			garbageCollectedObject = new object();
			return new Sys.WeakReference( obj );
		}
	}

	public static string MakeTechnicalTimeStamp( Clock clock )
	{
		Sys.DateTime utcTime = clock.GetUniversalTime();
		Sys.TimeZoneInfo localTz = clock.GetLocalTimeZone();
		Sys.TimeSpan offset = localTz.GetUtcOffset( utcTime );
		Sys.DateTime localTime = Sys.TimeZoneInfo.ConvertTimeFromUtc( utcTime, localTz );
		return $"{localTime.Year:D4}-{localTime.Month:D2}-{localTime.Day:D2} {localTime.Hour:D2}:{localTime.Minute:D2}:{localTime.Second:D2} GMT{offset.TotalHours:+#;-#;+0}";
	}

	public static Sys.DateTime UnixTimeStampToDateTime( double unixTimeStamp )
	{
		// Unix timestamp is number of seconds past epoch, where epoch = 1970/1/1 0:0:0
		Sys.DateTime dtDateTime = new( 1970, 1, 1, 0, 0, 0, 0, Sys.DateTimeKind.Utc );
		dtDateTime = dtDateTime.AddTicks( (long)(unixTimeStamp * 1e7) );
		return dtDateTime;
	}

	public static double ToUnixTimeStamp( Sys.DateTime dateTime ) => dateTime.Subtract( new Sys.DateTime( 1970, 1, 1, 0, 0, 0, 0, Sys.DateTimeKind.Utc ) ).TotalSeconds;

	public static string MakeTimeZoneDisplayName( Sys.TimeSpan timeSpan )
	{
		return $"GMT{timeSpan.Hours:+#;-#;+0}:{timeSpan.Minutes:D2}";
	}

	public static FilePath GetMainModuleFilePath()
	{
		SysDiag.ProcessModule mainModule = SysDiag.Process.GetCurrentProcess().MainModule.OrThrow();
		// PEARL: System.Diagnostics.Process.GetCurrentProcess().MainModule.Filename is not a filename, it is a pathname!
		string fullPathName = mainModule.FileName;
		return FilePath.FromAbsolutePath( fullPathName );
	}

	public static DirectoryPath GetMainModuleDirectoryPath()
	{
		return GetMainModuleFilePath().Directory;
	}

	static string? mainModuleName;

	public static string MainModuleName => mainModuleName ??= getMainModuleName();

	static string getMainModuleName()
	{
		string mainModuleName = SysDiag.Process.GetCurrentProcess().MainModule.OrThrow().ModuleName;
		const string exeExtension = ".exe";
		Assert( mainModuleName.EndsWithIgnoreCase( exeExtension ) );
		return mainModuleName[..^exeExtension.Length];
	}

	static DirectoryPath? applicationLocalAppDataFolder;

	///<summary>Returns something like C:\Users\(UserName)\Documents (which used to be called "My Documents")</summary>
	public static DirectoryPath UserDocumentsFolder => DirectoryPath.FromAbsolutePath( Sys.Environment.GetFolderPath( Sys.Environment.SpecialFolder.MyDocuments ) );

	///<summary>Returns something like C:\Users\(UserName)\AppData\Local\(ApplicationName)</summary>
	public static DirectoryPath UserAppDataLocalApplicationFolder => applicationLocalAppDataFolder ??= getApplicationLocalAppDataFolder();

	static DirectoryPath getApplicationLocalAppDataFolder()
	{
		DirectoryPath localAppDataFolder = DirectoryPath.FromAbsolutePath( Sys.Environment.GetFolderPath( Sys.Environment.SpecialFolder.LocalApplicationData ) );
		DirectoryPath applicationLocalAppDataFolder = localAppDataFolder.Directory( MainModuleName );
		applicationLocalAppDataFolder.CreateIfNotExist(); // necessary on the first run after a fresh installation.
		return applicationLocalAppDataFolder;
	}

	/// <summary>Returns the temporary directory for the current user and the current application.
	/// Typically, this is %USERPROFILE%\AppData\Local\Temp\{application-name}.</summary>
	public static DirectoryPath GetApplicationTempDirectoryPath()
	{
		string pathName = Sys.Environment.GetFolderPath( Sys.Environment.SpecialFolder.LocalApplicationData );
		DirectoryPath directoryPath = DirectoryPath.FromAbsolutePath( pathName ).Directory( "Temp" ).Directory( GetProductName() );
		directoryPath.CreateIfNotExist();
		return directoryPath;
	}

	public static DirectoryPath GetTempDirectoryPath()
	{
#pragma warning disable RS0030 // Do not use banned APIs
		string tempPath = SysIo.Path.GetTempPath(); //typically, this is C:\Users\UserName\AppData\Local\Temp
#pragma warning restore RS0030 // Do not use banned APIs
		return DirectoryPath.FromAbsolutePath( tempPath );
	}

	public static FilePath GetTempFilePath()
	{
		// PEARL: System.IO.Path.GetTempFileName() returns a unique filename with a ".tmp" extension, and there is
		//        nothing we can do about that.
		//        We cannot replace the ".tmp" extension with our own nor append our own extension to it, because:
		//        - there would be no guarantees anymore that the filename is unique.
		//        - a zero-length file with the returned filename has already been created.
		// PEARL ON PEARL: The Win32::GetTempFileName() which is used internally to implement this function DOES support
		//        passing the desired extension as a parameter; however, System.IO.Path.GetTempFileName() passes the
		//        hard-coded extension ".tmp" to it.
#pragma warning disable RS0030 // Do not use banned APIs
		string tempFileName = SysIo.Path.GetTempFileName();
#pragma warning restore RS0030 // Do not use banned APIs
		return FilePath.FromAbsolutePath( tempFileName );
	}

	/// <summary>Returns whatever Windows considers to be the "working" directory for the current process.</summary>
	/// <remarks>
	/// PEARL: In windows programming, there is a notion of a "current directory". This notion is entirely misguided.
	///        There should be no such thing. All paths, everywhere, should be absolute.
	///        The only piece of software that should perhaps have a notion of a "current directory" is the command
	///        prompt, but it should be keeping it for itself, meaning that absolutely no application should ever be
	///        aware of what some command prompt considers to be "the current directory".
	///        Unfortunately, that's not how things are.
	///        Under windows, the notion of "the current directory" is very popular.
	///        As such, it has come to be that when doing Windows programming you cannot simply refrain from dealing
	///        with the current directory; there are certain things that absolutely require getting dirty with it.
	///        For example, if you want to load an assembly and execute code in it, you have to set the current
	///        directory to be the same as the directory where the assembly is located, because the code that you
	///        are going to execute expects it to be so.
	///        To understand what is wrong with the notion of a "current directory", consider that it is a mutable global
	///        variable. Having said that, I should not need to add anything else, but here are a couple of facts that
	///        are worth freaking out over:
	///          - All threads within a process share the same "current directory". This means that when you change the
	///            "current directory" in one thread, you are actually changing it for all threads in the current
	///            process. Nice?
	///          - AppDomains are meant to isolate lots of things, but they do not isolate the "current directory": when
	///            you change its value, you are changing it for all AppDomains in the current process. Nice?
	/// </remarks>
	public static DirectoryPath GetWorkingDirectoryPath()
	{
#pragma warning disable RS0030 // Do not use banned APIs
		string pathName = SysIo.Path.GetFullPath( "." );
#pragma warning restore RS0030 // Do not use banned APIs
		return DirectoryPath.FromAbsolutePath( pathName );
	}

	public static byte[] ReadAll( SysIo.Stream self )
	{
		MutableList<byte[]> buffers = new();
		while( true )
		{
			byte[] fixedSizeBuffer = new byte[1024 * 1024];
			int length = self.Read( fixedSizeBuffer, 0, fixedSizeBuffer.Length );
			if( length == 0 )
				break;
			byte[] properSizeBuffer = new byte[length];
			Sys.Array.Copy( fixedSizeBuffer, properSizeBuffer, length );
			buffers.Add( properSizeBuffer );
		}
		switch( buffers.Count )
		{
			case 0:
				return Sys.Array.Empty<byte>();
			case 1:
				return buffers[0];
			default:
			{
				int totalLength = 0;
				foreach( byte[] buffer in buffers )
					totalLength += buffer.Length;
				byte[] result = new byte[totalLength];
				int offset = 0;
				foreach( byte[] buffer in buffers )
				{
					Sys.Array.Copy( buffer, 0, result, offset, buffer.Length );
					offset += buffer.Length;
				}
				return result;
			}
		}
	}

	public static bool ArraysEqual<T>( T[] a, T[] b ) => ListsEqual( a, b );

	public static bool ListsEqual<T>( IList<T> a, IList<T> b )
	{
		if( a.ReferenceEquals( b ) )
			return true;
		int n = Math.Min( a.Count, b.Count );
		for( int i = 0; i < n; i++ )
			if( !EqualityComparer<T>.Default.Equals( a[i], b[i] ) )
				return false;
		if( a.Count != b.Count )
			return false;
		return true;
	}

	public static bool Equal<T>( T a, T b )
	{
		return EqualityComparer<T>.Default.Equals( a, b );
	}

	public static IEnumerable<object?> EnumerableFromLegacy( LegacyCollections.IEnumerable enumerable )
	{
		LegacyCollections.IEnumerator enumerator = enumerable.GetEnumerator();
		try
		{
			while( enumerator.MoveNext() )
			{
				yield return enumerator.Current;
			}
		}
		finally
		{
			(enumerator as Sys.IDisposable)?.Dispose();
		}
	}

	// PEARL: Arrays in C# implement `IEnumerable` but provide no implementation for `Equals()`!
	//        This means that `object.Equals( array1, array2 )` will always return false, even if the arrays have
	//        identical contents! This is especially sinister since `IEnumerable`s are often cast from arrays, so you
	//        may have two instances of `IEnumerable` which yield identical elements and yet `object.Equals()` fails to
	//        return `true` on them.
	//        The standing advice is to use `a.SequenceEqual( b )` to compare `IEnumerable`, but this is retarded, due
	//        to the following reasons:
	//          1. This will only work when you know the exact types of the objects being compared; it might suit
	//             application programmers who are accustomed to writing copious amounts of mindless custom code to
	//             accomplish standard tasks, but it does not work when writing general-purpose code, which must operate
	//             on data without needing to know (nor wanting to know) the exact type of the data.
	//          2. This will not work when the `IEnumerable`s in turn contain other `IEnumerable`s (or arrays) because
	//             guess what `SequenceEqual()` uses internally to compare each pair of elements of the `IEnumerable`s?
	//             It uses `object.Equals()`! So, it miserably fails when comparing `IEnumerable`s of `IEnumerable`!
	//             Again, this might be fine for application programmers who will happily write thousands of lines of
	//             custom code to compare application data having intimate knowledge of the structure of the data, but
	//             it does not work when writing general-purpose code.
	//        This method fixes this insanity. It is meant to be used as a replacement for `object.Equals()` under all
	//        circumstances.
	// TODO: make sure it works under all circumstances. Specifically, make sure it works with generics.
	public static new bool Equals( object? a, object? b )
	{
		if( a.ReferenceEquals( b ) )
			return true;
		if( a == null || b == null )
			return false;
		if( a is LegacyCollections.IEnumerable enumerableA && b is LegacyCollections.IEnumerable enumerableB )
			return legacyEnumerablesEqual( enumerableA, enumerableB );
		return a.Equals( b );

		static bool legacyEnumerablesEqual( LegacyCollections.IEnumerable a, LegacyCollections.IEnumerable b )
		{
			LegacyCollections.IEnumerator enumerator1 = a.GetEnumerator();
			LegacyCollections.IEnumerator enumerator2 = b.GetEnumerator();
			try
			{
				while( enumerator1.MoveNext() )
				{
					if( !enumerator2.MoveNext() )
						return false;
					if( !Equals( enumerator1.Current, enumerator2.Current ) )
						return false;
				}
				if( enumerator2.MoveNext() )
					return false;
				return true;
			}
			finally
			{
				(enumerator1 as Sys.IDisposable)?.Dispose();
				(enumerator2 as Sys.IDisposable)?.Dispose();
			}
		}
	}

	// public static IEnumerable<(object, IReadOnlyList<int>)> Enumerate( Sys.Array array )
	// {
	// 	Assert( array.Rank > 1 );
	// 	IReadOnlyList<IEnumerable<int>> indexRanges       = Enumerable.Range( 0, array.Rank ).Select( array.IndexRange ).ToArray();
	// 	IEnumerable<IReadOnlyList<int>> indexCombinations = EnumerateCombinations( indexRanges );
	// 	return indexCombinations.Select( indices => (array.GetValue( indices.ToArray() ), indices) );
	// }
	//
	// public static IEnumerable<int> IndexRange( Sys.Array array, int dimension ) => Enumerable.Range( 0, array.GetLength( dimension ) - 1 );
	//
	// public static IEnumerable<IReadOnlyList<T>> EnumerateCombinations<T>( IReadOnlyList<IEnumerable<T>> ranges )
	// {
	// 	T[] values = new T[ranges.Count];
	// 	return recurse( 0 );
	//
	// 	IEnumerable<T[]> recurse( int depth )
	// 	{
	// 		foreach( T value in ranges[depth] )
	// 		{
	// 			values[depth] = value;
	// 			if( depth == ranges.Count - 1 )
	// 				yield return values;
	// 			else
	// 				foreach( var result in recurse( depth + 1 ) ) //NOTE: this might be inefficient, consider replacing with RecursiveSelect() below.
	// 					yield return result;
	// 		}
	// 	}
	// }
	//
	// public static IEnumerable<IReadOnlyList<T>> EnumerateCombinations2<T>( IReadOnlyList<IEnumerable<T>> ranges )
	// {
	// 	T[] values = new T[ranges.Count];
	//
	// 	IEnumerable<(T, int)> elements = ranges[0].RecursiveSelect( ( value, depth ) =>
	// 		{
	// 			values[depth] = value;
	// 			return ranges[depth];
	// 		} );
	// 	return elements.Where( ( (T value, int depth) tuple ) => tuple.depth == ranges.Count - 1 ).Select( ( (T value, int depth) tuple ) => values );
	// }
	//
	// public static IEnumerable<(T, int)> RecursiveSelect<T>( T source, Function<IEnumerable<T>, T, int> childSelector ) { return RecursiveSelect( childSelector( source, 0 ), childSelector ); }
	//
	// // From https://stackoverflow.com/a/30441479/773113
	// public static IEnumerable<(T, int)> RecursiveSelect<T>( IEnumerable<T> source, Function<IEnumerable<T>, T, int> childSelector )
	// {
	// 	var                         stack      = new Stack<IEnumerator<T>>();
	// 	IEnumerator<T>? enumerator = source.GetEnumerator();
	// 	try
	// 	{
	// 		while( true )
	// 		{
	// 			if( enumerator.MoveNext() )
	// 			{
	// 				T   element = enumerator.Current;
	// 				int depth   = stack.Count;
	// 				yield return (element, depth);
	// 				stack.Push( enumerator );
	// 				enumerator = childSelector( element, depth ).GetEnumerator();
	// 			}
	// 			else if( stack.Count > 0 )
	// 			{
	// 				enumerator.Dispose();
	// 				enumerator = stack.Pop();
	// 			}
	// 			else
	// 				yield break;
	// 		}
	// 	}
	// 	finally
	// 	{
	// 		enumerator.Dispose();
	// 		while( stack.Count > 0 ) // Clean up in case of an exception.
	// 		{
	// 			enumerator = stack.Pop();
	// 			enumerator.Dispose();
	// 		}
	// 	}
	// }
	//
	// public static void PopulateArray<T>( Sys.Array array, Function<T, IReadOnlyList<int>> valueProducer )
	// {
	// 	void action( T element, IReadOnlyList<int> indexArray )
	// 	{
	// 		T value = valueProducer.Invoke( indexArray );
	// 		array.SetValue( value, indexArray.ToArray() );
	// 	}
	//
	// 	array.ForEach<T>( action );
	// }

	public static int GetJaggedRank( Sys.Array jagged ) => GetJaggedRank( jagged.GetType() );

	public static int GetJaggedRank( Sys.Type type )
	{
		Assert( type.IsArray ); //guaranteed to succeed.
		Assert( type.GetArrayRank() == 1 ); //the array must be jagged, not multi-dimensional.
		for( int i = 1; true; i++ )
		{
			type = type.GetElementType()!;
			if( !type.IsArray || type.GetArrayRank() != 1 )
				return i;
		}
	}

	public static Sys.Type GetJaggedElementType( Sys.Array jagged, int rank ) => GetJaggedElementType( jagged.GetType(), rank );

	public static Sys.Type GetJaggedElementType( Sys.Type type, int rank )
	{
		Assert( type.IsArray ); //guaranteed to succeed.
		Assert( type.GetArrayRank() == 1 ); //the array must be jagged, not multi-dimensional.
		Assert( rank == GetJaggedRank( type ) ); //the given rank must be the correct rank.
		while( true )
		{
			type = type.GetElementType()!;
			rank--;
			if( rank == 0 )
				break;
		}
		return type;
	}

	public static int GetJaggedLength( Sys.Array jagged, int dimension )
	{
		Assert( jagged.Rank == 1 ); //the array must be jagged, not multi-dimensional.
		Assert( dimension >= 0 && dimension < GetJaggedRank( jagged ) ); //the dimension must be between 0 and the rank of the jagged array.
		if( jagged.GetLength( 0 ) > 0 )
			for( ; dimension > 0; dimension-- )
			{
				jagged = (Sys.Array)jagged.GetValue( 0 ).OrThrow();
				Assert( jagged.Rank == 1 ); //nested array must also be a jagged array
			}
		return jagged.GetLength( 0 );
	}

	public static int[] GetJaggedLengths( Sys.Array jagged, int rank )
	{
		Assert( jagged.Rank == 1 ); //the array must be jagged, not multi-dimensional.
		Assert( rank == GetJaggedRank( jagged ) ); //the given rank must be the correct rank.
		int[] lengths = new int[rank];
		for( int dimension = 0; dimension < rank; dimension++ )
			lengths[dimension] = GetJaggedLength( jagged, dimension );
		return lengths;
	}

	public static bool JaggedIsNormalAssertion( Sys.Array jagged )
	{
		int rank = GetJaggedRank( jagged );
		recurse( 0, jagged );
		return true;

		int recurse( int dimension, Sys.Array jagged )
		{
			Assert( jagged.GetLowerBound( 0 ) == 0 );
			int length = jagged.GetLength( 0 );
			if( length == 0 )
				return length;
			if( dimension == rank - 1 )
				return length;
			Sys.Array firstJaggedChild = (Sys.Array)jagged.GetValue( 0 ).OrThrow();
			int firstChildLength = recurse( dimension + 1, firstJaggedChild );
			for( int index = 1; index < length; index++ )
			{
				Sys.Array anotherJaggedChild = (Sys.Array)jagged.GetValue( index ).OrThrow();
				int anotherChildLength = recurse( dimension + 1, anotherJaggedChild );
				Assert( firstChildLength == anotherChildLength );
			}
			return length;
		}
	}

	public static Sys.Array JaggedFromMultiDimensional( Sys.Array multiDimensional )
	{
		Assert( multiDimensional.Rank > 1 ); //this is not a multi-dimensional array
		Assert( IsZeroBasedAssertion( multiDimensional ) ); //this is not a zero-based array
		int[] indices = new int[multiDimensional.Rank];
		return recurse( 0 );

		Sys.Array recurse( int dimension )
		{
			Sys.Type jaggedArrayElementType = makeJaggedArrayElementType( multiDimensional.GetType().GetElementType()!, multiDimensional.Rank - dimension - 1 );
			int length = multiDimensional.GetLength( dimension );
			Sys.Array jagged = Sys.Array.CreateInstance( jaggedArrayElementType, length );
			for( int index = 0; index < length; index++ )
			{
				indices[dimension] = index;
				jagged.SetValue( dimension == multiDimensional.Rank - 1 ? multiDimensional.GetValue( indices ) : recurse( dimension + 1 ), index );
			}
			return jagged;

			static Sys.Type makeJaggedArrayElementType( Sys.Type elementType, int rank )
			{
				Assert( rank >= 0 );
				return rank == 0 ? elementType : makeJaggedArrayElementType( elementType, rank - 1 ).MakeArrayType();
			}
		}
	}

	public static Sys.Array MultiDimensionalFromJagged( Sys.Array jagged )
	{
		Assert( jagged.Rank == 1 ); //this is not a jagged array!
		Assert( GetJaggedRank( jagged ) > 1 ); //jagged array has one dimension, so it is already continuous.
		int rank = GetJaggedRank( jagged );
		Assert( JaggedIsNormalAssertion( jagged ) );
		Sys.Type elementType = GetJaggedElementType( jagged, rank );
		Sys.Array multiDimensional = Sys.Array.CreateInstance( elementType, GetJaggedLengths( jagged, rank ) );
		int[] indices = new int[multiDimensional.Rank];
		recurse( 0, jagged );
		return multiDimensional;

		// ReSharper disable once VariableHidesOuterVariable
		void recurse( int dimension, Sys.Array jagged )
		{
			int length = jagged.GetLength( 0 );
			Assert( length == multiDimensional.GetLength( dimension ) );
			for( int index = 0; index < length; index++ )
			{
				indices[dimension] = index;
				if( dimension == multiDimensional.Rank - 1 )
					multiDimensional.SetValue( jagged.GetValue( index ), indices );
				else
					recurse( dimension + 1, (Sys.Array)jagged.GetValue( index ).OrThrow() );
			}
		}
	}

	public static bool IsZeroBasedAssertion( Sys.Array array )
	{
		int rank = array.Rank;
		for( int dimension = 0; dimension < rank; dimension++ )
			Assert( array.GetLowerBound( dimension ) == 0 ); //non-zero lower bounds are not supported. (They are easy to support, but highly unlikely to ever be used.)
		return true;
	}

	public static void CopyTo<T>( IEnumerable<T> enumerable, T[] array, int arrayIndex )
	{
		foreach( T item in enumerable )
			array[arrayIndex++] = item;
	}

	public static bool IsReferenceTypeOrNullableValueType( Sys.Type type )
	{
		if( !type.IsValueType )
			return true;
		return IsNullableValueType( type );
	}

	public static bool IsNullableValueType( Sys.Type type )
	{
		return type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Sys.Nullable<> );
	}

	public static Sys.Type GetNonNullableValueType( Sys.Type type )
	{
		Assert( IsNullableValueType( type ) );
		return type.GenericTypeArguments[0];
	}

	// PEARL: When trying to step into List<T>.Equals() with the debugger,
	//        there appears to be no source code available, (or not easily available,)
	//        and for some reason Visual Studio refuses to just disassemble it and step into it,
	//        and I cannot even find the type in the object browser,
	//        so in order to find out in exactly what ways two lists are not equal, I have no option but
	//        to implement and use this method so that I can single-step through it.
	// PEARL: There appears to be no built-in method in DotNet for comparing the contents of two arrays.
	//        The List<T>.Equals() method cannot be used, because it stupidly works on List<T>, not on IReadOnlyList<T>.
	//        People on StackOverflow suggest using the SequenceEqual method of linq, but I do not expect this to perform well at all.
	//        Luckily, the following method can also be used to compare the contents of two arrays.
	public static bool ListEquals<T>( IReadOnlyList<T> a, IReadOnlyList<T> b )
	{
		if( !DebugMode )
			return Equals( a, b );
		int n = Math.Min( a.Count, b.Count );
		for( int i = 0; i < n; i++ )
			if( !Equals( a[i], b[i] ) )
				return false; // <-- place breakpoint here.
		return a.Count == b.Count;
	}

	public static void ExecuteAndWait( DirectoryPath workingDirectory, FilePath executable, IEnumerable<string> arguments )
	{
		int exitCode = ExecuteAndWaitForExitCode( workingDirectory, executable, arguments );
		if( exitCode != 0 )
			throw Failure( $"The command \"{executable}\" in '{workingDirectory.Path}' returned exit code {exitCode}" );
	}

	public static int ExecuteAndWaitForExitCode( DirectoryPath workingDirectory, FilePath executable, IEnumerable<string> arguments )
	{
		SysDiag.Process process = Execute( workingDirectory, executable, arguments );
		process.WaitForExit();
		return process.ExitCode;
	}

	public static SysDiag.Process Execute( DirectoryPath workingDirectory, FilePath executable, params string[] arguments )
	{
		return Execute( workingDirectory, executable, EnumerableOf( arguments ) );
	}

	public static SysDiag.Process Execute( DirectoryPath workingDirectory, FilePath executable, IEnumerable<string> arguments )
	{
		SysDiag.Process process = new();
		process.StartInfo.FileName = executable.Path;
		process.StartInfo.Arguments = arguments.MakeString( " " );
		process.StartInfo.UseShellExecute = false; //PEARL: by "shell" here they mean the Windows Explorer, not cmd.exe
		process.StartInfo.WindowStyle = SysDiag.ProcessWindowStyle.Normal;
		process.StartInfo.CreateNoWindow = false;
		process.StartInfo.WorkingDirectory = workingDirectory.Path;
		try
		{
			process.Start();
		}
		catch( Sys.Exception exception )
		{
			throw Failure( $"Failed to execute command '{executable}' in '{workingDirectory}'", exception );
		}
		return process;
	}

	public static SysDiag.Process ShellLaunch( DirectoryPath workingDirectory, FilePath executable, params string[] arguments )
	{
		return ShellLaunch( workingDirectory, executable, EnumerableOf( arguments ) );
	}

	public static SysDiag.Process ShellLaunch( DirectoryPath workingDirectory, FilePath document, IEnumerable<string> arguments )
	{
		SysDiag.Process process = new();
		process.StartInfo.FileName = document.Path;
		process.StartInfo.Arguments = arguments.MakeString( " " );
		process.StartInfo.UseShellExecute = true; //PEARL: by "shell" here they mean the Windows Explorer, not cmd.exe
		process.StartInfo.WindowStyle = SysDiag.ProcessWindowStyle.Normal;
		process.StartInfo.CreateNoWindow = false;
		process.StartInfo.WorkingDirectory = workingDirectory.Path;
		try
		{
			process.Start();
		}
		catch( Sys.Exception exception )
		{
			throw Failure( $"Failed to launch '{document}' in '{workingDirectory}'", exception );
		}
		return process;
	}

	public static bool NamedPipeServerIsListening( string serverName, string pipeName )
	{
		//PEARL: this magical incantation will actually try to connect to the server and then immediately disconnect!
#pragma warning disable RS0030 // Do not use banned APIs
		return SysIo.File.Exists( $@"\\{serverName}\pipe\{pipeName}" ); //see https://stackoverflow.com/a/63739027/773113
#pragma warning restore RS0030 // Do not use banned APIs
	}

	public static E EnumFromUInt<E>( uint value ) where E : struct, Sys.Enum
	{
		int intValue = (int)value;
		E enumValue = (E)(object)intValue; //see https://stackoverflow.com/a/51025027/773113
		Assert( IsValidEnumValueAssertion( enumValue ) );
		return enumValue;
	}

	public static IReadOnlyList<E> GetEnumValues<E>()
	{
		Sys.Type enumType = typeof( E );
		Assert( enumType.IsEnum );
		return (E[])Sys.Enum.GetValues( enumType ); //see https://stackoverflow.com/a/105402/773113
	}

	public static bool IsValidEnumValueAssertion<E>( E enumValue ) where E : struct, Sys.Enum
	{
		Assert( IsValidEnumValue( enumValue ) );
		return true;
	}

	public static bool IsValidEnumValue<E>( E enumValue ) where E : struct, Sys.Enum
	{
		foreach( E value in GetEnumValues<E>() )
			if( Equals( value, enumValue ) )
				return true;
		return true;
	}

	// See https://stackoverflow.com/a/1987721/773113
	// this method will round and then append zeros if needed.
	// i.e. if you round .002 to two significant figures, the resulting number should be .0020.
	public static string ToString( double value, int significantDigits )
	{
		SysGlob.NumberFormatInfo currentInfo = SysGlob.CultureInfo.CurrentCulture.NumberFormat;

		if( double.IsNaN( value ) )
			return currentInfo.NaNSymbol;

		if( double.IsPositiveInfinity( value ) )
			return currentInfo.PositiveInfinitySymbol;

		if( double.IsNegativeInfinity( value ) )
			return currentInfo.NegativeInfinitySymbol;

		double roundedValue = roundSignificantDigits( value, significantDigits, out _ );

		// when rounding causes a cascading round affecting digits of greater significance, 
		// need to re-round to get a correct rounding position afterwards
		// this fixes a bug where rounding 9.96 to 2 figures yields 10.0 instead of 10
		roundSignificantDigits( roundedValue, significantDigits, out int roundingPosition );

		// use exponential notation format
		// ReSharper disable FormatStringProblem
		if( Math.Abs( roundingPosition ) > 9 )
			return string.Format( currentInfo, "{0:E" + (significantDigits - 1) + "}", roundedValue );
		// ReSharper restore FormatStringProblem
		// string.format is only needed with decimal numbers (whole numbers won't need to be padded with zeros to the right.)
		// ReSharper disable FormatStringProblem
		return roundingPosition > 0 ? string.Format( currentInfo, "{0:F" + roundingPosition + "}", roundedValue ) : roundedValue.ToString( currentInfo );
		// ReSharper restore FormatStringProblem
	}

	// this method will return a rounded double value at a number of significant figures.
	// the significantDigits parameter must be between 0 and 15, exclusive.
#pragma warning disable CA1021 // Avoid out parameters
	static double roundSignificantDigits( double value, int significantDigits, out int roundingPosition )
#pragma warning restore CA1021 // Avoid out parameters
	{
		roundingPosition = 0;

		if( DoubleEquals( value, 0d ) )
		{
			roundingPosition = significantDigits - 1;
			return 0d;
		}

		if( double.IsNaN( value ) )
			return double.NaN;

		if( double.IsPositiveInfinity( value ) )
			return double.PositiveInfinity;

		if( double.IsNegativeInfinity( value ) )
			return double.NegativeInfinity;

		Assert( significantDigits is >= 1 and <= 15 );

		// The resulting rounding position will be negative for rounding at whole numbers, and positive for decimal places.
		roundingPosition = significantDigits - 1 - (int)Math.Floor( Math.Log10( Math.Abs( value ) ) );

		// try to use a rounding position directly, if no scale is needed.
		// this is because the scale multiplication after the rounding can introduce error, although
		// this only happens when you're dealing with really tiny numbers, i.e 9.9e-14.
		if( roundingPosition is > 0 and < 16 )
			return Math.Round( value, roundingPosition, Sys.MidpointRounding.AwayFromZero );

		// Shouldn't get here unless we need to scale it.
		// Set the scaling value, for rounding whole numbers or decimals past 15 places
		double scale = Math.Pow( 10, Math.Ceiling( Math.Log10( Math.Abs( value ) ) ) );

		return Math.Round( value / scale, significantDigits, Sys.MidpointRounding.AwayFromZero ) * scale;
	}

	public static double Round( double value, int significantDigits )
	{
		return roundSignificantDigits( value, significantDigits, out _ );
	}

	public static int EnumerableHashCode<T>( IEnumerable<T> enumerable )
	{
		Sys.HashCode hashCode = new();
		foreach( T element in enumerable )
			hashCode.Add( element );
		return hashCode.ToHashCode();
	}

	public static IEnumerable<T> EnumerableFromArray<T>( Sys.Array target )
	{
		foreach( object item in target )
			yield return (T)item;
	}

	public static IEnumerable<T> EnumerableFromArray<T>( T[,] target )
	{
		foreach( T item in target )
			yield return item;
	}

	public static int ArrayHashCode<T>( T[] array ) => EnumerableHashCode( array );
	public static int ArrayHashCode<T>( T[,] array ) => EnumerableHashCode( EnumerableFromArray( array ) );
	public static int ArrayHashCode<T>( Sys.Array array ) => EnumerableHashCode( EnumerableFromArray<T>( array ) );

	public static T TryExceptDefault<T>( Sys.Func<T> function, Sys.Func<T> defaultResult, string logMessageOnError = "" )
	{
		try
		{
			return function.Invoke();
		}
		catch( Sys.Exception exception )
		{
			Log.Debug( string.IsNullOrEmpty( logMessageOnError ) ? $"Returned default due to exception: {exception.Message}" : logMessageOnError );
			return defaultResult.Invoke();
		}
	}

	public static IDictionary<K, V> NewIdentityDictionary<K, V>() where K : class
	{
		return new Dictionary<K, V>( ReferenceEqualityComparer.Instance );
	}

	public static ISet<T> NewIdentityHashSet<T>() where T : class
	{
		return new HashSet<T>( ReferenceEqualityComparer.Instance );
	}

	public static string StripForbiddenPathNameCharacters( string text )
	{
		if( !ContainsForbiddenPathNameCharacters( text ) )
			return text;
		SysText.StringBuilder stringBuilder = new();
		foreach( char c in text )
			if( !IsForbiddenPathNameCharacter( c ) )
				stringBuilder.Append( c );
		return stringBuilder.ToString();
	}

	public static bool ContainsForbiddenPathNameCharacters( string text )
	{
		foreach( char c in text )
			if( IsForbiddenPathNameCharacter( c ) )
				return true;
		return false;
	}

	public static bool IsForbiddenPathNameCharacter( char c )
	{
		if( @"<>:""/\|?*".Contains2( c ) ) //these characters are explicitly forbidden.
			return true;
		if( c < 32 ) //we also forbid control characters.
			return true;
		return false;
	}

	///<summary>Converts a <see cref="Sys.TimeSpan"/> to a number usable by various DotNet functions that require a timeout expressed as an integer number
	/// of milliseconds.</summary>
	///<remarks>If the TimeSpan is equal to MaxValue, then -1 is returned, to select an infinite timeout.</remarks>
	public static int ToMilliseconds( Sys.TimeSpan timeSpan )
	{
		if( timeSpan == Sys.TimeSpan.MaxValue )
			return -1; //infinity
		double milliseconds = timeSpan.TotalMilliseconds;
		Assert( milliseconds is >= 0.0 and < int.MaxValue );
		return (int)milliseconds;
	}

	public static byte[] GetResource( string resourceName, Sys.Type? locatorType )
	{
		SysReflect.Assembly locatorAssembly;
		string fullResourceName;
		if( locatorType == null )
		{
			locatorAssembly = SysReflect.Assembly.GetEntryAssembly().OrThrow();
			fullResourceName = resourceName;
		}
		else
		{
			locatorAssembly = locatorType.Assembly;
			fullResourceName = locatorType.Namespace == null ? resourceName : locatorType.Namespace + "." + resourceName;
		}

		SysIo.Stream? stream = locatorAssembly.GetManifestResourceStream( fullResourceName );
		Assert( stream != null, () => throw Failure( locatorAssembly.GetManifestResourceNames().MakeString( "'", "', '", "'", "none" ) ) );
		using( stream )
			return ReadAll( stream );
	}

	// This method exists here just so that I can search for 'IdentityHashCode' and find out how to do it in C#.
	// This is because I am tired of googling for "identity hash code" and always landing on https://stackoverflow.com/q/53273279/773113
	// The method is not meant to be invoked; the call to SysCompiler.RuntimeHelpers.GetHashCode() is meant to be copied and pasted.
	public static int IdentityHashCode( object obj )
	{
		return SysCompiler.RuntimeHelpers.GetHashCode( obj );
	}

	public static IEnumerable<(bool, string)> ExecuteAndGetOutput( DirectoryPath workingDirectory, string command, IEnumerable<string> input )
	{
		using( SysDiag.Process process = new() )
		{
			process.StartInfo.FileName = "cmd";
			process.StartInfo.Arguments = "/c " + command;
			process.StartInfo.UseShellExecute = false; //PEARL: by "shell" here they mean the Windows Explorer, not cmd.exe
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.WindowStyle = SysDiag.ProcessWindowStyle.Normal;
			process.StartInfo.CreateNoWindow = false;
			process.StartInfo.StandardOutputEncoding = BomlessUtf8;
			process.StartInfo.WorkingDirectory = workingDirectory.Path;
			try
			{
				process.Start();
			}
			catch( Sys.Exception exception )
			{
				throw Failure( $"Failed to execute command '{command}' in '{workingDirectory}'", exception );
			}
			SysThread.Thread thread = new( () =>
			{
				foreach( string line in input )
					process.StandardInput.WriteLine( line );
				process.StandardInput.Close();
			} );
			try
			{
				thread.Start();
				//TODO: make these async!
				// See "How to asynchronously read the standard output stream and standard error stream at once" https://stackoverflow.com/q/12566166/773113
				// See "process.WaitForExit() asynchronously" https://stackoverflow.com/q/470256/773113
				// See ReadLineAsync() and Task.WhenAll()
				while( !process.StandardOutput.EndOfStream )
				{
					string? line = process.StandardOutput.ReadLine();
					yield return (false, line.OrThrow()); // should never be null because we check for end-of-stream
				}
				while( !process.StandardError.EndOfStream )
				{
					string? line = process.StandardError.ReadLine();
					yield return (true, line.OrThrow()); // should never be null because we check for end-of-stream
				}
				process.WaitForExit();
				if( process.ExitCode != 0 )
					throw new ProcessExecutionException( command, workingDirectory, process.ExitCode );
			}
			finally
			{
				thread.Join();
			}
		}
	}

	/// CAUTION: do not use this method unless you really know what you are doing.
#pragma warning disable RS0030 // Do not use banned APIs
	public static Sys.DateTime GetWallClockTimeUtc() => Sys.DateTime.UtcNow;
#pragma warning restore RS0030 // Do not use banned APIs

	/// CAUTION: returns 1 for char.
	/// from Stack Overflow: "Size of generic structure" https://stackoverflow.com/a/18167584/773113
	public static int SizeOf<T>() where T : struct
	{
		return SysInterop.Marshal.SizeOf( default( T ) );
	}
}

public class ProcessExecutionException : SaneException
{
	public string Command { get; }
	public DirectoryPath WorkingDirectory { get; }
	public int ExitCode { get; }

	public ProcessExecutionException( string command, DirectoryPath workingDirectory, int exitCode )
	{
		Command = command;
		WorkingDirectory = workingDirectory;
		ExitCode = exitCode;
	}
}
