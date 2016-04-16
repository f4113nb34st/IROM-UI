namespace IROM.UI
{
	using System;
	using System.Collections.Generic;
	using IROM.Util;
	
	/// <summary>
	/// The types of input events available.
	/// </summary>
	internal enum InputEventType
	{
		MPress, MRelease, MMove, MWheel, MEnter, MExit, KPress, KRelease, CTyped
	}
	
	/// <summary>
	/// Handles input events for <see cref="Root"/>s.
	/// </summary>
	internal class RootInputHandler
	{
		/// <summary>
		/// The set of active components that are guaranteed an event call before the method returns.
		/// </summary>
		private readonly HashSet<Component> ActiveComponents = new HashSet<Component>();
		
		/// <summary>
		/// A dictionary that contains all the <see cref="Component"/>s waiting on release events from the mouse buttons.
		/// </summary>
		private MultiValueDictionary<MouseButton, Component> MouseActiveComponents = new MultiValueDictionary<MouseButton, Component>();
		
		/// <summary>
		/// A dictionary that contains all the <see cref="Component"/>s waiting on release events from the keyboard buttons.
		/// </summary>
		private MultiValueDictionary<KeyboardButton, Component> KeyboardActiveComponents = new MultiValueDictionary<KeyboardButton, Component>();
		
		/// <summary>
		/// <see cref="Component"/>s that have received <see cref="Component.OnMouseEnter"/> and are waiting on <see cref="Component.OnMouseEnter"/>.
		/// </summary>
		private HashSet<Component> EnteredComponents = new HashSet<Component>();
		
		/// <summary>
		/// Temporary set for storing <see cref="Component"/>s to receive <see cref="Component.OnMouseExit"/>.
		/// </summary>
		private HashSet<Component> ExitedComponents = new HashSet<Component>();
		
		//backing var
		private Component focusedComponent;
		
		/// <summary>
		/// The <see cref="Component"/> with the keyboard focus.
		/// </summary>
		private Component FocusedComponent
		{
			get
			{
				return focusedComponent;
			}
			set
			{
				if(focusedComponent != null)
				{
					focusedComponent.IsFocused.Value = false;
				}
				focusedComponent = value;
				if(focusedComponent != null)
				{
					focusedComponent.IsFocused.Value = true;
				}
			}
		}
		
		/// <summary>
		/// The parent root this <see cref="RootInputHandler"/> works for.
		/// </summary>
		private readonly Root Parent;
		
		/// <summary>
		/// The last cursor before any components with specials
		/// </summary>
		private Cursor previousCursor;
		
		/// <summary>
		/// The currently grabbed component.
		/// </summary>
		private Component grabbedComponent;
		
		/// <summary>
		/// The currently hovered drop zone.
		/// </summary>
		private IDropZone currentZone;
		
		private IDropZone CurrentZone
		{
			get
			{
				return currentZone;
			}
			set
			{
				if(currentZone != null)
				{
					if(grabbedComponent != null) currentZone.OnExit(grabbedComponent);
					(currentZone as Component).Position.OnUpdate -= CheckZone;
					(currentZone as Component).Size.OnUpdate -= CheckZone;
					currentZone = null;
				}
				currentZone = value;
				if(currentZone != null)
				{
					if(grabbedComponent != null) currentZone.OnEnter(grabbedComponent);
					(currentZone as Component).Position.OnUpdate += CheckZone;
					(currentZone as Component).Size.OnUpdate += CheckZone;
				}
			}
		}
		
		internal RootInputHandler(Root parent)
		{
			Parent = parent;
		}
		
		internal void Grab(Component comp)
		{
			CurrentZone = null;
			grabbedComponent = comp;
		}
		
		internal bool Drop(Component comp)
		{
			if(grabbedComponent != comp) return false;
			grabbedComponent = null;
			if(CurrentZone != null)
			{
				CurrentZone.OnDrop(comp);
				CurrentZone = null;
				return true;
			}else
			{
				return false;
			}
		}
		
		internal void CheckZone()
		{
			//exit drop zone if left bounds
			if(CurrentZone != null && !(CurrentZone as Component).Bounds.Contains(Parent.MousePosition))
			{
				CurrentZone = null;
			}
		}
		
		internal void MousePressEvent(MouseButton button)
		{
			if(Parent != null)
			{
				//prepare event list
				Component[] eventList = Parent.FindAll(Parent.MousePosition, false, true);

				//perform event passing
				foreach(Component comp in eventList)
				{
					//call event
					comp.InvokeMousePress(button);
					//add to components waiting on release
					MouseActiveComponents.Add(button, comp);
					//consume if opaque
					if(comp.InputOpaque.Value)
					{
						//if eligible for keyboard focus, give it
						if(comp.HasListeners(InputEventType.KPress) || comp.HasListeners(InputEventType.KRelease) || comp.HasListeners(InputEventType.CTyped))
					    {
					   		FocusedComponent = comp;
					    }
						break;
					}
				}
			}
		}
		
		internal void MouseReleaseEvent(MouseButton button)
		{
			if(Parent != null)
			{
				//store all components that are guaranteed a call
				ActiveComponents.Clear();
				ActiveComponents.UnionWith(MouseActiveComponents.GetValues(button));
				
				//prepare event list
				Component[] eventList = Parent.FindAll(Parent.MousePosition, false, true);
				
				//perform normal event passing
				foreach(Component comp in eventList)
				{
					//call event, it'll do nothing if it doesn't have listeners
					comp.InvokeMouseRelease(button);
					//remove from active components (we don't care if it doesn't contain the element, it'll just do nothing)
					ActiveComponents.Remove(comp);
					//consume if opaque
					if(comp.InputOpaque.Value)
					{
						break;
					}
				}
				
				//satisfy waiting components
				foreach(Component comp in ActiveComponents)
				{
					//call event
					comp.InvokeMouseRelease(button);
				}
				
				//remove all waiters
				MouseActiveComponents.Clear(button);
			}
		}
		
		internal void MouseMoveEvent(Point2D mouseCoords, Point2D delta)
		{
			if(Parent != null)
			{
				Point2D oldCoords = mouseCoords - delta;
				
				//store all components that are guaranteed a call
				ActiveComponents.Clear();
				foreach(MouseButton button in Enum.GetValues(typeof(MouseButton)))
				{
					ActiveComponents.UnionWith(MouseActiveComponents.GetValues(button));
				}
				
				//add all entered components to exited
				//if they don't get removed, then we schedule an exit
				ExitedComponents.Clear();
				ExitedComponents.UnionWith(EnteredComponents);
				
				CheckZone();
				
				//prepare event list
				Component[] eventList = Parent.FindAll(new Rectangle{Min = VectorUtil.Min(mouseCoords, oldCoords), Max = VectorUtil.Max(mouseCoords, oldCoords)}, false, true);
				
				//true if the cursor was set to a special value
				bool cursorSet = false;
				//true if the drop zone was updated
				bool zoneSet = false;
				
				//perform normal event passing
				foreach(Component comp in eventList)
				{
					//if not set yet and special cursor
					if(!cursorSet && 
					   comp.HoverCursor.Value != Cursor.UNSPECIFIED)
					{
						cursorSet = true;
						if(previousCursor == Cursor.UNSPECIFIED) previousCursor = Parent.WindowObj.CurrentCursor;
						Parent.WindowObj.CurrentCursor = comp.HoverCursor.Value;
					}
					//if haven't entered yet
					if(!EnteredComponents.Contains(comp))
					{
						//call enter
						comp.InvokeMouseEnter();
						EnteredComponents.Add(comp);
					}else //else prevent automatic exit
					{
						ExitedComponents.Remove(comp);
					}
					if(grabbedComponent != null && !zoneSet && comp is IDropZone && comp.Bounds.Contains(mouseCoords) && CurrentZone != comp)
					{
						IDropZone zone = (IDropZone)comp;
						if(zone.Handles(grabbedComponent))
						{
							CurrentZone = zone;
							zoneSet = true;
						}
					}
					//call event with local mouse coords
					comp.InvokeMouseMove(mouseCoords - (Point2D)comp.Position.Value, delta);
					//remove from active components (we don't care if it doesn't contain the element, it'll just do nothing)
					ActiveComponents.Remove(comp);
					//consume if opaque
					if(comp.InputOpaque.Value)
					{
						break;
					}
				}
				
				//if cursor was not 
				if(!cursorSet && previousCursor != Cursor.UNSPECIFIED)
				{
					Parent.WindowObj.CurrentCursor = previousCursor;
				}
				
				//satisfy all waiting components
				foreach(Component comp in ActiveComponents)
				{
					//call event with local mouse coords
					comp.InvokeMouseMove(mouseCoords - (Point2D)comp.Position.Value, delta);
				}
				
				//remove all exited from entered
				EnteredComponents.ExceptWith(ExitedComponents);
				//exit all exited
				foreach(Component comp in ExitedComponents)
				{
					//call exit
					comp.InvokeMouseExit();
				}
			}
		}
		
		internal void MouseExitEvent()
		{
			//exit all exited
			foreach(Component comp in EnteredComponents)
			{
				//call exit
				comp.InvokeMouseExit();
			}
			EnteredComponents.Clear();
		}
		
		internal void MouseWheelEvent(int delta)
		{
			if(Parent != null)
			{
				//prepare event list
				Component[] eventList = Parent.FindAll(Parent.MousePosition, false, true);
				
				//perform event passing
				foreach(Component comp in eventList)
				{
					//call event
					comp.InvokeMouseWheel(delta);
					//consume if opaque
					if(comp.InputOpaque.Value)
					{
						break;
					}
				}
			}
		}
		
		internal void KeyPressEvent(KeyboardButton button)
		{
			if(Parent != null)
			{
				//prepare event list
				Component[] eventList = Parent.FindAll(Parent.MousePosition, false, true);
				
				bool consumed = false;
				
				//perform event passing
				foreach(Component comp in eventList)
				{
					//call event
					comp.InvokeKeyPress(button);
					//add to components waiting on release
					KeyboardActiveComponents.Add(button, comp);
					//consume if opaque
					if(comp.InputOpaque.Value)
					{
						//if eligible for keyboard focus, give it
						if(comp.HasListeners(InputEventType.KPress) || comp.HasListeners(InputEventType.KRelease) || comp.HasListeners(InputEventType.CTyped))
					    {
					   		FocusedComponent = comp;
					   		consumed = true;
						}
						break;
					}
				}
				
				//if unconsumed, give to focused
				if(!consumed && FocusedComponent != null)
				{
					FocusedComponent.InvokeKeyPress(button);
				}
			}
		}
		
		internal void KeyReleaseEvent(KeyboardButton button)
		{
			if(Parent != null)
			{
				//store all components that are guaranteed a call
				ActiveComponents.Clear();
				ActiveComponents.UnionWith(KeyboardActiveComponents.GetValues(button));
				
				//prepare event list
				Component[] eventList = Parent.FindAll(Parent.MousePosition, false, true);
				
				bool consumed = false;
				
				//perform event passing
				foreach(Component comp in eventList)
				{
					//call event
					comp.InvokeKeyRelease(button);
					//remove from active components (we don't care if it doesn't contain the element, it'll just do nothing)
					ActiveComponents.Remove(comp);
					//consume if opaque
					if(comp.InputOpaque.Value)
					{
						//if eligible for keyboard focus, give it
						if(comp.HasListeners(InputEventType.KPress) || comp.HasListeners(InputEventType.KRelease) || comp.HasListeners(InputEventType.CTyped))
					    {
					   		FocusedComponent = comp;
					   		consumed = true;
						}
						break;
					}
				}
				
				//if unconsumed, give to focused
				if(!consumed && FocusedComponent != null)
				{
					FocusedComponent.InvokeKeyRelease(button);
					ActiveComponents.Remove(FocusedComponent);
				}
				
				//satisfy waiting components
				foreach(Component comp in ActiveComponents)
				{
					//call event
					comp.InvokeKeyRelease(button);
				}
				
				//remove all waiters
				KeyboardActiveComponents.Clear(button);
			}
		}
		
		internal void CharTypedEvent(char c, bool repeat)
		{
			if(Parent != null)
			{
				//prepare event list
				Component[] eventList = Parent.FindAll(Parent.MousePosition, false, true);
				
				bool consumed = false;
				
				//perform event passing
				foreach(Component comp in eventList)
				{
					//call event
					comp.InvokeCharTyped(c, repeat);
					//consume if opaque
					if(comp.InputOpaque.Value)
					{
						//if eligible for keyboard focus, give it
						if(comp.HasListeners(InputEventType.KPress) || comp.HasListeners(InputEventType.KRelease) || comp.HasListeners(InputEventType.CTyped))
					    {
					   		FocusedComponent = comp;
					   		consumed = true;
						}
						break;
					}
				}
				
				//if unconsumed, give to focused
				if(!consumed && FocusedComponent != null)
				{
					FocusedComponent.InvokeCharTyped(c, repeat);
				}
			}
		}
	}
}
