namespace IROM.UI
{
	using System;
	using IROM.Util;
	using IROM.Dynamix;
	
	/// <summary>
	/// A tooltip is a panel that pops up when it's parent is hovered over.
	/// </summary>
	public class Tooltip : Panel, IDisposable
	{
		/// <summary>
		/// The hover time before this <see cref="Tooltip"/> will appear.
		/// </summary>
		public double HoverTime = .5;
		
		/// <summary>
		/// Time until the tooltip appears.
		/// </summary>
		private readonly Dynx<double> TimeLeft = new Dynx<double>();
		
		/// <summary>
		/// The last coords of the mouse.
		/// </summary>
		private readonly Dynx<Point2D> mouseCoords = new Dynx<Point2D>();
		
		public Tooltip()
		{
			ZCoord.Value = 10000;//always display on top
			InputOpaque.Value = false;
			
			TimeLeft.Value = double.PositiveInfinity;
			Visible.Exp = () => TimeLeft.Value <= 0;
			
			Dynx<bool> flipX = new Dynx<bool>();
			Dynx<bool> flipY = new Dynx<bool>();
			flipX.Exp = () => (Parent.Value.Position.Value.X + mouseCoords.Value.X + 16 + Size.Value.X) > this.RootObj.Value.Size.Value.X;
			flipY.Exp = () => (Parent.Value.Position.Value.Y + mouseCoords.Value.Y + 16 + Size.Value.Y) > this.RootObj.Value.Size.Value.Y;
			
			Position.Exp = () =>
			{
				int x = mouseCoords.Value.X + 16 + ((!flipX.Value ? 0 : -1) * (Size.Value.X + 16));
				int y = mouseCoords.Value.Y + 16 + ((!flipY.Value ? 0 : -1) * (Size.Value.Y + 16));
				return new Point2D(x, y);
			};
		}
		
		public void Dispose()
		{
			Parent.Value = null;
		}
		
		protected override void OnParentChange(Component oldParent, Component newParent)
		{
			base.OnParentChange(oldParent, newParent);
			if(oldParent != NULL_PARENT)
			{
				oldParent.OnMouseEnter -= OnEnter;
				oldParent.OnMouseExit -= OnExit;
				oldParent.OnMouseMove -= OnMove;
			}
			if(newParent != NULL_PARENT)
			{
				newParent.OnMouseEnter += OnEnter;
				newParent.OnMouseExit += OnExit;
				newParent.OnMouseMove += OnMove;
			}
		}
		
		private void OnEnter()
		{
			TimeLeft.Value = HoverTime;
		}
		
		private void OnExit()
		{
			TimeLeft.Value = double.PositiveInfinity;
		}
		
		private void OnMove(Point2D coords, Point2D delta)
		{
			mouseCoords.Value = coords;
		}
		
		protected internal override void Tick(double dt)
		{
			base.Tick(dt);
			TimeLeft.Value -= dt;
		}
	}
}
