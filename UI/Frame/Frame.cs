namespace IROM.UI
{
	using System;
	using IROM.Util;
	using IROM.Dynamix;
	
	/// <summary>
	/// A Frame is a <see cref="Component"/> that can be moved, resized, and closed.
	/// Often used to organize other <see cref="Component"/>s.
	/// </summary>
	public class Frame : Component
	{
		/// <summary>
		/// The panel for the contents of this <see cref="Frame"/>.
		/// </summary>
		public readonly Panel Content;
		
		/// <summary>
		/// The panel for the contents of the title bar of this <see cref="Frame"/>
		/// </summary>
		public readonly Panel TitlePanel;
		
		/// <summary>
		/// The <see cref="Interpolation"/> method used for the edges.
		/// </summary>
		public readonly Dynx<InterpFunction> EdgeInterpolation = new Dynx<InterpFunction>();
		
		/// <summary>
		/// The border size of this <see cref="Component"/>.
		/// </summary>
		public readonly Dynx<Point2D> Border = new Dynx<Point2D>();
		
		/// <summary>
		/// The height of the title bar.
		/// </summary>
		public readonly Dynx<int> TitleHeight = new Dynx<int>();
		
		/// <summary>
		/// The fore color of this <see cref="Component"/>.
		/// </summary>
		public readonly Dynx<ARGB> ForeColor = new Dynx<ARGB>();
		
		/// <summary>
		/// The border color of this <see cref="Component"/>.
		/// </summary>
		public readonly Dynx<ARGB> BorderColor = new Dynx<ARGB>();
		
		/// <summary>
		/// The back color of this <see cref="Component"/>.
		/// </summary>
		public readonly Dynx<ARGB> BackColor = new Dynx<ARGB>();
		
		/// <summary>
		/// True if the edges should be rounded, not square.
		/// </summary>
		public readonly Dynx<bool> RoundEdges = new Dynx<bool>();
		
		/// <summary>
		/// True if the title bar should not be drawn.
		/// </summary>
		public readonly Dynx<bool> DisableTitle = new Dynx<bool>();
		
		/// <summary>
		/// True if the north edge should not be used.
		/// </summary>
		public readonly Dynx<bool> DisableNorthEdge = new Dynx<bool>();
		
		/// <summary>
		/// True if the south edge should not be used.
		/// </summary>
		public readonly Dynx<bool> DisableSouthEdge = new Dynx<bool>();
		
		/// <summary>
		/// True if the west edge should not be used.
		/// </summary>
		public readonly Dynx<bool> DisableWestEdge = new Dynx<bool>();
		
		/// <summary>
		/// True if the east edge should not be used.
		/// </summary>
		public readonly Dynx<bool> DisableEastEdge = new Dynx<bool>();
		
		/// <summary>
		/// Sizing multiplier for the resize border input handlers.
		/// Used for making the borders easier to use.
		/// </summary>
		public readonly Dynx<double> ResizeBorderMulti = new Dynx<double>();
		
		/// <summary>
		/// True if this <see cref="Frame"/> can not be resized.
		/// </summary>
		public readonly Dynx<bool> DisableResize = new Dynx<bool>();
		
		/// <summary>
		/// True if this <see cref="Frame"/> can not be moved.
		/// </summary>
		public readonly Dynx<bool> DisableMove = new Dynx<bool>();
		
		/// <summary>
		/// Invoked when this frame is resized.
		/// </summary>
		public event Action OnResize;
		
		/// <summary>
		/// Invoked when this frame moves.
		/// </summary>
		public event Action OnMove;
		
		//the sizes of each part
		protected readonly Dynx<int> northSize = new Dynx<int>();
		protected readonly Dynx<int> southSize = new Dynx<int>();
		protected readonly Dynx<int> westSize = new Dynx<int>();
		protected readonly Dynx<int> eastSize = new Dynx<int>();
		protected readonly Component northWestZone;
		protected readonly Component southWestZone;
		protected readonly Component northEastZone;
		protected readonly Component southEastZone;
		protected readonly Component northZone;
		protected readonly Component southZone;
		protected readonly Component eastZone;
		protected readonly Component westZone;
		
		public Frame()
		{
			//init border to 1%
			Border.Exp = () => Size.Value * .01;
			Border.OnUpdate += MarkDirty;
			FlushBeforeUpdate(Border);
			
			ForeColor.Value = RGB.White;
			ForeColor.OnUpdate += MarkDirty;
			FlushBeforeUpdate(ForeColor);
			
			BorderColor.Value = RGB.Grey;
			BorderColor.OnUpdate += MarkDirty;
			FlushBeforeUpdate(BorderColor);
			
			BackColor.Value = RGB.Black;
			BackColor.OnUpdate += MarkDirty;
			FlushBeforeUpdate(BackColor);
			
			EdgeInterpolation.Value = Interpolation.Linear;
			EdgeInterpolation.OnUpdate += MarkDirty;
			FlushBeforeUpdate(EdgeInterpolation);
			
			TitleHeight.Exp = () => (int)(Size.Value.Y * .025);
			TitleHeight.OnUpdate += MarkDirty;
			FlushBeforeUpdate(TitleHeight);
			
			RoundEdges.Value = true;
			RoundEdges.OnUpdate += MarkDirty;
			FlushBeforeUpdate(RoundEdges);
			
			DisableNorthEdge.Value = false;
			DisableNorthEdge.OnUpdate += MarkDirty;
			FlushBeforeUpdate(DisableNorthEdge);
			
			DisableSouthEdge.Value = false;
			DisableSouthEdge.OnUpdate += MarkDirty;
			FlushBeforeUpdate(DisableSouthEdge);
			
			DisableWestEdge.Value = false;
			DisableWestEdge.OnUpdate += MarkDirty;
			FlushBeforeUpdate(DisableWestEdge);
			
			DisableEastEdge.Value = false;
			DisableEastEdge.OnUpdate += MarkDirty;
			FlushBeforeUpdate(DisableEastEdge);
			
			DisableTitle.Value = false;
			DisableTitle.OnUpdate += MarkDirty;
			FlushBeforeUpdate(DisableTitle);
			
			ResizeBorderMulti.Value = 1;
			
			DisableResize.Value = false;
			DisableMove.Value = false;
			
			Size.OnFilter += v =>
			{
				Point2D min = 0;
				if(!DisableNorthEdge.Value) min.Y += Border.Value.Y;
				if(!DisableSouthEdge.Value) min.Y += Border.Value.Y;
				if(!DisableWestEdge.Value) min.X += Border.Value.X;
				if(!DisableEastEdge.Value) min.X += Border.Value.X;
				if(!DisableTitle.Value) min.Y += TitleHeight.Value;
				return VectorUtil.Max(v, min);
			};
			
			//create title panel
			TitlePanel = new Panel();
			TitlePanel.DisableAutoReparent();
			TitlePanel.Parent.Value = this;
			//init color
			TitlePanel.Color.Exp = () => BorderColor.Value;
			//center in button
			TitlePanel.Position.Exp = () =>
			{
				Point2D vec = this.Position.Value;
				if(!DisableWestEdge.Value) vec.X += Border.Value.X / 2;
				if(!DisableNorthEdge.Value) vec.Y += Border.Value.Y / 2;
				return vec;
			};
			//reduce size by twice border
			TitlePanel.Size.Exp = () =>
			{
				Point2D vec;
				vec.X = this.Size.Value.X;
				if(!DisableWestEdge.Value) vec.X -= Border.Value.X / 2;
				if(!DisableEastEdge.Value) vec.X -= Border.Value.X / 2;
				vec.Y = TitleHeight.Value;
				return vec;
			};
			//slightly offset z coord
			TitlePanel.ZCoord.Exp = () => ZCoord.Value + .001;
			TitlePanel.Hidden.Value = true;
			
			//create content panel
			Content = new Panel();
			Content.DisableAutoReparent();
			Content.Parent.Value = this;
			//init color
			Content.Color.Exp = () => ForeColor.Value;
			//center in frame
			Content.Position.Exp = () =>
			{
				Point2D vec = this.Position.Value;
				if(!DisableWestEdge.Value) vec.X += Border.Value.X;
				if(!DisableNorthEdge.Value) vec.Y += Border.Value.Y;
				if(!DisableTitle.Value) vec.Y += TitleHeight.Value;
				return vec;
			};
			//reduce size by twice border
			Content.Size.Exp = () =>
			{
				Point2D vec = this.Size.Value;
				if(!DisableWestEdge.Value) vec.X -= Border.Value.X;
				if(!DisableEastEdge.Value) vec.X -= Border.Value.X;
				if(!DisableNorthEdge.Value) vec.Y -= Border.Value.Y;
				if(!DisableSouthEdge.Value) vec.Y -= Border.Value.Y;
				if(!DisableTitle.Value) vec.Y -= TitleHeight.Value;
				return vec;
			};
			//slightly offset z coord
			Content.ZCoord.Exp = () => ZCoord.Value + .001;
			Content.Hidden.Value = true;
			
			northSize.Exp = () => DisableNorthEdge.Value ? 0 : (int)(Border.Value.Y * ResizeBorderMulti.Value);
			southSize.Exp = () => DisableSouthEdge.Value ? 0 : (int)(Border.Value.Y * ResizeBorderMulti.Value);
			westSize.Exp = () => DisableWestEdge.Value ? 0 : (int)(Border.Value.X * ResizeBorderMulti.Value);
			eastSize.Exp = () => DisableEastEdge.Value ? 0 : (int)(Border.Value.X * ResizeBorderMulti.Value);
			
			//create all resizers
			//diagonals
			northWestZone = CreateCorner(true, true);
			southWestZone = CreateCorner(true, false);
			northEastZone = CreateCorner(false, true);
			southEastZone = CreateCorner(false, false);
			//edges
			westZone = CreateEdge(true, true);
			eastZone = CreateEdge(true, false);
			northZone = CreateEdge(false, true);
			southZone = CreateEdge(false, false);
			
			//create title mover
			Panel titleZone = new Panel();
			titleZone.Parent.Value = this;
			titleZone.Position.Exp = () => 
			{
				Point2D vec = Position.Value;
				vec.X += northWestZone.Size.Value.X;
				vec.Y += northZone.Size.Value.Y;
				return vec;
			};
			titleZone.Size.Exp = () => 
			{
				Point2D vec;
				vec.X = Size.Value.X - northEastZone.Size.Value.X - northWestZone.Size.Value.X;
				if(!DisableTitle.Value)
				{
					vec.Y = TitleHeight.Value + Border.Value.Y - northZone.Size.Value.Y;
				}else
				{
					vec.Y = 0;
				}
				return vec;
			};
			titleZone.Opaque.Value = false;
			titleZone.Hidden.Value = true;
			titleZone.InputHidden.Value = false;
			bool isDragging = false;
			titleZone.OnMousePress += button => 
			{
				if(button == MouseButton.LEFT)
				{
					isDragging = true;
					Grab();
				}
			};
			titleZone.OnMouseRelease += button => 
			{
				if(button == MouseButton.LEFT)
				{
					isDragging = false;
					Drop();
				}
			};
			titleZone.OnMouseMove += (coords, delta) =>
			{
				if(!DisableMove.Value && isDragging)
				{
					Position.Value += delta;
					if(OnMove != null) OnMove();
				}
			};
		}
		
		private Component CreateCorner(bool left, bool top)
		{
			Panel zone = new Panel();
			zone.Parent.Value = this;
			zone.Position.Exp = () => 
			{
				Point2D vec = Position.Value;
				if(!left) vec.X += Size.Value.X - zone.Size.Value.X;
				if(!top) vec.Y += Size.Value.Y - zone.Size.Value.Y;
				return vec;
			};
			zone.Size.Exp = () =>
			{
				Point2D vec = 0;
				if(left) vec.X = westSize.Value;
				else     vec.X = eastSize.Value;
				if(top)  vec.Y = northSize.Value;
				else     vec.Y = southSize.Value;
				return vec;
			};
			zone.Opaque.Value = false;
			zone.Hidden.Value = true;
			zone.InputHidden.Value = false;
			zone.HoverCursor.Exp = () =>
			{
				if(DisableResize.Value) return Cursor.UNSPECIFIED;
				if(left ^ top) return Cursor.RESIZE_NESW;
				else		   return Cursor.RESIZE_NWSE;
			};
			bool dragging = false;
			zone.OnMousePress += button => {if(button == MouseButton.LEFT) dragging = true;};
			zone.OnMouseRelease += button => {if(button == MouseButton.LEFT) dragging = false;};
			zone.OnMouseMove += (coords, delta) =>
			{
				if(!DisableResize.Value && dragging)
				{
					Rectangle bounds = Bounds;
					if(left)
					{
						bounds.X += delta.X;
						bounds.Width -= delta.X;
					}else
					{
						bounds.Width += delta.X;
					}
					if(top)
					{
						bounds.Y += delta.Y;
						bounds.Height -= delta.Y;
					}else
					{
						bounds.Height += delta.Y;
					}
					Bounds = bounds;
					if(OnResize != null) OnResize();
				}
			};
			return zone;
		}
		
		private Component CreateEdge(bool vertical, bool minSide)
		{
			Panel zone = new Panel();
			zone.Parent.Value = this;
			zone.Position.Exp = () => 
			{
				Point2D vec = Position.Value;
				if(vertical)
				{
					if(minSide)
					{
						vec.Y += northWestZone.Size.Value.Y;
					}else
					{
						vec.X += Size.Value.X - zone.Size.Value.X;
						vec.Y += northEastZone.Size.Value.Y;
					}
				}else
				{
					if(minSide)
					{
						vec.X += northWestZone.Size.Value.X;
					}else
					{
						vec.X += southWestZone.Size.Value.X;
						vec.Y += Size.Value.Y - zone.Size.Value.Y;
					}
				}
				return vec;
			};
			zone.Size.Exp = () =>
			{
				Point2D vec;
				if(vertical)
				{
					if(minSide)
					{
						vec.X = westSize.Value;
						vec.Y = Size.Value.Y - northWestZone.Size.Value.Y - southWestZone.Size.Value.Y;
					}else
					{
						vec.X = eastSize.Value;
						vec.Y = Size.Value.Y - northEastZone.Size.Value.Y - southEastZone.Size.Value.Y;
					}
				}else
				{
					if(minSide)
					{
						vec.X = Size.Value.X - northWestZone.Size.Value.X - northEastZone.Size.Value.X;
						vec.Y = northSize.Value;
					}else
					{
						
						vec.X = Size.Value.X - southWestZone.Size.Value.X - southEastZone.Size.Value.X;
						vec.Y = southSize.Value;
					}
				}
				return vec;
			};
			zone.Opaque.Value = false;
			zone.Hidden.Value = true;
			zone.InputHidden.Value = false;
			zone.HoverCursor.Exp = () =>
			{
				if(DisableResize.Value) return Cursor.UNSPECIFIED;
				if(vertical) return Cursor.RESIZE_HORIZONTAL;
				else		 return Cursor.RESIZE_VERTICAL;
			};
			bool dragging = false;
			zone.OnMousePress += button => {if(button == MouseButton.LEFT) dragging = true;};
			zone.OnMouseRelease += button => {if(button == MouseButton.LEFT) dragging = false;};
			zone.OnMouseMove += (coords, delta) =>
			{
				if(!DisableResize.Value && dragging)
				{
					Rectangle bounds = Bounds;
					if(vertical)
					{
						if(minSide)
						{
							bounds.X += delta.X;
							bounds.Width -= delta.X;
						}else
						{
							bounds.Width += delta.X;
						}
					}else
					{
						if(minSide)
						{
							bounds.Y += delta.Y;
							bounds.Height -= delta.Y;
						}else
						{
							bounds.Height += delta.Y;
						}
					}
					Bounds = bounds;
					if(OnResize != null) OnResize();
				}
			};
			return zone;
		}
		
		protected override Component GetParentFor(Component child)
		{
			return Content;
		}
		
		protected override void Render(Image image)
		{
			int titleHeight = (int)TitleHeight.Value;
			Rectangle outBounds = (Rectangle)Size.Value;
			Rectangle inBounds = outBounds;
			if(!DisableNorthEdge.Value) inBounds.Min.Y += Border.Value.Y / 2;
			if(!DisableSouthEdge.Value) inBounds.Max.Y -= Border.Value.Y / 2;
			if(!DisableWestEdge.Value) inBounds.Min.X += Border.Value.X / 2;
			if(!DisableEastEdge.Value) inBounds.Max.X -= Border.Value.X / 2;
			
			//render outside edges
			RenderUtil.RenderBorder(image, outBounds, inBounds, BackColor.Value, BorderColor.Value, RoundEdges.Value, EdgeInterpolation.Value, true);
				
			if(!DisableTitle.Value)
			{
				//render title bar
				image.RenderSolid(new Rectangle{Position = inBounds.Position, Width = inBounds.Width, Height = titleHeight}, BorderColor.Value);
				
				inBounds.Y += titleHeight;
				inBounds.Height -= titleHeight;
			}
			
			outBounds = inBounds;
			if(!DisableNorthEdge.Value || !DisableTitle.Value) inBounds.Min.Y += Border.Value.Y / 2;
			if(!DisableSouthEdge.Value) inBounds.Max.Y -= Border.Value.Y / 2;
			if(!DisableWestEdge.Value) inBounds.Min.X += Border.Value.X / 2;
			if(!DisableEastEdge.Value) inBounds.Max.X -= Border.Value.X / 2;
			
			//render inside edges
			RenderUtil.RenderBorder(image, outBounds, inBounds, BorderColor.Value, ForeColor.Value, RoundEdges.Value, EdgeInterpolation.Value, true);
			
			//render inside
			image.RenderSolid(inBounds, ForeColor.Value);
		}
	}
}
