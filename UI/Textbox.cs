namespace IROM.UI
{
	using System;
	using IROM.Util;
	
	/// <summary>
	/// A TextBox is a simple editable <see cref="Label"/>.
	/// </summary>
	public class TextBox : Label
	{
		private bool BaseUseMarker = true;
		private volatile bool MarkerState = true;
		private double MarkerTime = 0;
		
		/// <summary>
		/// True to display the flashing | for the current position.
		/// </summary>
		public bool UseMarker
		{
			get{return BaseUseMarker;}
			set
			{
				BaseUseMarker = value;
				MarkerState = BaseUseMarker;
				MarkerTime = 0;
				Dirty.Value = true;
			}
		}
		
		/// <summary>
		/// The position of the flashing |.
		/// </summary>
		public volatile int MarkerPosition;
		
		public TextBox() : this(null)
		{
			
		}
		
		public TextBox(string text) : base(text)
		{
			Text.OnUpdate += () => MarkerPosition = Math.Min(MarkerPosition, Text.Value.Length);
			IsFocused.OnUpdate += () =>
            {
            	if(UseMarker)
				{
					MarkerState = IsFocused.Value;
					MarkerTime = 0;
					Dirty.Value = true;
				}
            };
			MarkerPosition = Text.Value.Length;
			//dummy mouse press handler so we get focus
			OnMousePress += button => {};
			OnKeyPress += button =>
			{
				if(button == KeyboardButton.LEFT)
				{
					if(MarkerPosition > 0)
					{
						MarkerPosition--;
						Dirty.Value = true;
					}
				}else
				if(button == KeyboardButton.RIGHT)
				{
					if(MarkerPosition < Text.Value.Length)
					{
						MarkerPosition++;
						Dirty.Value = true;
					}
				}
			};
			OnCharTyped += (c, repeat) => ProcessCharacter(c);
			HoverCursor.Value = Cursor.I_BEAM;
		}
		
		public virtual void ProcessCharacter(char c)
		{
			if(c == '\n') return;
			if(c == '\b')
			{
				if(MarkerPosition > 0)
				{
					MarkerPosition--;
					Text.Value = Text.Value.Substring(0, MarkerPosition) + Text.Value.Substring(MarkerPosition + 1);
				}
			}else
			{
				Text.Value = Text.Value.Substring(0, MarkerPosition) + c + Text.Value.Substring(MarkerPosition);
				MarkerPosition++;
			}
		}
		
		protected internal override void Tick(double dt)
		{
			base.Tick(dt);
			if(UseMarker)
			{
				if(IsFocused.Value)
				{
					MarkerTime += dt;
					while(MarkerTime >= .5)
					{
						MarkerTime -= .5;
						MarkerState = !MarkerState;
						Dirty.Value = true;
					}
				}
			}
		}
		
		// disable once RedundantOverridenMember
		protected override void Render(Image image)
		{
			base.Render(image, UseMarker && IsFocused.Value && MarkerState, MarkerPosition);
		}
	}
}
