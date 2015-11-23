namespace IROM.UI
{
	using System;
	using IROM.Util;
	
	/// <summary>
	/// Base class for <see cref="UIVariable{T, W}">UIVariables</see> with a min and max value.
	/// </summary>
	public abstract class UIClippedVariable<T, W> : UIVariable<T, W> where T : struct where W : struct
	{
		/// Backing fields
		private T BaseMinValue;
		private T BaseMaxValue;
		
		/// <summary>
		/// The minimum value.
		/// </summary>
		public virtual T MinValue
		{
			get{return BaseMinValue;}
			set
			{
				BaseMinValue = value; 
				Update();
			}
		}
		
		/// <summary>
		/// The maximum value.
		/// </summary>
		public virtual T MaxValue
		{
			get{return BaseMaxValue;}
			set
			{
				BaseMaxValue = value; 
				Update();
			}
		}
		
		protected UIClippedVariable()
		{
			// disable once ConvertClosureToMethodGroup
			Filters.Add(Clip);
		}
		
		/// <summary>
		/// Clips the given value to between minValue and maxValue. Allows custom clipping implementation.
		/// </summary>
		/// <param name="value">The value to clip.</param>
		/// <returns>The clipped value.</returns>
		protected virtual T Clip(T value)
		{
			return Util.Clip(Value, MinValue, MaxValue);
		}
	}
}
