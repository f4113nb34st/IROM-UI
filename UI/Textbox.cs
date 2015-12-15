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
				Dirty = true;
			}
		}
		
		/// <summary>
		/// The position of the flashing |.
		/// </summary>
		public volatile int MarkerPosition;
		
		public TextBox(Component parent, string txt) : this(parent, false, txt)
		{
			Text.Subscribe(() => MarkerPosition = Math.Min(MarkerPosition, Text.Value.Length));
		}
		
		public TextBox(Component parent, bool bypass, string txt) : base(parent, bypass, txt)
		{
			OnFocusChange += (sender, focused) =>
			{
				if(UseMarker)
				{
					MarkerState = focused;
					MarkerTime = 0;
					Dirty = true;
				}
			};
			MarkerPosition = Text.Value.Length;
			//dummy mouse press handler so we get focus
			OnMousePress += (sender, e) => {};
			OnKeyPress += (sender, args) =>
			{
				if(args.Button == KeyboardButton.LEFT)
				{
					if(MarkerPosition > 0)
					{
						MarkerPosition--;
						Dirty = true;
						args.Consumed = true;
					}
				}else
				if(args.Button == KeyboardButton.RIGHT)
				{
					if(MarkerPosition < Text.Value.Length)
					{
						MarkerPosition++;
						Dirty = true;
						args.Consumed = true;
					}
				}
			};
			OnCharTyped += (sender, args) => 
			{
				args.Consumed = true;
				ProcessCharacter(args.Character);
			};
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
				if(IsFocused)
				{
					MarkerTime += dt;
					while(MarkerTime >= .5)
					{
						MarkerTime -= .5;
						MarkerState = !MarkerState;
						Dirty = true;
					}
				}
			}
		}
		
		// disable once RedundantOverridenMember
		protected override void Render(Image image)
		{
			base.Render(image, UseMarker && IsFocused && MarkerState, MarkerPosition);
		}
	}
}
