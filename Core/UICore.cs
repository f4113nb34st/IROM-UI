namespace IROM.Core
{
	using System;
	using IROM.UI;
	using IROM.Util;
	
	/// <summary>
    /// Abstract base class for <see cref="Core"/>s with UISystems.
    /// </summary>
	public abstract class UICore : Core
	{
		protected UICore(String title) : base(title)
        {
			UIFrame = new Frame(WindowObj);
		}
		
		protected UICore(String title, double tickRate) : base(title, tickRate)
        {
			UIFrame = new Frame(WindowObj);
		}
		
		protected Frame UIFrame;
		
		protected override void Tick(double deltaTime)
		{
			UIFrame.Tick(deltaTime);
		}
		
        protected override void Render(Image image)
        {
        	UIFrame.Render(image);
        }
	}
}
