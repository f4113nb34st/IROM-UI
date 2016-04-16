namespace IROM.UI
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;
	using System.Drawing.Imaging;
	using System.Windows.Forms;
	using System.Runtime.InteropServices;
	using IROM.Util;
	
	/// <summary>
	/// Represents a monospaced (constant char width) font for use with the UI system. Cannot be resized, but can be created with any size.
	/// </summary>
	public class Font
	{
		[Flags]
		public enum FontType
		{
			PLAIN = 0, 
			BOLD = 1, 
			ITALIC = 2,
			//UNDERLINE = 4, underline not supported
			STRIKE_OUT = 8,
		}
		
		private struct FontAttributes
		{
			public Point2D size;
			public FontType type;
		}
		
		private static readonly Dictionary<FontAttributes, GCHandle> fontIndex = new Dictionary<FontAttributes, GCHandle>();
		
		/// <summary>
		/// Returns a new Font with the given characteristics.
		/// </summary>
		/// <param name="size">The max size of the <see cref="Font"/> in pixels.</param>
		/// <param name="type">The <see cref="FontType"/>.</param>
		/// <returns>A font with the given characteristics.</returns>
		public static Font GetFont(Point2D size, FontType type = FontType.PLAIN)
		{
			//validate size
			size = VectorUtil.Max(size, new Point2D(1, 1));
			
			FontAttributes attr = new FontAttributes{size = size, type = type};
			GCHandle handle;
			Font font = null;
			//if has and not gc'ed
			if(fontIndex.TryGetValue(attr, out handle) && handle.IsAllocated)
			{
				font = handle.Target as Font;
			}
			if(font == null)
			{
				font = new Font(attr);
				handle = GCHandle.Alloc(font, GCHandleType.Weak);
				fontIndex[attr] = handle;
			}
			return font;
		}
		
		/// <summary>
		/// The width of each character in this font.
		/// </summary>
		public int Width
		{
			get;
			private set;
		}
		
		/// <summary>
		/// The height of each character in this font.
		/// </summary>
		public int Height
		{
			get;
			private set;
		}
		
		/// <summary>
		/// The size of each character in this font.
		/// </summary>
		public Point2D Size
		{
			get
			{
				return new Point2D(Width, Height);
			}
			private set
			{
				Width = value.X;
				Height = value.Y;
			}
		}
		
		/// <summary>
		/// The Windows font used internally to generate characters.
		/// </summary>
		private readonly System.Drawing.Font InternalFont;
		
		/// <summary>
		/// <see cref="Dictionary{K, V}">Dictionary</see> of characters already rendered.
		/// </summary>
		private readonly Dictionary<char, FontRender> Renders = new Dictionary<char, FontRender>();
		
		/// <summary>
		/// Creates a new <see cref="Font"/> with the given attributes
		/// </summary>
		/// <param name="attr">The font attributes.</param>
		private Font(FontAttributes attr)
		{
			System.Drawing.FontStyle style = (System.Drawing.FontStyle)attr.type;
			System.Drawing.FontFamily family = System.Drawing.FontFamily.GenericMonospace;
			
			Size = attr.size;
			//guess size by height
			InternalFont = new System.Drawing.Font(family, Height, style, System.Drawing.GraphicsUnit.Pixel);
			
			//monospaced, so width is for any character
			//measure text adds an absurd amount of unavoidable padding so find just one char width like so (all char choices arbitrary)
			int trueWidth = BoundsOfString(".X.").Width - BoundsOfString("..").Width;
			
			//if too big
			if(trueWidth > Width)
			{
				//scale down
				InternalFont = new System.Drawing.Font(family, Height * Width / trueWidth, style, System.Drawing.GraphicsUnit.Pixel);
				Width = BoundsOfString(".X.").Width - BoundsOfString("..").Width;
				Height = BoundsOfString("X").Height;
			}else
			{
				Width = trueWidth;
			}
		}
		
		private System.Drawing.Size BoundsOfString(string s)
		{
			return TextRenderer.MeasureText(s, InternalFont, new System.Drawing.Size(int.MaxValue, int.MaxValue), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
		
		/// <summary>
		/// Draws the given string on the given image in the given location.
		/// </summary>
		/// <param name="image">The image to draw on.</param>
		/// <param name="x">The x coord.</param>
		/// <param name="y">The y coord.</param>
		/// <param name="s">The string.</param>
		public void Draw(Image image, int x, int y, String s)
		{
			Draw(image, x, y, s, RGB.Black);
		}
		
		/// <summary>
		/// Draws the given string on the given image in the given location with the given color.
		/// </summary>
		/// <param name="image">The image to draw on.</param>
		/// <param name="x">The x coord.</param>
		/// <param name="y">The y coord.</param>
		/// <param name="s">The string.</param>
		/// <param name="color">The color.</param>
		public void Draw(Image image, int x, int y, String s, ARGB color)
		{
			for(int i = 0; i < s.Length; i++)
			{
				Draw(image, x + (Width * i), y, s[i], color);
			}
		}
		
		/// <summary>
		/// Draws the given char on the given image in the given location.
		/// </summary>
		/// <param name="image">The image to draw on.</param>
		/// <param name="x">The x coord.</param>
		/// <param name="y">The y coord.</param>
		/// <param name="c">The char.</param>
		public void Draw(Image image, int x, int y, char c)
		{
			Draw(image, x, y, c, RGB.Black);
		}
		
		/// <summary>
		/// Draws the given char on the given image in the given location with the given color.
		/// </summary>
		/// <param name="image">The image to draw on.</param>
		/// <param name="x">The x coord.</param>
		/// <param name="y">The y coord.</param>
		/// <param name="c">The char.</param>
		/// <param name="color">The color.</param>
		public void Draw(Image image, int x, int y, char c, ARGB color)
		{
			if(!Renders.ContainsKey(c))
			{
				FontRender map = new FontRender(Width, Height);
				RenderChar(map, c);
				Renders[c] = map;
			}
			DrawRender(image, new Point2D(x, y), Renders[c], color);
		}
		
		private void DrawRender(Image image, Point2D position, FontRender render, ARGB color)
		{
			Rectangle view = ShapeUtil.Overlap((Rectangle)image.Size, ((Rectangle)render.Size) + position, image.GetClip());
			for(int i = view.Min.X; i < view.Max.X; i++)
			{
				for(int j = view.Min.Y; j < view.Max.Y; j++)
				{
					float alpha = render[i - position.X, j - position.Y];
					image[i, j] &= new ARGB((byte)(color.A * alpha), color.RGB);
				}
			}
		}
		
		private unsafe void RenderChar(FontRender render, char c)
		{
			using(System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(Width, Height, PixelFormat.Format32bppRgb))
			{
				using(System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
				{
					g.Clear(System.Drawing.Color.Black);
					g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
					TextRenderer.DrawText(g, c.ToString(), InternalFont, new Rectangle{Size = bitmap.Size}, System.Drawing.Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				}
				
				BitmapData data = bitmap.LockBits(new Rectangle{Size = bitmap.Size}, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
				uint* scan0 = (uint*)data.Scan0;
				
				for(int j = 0; j < Height; j++)
				{
					for(int i = 0; i < Width; i++)
					{
						render[i, j] = ((*(scan0 + (i + (Width * j)))) & 0xFF) / (float)255;
					}
				}
				
				bitmap.UnlockBits(data);
			}
		}
		
		private class FontRender : DataMap<float>
		{
			private float[,] Data;
			
			public FontRender(int width, int height) : base(width, height)
			{
				
			}
			
			public new float this[int x, int y]
	        {
	        	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	        	get
				{
	        		return Data[y, x];
				}
	        	[MethodImpl(MethodImplOptions.AggressiveInlining)]
				set
				{
					Data[y, x] = value;
				}
	        }
			
			protected override float BaseGet(int x, int y)
			{
				return Data[y, x];
			}
			
			protected override void BaseSet(int x, int y, float value)
			{
				Data[y, x] = value;
			}
			
			protected override void BaseResize(int width, int height)
			{
				Data = new float[height, width];
			}
		}
	}
}
