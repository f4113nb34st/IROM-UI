namespace IROM.UI
{
	using System;
	using IROM.Util;
	
	/// <summary>
	/// An icon is a simple <see cref="Component"/> with an image.
	/// </summary>
	public class Icon : Component
	{
		private Image BaseImage;
		
		/// <summary>
		/// The current <see cref="Image"/> of this <see cref="Icon"/>
		/// </summary>
		public Image CurrentImage
		{
			get
			{
				return BaseImage;
			}
			set
			{
				BaseImage = value;
				Dirty = true;
			}
		}
		
		public Icon(Component parent, Image image) : this(parent, false, image)
		{
			
		}
		
		public Icon(Component parent, bool bypass, Image image) : base(parent, bypass)
		{
			CurrentImage = image;
		}
		
		protected override void Render(Image image)
		{
			double dx = BaseImage.Width / (double)image.Width;
			double dy = BaseImage.Height / (double)image.Height;
			
			double x = 0;
			for(int i = 0; i < image.Width; i++, x += dx)
			{
				double y = 0;
				for(int j = 0; j < image.Height; j++, y += dy)
				{
					image[i, j] = BaseImage[(int)x, (int)y];
				}
			}
		}
	}
}
