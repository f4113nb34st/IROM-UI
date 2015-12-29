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
		protected UICore(String title) : base(title, typeof(DoubleBufferStrategy))
        {
			UIFrame = new Frame(WindowObj);
			UIFrame.OnDirtyChange += MarkDirty;
			AutoDirty = false;
		}
		
		protected UICore(String title, double tickRate) : base(title, tickRate, typeof(DoubleBufferStrategy))
        {
			UIFrame = new Frame(WindowObj);
			UIFrame.OnDirtyChange += MarkDirty;
			AutoDirty = false;
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
