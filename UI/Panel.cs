namespace IROM.UI
{
	using System;
	using IROM.Util;
	using IROM.Dynamix;
	
	/// <summary>
	/// A Panel is a simple <see cref="Component"/> with a color.
	/// Often used to organize other <see cref="Component"/>s.
	/// </summary>
	public class Panel : Component
	{
		/// <summary>
		/// The color of this <see cref="Panel"/>.
		/// </summary>
		public readonly Dynx<ARGB> Color = new Dynx<ARGB>();
		
		public Panel(Component parent) : this(parent, false)
		{
			
		}
		
		public Panel(Component parent, bool bypass) : base(parent, bypass)
		{
			Color.Value = ARGB.Clear;
			Color.Subscribe(MarkDirty);
		}
		
		protected override void Render(Image image)
		{
			image.Fill(Color.Value);
		}
	}
}
