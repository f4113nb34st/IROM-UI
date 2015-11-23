namespace IROM.UI
{
	using System;
	using IROM.Util;
	
	/// <summary>
	/// A screen is a reusable base component for the gui system.
	/// </summary>
	public class Screen : Panel
	{
		public Screen() : base(null)
		{
			//position and z are 0
			Position.Pixels = Vec2D.Zero;
			Size.MinValue = new Vec2D(1, 1);
			ZCoord.Offset = 0;
		}
	}
}
