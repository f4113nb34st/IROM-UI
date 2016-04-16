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
		public readonly Dynx<Point2D> Border = new Dynx<Point2D>();
		
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
			FlushBeforeUpdate(Border);
			
			ForeColor.Value = RGB.White;
			ForeColor.OnUpdate += MarkDirty;
			FlushBeforeUpdate(ForeColor);
			
			BackColor.Value = RGB.Black;
			BackColor.OnUpdate += MarkDirty;
			FlushBeforeUpdate(BackColor);
			
			//init hover color to average of fore and back
			HoverColor.Exp = () => (ForeColor.Value / 2) + (BackColor.Value / 2);
			HoverColor.OnUpdate += MarkDirty;
			FlushBeforeUpdate(HoverColor);
			
			EdgeInterpolation.Value = Interpolation.Linear;
			EdgeInterpolation.OnUpdate += MarkDirty;
			FlushBeforeUpdate(EdgeInterpolation);
			
			//create content panel
			Content = new Panel();
			Content.DisableAutoReparent();
			Content.Parent.Value = this;
			//init color
			Content.Color.Exp = () => ForeColor.Value;
			//center in button
			Content.Position.Exp = () =>
			{
				Point2D vec = Position.Value;
				vec += Size.Value / 2;
				vec -= Content.Size.Value / 2;
				return vec;
			};
			//reduce size by twice border
			Content.Size.Exp = () =>
			{
				Point2D vec = Size.Value;
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
				Hovered = false;
				Content.Color.Exp = () => ForeColor.Value;
			};
		}
		
		protected override Component GetParentFor(Component child)
		{
			return Content;
		}
		
		protected override void Render(Image image)
		{
			Rectangle outBounds = Bounds;
			Rectangle inBounds = outBounds.Contract(Border.Value);
			
			//render edges
			RenderUtil.RenderBorder(image, outBounds, inBounds, BackColor.Value, Content.Color.Value, RoundEdges, EdgeInterpolation.Value);
			
			//render center
			image.RenderSolid(inBounds, Content.Color.Value);
		}
	}
}
