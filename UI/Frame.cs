namespace IROM.UI
{
	using System;
	using System.Collections.Generic;
	using IROM.Util;
	using IROM.Dynamix;
	
	/// <summary>
	/// A Frame is the root <see cref="Component"/> of a UI system.
	/// </summary>
	public class Frame : IDisposable
	{
		/// <summary>
		/// Null value to prevent NullPointerExceptions.
		/// </summary>
		public static readonly Frame NULL_FRAME = new Frame();
		
		/// <summary>
		/// The display window of this <see cref="Frame"/>.
		/// </summary>
		private Window windowObj;
		
		/// <summary>
		/// The root component of this <see cref="Frame"/>.
		/// </summary>
		private Component root;
		
		/// <summary>
		/// The current size of this frame.
		/// </summary>
		public readonly Dynx<Point2D> Size = new Dynx<Point2D>();
		
		/// <summary>
		/// The input handler for this Frame.
		/// </summary>
		internal readonly FrameInputHandler InputHandler;
		
		/// <summary>
		/// A simple set of <see cref="Component"/>s to render, sorted by z value. (lower values first so they are rendered below)
		/// </summary>
		private readonly SortedSet<Component> RenderSet = new SortedSet<Component>(Comparer<Component>.Create((x, y) => ((x.ZCoord.Value > y.ZCoord.Value) ? 1 : -1)));
		
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
					windowObj.OnMouseWheel -= InputHandler.MouseWheelEvent;
					windowObj.OnKeyPress -= InputHandler.KeyPressEvent;
					windowObj.OnKeyRelease -= InputHandler.KeyReleaseEvent;
					windowObj.OnCharTyped -= InputHandler.CharTypedEvent;
				}
				windowObj = value;
				if(windowObj != null)
				{
					FrameBuffer[] buffers = windowObj.BufferStrategy.GetBuffers();
					var newFrameDirtyRegions = new Entry<Image, FastLinkedList<Rectangle>>[buffers.Length];
					for(int i = 0; i < newFrameDirtyRegions.Length; i++)
					{
						newFrameDirtyRegions[i] = new Entry<Image, FastLinkedList<Rectangle>>(buffers[i].Image, new FastLinkedList<Rectangle>());
					}
					frameDirtyRegions = newFrameDirtyRegions;
					
					windowObj.OnResize += OnResize;
					//add input listeners
					windowObj.OnMousePress += InputHandler.MousePressEvent;
					windowObj.OnMouseRelease += InputHandler.MouseReleaseEvent;
					windowObj.OnMouseMove += InputHandler.MouseMoveEvent;
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
		public Point2D MousePosition
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
		/// The lowest component attached to this Frame.
		/// </summary>
		public Component Root
		{
			get
			{
				return root;
			}
			set
			{
				if(root != null)
				{
					root.Size.Value = 1;
				}
				root = value;
				if(root != null)
				{
					root.FrameObj.Value = this;
					root.Size.Exp = () => Size.Value;
				}
				if(OnScreenChange != null) OnScreenChange(root);
			}
		}
		
		/// <summary>
		/// Called when the <see cref="Root"/> changes.
		/// </summary>
		public event Action<Component> OnScreenChange;
		
		/// <summary>
		/// Called when the <see cref="Root"/> becomes dirty.
		/// </summary>
		public event Action OnDirtyChange;
		
		/// <summary>
		/// The queue of rectangles that need to be redrawn.
		/// </summary>
		private FastLinkedList<Rectangle> dirtyRegions = new FastLinkedList<Rectangle>();
		
		/// <summary>
		/// The array of dirty lists for each buffer.
		/// </summary>
		private Entry<Image, FastLinkedList<Rectangle>>[] frameDirtyRegions = new Entry<Image, FastLinkedList<Rectangle>>[0];
		
		/// <summary>
		/// The intermediate buffer between the frame and the screen.
		/// </summary>
		private Image renderBuffer = new Image(1, 1);
		
		/// <summary>
		/// Creates a new Frame with no parent window.
		/// </summary>
		public Frame() : this(null)
		{
			
		}
		
		/// <summary>
		/// Creates a new Frame for the given Window.
		/// </summary>
		public Frame(Window window)
		{
			InputHandler = new FrameInputHandler(this);
			WindowObj = window;
			Size.OnFilter += size => VectorUtil.Max(size, 1);
		}

		public void Dispose()
		{
			//remove listeners
			WindowObj = null;
		}
		
		public void MarkDirty(Rectangle region)
		{
			bool expanded = false;
			Rectangle value;
			foreach(DoubleNode<Rectangle> node in dirtyRegions.GetNodes())
			{
				value = node.Value;
				//if already dirty, don't do more
				if(value.Contains(region))
					return;
				//if larger than prev, expand it
				if(region.Contains(value))
				{
					expanded = true;
					node.Value = region;
					break;
				}
				//if can be combined, expand it
				if((region.Min.X == value.Min.X && region.Max.X == value.Max.X) && (region.Min.Y <= value.Max.Y && region.Max.Y >= value.Min.Y))
				{
					expanded = true;
					node.Value.Min.Y = Math.Min(region.Min.Y, value.Min.Y);
					node.Value.Max.Y = Math.Max(region.Max.Y, value.Max.Y);
					break;
				}else
				if((region.Min.Y == value.Min.Y && region.Max.Y == value.Max.Y) && (region.Min.X <= value.Max.X && region.Max.X >= value.Min.X))
				{
					expanded = true;
					node.Value.Min.X = Math.Min(region.Min.X, value.Min.X);
					node.Value.Max.X = Math.Max(region.Max.X, value.Max.X);
					break;
				}
			}
			if(!expanded)
			{
				dirtyRegions.Add(region);
			}
			
			if(OnDirtyChange != null) OnDirtyChange();
		}
		
		/// <summary>
		/// Resizes this <see cref="Frame"/> to ensure it always matches its <see cref="Window"/>
		/// </summary>
		/// <param name="size">The new size.</param>
		private void OnResize(Point2D size)
		{
			Size.Value = size;
		}
		
		internal void Tick(double dt)
		{
			if(Root != null)
			{
				Root.Tick(dt);
			}
		}
		
		public void Render(Image image)
		{
			var regions = dirtyRegions;
			dirtyRegions = new FastLinkedList<Rectangle>();
			if(renderBuffer.Size != image.Size)
			{
				renderBuffer.Resize(image.Size);
				//clear all dirty regions
				regions.Clear();
				foreach(var entry in frameDirtyRegions)
				{
					entry.Value.Clear();
				}
				//add entire screen
				regions.Add(new Rectangle{Size = renderBuffer.Size});
			}
			foreach(Rectangle region in regions)
			{
				renderBuffer.PushClip(region);
				if(Root != null)
				{
					//prepare render list
					RenderSet.Clear();
					RecursiveRenderAdd(Root, RenderSet, region);
					//perform renders
					foreach(Component comp in RenderSet)
					{
						comp.BaseRender(renderBuffer);
					}
				}else
				{
					renderBuffer.Fill(RGB.Black);
				}
				renderBuffer.PopClip();
				
				foreach(var entry in frameDirtyRegions)
				{
					entry.Value.Add(region);
				}
			}
			regions.Clear();
			regions = null;
			foreach(var entry in frameDirtyRegions)
			{
				if(entry.Key == image)
				{
					regions = entry.Value;
					break;
				}
			}
			if(regions == null) throw new Exception("Frame.frameDirtyRegions has been corrupted! This shouldn't be possible!");
			foreach(Rectangle region in regions)
			{
				image.PushClip(region);
				image.Copy(renderBuffer);
				image.PopClip();
			}
			regions.Clear();
		}
		
		/// <summary>
		/// Adds the given <see cref="Component"/> to the given collection if renderable, as well as all of it's descendants.
		/// Also checks for whether the <see cref="Component"/> is in the region.
		/// </summary>
		/// <param name="comp">The <see cref="Component"/>.</param>
		/// <param name="collection">The collection to add to.</param>
		/// <param name="region">The region to check.</param>
		private void RecursiveRenderAdd(Component comp, ICollection<Component> collection, Rectangle region)
		{
			if(comp.Visible.Value)
			{
				if(VectorUtil.Overlap(region, comp.Bounds).IsValid())
				{
					if(!comp.Hidden.Value)
					{
						collection.Add(comp);
					}
					foreach(Component child in comp.Children)
					{
						RecursiveRenderAdd(child, collection, region);
					}
				}
			}
		}
	}
}
