namespace IROM.UI
{
	using System;
	using IROM.Util;
	using IROM.Dynamix;
	
	/// <summary>
	/// A Button is a component that can be pressed.
	/// </summary>
	public class Button : Component
	{
		/// <summary>
		/// The panel for the contents of this button.
		/// </summary>
		public Panel Content;
		
		/// <summary>
		/// The <see cref="Interpolation"/> method used for the edges.
		/// </summary>
		public Interpolation.InterpFunction EdgeInterpolation = Interpolation.Linear;
		
		/// <summary>
		/// The border size of this <see cref="Component"/>.
		/// </summary>
		public readonly Dynx<Vec2D> Border = new Dynx<Vec2D>();
		
		/// <summary>
		/// The fore color of this <see cref="Component"/>.
		/// </summary>
		public readonly Dynx<ARGB> ForeColor = new Dynx<ARGB>();
		
		/// <summary>
		/// The back color of this <see cref="Component"/>.
		/// </summary>
		public readonly Dynx<ARGB> BackColor = new Dynx<ARGB>();
		
		/// <summary>
		/// The hover color of this <see cref="Component"/>.
		/// </summary>
		public readonly Dynx<ARGB> HoverColor = new Dynx<ARGB>();
		
		/// <summary>
		/// True if the edges should be rounded, not square.
		/// </summary>
		public bool RoundEdges = true;
		
		/// <summary>
		/// True to disable the color change on hovering.
		/// </summary>
		public bool DisableHoverColor = false;
		
		/// <summary>
		/// True if this <see cref="Button"/> is hovered over.
		/// </summary>
		public bool Hovered
		{
			get;
			protected set;
		}
		
		public Button(Component parent) : this(parent, false)
		{
			
		}
		
		public Button(Component parent, bool bypass) : base(parent, bypass)
		{
			//init border to 5%
			Border.Exp = () => Size.Value * .05;
			
			ForeColor.Value = RGB.White;
			BackColor.Value = RGB.Black;
			
			//init hover color to average of fore and back
			HoverColor.Exp = () => (ForeColor.Value / 2) + (BackColor.Value / 2);
			
			Content = new Panel(this, true);
			//init color
			Content.Color.Exp = () => ForeColor;
			//center in button
			Content.Position += (Size - Content.Size) / 2;
			//slightly offset z coord
			Content.ZCoord += .001;
			//reduce size by twice border
			Content.Size -= Border * 2;
			
			Content.Hidden = true;
			
			Border.Subscribe(MarkDirty);
			ForeColor.Subscribe(MarkDirty);
			BackColor.Subscribe(MarkDirty);
			HoverColor.Subscribe(MarkDirty);
			//re-render content on fore color change
			ForeColor.Subscribe(Content.MarkDirty);
			
			//exit when color changes
			OnMouseEnter += (sender, e) => 
			{
				if(!DisableHoverColor) 
				{
					Hovered = true;
					Content.Color = HoverColor;
				}
			};
			OnMouseExit += (sender, e) => 
			{
				if(!DisableHoverColor) 
				{
					Hovered = false;
					Content.Color = ForeColor;
				}
			};
		}
		
		protected override Component GetParentFor(Component child)
		{
			return Content;
		}
		
		protected override void Render(Image image)
		{
			int width = image.Width;
			int height = image.Height;
			int xBorder = (int)Border.Value.X;
			int yBorder = (int)Border.Value.Y;
			ARGB color = Hovered ? HoverColor.Value : ForeColor.Value;
			
			if(xBorder > 0 && yBorder > 0)
			{
				//for top left corner
				FillCorner(image, false, false, 0, 0, xBorder, yBorder, xBorder, yBorder, color);
				
				//for top right corner
				FillCorner(image, true, false, width - xBorder, 0, width, yBorder, xBorder, yBorder, color);
				
				//for bottom left corner
				FillCorner(image, false, true, 0, height - xBorder, xBorder, height, xBorder, yBorder, color);
				
				//for bottom right corner
				FillCorner(image, true, true, width - xBorder, height - xBorder, width, height, xBorder, yBorder, color);
			}
			if(xBorder > 0)
			{
				//for left side
				FillXSide(image, false, 0, yBorder, xBorder, height - yBorder, xBorder, color);
						
				//for right side
				FillXSide(image, true, width - xBorder, yBorder, width, height - yBorder, xBorder, color);
			}
			if(yBorder > 0)
			{
				//for top side
				FillYSide(image, false, xBorder, 0, width - xBorder, yBorder, yBorder, color);
				
				//for bottom side
				FillYSide(image, true, xBorder, height - yBorder, width - xBorder, height, yBorder, color);
			}
			
			//for center
			image.FillRectangle(new Rectangle(xBorder, yBorder, width - (xBorder * 2), height - (yBorder * 2)), color);
		}
		
		private void FillXSide(Image image, bool top, int minX, int minY, int maxX, int maxY, int xBorder, ARGB color)
		{
			for(int i = minX; i < maxX; i++)
			{
				double mu = (maxX - i) / (double)xBorder;
				if(top) 
				{
					mu = 1 - mu;
				}
				mu = Util.Clip(mu, 0, 1);
				image.FillYScan(i, minY, maxY, ColorUtil.Interpolate(color, BackColor.Value, mu, EdgeInterpolation));
			}
		}
		
		private void FillYSide(Image image, bool top, int minX, int minY, int maxX, int maxY, int yBorder, ARGB color)
		{
			for(int j = minY; j < maxY; j++)
			{
				double mu = (maxY - j) / (double)yBorder;
				if(top) 
				{
					mu = 1 - mu;
				}
				mu = Util.Clip(mu, 0, 1);
				image.FillXScan(minX, maxX, j, ColorUtil.Interpolate(color, BackColor.Value, mu, EdgeInterpolation));
			}
		}
		
		private void FillCorner(Image image, bool topX, bool topY, int minX, int minY, int maxX, int maxY, int xBorder, int yBorder, ARGB color)
		{
			for(int i = minX; i < maxX; i++)
			{
				for(int j = minY; j < maxY; j++)
				{
					double dx = (maxX - i) / (double)xBorder;
					if(topX) dx = 1 - dx;
					double dy = (maxY - j) / (double)yBorder;
					if(topY) dy = 1 - dy;
					double mu;
					if(RoundEdges)
					{
						mu = Math.Sqrt(dx * dx + dy * dy);
					}else
					{
						mu = Math.Max(Math.Abs(dx), Math.Abs(dy));
					}
					bool unused = mu > 1;
					mu = Util.Clip(mu, 0, 1);
					
					image[i, j] = ColorUtil.Interpolate(color, BackColor.Value, mu, EdgeInterpolation);
				}
			}
		}
	}
}
