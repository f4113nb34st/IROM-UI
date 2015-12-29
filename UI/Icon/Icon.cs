namespace IROM.UI
{
	using System;
	using IROM.Util;
	
	/// <summary>
	/// An icon is a simple <see cref="Component"/> with an image.
	/// </summary>
	public class Icon : Component
	{
		//backing var
		private Image currentImage;
		
		/// <summary>
		/// The current <see cref="Image"/> of this <see cref="Icon"/>
		/// </summary>
		public Image CurrentImage
		{
			get
			{
				return currentImage;
			}
			set
			{
				currentImage = value;
				MarkDirty();
			}
		}
		
		public Icon() : this(null)
		{
		}
		
		public Icon(Image image)
		{
			CurrentImage = image;
		}
		
		protected override void Render(Image image)
		{
			if(CurrentImage != null)
			{
				double dx = CurrentImage.Width / (double)image.Width;
				double dy = CurrentImage.Height / (double)image.Height;
				
				double x = 0;
				for(int i = 0; i < image.Width; i++, x += dx)
				{
					double y = 0;
					for(int j = 0; j < image.Height; j++, y += dy)
					{
						image[i, j] = CurrentImage[(int)x, (int)y];
					}
				}
			}
		}
	}
}
