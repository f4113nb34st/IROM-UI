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
	/// Handles input events for <see cref="Frame"/>s.
	/// </summary>
	internal class FrameInputHandler
	{
		/// <summary>
		/// A simple set of <see cref="Component"/>s to pass events to, sorted by z value. (highers values first so they get events early)
		/// </summary>
		private readonly SortedSet<Component> EventSet = new SortedSet<Component>(Comparer<Component>.Create((x, y) => ((x.ZCoord.Value < y.ZCoord.Value) ? 1 : -1)));
		
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
		/// Temporary set for storing <see cref="Component"/>s to receive <see cref="Component.OnMouseEnter"/>.
		/// </summary>
		private HashSet<Component> ExitedComponents = new HashSet<Component>();
		
		//backing var
		private Component BaseFocusedComponent;
		
		/// <summary>
		/// The <see cref="Component"/> with the keyboard focus.
		/// </summary>
		private Component FocusedComponent
		{
			get
			{
				return BaseFocusedComponent;
			}
			set
			{
				if(BaseFocusedComponent != null)
				{
					BaseFocusedComponent.IsFocused.Value = false;
				}
				BaseFocusedComponent = value;
				if(BaseFocusedComponent != null)
				{
					BaseFocusedComponent.IsFocused.Value = true;
				}
			}
		}
		
		/// <summary>
		/// The parent frame this <see cref="FrameInputHandler"/> works for.
		/// </summary>
		private Frame Parent;
		
		internal FrameInputHandler(Frame parent)
		{
			Parent = parent;
		}
		
		internal void MousePressEvent(MouseButton button)
		{
			if(Parent.Root != null)
			{
				Point2D mouseCoords = Parent.MousePosition;
				Rectangle bounds = new Rectangle();
				
				//prepare event list
				EventSet.Clear();
				RecursiveEventAdd(Parent.Root, EventSet, InputEventType.MPress);
				
				//perform event passing
				foreach(Component comp in EventSet)
				{
					//set bounds
					bounds.Min = (Point2D)comp.Position.Value;
					bounds.Max = (Point2D)comp.Size.Value + bounds.Min;
					//if within component bounds
					if(bounds.Contains(mouseCoords))
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
		}
		
		internal void MouseReleaseEvent(MouseButton button)
		{
			if(Parent.Root != null)
			{
				Point2D mouseCoords = Parent.MousePosition;
				Rectangle bounds = new Rectangle();
				
				//store all components that are guaranteed a call
				ActiveComponents.Clear();
				ActiveComponents.UnionWith(MouseActiveComponents.GetValues(button));
				
				//prepare event list
				EventSet.Clear();
				RecursiveEventAdd(Parent.Root, EventSet, InputEventType.MRelease);
				
				//perform normal event passing
				foreach(Component comp in EventSet)
				{
					//set bounds
					bounds.Min = (Point2D)comp.Position.Value;
					bounds.Max = (Point2D)comp.Size.Value + bounds.Min;
					//if within component bounds
					if(bounds.Contains(mouseCoords))
					{
						//call event
						comp.InvokeMouseRelease(button);
						//remove from active components (we don't care if it doesn't contain the element, it'll just do nothing)
						ActiveComponents.Remove(comp);
						//consume if opaque
						if(comp.InputOpaque.Value)
						{
							break;
						}
					}
				}
				
				//satisfy waiting components
				foreach(Component comp in ActiveComponents)
				{
					//only invoke if has listener
					if(comp.HasListeners(InputEventType.MRelease))
					{
						//set bounds
						bounds.Min = (Point2D)comp.Position.Value;
						bounds.Max = (Point2D)comp.Size.Value + bounds.Min;
						//call event
						comp.InvokeMouseRelease(button);
					}
				}
				
				//remove all waiters
				MouseActiveComponents.Clear(button);
			}
		}
		
		internal void MouseMoveEvent(Point2D mouseCoords, Point2D delta)
		{
			if(Parent.Root != null)
			{
				Rectangle bounds = new Rectangle();
				
				//store all components that are guaranteed a call
				ActiveComponents.Clear();
				foreach(MouseButton button in Enum.GetValues(typeof(MouseButton)))
				{
					ActiveComponents.UnionWith(MouseActiveComponents.GetValues(button));
				}
				
				//prepare event list
				EventSet.Clear();
				RecursiveEventAdd(Parent.Root, EventSet, InputEventType.MMove);
				
				//add all entered components to exited
				//if they don't get removed, then we schedule an exit
				ExitedComponents.Clear();
				ExitedComponents.UnionWith(EnteredComponents);
				
				//perform normal event passing
				foreach(Component comp in EventSet)
				{
					//set bounds
					bounds.Min = (Point2D)comp.Position.Value;
					bounds.Max = (Point2D)comp.Size.Value + bounds.Min;
					//if within component bounds
					if(bounds.Contains(mouseCoords))
					{
						if(comp.HasListeners(InputEventType.MEnter) || comp.HasListeners(InputEventType.MExit))
						{
							//if haven't entered yet
							if(!EnteredComponents.Contains(comp))
							{
								//call enter
								if(comp.HasListeners(InputEventType.MEnter))
								{
									comp.InvokeMouseEnter();
								}
								EnteredComponents.Add(comp);
							}else //else prevent automatic exit
							{
								ExitedComponents.Remove(comp);
							}
						}
						
						//call event with local mouse coords
						comp.InvokeMouseMove(mouseCoords - bounds.Min, delta);
						//remove from active components (we don't care if it doesn't contain the element, it'll just do nothing)
						ActiveComponents.Remove(comp);
						//consume if opaque
						if(comp.InputOpaque.Value)
						{
							break;
						}
					}
				}
				
				//satisfy all waiting components
				foreach(Component comp in ActiveComponents)
				{
					//only invoke if has listener
					if(comp.HasListeners(InputEventType.MMove))
					{
						//set bounds
						bounds.Min = (Point2D)comp.Position.Value;
						bounds.Max = (Point2D)comp.Size.Value + bounds.Min;
						//call event with local mouse coords
						comp.InvokeMouseMove(mouseCoords - bounds.Min, delta);
					}
				}
				
				//exit all exited
				foreach(Component comp in ExitedComponents)
				{
					//call exit
					if(comp.HasListeners(InputEventType.MExit))
					{
						comp.InvokeMouseExit();
					}
					EnteredComponents.Remove(comp);
				}
			}
		}
		
		internal void MouseWheelEvent(int delta)
		{
			if(Parent.Root != null)
			{
				Point2D mouseCoords = Parent.MousePosition;
				Rectangle bounds = new Rectangle();
				
				//prepare event list
				EventSet.Clear();
				RecursiveEventAdd(Parent.Root, EventSet, InputEventType.MWheel);
				
				//perform event passing
				foreach(Component comp in EventSet)
				{
					//set bounds
					bounds.Min = (Point2D)comp.Position.Value;
					bounds.Max = (Point2D)comp.Size.Value + bounds.Min;
					//if within component bounds
					if(bounds.Contains(mouseCoords))
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
		}
		
		internal void KeyPressEvent(KeyboardButton button)
		{
			if(Parent.Root != null)
			{
				Point2D mouseCoords = Parent.MousePosition;
				Rectangle bounds = new Rectangle();
				
				//prepare event list
				EventSet.Clear();
				RecursiveEventAdd(Parent.Root, EventSet, InputEventType.KPress);
				
				bool consumed = false;
				
				//perform event passing
				foreach(Component comp in EventSet)
				{
					//set bounds
					bounds.Min = (Point2D)comp.Position.Value;
					bounds.Max = (Point2D)comp.Size.Value + bounds.Min;
					//if within component bounds
					if(bounds.Contains(mouseCoords))
					{
						//call event
						comp.InvokeKeyPress(button);
						//add to components waiting on release
						KeyboardActiveComponents.Add(button, comp);
						//consume if opaque
						if(comp.InputOpaque.Value)
						{
							consumed = true;
							break;
						}
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
			if(Parent.Root != null)
			{
				Point2D mouseCoords = Parent.MousePosition;
				Rectangle bounds = new Rectangle();
				
				//store all components that are guaranteed a call
				ActiveComponents.Clear();
				ActiveComponents.UnionWith(KeyboardActiveComponents.GetValues(button));
				
				//prepare event list
				EventSet.Clear();
				RecursiveEventAdd(Parent.Root, EventSet, InputEventType.KRelease);
				
				bool consumed = false;
				
				//perform event passing
				foreach(Component comp in EventSet)
				{
					//set bounds
					bounds.Min = (Point2D)comp.Position.Value;
					bounds.Max = (Point2D)comp.Size.Value + bounds.Min;
					//if within component bounds
					if(bounds.Contains(mouseCoords))
					{
						//call event
						comp.InvokeKeyRelease(button);
						//remove from active components (we don't care if it doesn't contain the element, it'll just do nothing)
						ActiveComponents.Remove(comp);
						//consume if opaque
						if(comp.InputOpaque.Value)
						{
							consumed = true;
							break;
						}
					}
				}
				
				//if unconsumed, give to focused
				if(!consumed && FocusedComponent != null)
				{
					FocusedComponent.InvokeKeyRelease(button);
				}
				
				//satisfy waiting components
				foreach(Component comp in ActiveComponents)
				{
					//only invoke if has listener
					if(comp.HasListeners(InputEventType.KRelease))
					{
						//call event
						comp.InvokeKeyRelease(button);
					}
				}
				
				//remove all waiters
				KeyboardActiveComponents.Clear(button);
			}
		}
		
		internal void CharTypedEvent(char c, bool repeat)
		{
			if(Parent.Root != null)
			{
				Point2D mouseCoords = Parent.MousePosition;
				Rectangle bounds = new Rectangle();
				
				//prepare event list
				EventSet.Clear();
				RecursiveEventAdd(Parent.Root, EventSet, InputEventType.CTyped);
				
				bool consumed = false;
				
				//perform event passing
				foreach(Component comp in EventSet)
				{
					//set bounds
					bounds.Min = (Point2D)comp.Position.Value;
					bounds.Max = (Point2D)comp.Size.Value + bounds.Min;
					//if within component bounds
					if(bounds.Contains(mouseCoords))
					{
						//call event
						comp.InvokeCharTyped(c, repeat);
						//consume if opaque
						if(comp.InputOpaque.Value)
						{
							consumed = true;
							break;
						}
					}
				}
				
				//if unconsumed, give to focused
				if(!consumed && FocusedComponent != null)
				{
					FocusedComponent.InvokeCharTyped(c, repeat);
				}
			}
		}
		
		/// <summary>
		/// Adds the given <see cref="Component"/> to the given collection if it can receive the given event type, as well as all of it's descendants.
		/// </summary>
		/// <param name="comp">The <see cref="Component"/>.</param>
		/// <param name="collection">The collection to add to.</param>
		/// <param name="type">The type of event</param>
		private void RecursiveEventAdd(Component comp, ICollection<Component> collection, InputEventType type)
		{
			if(comp.InputVisible.Value)
			{
				if(!comp.InputHidden.Value && comp.HasListeners(type))
				{
					collection.Add(comp);
				}
				foreach(Component child in comp.Children)
				{
					RecursiveEventAdd(child, collection, type);
				}
			}
		}
	}
}
