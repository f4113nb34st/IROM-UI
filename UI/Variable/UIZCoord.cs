namespace IROM.UI
{
	using System;
	using IROM.Util;
	
	/// <summary>
	/// A <see cref="UIVariable{T, W}">UIVariable</see> for <see cref="Component"/> z coordinates.
	/// </summary>
	public class UIZCoord : UIClippedVariable<double, bool>
	{
		/// <summary>
		/// The parent's <see cref="UIZCoord"/> reference.
		/// </summary>
		public UIVariable<double, bool> ParentZ
		{
			get
			{
				return GetParent("ParentZ");
			}
			set
			{
				SetParent("ParentZ", value);
			}
		}
		
		public UIZCoord()
		{
			MinValue = double.NegativeInfinity;
			MaxValue = double.PositiveInfinity;
			//init offset to 1
			Offset = 1;
		}
		
		public override bool GetWeight(string tag)
		{
			throw new NotSupportedException("UIZCoords do not support weights.");
		}
		
		public override void SetWeight(string tag, bool weight)
		{
			throw new NotSupportedException("UIZCoords do not support weights.");
		}
		
		protected override double Modify(double currentValue, double parent, bool weight)
		{
			return currentValue + parent;
		}
		
		protected override double Clip(double value)
		{
			return Util.Clip(value, MinValue, MaxValue);
		}
		
		protected override bool Equals(double newValue, double oldValue)
		{
			return newValue == oldValue;
		}
	}
}
