using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Utility;

public class Pool<T>(Func<T> create, Func<T, bool> isReleased) where T : class
{
	readonly List<T> pool = [];
	public T GetReleased()
	{
		T val = pool.FirstOrDefault(isReleased);
		if (val != null) return val;

		val = create();
		pool.Add(val);
		return val;
	}
}
