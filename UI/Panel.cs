namespace IROM.UI
{
	using System;
	using IROM.Util;
	
	/// <summary>
	/// A Panel is a simple <see cref="Component"/> with a color.
	/// Often used to organize other <see cref="Component"/>s.
	/// </summary>
	public class Panel : Component
	{
		//the backing variable classes
		private UIColor color;
		
		/// <summary>
		/// Gets or sets the color of this <see cref="Panel"/>.
		/// </summary>
		public UIColor Color
		{
			get
			{
				return color;
			}
			set
			{
				if(color != value)
				{
					color = value;
					if(OnColorChange != null) OnColorChange(this, color);
				}
			}
		}
		
		/// <summary>
		/// Invoked whenever <see cref="Color"/> changes.
		/// </summary>
		public event EventHandler<UIColor> OnColorChange;
		
		public Panel(Component parent) : this(parent, false)
		{
			
		}
		
		public Panel(Component parent, bool bypass) : base(parent, bypass)
		{
			Color = new UIColor(this);
		}
		
		protected override void Render(Image image)
		{
			image.Fill(Color.Value);
		}
	}
}
