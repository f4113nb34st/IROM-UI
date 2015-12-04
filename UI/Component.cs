namespace IROM.UI
{
	using System;
	using IROM.Util;
	
	/// <summary>
	/// A part of a GUI. Example components include buttons, panels, and labels.
	/// </summary>
	public abstract class Component
	{
		/// <summary>
		/// The parent <see cref="Component"/>.
		/// </summary>
		public readonly Component Parent;
		
		/// <summary>
		/// The child component list.
		/// </summary>
		public readonly LocklessLinkedList<Component> Children = new LocklessLinkedList<Component>();
		
		//the backing variable classes
		private UIPosition position;
		private UIZCoord zCoord;
		private UISize size;
		private UIClip clip;
		private bool isFocused;
		private bool visible = true;
		private bool hidden = false;
		private volatile bool dirty = true;
		
		/// <summary>
		/// The master <see cref="Frame"/>.
		/// </summary>
		public virtual Frame MasterFrame
		{
			get
			{
				return Parent != null ? Parent.MasterFrame : null;
			}
		}
		
		/// <summary>
		/// The master <see cref="Frame"/>.
		/// </summary>
		public virtual Screen MasterScreen
		{
			get
			{
				return Parent != null ? Parent.MasterScreen : null;
			}
		}
		
		/// <summary>
		/// False if this <see cref="Component"/> should not be rendered. Hides children as well.
		/// Also prevents receiving input events.
		/// </summary>
		public bool Visible
		{
			get{return visible;}
			set
			{
				visible = value;
				MarkMasterDirty(this, EventArgs.Empty);
			}
		}
		
		/// <summary>
		/// True if this <see cref="Component"/> should not be rendered. Does not hide children.
		/// Also prevents receiving input events.
		/// </summary>
		public bool Hidden
		{
			get{return hidden;}
			set
			{
				hidden = value;
				MarkMasterDirty(this, EventArgs.Empty);
			}
		}
		
		/// <summary>
		/// True if this <see cref="Component"/> should be re-rendered next frame.
		/// </summary>
		public virtual bool Dirty
		{
			get{return dirty;}
			set
			{
				dirty = value;
				if(dirty) MarkMasterDirty(this, EventArgs.Empty);
			}
		}
		
		/// <summary>
		/// True if the component is rendered to a buffer and blitted to the screen instead of directly rendered.
		/// Faster, but does not support incomplete opacity.
		/// </summary>
		public bool Buffered = true;
		
		/// <summary>
		/// True if this <see cref="Component"/> is completely visually opaque.
		/// </summary>
		public bool Opaque = true;
		
		/// <summary>
		/// False if this <see cref="Component"/> cannot receive input events. Hides children as well.
		/// </summary>
		public bool InputVisible = true;
		
		/// <summary>
		/// False if this <see cref="Component"/> cannot receive input events. Does not hide children.
		/// </summary>
		public bool InputHidden = false;
		
		/// <summary>
		/// True if this <see cref="Component"/> blocks input events from reaching obscured <see cref="Component"/>s.
		/// </summary>
		public bool InputOpaque = true;
		
		/// <summary>
		/// A "snap shot" of this component, re-rendered whenever dirty.
		/// </summary>
		protected internal Image Rendering = new Image(1, 1);
		
		/// <summary>
		/// Gets or sets the position of this <see cref="Component"/>. By default equal to the parent's position.
		/// </summary>
		public UIPosition Position
		{
			get
			{
				return position;
			}
			set
			{
				if(position != value)
				{
					position = value;
					if(OnPositionChange != null) OnPositionChange(this, position);
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the z coordinate of this <see cref="Component"/>. Indicates rendering order (bigger on top). By default 1 greater than its parent.
		/// </summary>
		public UIZCoord ZCoord
		{
			get
			{
				return zCoord;
			}
			set
			{
				if(zCoord != value)
				{
					zCoord = value;
					if(OnZCoordChange != null) OnZCoordChange(this, zCoord);
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the size of this <see cref="Component"/>. By default equal to the parent's size.
		/// </summary>
		public UISize Size
		{
			get
			{
				return size;
			}
			set
			{
				if(size != value)
				{
					size = value;
					if(OnSizeChange != null) OnSizeChange(this, size);
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the clipping bounds of this <see cref="Component"/>.
		/// </summary>
		public UIClip Clip
		{
			get
			{
				return clip;
			}
			set
			{
				if(clip != value)
				{
					clip = value;
					if(OnClipChange != null) OnClipChange(this, clip);
				}
			}
		}
		
		/// <summary>
		/// True if this <see cref="Component"/> is focused.
		/// </summary>
		public bool IsFocused
		{
			get
			{
				return isFocused;
			}
			internal set
			{
				if(isFocused != value)
				{
					isFocused = value;
					if(OnFocusChange != null) OnFocusChange(this, isFocused);
				}
			}
		}
		
		/// <summary>
		/// Invoked whenever <see cref="Position"/> changes.
		/// </summary>
		public event EventHandler<UIPosition> OnPositionChange;
		
		/// <summary>
		/// Invoked whenever <see cref="ZCoord"/> changes.
		/// </summary>
		public event EventHandler<UIZCoord> OnZCoordChange;
		
		/// <summary>
		/// Invoked whenever <see cref="Size"/> changes.
		/// </summary>
		public event EventHandler<UISize> OnSizeChange;
		
		/// <summary>
		/// Invoked whenever <see cref="Clip"/> changes.
		/// </summary>
		public event EventHandler<UIClip> OnClipChange;
		
		/// <summary>
		/// Invoked whenever <see cref="IsFocused"/> changes.
		/// </summary>
		public event EventHandler<bool> OnFocusChange;
		
		/// <summary>
		/// Invoked before a render call.
		/// </summary>
		public event EventHandler OnPreRender;
		
		/// <summary>
		/// Invoked after a render call.
		/// </summary>
		public event EventHandler OnPostRender;
		
		/// <summary>
		/// Invoked during a tick.
		/// </summary>
		public event EventHandler<double> OnTick;
		
		/// <summary>
		/// Invoked whenever a <see cref="MouseButton"/> is pressed.
		/// A mouse press event on this <see cref="Component"/> happens whenever the mouse is 
		/// pressed within the bounds of this <see cref="Component"/> and there
		/// are no opaque <see cref="Component"/>s higher in the z order in the 
		/// mouse location.
		/// </summary>
		public event EventHandler<MouseButtonEventArgs> OnMousePress;
		
		/// <summary>
		/// Invoked whenever a <see cref="MouseButton"/> is released.
		/// A mouse release event on this <see cref="Component"/> happens whenever the mouse is 
		/// released within the bounds of this <see cref="Component"/> and there
		/// are no opaque <see cref="Component"/>s higher in the z order in the 
		/// mouse location. Also guaranteed to be called whenever this <see cref="Component"/>
		/// previously received a mouse press event without a matching mouse release event for the same button.
		/// </summary>
		public event EventHandler<MouseButtonEventArgs> OnMouseRelease;
		
		/// <summary>
		/// Invoked whenever the mouse is moved.
		/// A mouse move event on this <see cref="Component"/> happens whenever the mouse is
		/// moved within the bounds of this <see cref="Component"/> and there
		/// are no opaque <see cref="Component"/>s higher in the z order in the 
		/// mouse location. Also guaranteed to be called whenever this <see cref="Component"/>
		/// previously received a mouse press event without a matching mouse release event for the same button.
		/// </summary>
		public event EventHandler<MouseMoveEventArgs> OnMouseMove;
		
		/// <summary>
		/// Invoked whenever the mouse wheel is moved.
		/// A wheel event on this <see cref="Component"/> happens whenever the mouse wheel 
		/// is rotated within the bounds of this <see cref="Component"/> and there
		/// are no opaque <see cref="Component"/>s higher in the z order in the 
		/// mouse location. 
		/// </summary>
		public event EventHandler<MouseWheelEventArgs> OnMouseWheel;
		
		/// <summary>
		/// Invoked whenever the mouse enters the bounds of this <see cref="Component"/>.
		/// A mouse enter event on this <see cref="Component"/> happens whenever the mouse is
		/// moved within the bounds of this <see cref="Component"/> and there
		/// are no opaque <see cref="Component"/>s higher in the z order in the 
		/// mouse location.
		/// </summary>
		public event EventHandler OnMouseEnter;
		
		/// <summary>
		/// Invoked whenever the mouse leaves the bounds of this <see cref="Component"/>.
		/// A mouse leave event on this <see cref="Component"/> happens whenever the mouse is
		/// moved within the bounds of this <see cref="Component"/> and there
		/// are no opaque <see cref="Component"/>s higher in the z order in the 
		/// mouse location.
		/// </summary>
		public event EventHandler OnMouseExit;
		
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
		/// </summary>
		public event EventHandler<KeyEventArgs> OnKeyPress;
		
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
		/// </summary>
		public event EventHandler<KeyEventArgs> OnKeyRelease;
		
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
		/// </summary>
		public event EventHandler<CharEventArgs> OnCharTyped;
		
		protected Component(Component parent) : this(parent, false)
		{
			
		}
		
		protected Component(Component parent, bool bypass)
		{
			Position = new UIPosition();
			Position.OnChange += MarkMasterDirty;
			ZCoord = new UIZCoord();
			ZCoord.OnChange += MarkMasterDirty;
			Size = new UISize();
			Size.OnChange += MarkMasterDirty;
			Clip = new UIClip();
			Clip.OnChange += MarkMasterDirty;
			
			Position.OwnSize = Size;
			
			if(parent != null)
			{
				if(!bypass) 
				{
					parent = parent.GetParentFor(this);
				}
				Parent = parent;
				parent.Children.Add(this);
				
				//update references
				Position.ParentPos = parent.Position;
				Position.RatioPos = new Vec2D(1, 1);
				Position.ParentSize = parent.Size;
				Position.Ratio = new Vec2D(0, 0);
				Size.ParentSize = parent.Size;
				Size.Ratio = new Vec2D(1, 1);
				ZCoord.ParentZ = parent.ZCoord;
				Clip.ParentClip = parent.Clip;
			}
		}
		
		protected void MarkMasterDirty(object sender, EventArgs e)
		{
			Frame frame = MasterFrame;
			if(frame != null) frame.MarkDirty();
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
		/// Ticks happen at the tickRate of the UICore.
		/// By default calls TickChildren().
		/// </summary>
		/// <param name="dt">The time since the last tick.</param>
		protected internal virtual void Tick(double dt)
		{
			if(OnTick != null) OnTick(this, dt);
			TickChildren(dt);
		}
		
		/// <summary>
		/// Ticks the children of this <see cref="Component"/>.
		/// Ticks happen at the tickRate of the UICore.
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
			if(OnPreRender != null) OnPreRender(this, EventArgs.Empty);
		}
		
		/// <summary>
		/// Calls <see cref="OnPostRender"/>.
		/// </summary>
		protected void PostRender()
		{
			if(OnPostRender != null) OnPostRender(this, EventArgs.Empty);
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
				if(Rendering.Width != Math.Max(1, (int)Math.Round(Size.Value.X)) || Rendering.Height != Math.Max(1, (int)Math.Round(Size.Value.Y)))
				{
					Dirty = true;
					Rendering.Resize(Math.Max(1, (int)Math.Round(Size.Value.X)), Math.Max(1, (int)Math.Round(Size.Value.Y)));
				}
				if(Dirty)
				{
					Dirty = false;
					Render(Rendering);
				}
				image.SetClip(Clip.Value);
				if(!Opaque)
				{
					image.BlendBlit(Rendering, (Point2D)Position.Value);
				}else
				{
					image.MaskBlit(Rendering, (Point2D)Position.Value);
				}
				image.ClearClip();
			}else
			{
				image.SetClip(Clip.Value);
				Render(image);
				image.ClearClip();
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
		
		internal void InvokeMousePress(object sender, MouseButtonEventArgs args){if(OnMousePress != null) OnMousePress(sender, args);}
		internal void InvokeMouseRelease(object sender, MouseButtonEventArgs args){if(OnMouseRelease != null) OnMouseRelease(sender, args);}
		internal void InvokeMouseMove(object sender, MouseMoveEventArgs args){if(OnMouseMove != null) OnMouseMove(sender, args);}
		internal void InvokeMouseWheel(object sender, MouseWheelEventArgs args){if(OnMouseWheel != null) OnMouseWheel(sender, args);}
		internal void InvokeMouseEnter(object sender, EventArgs args){if(OnMouseEnter != null) OnMouseEnter(sender, args);}
		internal void InvokeMouseExit(object sender, EventArgs args){if(OnMouseExit != null) OnMouseExit(sender, args);}
		internal void InvokeKeyPress(object sender, KeyEventArgs args){if(OnKeyPress != null) OnKeyPress(sender, args);}
		internal void InvokeKeyRelease(object sender, KeyEventArgs args){if(OnKeyRelease != null) OnKeyRelease(sender, args);}
		internal void InvokeCharTyped(object sender, CharEventArgs args){if(OnCharTyped != null) OnCharTyped(sender, args);}
	}
}
