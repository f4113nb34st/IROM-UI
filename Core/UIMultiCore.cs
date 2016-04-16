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
		protected UIMultiCore(String title) : this(title, 60){}
		
		protected UIMultiCore(String title, double tickRate) : base(title, tickRate, typeof(DoubleBufferStrategy))
        {
			RootObj = new Root(WindowObj);
			RootObj.Dirty.OnUpdate += () => {if(RootObj.Dirty.Value) MarkDirty();};
			AutoDirty = false;
			//initial dirty to get started
			MarkDirty();
		}
		
		protected Root RootObj;
		
		protected override void Tick(double deltaTime)
		{
			RootObj.Tick(deltaTime);
		}
		
        protected override void Render(Image image)
        {
        	RootObj.RootRender(image);
        }
	}
}
