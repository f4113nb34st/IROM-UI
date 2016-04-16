namespace IROM.UI
{
	using System;
	using IROM.Util;
	using IROM.Dynamix;
	
	/// <summary>
	/// A divider is a thin panel that provides a colored border between <see cref="Component"/>s.
	/// </summary>
	public class Divider : Panel
	{
		/// <summary>
		/// The left/top color of this <see cref="Divider"/>.
		/// </summary>
		public readonly Dynx<ARGB> MinColor = new Dynx<ARGB>();
		
		/// <summary>
		/// The right/bottom color of this <see cref="Divider"/>.
		/// </summary>
		public readonly Dynx<ARGB> MaxColor = new Dynx<ARGB>();
		
		/// <summary>
		/// The <see cref="Interpolation"/> method used for the edges.
		/// </summary>
		public readonly Dynx<InterpFunction> EdgeInterpolation = new Dynx<InterpFunction>();
		
		public Divider()
		{
			MinColor.Value = RGB.White;
			MinColor.OnUpdate += MarkDirty;
			FlushBeforeUpdate(MinColor);
			
			MaxColor.Value = RGB.White;
			MaxColor.OnUpdate += MarkDirty;
			FlushBeforeUpdate(MaxColor);
			
			EdgeInterpolation.Value = Interpolation.Linear;
			EdgeInterpolation.OnUpdate += MarkDirty;
			FlushBeforeUpdate(EdgeInterpolation);
		}
		
		protected override void Render(Image image)
		{
			RenderUtil.RenderDivider(image, Bounds, MinColor.Value, Color.Value, MaxColor.Value, EdgeInterpolation.Value);
		}
	}
}
