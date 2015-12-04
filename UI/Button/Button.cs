namespace IROM.UI
{
	using System;
	using IROM.Util;
	
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
		
		//the backing variable classes
		private UISize border;
		private UIColor foreColor;
		private UIColor backColor;
		private UIColor hoverColor;
		
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
		
		/// <summary>
		/// Gets or sets the border size of this <see cref="Component"/>.
		/// </summary>
		public UISize Border
		{
			get
			{
				return border;
			}
			set
			{
				if(border != value)
				{
					border = value;
					if(OnBorderChange != null) OnBorderChange(this, border);
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the fore color of this <see cref="Component"/>.
		/// </summary>
		public UIColor ForeColor
		{
			get
			{
				return foreColor;
			}
			set
			{
				if(foreColor != value)
				{
					foreColor = value;
					if(OnForeColorChange != null) OnForeColorChange(this, foreColor);
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the back color of this <see cref="Component"/>.
		/// </summary>
		public UIColor BackColor
		{
			get
			{
				return backColor;
			}
			set
			{
				if(backColor != value)
				{
					backColor = value;
					if(OnBackColorChange != null) OnBackColorChange(this, backColor);
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the hover color of this <see cref="Component"/>.
		/// </summary>
		public UIColor HoverColor
		{
			get
			{
				return hoverColor;
			}
			set
			{
				if(hoverColor != value)
				{
					hoverColor = value;
					if(OnHoverColorChange != null) OnHoverColorChange(this, hoverColor);
				}
			}
		}
		
		/// <summary>
		/// Invoked whenever <see cref="Border"/> changes.
		/// </summary>
		public event EventHandler<UISize> OnBorderChange;
		
		/// <summary>
		/// Invoked whenever <see cref="ForeColor"/> changes.
		/// </summary>
		public event EventHandler<UIColor> OnForeColorChange;
		
		/// <summary>
		/// Invoked whenever <see cref="BackColor"/> changes.
		/// </summary>
		public event EventHandler<UIColor> OnBackColorChange;
		
		/// <summary>
		/// Invoked whenever <see cref="HoverColor"/> changes.
		/// </summary>
		public event EventHandler<UIColor> OnHoverColorChange;
		
		public Button(Component parent) : this(parent, false)
		{
			
		}
		
		public Button(Component parent, bool bypass) : base(parent, bypass)
		{
			Border = new UISize();
			Border.OnChange += MarkMasterDirty;
			ForeColor = new UIColor(this);
			ForeColor.OnChange += MarkMasterDirty;
			BackColor = new UIColor(this);
			BackColor.OnChange += MarkMasterDirty;
			HoverColor = new UIColor(this);
			HoverColor.OnChange += MarkMasterDirty;
			
			//init border to 5%
			Border.ParentSize = Size;
			Border.Ratio = new Vec2D(.05, .05);
			//init hover color to average of fore and back
			HoverColor.SetParent("ForeColor", ForeColor, .5);
			HoverColor.SetParent("BackColor", BackColor, .5);
			
			Content = new Panel(this, true);
			//init color
			Content.Color.SetParent("Parent", ForeColor, 1);
			//center in button
			Content.Position.Ratio = new Vec2D(.5, .5);
			Content.Position.RatioOwn = new Vec2D(-.5, -.5);
			//slightly offset z coord
			Content.ZCoord.Offset = .001;
			//reduce size by twice border
			Content.Size.SetParent("Border", Border, new Vec2D(-2, -2));
			//re-render content on fore color change
			ForeColor.OnChange += ((sender, args) => Content.Dirty = true);
			Content.Hidden = true;
			
			//exit when color changes
			OnMouseEnter += (sender, e) => 
			{
				if(!DisableHoverColor) 
				{
					Hovered = true;
					Content.Color.SetParent("Parent", HoverColor);
					Dirty = true;
				}
			};
			OnMouseExit += (sender, e) => 
			{
				if(!DisableHoverColor) 
				{
					Hovered = false;
					Content.Color.SetParent("Parent", ForeColor);
					Dirty = true;
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
