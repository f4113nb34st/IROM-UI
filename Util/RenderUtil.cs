namespace IROM.UI
{
	using System;
	using IROM.Util;
	
	/// <summary>
	/// Simple class with static render helper methods.
	/// </summary>
	public static class RenderUtil
	{
		/// <summary>
		/// Renders a border between the given inner and outer rectangles.
		/// </summary>
		/// <param name="image">The render target.</param>
		/// <param name="outBoundary">The outer rectangle.</param>
		/// <param name="inBoundary">The inner rectangle.</param>
		/// <param name="outColor">The outer color.</param>
		/// <param name="inColor">The inner color.</param>
		/// <param name="roundEdges">True if the edges should be rounded.</param>
		/// <param name="interp">The interpolation function to use.</param>
		/// <param name="disableCenter">If true, disables filling of the center.</param>
		public static void RenderBorder(Image image, Rectangle outBoundary, Rectangle inBoundary, ARGB outColor, ARGB inColor, bool roundEdges, InterpFunction interp, bool disableCenter = false)
		{
			//render north west
			RenderCorner(image, false, false, new Rectangle{Min = outBoundary.Min, Max = inBoundary.Min - 1}, outColor, inColor, roundEdges, interp);
			//render north east
			RenderCorner(image, true, false, new Rectangle{Min = new Point2D(inBoundary.Max.X + 1, outBoundary.Min.Y), Max = new Point2D(outBoundary.Max.X, inBoundary.Min.Y - 1)}, outColor, inColor, roundEdges, interp);
			//render south west
			RenderCorner(image, false, true, new Rectangle{Min = new Point2D(outBoundary.Min.X, inBoundary.Max.Y + 1), Max = new Point2D(inBoundary.Min.X - 1, outBoundary.Max.Y)}, outColor, inColor, roundEdges, interp);
			//render south east
			RenderCorner(image, true, true, new Rectangle{Min = inBoundary.Max + 1, Max = outBoundary.Max}, outColor, inColor, roundEdges, interp);
			//render north
			RenderYSide(image, false, new Rectangle{X = inBoundary.X, Y = outBoundary.Min.Y   , Width = inBoundary.Width, Height = inBoundary.Min.Y - outBoundary.Min.Y}, outColor, inColor, interp);
			//render south
			RenderYSide(image, true, new Rectangle{X = inBoundary.X, Y = inBoundary.Max.Y + 1, Width = inBoundary.Width, Height = outBoundary.Max.Y - inBoundary.Max.Y}, outColor, inColor, interp);
			//render west
			RenderXSide(image, false, new Rectangle{X = outBoundary.Min.X,    Y = inBoundary.Y, Width = inBoundary.Min.X - outBoundary.Min.X, Height = inBoundary.Height}, outColor, inColor, interp);
			//render east
			RenderXSide(image, true, new Rectangle{X = inBoundary.Max.X + 1, Y = inBoundary.Y, Width = outBoundary.Max.X - inBoundary.Max.X, Height = inBoundary.Height}, outColor, inColor, interp);
			//render center
			if(!disableCenter) image.RenderSolid(inBoundary, inColor);
		}
		
		/// <summary>
		/// Renders a divider within the given rectangle. Uses the longer dimension as "with the grain".
		/// </summary>
		/// <param name="image">The image to render to.</param>
		/// <param name="boundary">The boundary to render in.</param>
		/// <param name="minColor">The minimum color.</param>
		/// <param name="midColor">The middle color.</param>
		/// <param name="maxColor">The maximum color.</param>
		/// <param name="interp">The interpolation function.</param>
		public static void RenderDivider(Image image, Rectangle boundary, ARGB minColor, ARGB midColor, ARGB maxColor, InterpFunction interp)
		{
			Rectangle area;
			if(boundary.Width >= boundary.Height)
			{
				area = boundary;
				int mid = (boundary.Min.Y + boundary.Max.Y) / 2;
				area.Max.Y = mid - 1;
				RenderXSide(image, false, boundary, minColor, midColor, interp);
				
				image.RenderSolid(new Rectangle{Min = new Point2D(mid, boundary.Min.Y), Max = new Point2D(mid, boundary.Max.Y)}, midColor);
				
				area = boundary;
				area.Min.Y = mid + 1;
				RenderXSide(image, true, boundary, maxColor, minColor, interp);
			}else
			{
				area = boundary;
				int mid = (boundary.Min.X + boundary.Max.X) / 2;
				area.Max.X = mid - 1;
				RenderYSide(image, false, boundary, minColor, midColor, interp);
				
				image.RenderSolid(new Rectangle{Min = new Point2D(boundary.Min.X, mid), Max = new Point2D(boundary.Max.X, mid)}, midColor);
				
				area = boundary;
				area.Min.X = mid + 1;
				RenderYSide(image, true, boundary, maxColor, minColor, interp);
			}
		}
		
		/// <summary>
		/// Renders a x-oriented gradient between the given colors. Includes outColor in the gradient but not inColor.
		/// </summary>
		/// <param name="image">The image to render to.</param>
		/// <param name="reverse">True to reverse the gradient.</param>
		/// <param name="area">The area to fill.</param>
		/// <param name="outColor">The "outer" color.</param>
		/// <param name="inColor">The "inner" color.</param>
		/// <param name="interp">The interpolation function to use.</param>
		private static void RenderXSide(Image image, bool reverse, Rectangle area, ARGB outColor, ARGB inColor, InterpFunction interp)
		{
			Rectangle clip = ShapeUtil.Overlap((Rectangle)image.Size, image.GetClip(), area);
			if(clip.IsValid())
			{
				for(int i = clip.Min.X; i <= clip.Max.X; i++)
				{
					double mu;
					if(reverse) mu = i - area.Min.X;
					else 		mu = area.Max.X - i;
					mu++;
					mu /= area.Width;
					mu = Util.Clip(mu, 0, 1);
					image.RenderSolid(new Rectangle{Min = new Point2D(i, clip.Min.Y), Max = new Point2D(i, clip.Max.Y)}, ColorUtil.Interpolate(inColor, outColor, mu, interp));
				}
			}
		}
		
		/// <summary>
		/// Renders a y-oriented gradient between the given colors. Includes outColor in the gradient but not inColor.
		/// </summary>
		/// <param name="image">The image to render to.</param>
		/// <param name="reverse">True to reverse the gradient.</param>
		/// <param name="area">The area to fill.</param>
		/// <param name="outColor">The "outer" color.</param>
		/// <param name="inColor">The "inner" color.</param>
		/// <param name="interp">The interpolation function to use.</param>
		private static void RenderYSide(Image image, bool reverse, Rectangle area, ARGB outColor, ARGB inColor, InterpFunction interp)
		{
			Rectangle clip = ShapeUtil.Overlap((Rectangle)image.Size, image.GetClip(), area);
			if(clip.IsValid())
			{
				for(int j = clip.Min.Y; j <= clip.Max.Y; j++)
				{
					double mu;
					if(reverse) mu = j - area.Min.Y;
					else 		mu = area.Max.Y - j;
					mu++;
					mu /= area.Height;
					mu = Util.Clip(mu, 0, 1);
					image.RenderSolid(new Rectangle{Min = new Point2D(clip.Min.X, j), Max = new Point2D(clip.Max.X, j)}, ColorUtil.Interpolate(inColor, outColor, mu, interp));
				}
			}
		}
		
		/// <summary>
		/// Renders a corner gradient between the given colors. Includes outColor in the gradient but not inColor.
		/// </summary>
		/// <param name="image">The image to render to.</param>
		/// <param name="reverseX">True to reverse the gradient in the x direction.</param>
		/// <param name="reverseY">True to reverse the gradient in the y direction.</param>
		/// <param name="area">The area to fill.</param>
		/// <param name="outColor">The "outer" color.</param>
		/// <param name="inColor">The "inner" color.</param>
		/// <param name="roundEdges">True to round the corner, false for square.</param>
		/// <param name="interp">The interpolation function to use.</param>
		private static void RenderCorner(Image image, bool reverseX, bool reverseY, Rectangle area, ARGB outColor, ARGB inColor, bool roundEdges, InterpFunction interp)
		{
			Rectangle clip = ShapeUtil.Overlap((Rectangle)image.Size, image.GetClip(), area);
			if(clip.IsValid())
			{
				for(int i = clip.Min.X; i <= clip.Max.X; i++)
				{
					for(int j = clip.Min.Y; j <= clip.Max.Y; j++)
					{
						double dx;
						double dy;
						if(reverseX) dx = i - area.Min.X;
						else 		 dx = area.Max.X - i;
						if(reverseY) dy = j - area.Min.Y;
						else 		 dy = area.Max.Y - j;
						dx++;
						dx /= area.Width;
						dy++;
						dy /= area.Height;
					
						double mu;
						if(roundEdges)
						{
							mu = Math.Sqrt(dx * dx + dy * dy);
						}else
						{
							mu = Math.Max(Math.Abs(dx), Math.Abs(dy));
						}
						mu = Util.Clip(mu, 0, 1);
						
						image[i, j] = ColorUtil.Interpolate(inColor, outColor, mu, interp);
					}
				}
			}
		}
	}
}
