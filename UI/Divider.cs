namespace IROM.UI
{
	using System;
	using IROM.Util;
	
	/// <summary>
	/// A divider is a thin panel that provides a colored border between <see cref="Component"/>s.
	/// </summary>
	public class Divider : Panel
	{
		/// <summary>
		/// True if vertical, false if horizontal.
		/// </summary>
		public bool IsVertical;
			
		//the backing variable classes
		private UIColor minColor;
		private UIColor maxColor;
		
		/// <summary>
		/// The <see cref="Interpolation"/> method used for the edges.
		/// </summary>
		public Interpolation.InterpFunction EdgeInterpolation = Interpolation.Linear;
		
		/// <summary>
		/// Gets or sets the left/top color of this <see cref="Component"/>.
		/// </summary>
		public UIColor MinColor
		{
			get
			{
				return minColor;
			}
			set
			{
				if(minColor != value)
				{
					minColor = value;
					if(OnMinColorChange != null) OnMinColorChange(this, minColor);
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the right/bottom color of this <see cref="Component"/>.
		/// </summary>
		public UIColor MaxColor
		{
			get
			{
				return maxColor;
			}
			set
			{
				if(maxColor != value)
				{
					maxColor = value;
					if(OnMaxColorChange != null) OnMaxColorChange(this, maxColor);
				}
			}
		}
		
		/// <summary>
		/// Invoked whenever <see cref="MinColor"/> changes.
		/// </summary>
		public event EventHandler<UIColor> OnMinColorChange;
		
		/// <summary>
		/// Invoked whenever <see cref="MaxColor"/> changes.
		/// </summary>
		public event EventHandler<UIColor> OnMaxColorChange;
		
		public Divider(Component parent) : this(parent, false)
		{
			
		}
		
		public Divider(Component parent, bool bypass) : base(parent, bypass)
		{
			MinColor = new UIColor(this);
			MinColor.OnChange += MarkMasterDirty;
			MaxColor = new UIColor(this);
			MaxColor.OnChange += MarkMasterDirty;
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
