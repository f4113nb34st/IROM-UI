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
		/// True if vertical, false if horizontal.
		/// </summary>
		public bool IsVertical;
			
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
		public Interpolation.InterpFunction EdgeInterpolation = Interpolation.Linear;
		
		public Divider(Component parent) : this(parent, false)
		{
			
		}
		
		public Divider(Component parent, bool bypass) : base(parent, bypass)
		{
			MinColor.Value = RGB.White;
			MaxColor.Value = RGB.White;
			MinColor.Subscribe(MarkDirty);
			MaxColor.Subscribe(MarkDirty);
		}
		
		protected override void Render(Image image)
		{
			if(IsVertical)
			{
				double scale = image.Width / 2D;
				int maxX = image.Width - 1;
				//perform x walk
				for(int x = 0; x < image.Width; x++)
				{
					double mu;
					ARGB color;
					//if min side
					if(x < (maxX - x))
					{
						mu = x / scale;
						color = MinColor.Value;
					}else//if max side
					{
						mu = (maxX - x) / scale;
						color = MaxColor.Value;
					}
					//interpolate
					color = ColorUtil.Interpolate(color, Color.Value, mu, EdgeInterpolation);
					//fill vert scan
					for(int y = 0; y < image.Height; y++)
					{
						image[x, y] = color;
					}
				}
			}else
			{
				double scale = image.Height / 2D;
				int maxY = image.Height - 1;
				//perform y walk
				for(int y = 0; y < image.Height; y++)
				{
					double mu;
					ARGB color;
					//if min side
					if(y < (maxY - y))
					{
						mu = y / scale;
						color = MinColor.Value;
					}else//if max side
					{
						mu = (maxY - y) / scale;
						color = MaxColor.Value;
					}
					//interpolate
					color = ColorUtil.Interpolate(color, Color.Value, mu, EdgeInterpolation);
					//fill hori scan
					for(int x = 0; x < image.Width; x++)
					{
						image[x, y] = color;
					}
				}
			}
		}
	}
}
