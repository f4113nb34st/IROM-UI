namespace IROM.UI
{
	using System;
	using System.Collections.Generic;
	using IROM.Util;
	
	/// <summary>
	/// A Frame is the root <see cref="Component"/> of a UI system.
	/// </summary>
	public class Frame : IDisposable
	{
		/// <summary>
		/// The display window of this <see cref="Frame"/>.
		/// </summary>
		private Window BaseWindow;
		
		/// <summary>
		/// The screen of this <see cref="Frame"/>.
		/// </summary>
		private Screen BaseScreen;
		
		/// <summary>
		/// A simple set of <see cref="Component"/>s to render, sorted by z value. (lower values first so they are rendered below)
		/// </summary>
		private readonly SortedSet<Component> RenderSet = new SortedSet<Component>(Comparer<Component>.Create((x, y) => ((x.ZCoord.Value > y.ZCoord.Value) ? 1 : -1)));
		
		/// <summary>
		/// The parent window.
		/// </summary>
		public Window CurrentWindow
		{
			get
			{
				return BaseWindow;
			}
			protected set
			{
				if(value == null)
				{
					// disable once NotResolvedInText
					throw new ArgumentNullException("Window cannot be null!");
				}
				if(BaseWindow != null)
				{
					BaseWindow.OnResize -= OnResize;
					//remove input listeners
					BaseWindow.OnMousePress -= InputHandler.MousePressEvent;
					BaseWindow.OnMouseRelease -= InputHandler.MouseReleaseEvent;
					BaseWindow.OnMouseMove -= InputHandler.MouseMoveEvent;
					BaseWindow.OnMouseWheel -= InputHandler.MouseWheelEvent;
					BaseWindow.OnKeyPress -= InputHandler.KeyPressEvent;
					BaseWindow.OnKeyRelease -= InputHandler.KeyReleaseEvent;
					BaseWindow.OnCharTyped -= InputHandler.CharTypedEvent;
				}
				BaseWindow = value;
				BaseWindow.OnResize += OnResize;
				//add input listeners
				BaseWindow.OnMousePress += InputHandler.MousePressEvent;
				BaseWindow.OnMouseRelease += InputHandler.MouseReleaseEvent;
				BaseWindow.OnMouseMove += InputHandler.MouseMoveEvent;
				BaseWindow.OnMouseWheel += InputHandler.MouseWheelEvent;
				BaseWindow.OnKeyPress += InputHandler.KeyPressEvent;
				BaseWindow.OnKeyRelease += InputHandler.KeyReleaseEvent;
				BaseWindow.OnCharTyped += InputHandler.CharTypedEvent;
			}
		}
		
		internal readonly FrameInputHandler InputHandler;
		
		public Screen CurrentScreen
		{
			get
			{
				return BaseScreen;
			}
			set
			{
				BaseScreen = value;
				if(BaseScreen != null)
				{
					BaseScreen.Size.Pixels = new Vec2D(CurrentWindow.Width, CurrentWindow.Height);
				}
				if(OnScreenChange != null) OnScreenChange(this, BaseScreen);
			}
		}
		
		/// <summary>
		/// Called when the <see cref="CurrentScreen"/> changes.
		/// </summary>
		public event EventHandler<Screen> OnScreenChange;
		
		/// <summary>
		/// Creates a new Frame for the given Window.
		/// </summary>
		public Frame(Window window)
		{
			InputHandler = new FrameInputHandler(this);
			CurrentWindow = window;
		}

		public void Dispose()
		{
			//remove listeners
			if(BaseWindow != null)
			{
				BaseWindow.OnResize -= OnResize;
			}
		}
		
		/// <summary>
		/// Resizes this <see cref="Frame"/> to ensure it always matches its <see cref="Window"/>
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="args">The resize arguments.</param>
		protected void OnResize(object sender, ResizeEventArgs args)
		{
			if(CurrentScreen != null)
			{
				CurrentScreen.Size.Pixels = args.Size;
			}
		}
		
		internal void Tick(double dt)
		{
			if(CurrentScreen != null)
			{
				CurrentScreen.Tick(dt);
			}
		}
		
		public void Render(Image image)
		{
			if(CurrentScreen != null)
			{
				//prepare render list
				RenderSet.Clear();
				RecursiveRenderAdd(CurrentScreen, RenderSet);
				//perform renders
				foreach(Component comp in RenderSet)
				{
					comp.BaseRender(image);
				}
			}else
			{
				image.Fill(RGB.Black);
			}
		}
		
		/// <summary>
		/// Adds the given <see cref="Component"/> to the given collection if renderable, as well as all of it's descendants.
		/// </summary>
		/// <param name="comp">The <see cref="Component"/>.</param>
		/// <param name="collection">The collection to add to.</param>
		private void RecursiveRenderAdd(Component comp, ICollection<Component> collection)
		{
			if(comp.Visible)
			{
				if(!comp.Hidden)
				{
					collection.Add(comp);
				}
				foreach(Component child in comp.Children)
				{
					RecursiveRenderAdd(child, collection);
				}
			}
		}
	}
}
