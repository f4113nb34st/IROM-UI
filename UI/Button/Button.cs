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
		public readonly Panel Content;
		
		/// <summary>
		/// The <see cref="Interpolation"/> method used for the edges.
		/// </summary>
		public readonly Dynx<InterpFunction> EdgeInterpolation = new Dynx<InterpFunction>();
		
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
		
		public Button()
		{
			//init border to 5%
			Border.Exp = () => Size.Value * .05;
			Border.OnUpdate += MarkDirty;
			
			ForeColor.Value = RGB.White;
			ForeColor.OnUpdate += MarkDirty;
			
			BackColor.Value = RGB.Black;
			BackColor.OnUpdate += MarkDirty;
			
			//init hover color to average of fore and back
			HoverColor.Exp = () => (ForeColor.Value / 2) + (BackColor.Value / 2);
			HoverColor.OnUpdate += MarkDirty;
			
			EdgeInterpolation.Value = Interpolation.Linear;
			EdgeInterpolation.OnUpdate += MarkDirty;
			
			//create content panel
			Content = new Panel();
			Content.Parent.Value = this;
			//init color
			Content.Color.Exp = () => ForeColor.Value;
			//center in button
			Content.Position.Exp = () =>
			{
				Vec2D vec = Position.Value;
				vec += Size.Value / 2;
				vec -= Content.Size.Value / 2;
				return vec;
			};
			//reduce size by twice border
			Content.Size.Exp = () =>
			{
				Vec2D vec = Size.Value;
				vec -= Border.Value * 2;
				return vec;
			};
			//slightly offset z coord
			Content.ZCoord.Exp = () => ZCoord.Value + .001;
			Content.Hidden.Value = true;
			
			//re-render when content color changes
			Content.Color.OnUpdate += MarkDirty;
			
			OnMouseEnter += () => 
			{
				if(!DisableHoverColor) 
				{
					Hovered = true;
					Content.Color.Exp = () => HoverColor.Value;
				}
			};
			OnMouseExit += () => 
			{
				if(!DisableHoverColor) 
				{
					Hovered = false;
					Content.Color.Exp = () => ForeColor.Value;
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
			Point2D border = (Point2D)Border.Value;
			ARGB color = Content.Color.Value;
			
			if(border.X > 0 && border.Y > 0)
			{
				//for top left corner
				FillCorner(image, false, false, 0, 0, border.X, border.Y, border.X, border.Y, color);
				
				//for top right corner
				FillCorner(image, true, false, width - border.X, 0, width, border.Y, border.X, border.Y, color);
				
				//for bottom left corner
				FillCorner(image, false, true, 0, height - border.X, border.X, height, border.X, border.Y, color);
				
				//for bottom right corner
				FillCorner(image, true, true, width - border.X, height - border.X, width, height, border.X, border.Y, color);
			}
			if(border.X > 0)
			{
				//for left side
				FillXSide(image, false, 0, border.Y, border.X, height - border.Y, border.X, color);
						
				//for right side
				FillXSide(image, true, width - border.X, border.Y, width, height - border.Y, border.X, color);
			}
			if(border.Y > 0)
			{
				//for top side
				FillYSide(image, false, border.X, 0, width - border.X, border.Y, border.Y, color);
				
				//for bottom side
				FillYSide(image, true, border.X, height - border.Y, width - border.X, height, border.Y, color);
			}
			
			//for center
			image.FillRectangle(new Rectangle{Position = border, Size = image.Size - (border * 2)}, color);
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
				image.FillYScan(i, minY, maxY, ColorUtil.Interpolate(color, BackColor.Value, mu, EdgeInterpolation.Value));
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
				image.FillXScan(minX, maxX, j, ColorUtil.Interpolate(color, BackColor.Value, mu, EdgeInterpolation.Value));
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
					
					image[i, j] = ColorUtil.Interpolate(color, BackColor.Value, mu, EdgeInterpolation.Value);
				}
			}
		}
	}
}
