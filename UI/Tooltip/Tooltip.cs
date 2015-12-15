namespace IROM.UI
{
	using System;
	using IROM.Util;
	using IROM.Dynamix;
	
	/// <summary>
	/// A tooltip is a panel that pops up when it's parent is hovered over.
	/// </summary>
	public class Tooltip : Panel
	{
		/// <summary>
		/// The hover time before this <see cref="Tooltip"/> will appear.
		/// </summary>
		public double HoverTime = .5;
		
		/// <summary>
		/// Time until the tooltip appears.
		/// </summary>
		private double TimeLeft = double.PositiveInfinity;
		
		/// <summary>
		/// The last coords of the mouse.
		/// </summary>
		private readonly Dynx<Point2D> mouseCoords = new Dynx<Point2D>();
		
		public Tooltip(Component parent) : this(parent, false)
		{
			
		}
		
		public Tooltip(Component parent, bool bypass) : base(parent, bypass)
		{
			Visible = false;
			ZCoord.Value = 10000;//always display on top
			InputOpaque = false;
			
			Component screen = MasterScreen;
			
			Dynx<bool> flipX = () => (parent.Position.Value.X + mouseCoords.Value.X + 16 + Size.Value.X) > screen.Size.Value.X;
			Dynx<bool> flipY = () => (parent.Position.Value.Y + mouseCoords.Value.Y + 16 + Size.Value.Y) > screen.Size.Value.Y;
			
			Position.Exp = () =>
			{
				double x = mouseCoords.Value.X + 16 + ((!flipX ? 0 : -1) * (Size.Value.X + 16));
				double y = mouseCoords.Value.Y + 16 + ((!flipY ? 0 : -1) * (Size.Value.Y + 16));
				return new Vec2D(x, y);
			};
			
			parent.OnMouseEnter += (sender, e) => TimeLeft = HoverTime;
			parent.OnMouseExit += (sender, e) => TimeLeft = double.PositiveInfinity;
			parent.OnMouseMove += (sender, e) => mouseCoords.Value = e.Coords;
		}
		
		protected internal override void Tick(double dt)
		{
			base.Tick(dt);
			TimeLeft -= dt;
			Visible = TimeLeft <= 0;
		}
	}
}
