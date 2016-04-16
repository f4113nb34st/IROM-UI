namespace IROM.UI
{
	using System;
	using System.Collections.Generic;
	using IROM.Util;
	using IROM.Dynamix;
	
	/// <summary>
	/// A Root is the root <see cref="Component"/> of a UI system.
	/// </summary>
	public class Root : Component, IDisposable
	{
		/// <summary>
		/// The display window of this <see cref="Root"/>.
		/// </summary>
		private Window windowObj;
		
		/// <summary>
		/// The input handler for this Root.
		/// </summary>
		internal readonly RootInputHandler InputHandler;
		
		/// <summary>
		/// The parent window.
		/// </summary>
		public Window WindowObj
		{
			get
			{
				return windowObj;
			}
			protected set
			{
				if(windowObj != null)
				{
					windowObj.OnResize -= OnResize;
					//remove input listeners
					windowObj.OnMousePress -= InputHandler.MousePressEvent;
					windowObj.OnMouseRelease -= InputHandler.MouseReleaseEvent;
					windowObj.OnMouseMove -= InputHandler.MouseMoveEvent;
					windowObj.OnMouseExit -= InputHandler.MouseExitEvent;
					windowObj.OnMouseWheel -= InputHandler.MouseWheelEvent;
					windowObj.OnKeyPress -= InputHandler.KeyPressEvent;
					windowObj.OnKeyRelease -= InputHandler.KeyReleaseEvent;
					windowObj.OnCharTyped -= InputHandler.CharTypedEvent;
				}
				windowObj = value;
				if(windowObj != null)
				{
					RenderBuffer[] buffers = windowObj.BufferStrategy.GetBuffers();
					var newRootDirtyRegions = new Entry<Image, RegionSet>[buffers.Length];
					for(int i = 0; i < newRootDirtyRegions.Length; i++)
					{
						newRootDirtyRegions[i] = new Entry<Image, RegionSet>(buffers[i].Image, new RegionSet());
					}
					rootDirtyRegions = newRootDirtyRegions;
					
					windowObj.OnResize += OnResize;
					//add input listeners
					windowObj.OnMousePress += InputHandler.MousePressEvent;
					windowObj.OnMouseRelease += InputHandler.MouseReleaseEvent;
					windowObj.OnMouseMove += InputHandler.MouseMoveEvent;
					windowObj.OnMouseExit += InputHandler.MouseExitEvent;
					windowObj.OnMouseWheel += InputHandler.MouseWheelEvent;
					windowObj.OnKeyPress += InputHandler.KeyPressEvent;
					windowObj.OnKeyRelease += InputHandler.KeyReleaseEvent;
					windowObj.OnCharTyped += InputHandler.CharTypedEvent;
				}
			}
		}
		
		/// <summary>
		/// The current coords of the mouse.
		/// </summary>
		public override Point2D MousePosition
		{
			get
			{
				if(WindowObj != null)
				{
					return WindowObj.InputStates.MousePosition;
				}else
				{
					return default(Point2D);
				}
			}
		}
		
		/// <summary>
		/// The queue of rectangles that need to be redrawn.
		/// </summary>
		private RegionSet dirtyRegions = new RegionSet();
		
		/// <summary>
		/// The array of dirty lists for each buffer.
		/// </summary>
		private Entry<Image, RegionSet>[] rootDirtyRegions = new Entry<Image, RegionSet>[0];
		
		/// <summary>
		/// Creates a new Root with no parent window.
		/// </summary>
		public Root() : this(null)
		{
			
		}
		
		/// <summary>
		/// Creates a new Root for the given Window.
		/// </summary>
		public Root(Window window)
		{
			//set values
			Position.Value = 0;
			Size.Value = 1;
			Size.OnFilter += size => VectorUtil.Max(size, 1);
			Size.OnUpdate += () => Dirty.Value = true;
			ZCoord.Value = 0;
			Clip.Value = new Rectangle{Min = int.MinValue, Max = int.MaxValue};
			RootObj.Value = this;
			Dirty.Value = true;
			Visible.Value = true;
			Hidden.Value = true;
			Opaque.Value = true;
			InputVisible.Exp = () => Visible.Value;
			InputHidden.Exp = () => Hidden.Value;
			InputOpaque.Exp = () => Opaque.Value;
			
			InputHandler = new RootInputHandler(this);
			WindowObj = window;
		}

		public void Dispose()
		{
			//remove listeners
			WindowObj = null;
		}
		
		/// <summary>
		/// Finds every visible, non-hidden component in the given location in decreasing z order.
		/// </summary>
		/// <param name="location">The location.</param>
		/// <param name="findInvisible">If true, the search will include invisible and hidden components.</param>
		/// <param name="inputMode">If true, uses input visible and hidden.</param>
		/// <returns>The components in z order.</returns>
		public Component[] FindAll(Point2D location, bool findInvisible = false, bool inputMode = false)
		{
			return FindAll(new Rectangle{Min = location, Max = location}, findInvisible, inputMode);
		}
		
		/// <summary>
		/// Finds every visible, non-hidden component in the given region in decreasing z order.
		/// </summary>
		/// <param name="region">The region.</param>
		/// <param name="findInvisible">If true, the search will include invisible and hidden components.</param>
		/// <param name="inputMode">If true, uses input visible and hidden.</param>
		/// <returns>The components in z order.</returns>
		public Component[] FindAll(Rectangle region, bool findInvisible = false, bool inputMode = false)
		{
			List<Component> comps = new List<Component>();
			RecursiveAdd(region, findInvisible, inputMode, this, comps);
			Component[] array = comps.ToArray();
			Array.Sort(array, (a, b) => 
			{
	           	if(b.ZCoord.Value > a.ZCoord.Value) return 1;
	           	if(b.ZCoord.Value < a.ZCoord.Value) return -1;
	           	return 0;
			});
			return array;
		}
		
		/// <summary>
		/// Adds the given <see cref="Component"/> to the given collection if renderable, as well as all of it's descendants,
		///if they are visible, non-hidden, and overlap the given region.
		/// </summary>
		/// <param name="region">The region to check.</param>
		/// <param name="findInvisible">If true, the search will include invisible and hidden components.</param>
		/// <param name="inputMode">If true, uses input visible and hidden.</param>
		/// <param name="comp">The <see cref="Component"/>.</param>
		/// <param name="collection">The collection to add to.</param>
		private void RecursiveAdd(Rectangle region, bool findInvisible, bool inputMode, Component comp, ICollection<Component> collection)
		{
			if(findInvisible || (inputMode ? comp.InputVisible.Value : comp.Visible.Value))
			{
				if((findInvisible || !(inputMode ? comp.InputHidden.Value : comp.Hidden.Value)) && ShapeUtil.Overlap(region, comp.Bounds).IsValid())
				{
					collection.Add(comp);
				}
				foreach(Component child in comp.Children)
				{
					RecursiveAdd(region, findInvisible, inputMode, child, collection);
				}
			}
		}
		
		/// <summary>
		/// Marks the given region as dirty for re-rendering.
		/// </summary>
		/// <param name="region">The region to dirty.</param>
		public void MarkDirty(Rectangle region)
		{
			dirtyRegions.Add(region);
			Dirty.Value = true;
		}
		
		/// <summary>
		/// Resizes this <see cref="Root"/> to ensure it always matches its <see cref="Window"/>
		/// </summary>
		/// <param name="size">The new size.</param>
		private void OnResize(Point2D size)
		{
			Size.Value = size;
		}
		
		internal void RootRender(Image image)
		{
			PreRender();
			Dirty.Value = false;
			if(RenderBuffer.Size != image.Size)
			{
				RenderBuffer.Resize(image.Width, image.Height);
				//clear all dirty regions
				dirtyRegions.Clear();
				foreach(var entry in rootDirtyRegions)
				{
					entry.Value.Clear();
				}
				//add entire screen
				dirtyRegions.Add(new Rectangle{Size = RenderBuffer.Size});
			}
			foreach(Rectangle region in dirtyRegions)
			{
				RenderBuffer.PushClip(region);

				//prepare render list
				Component[] renderList = FindAll(region);
				//perform renders
				for(int i = renderList.Length - 1; i >= 0; i--)
				{
					renderList[i].BaseRender(RenderBuffer);
				}
				
				RenderBuffer.PopClip();
				
				foreach(var entry in rootDirtyRegions)
				{
					entry.Value.Add(region);
				}
			}
			RegionSet frameRegions = null;
			foreach(var entry in rootDirtyRegions)
			{
				if(entry.Key == image)
				{
					frameRegions = entry.Value;
					break;
				}
			}
			if(frameRegions == null) throw new Exception("Root.rootDirtyRegions has been corrupted! This shouldn't be possible!");
			foreach(Rectangle region in frameRegions)
			{
				image.PushClip(region);
				image.Blit(RenderBuffer);
				image.PopClip();
			}
			PostRender();
		}
		
		protected override void Render(Image image)
		{
			//do nothing
		}
	}
}
