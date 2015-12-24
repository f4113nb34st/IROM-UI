namespace IROM.Core
{
	using System;
	using IROM.UI;
	using IROM.Util;
	
	/// <summary>
    /// Abstract base class for multi-threaded <see cref="Core"/>s with UISystems.
    /// </summary>
	public abstract class UIMultiCore : MultiCore
	{
		protected UIMultiCore(String title) : base(title)
        {
			UIFrame = new Frame(WindowObj);
			UIFrame.OnDirtyChange += (sender, e) => MarkDirty();
			AutoDirty = false;
		}
		
		protected UIMultiCore(String title, double tickRate) : base(title, tickRate)
        {
			UIFrame = new Frame(WindowObj);
			UIFrame.OnDirtyChange += (sender, e) => MarkDirty();
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
