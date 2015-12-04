namespace IROM.UI
{
	using System;
	using IROM.Util;
	
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
		
		//the backing variable classes
		private string BaseText;
		private UIColor textColor;
		private UIColor backColor;
		
		/// <summary>
		/// The text of this <see cref="Label"/>.
		/// </summary>
		public virtual string Text
		{
			get
			{
				return BaseText;
			}
			set
			{
				if(BaseText != value)
				{
					BaseText = value;
					Dirty = true;
				}
			}
		}
		
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
		
		/// <summary>
		/// Gets or sets the text color of this <see cref="Component"/>.
		/// </summary>
		public UIColor TextColor
		{
			get
			{
				return textColor;
			}
			set
			{
				if(textColor != value)
				{
					textColor = value;
					if(OnTextColorChange != null) OnTextColorChange(this, textColor);
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the background color of this <see cref="Component"/>.
		/// </summary>
		public UIColor BackColor
		{
			get
			{
				return backColor;
			}
			set
			{
				if(backColor != value)
				{
					backColor = value;
					if(OnBackColorChange != null) OnBackColorChange(this, backColor);
				}
			}
		}
		
		/// <summary>
		/// Invoked whenever <see cref="TextColor"/> changes.
		/// </summary>
		public event EventHandler<UIColor> OnTextColorChange;
		
		/// <summary>
		/// Invoked whenever <see cref="BackColor"/> changes.
		/// </summary>
		public event EventHandler<UIColor> OnBackColorChange;
		
		public Label(Component parent) : this(parent, "")
		{
			
		}
		
		public Label(Component parent, string text) : this(parent, false, text)
		{
			
		}
		
		public Label(Component parent, bool bypass, string text) : base(parent, bypass)
		{
			Text = text;
			TextColor = new UIColor(this);
			TextColor.OnChange += MarkMasterDirty;
			BackColor = new UIColor(this);
			BackColor.OnChange += MarkMasterDirty;
		}
		
		protected override void Render(Image image)
		{
			Render(image, false);
		}
		
		protected void Render(Image image, bool useMarker, int markerLoc = -1)
		{
			image.Fill(BackColor.Value);
			
			int length = Text.Length;
			if(length == 0)
			{
				return;
			}
			if(!LockFont)
			{
				Point2D idealSize = new Point2D(image.Width / Math.Max(length, lengthOverride), image.Height);
				if(CurrentFont == null || CurrentFont.Size != idealSize)
				{
					CurrentFont = new Font(idealSize, Style);
				}
			}
			int x = 0;
			int y = 0;
			switch(HorizontalJustify)
			{
				case Justify.MIN: x = 0; break;
				case Justify.MIDDLE: x = (image.Width / 2) - (CurrentFont.Width * length / 2); break;
				case Justify.MAX: x = image.Width - (CurrentFont.Width * length); break;
			}
			switch(VerticalJustify)
			{
				case Justify.MIN: y = 0; break;
				case Justify.MIDDLE: y = (image.Height / 2) - (CurrentFont.Height / 2); break;
				case Justify.MAX: y = image.Height - CurrentFont.Height; break;
			}
			CurrentFont.Draw(image, x, y, Text, TextColor.Value);
			if(useMarker)
			{
				CurrentFont.Draw(image, x + (int)((markerLoc - .5) * CurrentFont.Width), y, '|', TextColor.Value);
			}
		}
	}
}
