namespace IROM.UI
{
	using System;
	using IROM.Util;
	
	/// <summary>
	/// A <see cref="UIVariable{T, W}">UIVariable</see> for <see cref="Component"/> clipping windows.
	/// </summary>
	public class UIClip : UIVariable<Rectangle, bool>
	{
		/// <summary>
		/// The parent's <see cref="UIClip"/> reference.
		/// </summary>
		public UIVariable<Rectangle, bool> ParentClip
		{
			get
			{
				return GetParent("ParentClip");
			}
			set
			{
				SetParent("ParentClip", value);
			}
		}
		
		/// <summary>
		/// The x and y minimum clip values.
		/// </summary>
		public Point2D Min
		{
			get
			{
				return Offset.Min;
			}
			set
			{
				Rectangle clip = Offset;
				clip.Min = value;
				Offset = clip;
			}
		}
		
		/// <summary>
		/// The x and y maximum clip values.
		/// </summary>
		public Point2D Max
		{
			get
			{
				return Offset.Max;
			}
			set
			{
				Rectangle clip = Offset;
				clip.Max = value;
				Offset = clip;
			}
		}
		
		public UIClip()
		{
			Min = new Point2D(int.MinValue, int.MinValue);
			Max = new Point2D(int.MaxValue, int.MaxValue);
		}
		
		public override bool GetWeight(string tag)
		{
			throw new NotSupportedException("UIClips do not support weights.");
		}
		
		public override void SetWeight(string tag, bool weight)
		{
			throw new NotSupportedException("UIClips do not support weights.");
		}
		
		protected override Rectangle Modify(Rectangle currentValue, Rectangle parent, bool weight)
		{
			return VectorUtil.Overlap(currentValue, parent);
		}
		
		protected override bool Equals(Rectangle newValue, Rectangle oldValue)
		{
			return newValue == oldValue;
		}
	}
}
