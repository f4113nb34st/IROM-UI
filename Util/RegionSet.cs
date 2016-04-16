namespace IROM.UI
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using IROM.Util;
	
	/// <summary>
	/// A simple thread-unsafe collection of rectangles. 
	/// Automatically replaces any regions that overlap with their encompassing rectangle.
	/// Enumeration over the set also clear it.
	/// </summary>
	public class RegionSet : IEnumerable<Rectangle>
	{
		private FastLinkedList<Rectangle> regions = new FastLinkedList<Rectangle>();
		
		/// <summary>
		/// Adds the given region to the set.
		/// </summary>
		/// <param name="rect">The region to add.</param>
		public void Add(Rectangle rect)
		{
			foreach(DoubleNode<Rectangle> node in regions.GetNodes())
			{
				if(ShapeUtil.Overlap(rect, node.Value).IsValid())
				{
					node.Value = ShapeUtil.Encompass(rect, node.Value);
					return;
				}
			}
			regions.Add(rect);
		}
		
		/// <summary>
		/// Clears all values from the set.
		/// </summary>
		public void Clear()
		{
			regions.Clear();
		}

		public IEnumerator<Rectangle> GetEnumerator()
		{
			return Interlocked.Exchange(ref regions, new FastLinkedList<Rectangle>()).GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
