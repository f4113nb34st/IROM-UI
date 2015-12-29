namespace IROM.UI
{
	using System;
	using IROM.Util;
	using IROM.Dynamix;
	
	/// <summary>
	/// A part of a GUI. Example components include buttons, panels, and labels.
	/// </summary>
	public abstract class Component
	{	
		/// <summary>
		/// Null parent value to prevent NullPointerExceptions for parent refering Dynxs.
		/// </summary>
		public static readonly Component NULL_PARENT;
		
		static Component()
		{
			NULL_PARENT = new Panel();
			NULL_PARENT.Position.Value = 0;
			NULL_PARENT.ZCoord.Value = 0;
			NULL_PARENT.Size.Value = 0;
			NULL_PARENT.Clip.Value = new Rectangle{Min = int.MinValue, Max = int.MaxValue};
			NULL_PARENT.FrameObj.Value = Frame.NULL_FRAME;
		}
		
		/// <summary>
		/// The parent of this <see cref="Component"/>
		/// </summary>
		public readonly Dynx<Component> Parent = new Dynx<Component>();
		
		/// <summary>
		/// Saves previous parent value for unsubscription.
		/// </summary>
		private Component prevParent = NULL_PARENT;
		
		/// <summary>
		/// The child component list.
		/// </summary>
		public readonly FastLinkedList<Component> Children = new FastLinkedList<Component>();
		
		/// <summary>
		/// The position of this <see cref="Component"/>. By default equal to the parent's position.
		/// </summary>
		public readonly Dynx<Vec2D> Position = new Dynx<Vec2D>();
		
		/// <summary>
		/// The size of this <see cref="Component"/>. By default equal to the parent's size.
		/// </summary>
		public readonly Dynx<Vec2D> Size = new Dynx<Vec2D>();
		
		/// <summary>
		/// The z coordinate of this <see cref="Component"/>. Indicates rendering order (bigger on top). By default 1 greater than its parent.
		/// </summary>
		public readonly Dynx<double> ZCoord = new Dynx<double>();
		
		/// <summary>
		/// The clipping bounds of this <see cref="Component"/>.
		/// </summary>
		public readonly Dynx<Rectangle> Clip = new Dynx<Rectangle>();
		
		/// <summary>
		/// The master <see cref="Frame"/>.
		/// </summary>
		public readonly Dynx<Frame> FrameObj = new Dynx<Frame>();
		
		/// <summary>
		/// True if this <see cref="Component"/> will be re-rendered next frame.
		/// </summary>
		public readonly Dynx<bool> Dirty = new Dynx<bool>();
		
		/// <summary>
		/// False if this <see cref="Component"/> should not be rendered. Hides children as well.
		/// </summary>
		public readonly Dynx<bool> Visible = new Dynx<bool>();
		
		/// <summary>
		/// True if this <see cref="Component"/> should not be rendered. Does not hide children.
		/// </summary>
		public readonly Dynx<bool> Hidden = new Dynx<bool>();
		
		/// <summary>
		/// True if this <see cref="Component"/> is completely visually opaque.
		/// </summary>
		public readonly Dynx<bool> Opaque = new Dynx<bool>();
		
		/// <summary>
		/// False if this <see cref="Component"/> cannot receive input events. Hides children as well.
		/// </summary>
		public readonly Dynx<bool> InputVisible = new Dynx<bool>();
		
		/// <summary>
		/// True if this <see cref="Component"/> cannot receive input events. Does not hide children.
		/// </summary>
		public readonly Dynx<bool> InputHidden = new Dynx<bool>();
		
		/// <summary>
		/// True if this <see cref="Component"/> blocks input events from reaching obscured <see cref="Component"/>s.
		/// </summary>
		public readonly Dynx<bool> InputOpaque = new Dynx<bool>();
		
		/// <summary>
		/// True if this <see cref="Component"/> is focused.
		/// </summary>
		public readonly Dynx<bool> IsFocused = new Dynx<bool>(false);
		
		/// <summary>
		/// True if the component is rendered to a buffer and blitted to the screen instead of directly rendered.
		/// Faster if the rendering is more complex than a blit.
		/// </summary>
		public bool Buffered = true;
		
		/// <summary>
		/// A "snap shot" of this component, re-rendered whenever dirty.
		/// </summary>
		protected internal Image Rendering = new Image(1, 1);
		
		/// <summary>
		/// Invoked before a render call.
		/// </summary>
		public event Action OnPreRender;
		
		/// <summary>
		/// Invoked after a render call.
		/// </summary>
		public event Action OnPostRender;
		
		/// <summary>
		/// Invoked during a tick.
		/// </summary>
		public event Action<double> OnTick;
		
		/// <summary>
		/// Invoked when this <see cref="Component"/> is destroyed.
		/// </summary>
		public event Action OnDestroy;
		
		/// <summary>
		/// Invoked whenever a <see cref="MouseButton"/> is pressed.
		/// A mouse press event on this <see cref="Component"/> happens whenever the mouse is 
		/// pressed within the bounds of this <see cref="Component"/> and there
		/// are no opaque <see cref="Component"/>s higher in the z order in the 
		/// mouse location.
		/// Argument is button that was pressed.
		/// </summary>
		public event Action<MouseButton> OnMousePress;
		
		/// <summary>
		/// Invoked whenever a <see cref="MouseButton"/> is released.
		/// A mouse release event on this <see cref="Component"/> happens whenever the mouse is 
		/// released within the bounds of this <see cref="Component"/> and there
		/// are no opaque <see cref="Component"/>s higher in the z order in the 
		/// mouse location. Also guaranteed to be called whenever this <see cref="Component"/>
		/// previously received a mouse press event without a matching mouse release event for the same button.
		/// Argument is button that was released.
		/// </summary>
		public event Action<MouseButton> OnMouseRelease;
		
		/// <summary>
		/// Invoked whenever the mouse is moved.
		/// A mouse move event on this <see cref="Component"/> happens whenever the mouse is
		/// moved within the bounds of this <see cref="Component"/> and there
		/// are no opaque <see cref="Component"/>s higher in the z order in the 
		/// mouse location. Also guaranteed to be called whenever this <see cref="Component"/>
		/// previously received a mouse press event without a matching mouse release event for the same button.
		/// First Argument is mouse location.
		/// Second Argument is mouse delta.
		/// </summary>
		public event Action<Point2D, Point2D> OnMouseMove;
		
		/// <summary>
		/// Invoked whenever the mouse wheel is moved.
		/// A wheel event on this <see cref="Component"/> happens whenever the mouse wheel 
		/// is rotated within the bounds of this <see cref="Component"/> and there
		/// are no opaque <see cref="Component"/>s higher in the z order in the 
		/// mouse location. 
		/// Argument is mouse location.
		/// </summary>
		public event Action<int> OnMouseWheel;
		
		/// <summary>
		/// Invoked whenever the mouse enters the bounds of this <see cref="Component"/>.
		/// A mouse enter event on this <see cref="Component"/> happens whenever the mouse is
		/// moved within the bounds of this <see cref="Component"/> and there
		/// are no opaque <see cref="Component"/>s higher in the z order in the 
		/// mouse location.
		/// </summary>
		public event Action OnMouseEnter;
		
		/// <summary>
		/// Invoked whenever the mouse leaves the bounds of this <see cref="Component"/>.
		/// A mouse leave event on this <see cref="Component"/> happens whenever the mouse is
		/// moved within the bounds of this <see cref="Component"/> and there
		/// are no opaque <see cref="Component"/>s higher in the z order in the 
		/// mouse location.
		/// </summary>
		public event Action OnMouseExit;
		
		/// <summary>
		/// Invoked whenever a <see cref="KeyboardButton"/> is pressed.
		/// A key press event on this <see cref="Component"/> happens whenever a keyboard key 
		/// is pressed while the mouse within the bounds of this <see cref="Component"/> and there
		/// are no opaque <see cref="Component"/>s higher in the z order in the 
		/// mouse location. If no <see cref="Component"/> consumes the event,
		/// it is passed to the last clicked opaque <see cref="Component"/> that receives any key event 
		/// (<see cref="OnKeyPress"/>, <see cref="OnKeyRelease"/>, or <see cref="OnCharTyped"/>).
		/// <see cref="Component"/>s that do not also receive <see cref="OnMousePress"/> are not eligible for these
		/// delayed events.
		/// Argument is key pressed.
		/// </summary>
		public event Action<KeyboardButton> OnKeyPress;
		
		/// <summary>
		/// Invoked whenever a <see cref="KeyboardButton"/> is released.
		/// A key release event on this <see cref="Component"/> happens whenever a keyboard key 
		/// is released while the mouse within the bounds of this <see cref="Component"/> and there
		/// are no opaque <see cref="Component"/>s higher in the z order in the 
		/// mouse location. Also guaranteed to be called whenever this <see cref="Component"/>
		/// previously recieved a key press event for the same key. If no <see cref="Component"/> consumes the event,
		/// it is passed to the last clicked opaque <see cref="Component"/> that receives any key event 
		/// (<see cref="OnKeyPress"/>, <see cref="OnKeyRelease"/>, or <see cref="OnCharTyped"/>).
		/// <see cref="Component"/>s that do not also receive <see cref="OnMousePress"/> are not eligible for these
		/// delayed events.
		/// Argument is key released.
		/// </summary>
		public event Action<KeyboardButton> OnKeyRelease;
		
		/// <summary>
		/// Invoked whenever a character is typed.
		/// A char typed event on this <see cref="Component"/> happens whenever a keyboard key 
		/// is pressed that causes a text character while the mouse within the bounds of this 
		/// <see cref="Component"/> and there are no opaque <see cref="Component"/>s higher in the z order 
		/// in the mouse location. If no <see cref="Component"/> consumes the event,
		/// it is passed to the last clicked opaque <see cref="Component"/> that receives any key event 
		/// (<see cref="OnKeyPress"/>, <see cref="OnKeyRelease"/>, or <see cref="OnCharTyped"/>).
		/// <see cref="Component"/>s that do not also receive <see cref="OnMousePress"/> are not eligible for these
		/// delayed events.
		/// First Argument is char typed.
		/// Second Argument is true if a repeat from holding down key.
		/// </summary>
		public event Action<char, bool> OnCharTyped;
		
		/// <summary>
		/// The bounds of this <see cref="Component"/>. By default equal to parent's bounds.
		/// </summary>
		public Rectangle Bounds
		{
			get
			{
				return new Rectangle{Position = (Point2D)Position.Value, Size = (Point2D)Size.Value};
			}
		}
		
		/// <summary>
		/// Returns the current position of the mouse, local to this component.
		/// </summary>
		public Point2D MousePosition
		{
			get
			{
				return FrameObj.Value.MousePosition - (Point2D)Position.Value;
			}
		}
		
		/// <summary>
		/// Saves the previous dirty region for dirtying.
		/// </summary>
		private Rectangle prevRegion;
		
		protected Component()
		{
			//special case for NULL_PARENT to prevent errors
			if(NULL_PARENT == null) return;
			
			//init to null parent
			Parent.Value = NULL_PARENT;
			//don't allow null parents
			Parent.OnFilter += comp => (comp ?? NULL_PARENT);
			//automatically reparent
			EnableAutoReparent();
			//on update, update children collections
			Parent.OnUpdate += () =>
			{
				OnParentChange(prevParent, Parent.Value);
				prevParent = Parent.Value;
			};
			
			//set default values
			Position.Exp = () => Parent.Value.Position.Value;
			Size.Exp = () => Parent.Value.Size.Value;
			ZCoord.Exp = () => Parent.Value.ZCoord.Value + 1;
			Clip.Exp = () => Parent.Value.Clip.Value;
			FrameObj.Exp = () => Parent.Value.FrameObj.Value;
			Dirty.Value = true;
			Visible.Value = true;
			Hidden.Value = false;
			Opaque.Value = true;
			InputVisible.Exp = () => Visible.Value;
			InputHidden.Exp = () => Hidden.Value;
			InputOpaque.Exp = () => Opaque.Value;
			
			//subscribe changes
			Position.OnUpdate += MarkRegionDirty;
			Size.OnUpdate += MarkDirty;
			Clip.OnUpdate += MarkRegionDirty;
			ZCoord.OnUpdate += MarkRegionDirty;
			Visible.OnUpdate += () => MarkRegionDirty(true);
			Hidden.OnUpdate += () => MarkRegionDirty(true);
			Opaque.OnUpdate += MarkRegionDirty;
		}
		
		~Component()
		{
			if(OnDestroy != null) OnDestroy();
		}
		
		/// <summary>
		/// Enables automatic reparenting when <see cref="Parent"/> is set.
		/// Enabled by default.
		/// </summary>
		public void EnableAutoReparent()
		{
			Parent.OnFilter += Reparent;
		}
		
		/// <summary>
		/// Disables automatic reparenting when <see cref="Parent"/> is set.
		/// Enabled by default.
		/// </summary>
		public void DisableAutoReparent()
		{
			Parent.OnFilter -= Reparent;
		}
		
		protected internal Component Reparent(Component parent)
		{
			return parent.GetParentFor(this);
		}
		
		protected virtual void OnParentChange(Component oldParent, Component newParent)
		{
			if(oldParent != NULL_PARENT) oldParent.Children.Remove(this);
			if(newParent != NULL_PARENT) newParent.Children.Add(this);
		}
		
		/// <summary>
		/// Marks this <see cref="Component"/> for re-rendering, and mark's its bounds as dirty.
		/// </summary>
		protected void MarkDirty()
		{
			Dirty.Value = true;
			MarkRegionDirty();
		}
		
		/// <summary>
		/// Marks both this <see cref="Component"/>'s bounds and the old bounds as dirty.
		/// Only dirties regions that might have changed.
		/// </summary>
		protected void MarkRegionDirty()
		{
			MarkRegionDirty(false);
		}
		
		/// <summary>
		/// Marks both this <see cref="Component"/>'s bounds and the old bounds as dirty.
		/// Only dirties regions that might have changed.
		/// </summary>
		protected void MarkRegionDirty(bool force)
		{
			if((Visible.Value == true && Hidden.Value == false) || force)
			{
				Rectangle dirtyRegion = VectorUtil.Overlap(Bounds, Clip.Value);
				FrameObj.Value.MarkDirty(VectorUtil.Encompass(dirtyRegion, prevRegion));
				prevRegion = dirtyRegion;
			}
		}
		
		/// <summary>
		/// Returns the parent that should be used for the given child. 
		/// Used by components like buttons to hand parentage off to their content panels.
		/// </summary>
		/// <param name="child">The child component.</param>
		/// <returns>The parent to use. Frequently this.</returns>
		protected virtual Component GetParentFor(Component child)
		{
			return this;
		}
		
		/// <summary>
		/// Ticks this <see cref="Component"/>.
		/// By default calls TickChildren().
		/// </summary>
		/// <param name="dt">The time since the last tick.</param>
		protected internal virtual void Tick(double dt)
		{
			if(OnTick != null) OnTick(dt);
			TickChildren(dt);
		}
		
		/// <summary>
		/// Ticks the children of this <see cref="Component"/>.
		/// </summary>
		/// <param name="dt">The time since the last tick.</param>
		protected void TickChildren(double dt)
		{
			foreach(Component child in Children)
			{
				child.Tick(dt);
			}
		}
		
		/// <summary>
		/// Renders this <see cref="Component"/> to the given image.
		/// </summary>
		/// <param name="image">The render target.</param>
		protected abstract void Render(Image image);
		
		/// <summary>
		/// Calls <see cref="OnPreRender"/>.
		/// </summary>
		protected void PreRender()
		{
			if(OnPreRender != null) OnPreRender();
		}
		
		/// <summary>
		/// Calls <see cref="OnPostRender"/>.
		/// </summary>
		protected void PostRender()
		{
			if(OnPostRender != null) OnPostRender();
		}
		
		/// <summary>
		/// Renders this <see cref="Component"/> and its kids. For most cases, use Render().
		/// </summary>
		/// <param name="image">The render target.</param>
		protected internal virtual void BaseRender(Image image)
		{
			PreRender();
			if(Buffered)
			{
				bool dirty = Dirty.Value;
				if(Rendering.Width != Math.Max(1, (int)Math.Round(Size.Value.X)) || Rendering.Height != Math.Max(1, (int)Math.Round(Size.Value.Y)))
				{
					dirty = true;
					Rendering.Resize(Math.Max(1, (int)Math.Round(Size.Value.X)), Math.Max(1, (int)Math.Round(Size.Value.Y)));
				}
				if(dirty)
				{
					Dirty.Value = false;
					Render(Rendering);
				}
				image.PushClip(Clip.Value);
				image.Blit(Rendering, (Point2D)Position.Value, Opaque.Value ? RenderMode.MASK : RenderMode.BLEND);
				image.PopClip();
			}else
			{
				image.PushClip(Clip.Value);
				Render(image);
				image.PopClip();
			}
			PostRender();
		}
		
		internal bool HasListeners(InputEventType type)
		{
			switch(type)
			{
				case InputEventType.MPress: return OnMousePress != null;
				case InputEventType.MRelease: return OnMouseRelease != null;
				case InputEventType.MMove: return OnMouseMove != null || OnMouseEnter != null || OnMouseExit != null;//enter and exit are functions of move
				case InputEventType.MWheel: return OnMouseWheel != null;
				case InputEventType.MEnter: return OnMouseEnter != null;
				case InputEventType.MExit: return OnMouseExit != null;
				case InputEventType.KPress: return OnKeyPress != null;
				case InputEventType.KRelease: return OnKeyRelease != null;
				case InputEventType.CTyped: return OnCharTyped != null;
				default: return false;
			}
		}
		
		internal void InvokeMousePress(MouseButton button){if(OnMousePress != null) OnMousePress(button);}
		internal void InvokeMouseRelease(MouseButton button){if(OnMouseRelease != null) OnMouseRelease(button);}
		internal void InvokeMouseMove(Point2D coords, Point2D delta){if(OnMouseMove != null) OnMouseMove(coords, delta);}
		internal void InvokeMouseWheel(int delta){if(OnMouseWheel != null) OnMouseWheel(delta);}
		internal void InvokeMouseEnter(){if(OnMouseEnter != null) OnMouseEnter();}
		internal void InvokeMouseExit(){if(OnMouseExit != null) OnMouseExit();}
		internal void InvokeKeyPress(KeyboardButton button){if(OnKeyPress != null) OnKeyPress(button);}
		internal void InvokeKeyRelease(KeyboardButton button){if(OnKeyRelease != null) OnKeyRelease(button);}
		internal void InvokeCharTyped(char c, bool r){if(OnCharTyped != null) OnCharTyped(c, r);}
	}
}
