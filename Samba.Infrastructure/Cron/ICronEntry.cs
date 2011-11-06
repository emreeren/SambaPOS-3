using System.Collections.Generic;

namespace Samba.Infrastructure.Cron
{
	public interface ICronEntry
	{
		List<int> Values { get; }
		string Expression { get; }
		int MinValue { get; }
		int MaxValue { get; }

		/// <summary>
		/// Gets the first value.
		/// </summary>
		/// <value>The first.</value>
		int First { get; }

		/// <summary>
		/// Nexts the specified value.
		/// </summary>
		/// <param name="start">The start.</param>
		/// <returns></returns>
		int Next(int start);
	}
}