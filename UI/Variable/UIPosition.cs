namespace IROM.UI
{
	using System;
	using IROM.Util;
	
	/// <summary>
	/// A <see cref="UIVariable{T, W}">UIVariable</see> for <see cref="Component"/> positions.
	/// </summary>
	public class UIPosition : UIClippedVariable<Vec2D, Vec2D>
	{
		/// <summary>
		/// The Pixel location of this <see cref="UISize"/>.
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
		/// The parent's <see cref="UIPosition"/> reference.
		/// </summary>
		public UIVariable<Vec2D, Vec2D> ParentPos
		{
			get
			{
				return GetParent("ParentPos");
			}
			set
			{
				SetParent("ParentPos", value);
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
		/// Our own <see cref="UISize"/> reference.
		/// </summary>
		public UIVariable<Vec2D, Vec2D> OwnSize
		{
			get
			{
				return GetParent("OwnSize");
			}
			set
			{
				SetParent("OwnSize", value);
			}
		}
		
		/// <summary>
		/// Ratio of parent's <see cref="UIPosition"/>.
		/// </summary>
		public Vec2D RatioPos
		{
			get
			{
				return GetWeight("ParentPos");
			}
			set
			{
				SetWeight("ParentPos", value);
			}
		}
		
		/// <summary>
		/// Ratio of parent's <see cref="UISize"/>.
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
		
		/// <summary>
		/// Ratio of our own <see cref="UISize"/>.
		/// </summary>
		public Vec2D RatioOwn
		{
			get
			{
				return GetWeight("OwnSize");
			}
			set
			{
				SetWeight("OwnSize", value);
			}
		}
		
		public UIPosition()
		{
			MinValue = new Vec2D(double.NegativeInfinity, double.NegativeInfinity);
			MaxValue = new Vec2D(double.PositiveInfinity, double.PositiveInfinity);
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
