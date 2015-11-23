namespace IROM.UI
{
	using System;
	using IROM.Util;
	
	/// <summary>
	/// A <see cref="UIVariable{T, W}">UIVariable</see> for <see cref="Component"/> colors.
	/// </summary>
	public class UIColor : UIClippedVariable<RGB, double>
	{
		/// <summary>
		/// The base color of this <see cref="UIColor"/>.
		/// </summary>
		public RGB BaseColor
		{
			get{return Offset;}
			set{Offset = value;}
		}
		
		public UIColor(Component component)
		{
			MinValue = new RGB(0, 0, 0);
			MaxValue = new RGB(255, 255, 255);
			//re-render parent on change
			OnChange += ((sender, args) => component.Dirty = true);
		}
		
		protected override RGB Modify(RGB currentValue, RGB parent, double weight)
		{
			return currentValue + (parent * weight);
		}
		
		protected override RGB Clip(RGB value)
		{
			return value.Clip(MinValue, MaxValue);
		}
		
		protected override bool Equals(RGB newValue, RGB oldValue)
		{
			return newValue == oldValue;
		}
	}
}
