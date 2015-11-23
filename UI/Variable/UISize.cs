namespace IROM.UI
{
	using System;
	using IROM.Util;
	
	/// <summary>
	/// A <see cref="UIVariable{T, W}">UIVariable</see> for <see cref="Component"/> sizes.
	/// </summary>
	public class UISize : UIClippedVariable<Vec2D, Vec2D>
	{
		/// <summary>
		/// The Pixel size of this <see cref="UISize"/>.
		/// </summary>
		public Vec2D Pixels
		{
			get
			{
				return Offset;
			}
			set
			{
				Offset = value;
			}
		}
		
		/// <summary>
		/// The parent's <see cref="UISize"/> reference.
		/// </summary>
		public UIVariable<Vec2D, Vec2D> ParentSize
		{
			get
			{
				return GetParent("ParentSize");
			}
			set
			{
				SetParent("ParentSize", value);
			}
		}
		
		/// <summary>
		/// The ratio of parent's size.
		/// </summary>
		public Vec2D Ratio
		{
			get
			{
				return GetWeight("ParentSize");
			}
			set
			{
				SetWeight("ParentSize", value);
			}
		}
		
		public UISize()
		{
			MinValue = new Vec2D(1, 1);
			MaxValue = new Vec2D(double.PositiveInfinity, double.PositiveInfinity);
			//init ratio to 1
			Ratio = new Vec2D(1, 1);
		}
		
		protected override Vec2D Modify(Vec2D currentValue, Vec2D parent, Vec2D weight)
		{
			return currentValue + (parent * weight);
		}
		
		protected override Vec2D Clip(Vec2D value)
		{
			return value.Clip(MinValue, MaxValue);
		}
		
		protected override bool Equals(Vec2D newValue, Vec2D oldValue)
		{
			return newValue == oldValue;
		}
	}
}
