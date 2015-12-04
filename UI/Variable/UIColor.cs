namespace IROM.UI
{
	using System;
	using IROM.Util;
	
	/// <summary>
	/// A <see cref="UIVariable{T, W}">UIVariable</see> for <see cref="Component"/> colors.
	/// </summary>
	public class UIColor : UIClippedVariable<ARGB, double>
	{
		/// <summary>
		/// The base color of this <see cref="UIColor"/>.
		/// </summary>
		public ARGB BaseColor
		{
			get{return Offset;}
			set{Offset = value;}
		}
		
		public UIColor(Component component)
		{
			MinValue = new ARGB(0, 0, 0, 0);
			MaxValue = new ARGB(255, 255, 255, 255);
			//re-render parent on change
			OnChange += ((sender, args) => component.Dirty = true);
		}
		
		protected override ARGB Modify(ARGB currentValue, ARGB parent, double weight)
		{
			return currentValue + (parent * weight);
		}
		
		protected override ARGB Clip(ARGB value)
		{
			return value.Clip(MinValue, MaxValue);
		}
		
		protected override bool Equals(ARGB newValue, ARGB oldValue)
		{
			return newValue == oldValue;
		}
	}
}
