namespace IROM.UI
{
	using System;
	using IROM.Util;
	using IROM.Dynamix;
	
	/// <summary>
	/// A Label is a simple <see cref="Component"/> with text.
	/// </summary>
	public class Label : Component
	{
		public enum Justify : uint
		{
			MIN = 0,
			MIDDLE = 1,
			MAX = 2,
			LEFT = 0,
			RIGHT = 2,
			TOP = 0,
			BOTTOM = 2
		}
		
		/// <summary>
		/// The text of this <see cref="Label"/>.
		/// </summary>
		public readonly Dynx<string> Text = new Dynx<string>();
		
		/// <summary>
		/// The text color of this <see cref="Label"/>.
		/// </summary>
		public readonly Dynx<ARGB> TextColor = new Dynx<ARGB>();
		
		/// <summary>
		/// The background color of this <see cref="Label"/>.
		/// </summary>
		public readonly Dynx<ARGB> BackColor = new Dynx<ARGB>();
		
		/// <summary>
		/// The margins on the text of this <see cref="Label"/>
		/// </summary>
		public readonly Dynx<Point2D> Margin = new Dynx<Point2D>();
		
		/// <summary>
		/// The font type of this <see cref="Label"/>
		/// </summary>
		public Font.FontType Style = Font.FontType.PLAIN;
		
		/// <summary>
		/// The current font of this <see cref="Label"/>'s <see cref="Text"/>.
		/// </summary>
		public Font CurrentFont;
		
		/// <summary>
		/// True if the <see cref="CurrentFont"/> will not be resized automatically.
		/// </summary>
		public bool LockFont = false;
		
		/// <summary>
		/// The vertical justification method.
		/// </summary>
		public Justify VerticalJustify = Justify.MIDDLE;
		
		/// <summary>
		/// The horizontal justification method.
		/// </summary>
		public Justify HorizontalJustify = Justify.MIDDLE;
		
		/// <summary>
		/// Override for the length of the text. If this value is non negative, uses it instead of the text length if it is greater.
		/// </summary>
		public int lengthOverride = -1;
		
		public Label()
		{
			Text.OnUpdate += MarkDirty;
			FlushBeforeUpdate(Text);
			
			TextColor.Value = RGB.Black;
			TextColor.OnUpdate += MarkDirty;
			FlushBeforeUpdate(TextColor);
			
			BackColor.Value = RGB.White;
			BackColor.OnUpdate += MarkDirty;
			FlushBeforeUpdate(BackColor);
			
			Margin.Value = 0;
			Margin.OnUpdate += MarkDirty;
			FlushBeforeUpdate(Margin);
		}
		
		public Label(string text) : this()
		{
			Text.Value = text;
		}
		
		public Label(Func<string> textExp) : this()
		{
			Text.Exp = textExp;
		}
		
		protected override void Render(Image image)
		{
			Render(image, false);
		}
		
		protected void Render(Image image, bool useMarker, int markerLoc = -1)
		{
			image.Fill(BackColor.Value);
			
			string text = Text.Value;
			if(text == null) text = "null";
			int length = text.Length;
			if(length == 0)
			{
				return;
			}
			if(!LockFont)
			{
				Point2D idealSize = new Point2D(image.Width / (Math.Max(length, lengthOverride) - Margin.Value.X * 2), image.Height - Margin.Value.Y * 2);
				if(CurrentFont == null || CurrentFont.Size != idealSize)
				{
					CurrentFont = Font.GetFont(idealSize, Style);
				}
			}
			int x = 0;
			int y = 0;
			switch(HorizontalJustify)
			{
				case Justify.MIN: x = Margin.Value.X; break;
				case Justify.MIDDLE: x = (image.Width / 2) - (CurrentFont.Width * length / 2); break;
				case Justify.MAX: x = image.Width - (CurrentFont.Width * length) - Margin.Value.X; break;
			}
			switch(VerticalJustify)
			{
				case Justify.MIN: y = Margin.Value.Y; break;
				case Justify.MIDDLE: y = (image.Height / 2) - (CurrentFont.Height / 2); break;
				case Justify.MAX: y = image.Height - CurrentFont.Height - Margin.Value.Y; break;
			}
			CurrentFont.Draw(image, x, y, text, TextColor.Value);
			if(useMarker)
			{
				CurrentFont.Draw(image, x + (int)((markerLoc - .5) * CurrentFont.Width), y, '|', TextColor.Value);
			}
		}
	}
}
