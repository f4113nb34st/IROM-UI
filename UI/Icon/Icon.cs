namespace IROM.UI
{
	using System;
	using IROM.Util;
	using IROM.Dynamix;
	
	/// <summary>
	/// An icon is a simple <see cref="Component"/> with an image.
	/// </summary>
	public class Icon : Component
	{
		/// <summary>
		/// The current <see cref="Image"/> of this <see cref="Icon"/>
		/// </summary>
		public readonly Dynx<Image> CurrentImage = new Dynx<Image>();
		
		public Icon() : this(null)
		{
		}
		
		public Icon(Image image)
		{
			CurrentImage.Value = image;
			FlushBeforeUpdate(CurrentImage);
		}
		
		protected override void Render(Image image)
		{
			Image src = CurrentImage.Value;
			if(src != null)
			{
				double dx = src.Width / (double)image.Width;
				double dy = src.Height / (double)image.Height;
				
				double x = 0;
				for(int i = 0; i < image.Width; i++, x += dx)
				{
					double y = 0;
					for(int j = 0; j < image.Height; j++, y += dy)
					{
						image[i, j] = src[(int)x, (int)y];
					}
				}
			}
		}
	}
}
