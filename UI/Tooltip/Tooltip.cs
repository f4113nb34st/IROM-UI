namespace IROM.UI
{
	using System;
	using IROM.Util;
	
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
		
		public Tooltip(Component parent) : this(parent, false)
		{
			
		}
		
		public Tooltip(Component parent, bool bypass) : base(parent, bypass)
		{
			Visible = false;
			ZCoord.Offset = 100;//always display on top
			InputOpaque = false;
			
			Component root = this;
			while(root.Parent != null)
			{
				root = root.Parent;
			}
			
			parent.OnMouseEnter += (sender, e) => TimeLeft = HoverTime;
			parent.OnMouseExit += (sender, e) => TimeLeft = double.PositiveInfinity;
			parent.OnMouseMove += (sender, e) =>
			{
				bool flipX = parent.Position.Value.X + e.Coords.X + 16 + Size.Value.X > root.Size.Value.X;
				bool flipY = parent.Position.Value.Y + e.Coords.Y + 16 + Size.Value.Y > root.Size.Value.Y;
				
				Position.Pixels = new Vec2D(e.Coords.X + (!flipX ? 16 : 0), e.Coords.Y + (!flipY ? 16 : 0));
				Position.RatioOwn = new Vec2D(!flipX ? 0 : -1, !flipY ? 0 : -1);
			};
		}
		
		protected internal override void Tick(double dt)
		{
			base.Tick(dt);
			TimeLeft -= dt;
			Visible = TimeLeft <= 0;
		}
	}
}
