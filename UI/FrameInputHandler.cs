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
		/// The current coords of the mouse.
		/// </summary>
		private Point2D MouseCoords;
		
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
					BaseFocusedComponent.IsFocused = false;
				}
				BaseFocusedComponent = value;
				if(value != null)
				{
					value.IsFocused = true;
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
		
		internal void MousePressEvent(object sender, MouseButtonEventArgs args)
		{
			if(Parent.CurrentScreen != null)
			{
				//create new args
				MouseButtonEventArgs args2 = new MouseButtonEventArgs(args);
				Rectangle bounds = new Rectangle();
				
				//prepare event list
				EventSet.Clear();
				RecursiveEventAdd(Parent.CurrentScreen, EventSet, InputEventType.MPress);
				
				//perform event passing
				foreach(Component comp in EventSet)
				{
					//set bounds
					bounds.Min = (Point2D)comp.Position.Value;
					bounds.Max = (Point2D)comp.Size.Value + bounds.Min;
					//if within component bounds
					if(bounds.Contains(args.Coords))
					{
						//make coords relative to component
						args2.Coords = args.Coords - bounds.Min;
						//call event
						comp.InvokeMousePress(Parent, args2);
						//add to components waiting on release
						MouseActiveComponents.Add(args.Button, comp);
						//auto consume if opaque
						if(comp.Opaque)
						{
							//if eligible for keyboard focus, give it
							if(comp.HasListeners(InputEventType.KPress) || comp.HasListeners(InputEventType.KRelease) || comp.HasListeners(InputEventType.CTyped))
						    {
						   		FocusedComponent = comp;
						    }
							args2.Consumed = true;
						}
						//break if consumed
						if(args2.Consumed)
						{
							break;
						}
					}
				}
			}
		}
		
		internal void MouseReleaseEvent(object sender, MouseButtonEventArgs args)
		{
			if(Parent.CurrentScreen != null)
			{
				//create new args
				MouseButtonEventArgs args2 = new MouseButtonEventArgs(args);
				Rectangle bounds = new Rectangle();
				
				//store all components that are guaranteed a call
				ActiveComponents.Clear();
				ActiveComponents.UnionWith(MouseActiveComponents[args.Button]);
				
				//prepare event list
				EventSet.Clear();
				RecursiveEventAdd(Parent.CurrentScreen, EventSet, InputEventType.MRelease);
				
				//perform normal event passing
				foreach(Component comp in EventSet)
				{
					//set bounds
					bounds.Min = (Point2D)comp.Position.Value;
					bounds.Max = (Point2D)comp.Size.Value + bounds.Min;
					//if within component bounds
					if(bounds.Contains(args.Coords))
					{
						//make coords relative to component
						args2.Coords = args.Coords - bounds.Min;
						//call event
						comp.InvokeMouseRelease(Parent, args2);
						//remove from active components (we don't care if it doesn't contain the element, it'll just do nothing)
						ActiveComponents.Remove(comp);
						//auto consume if opaque
						if(comp.Opaque)
						{
							args2.Consumed = true;
						}
						//break if consumed
						if(args2.Consumed)
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
						//make coords relative to component
						args2.Coords = args.Coords - bounds.Min;
						//call event
						comp.InvokeMouseRelease(Parent, args2);
					}
				}
				
				//remove all waiters
				MouseActiveComponents.Clear(args.Button);
			}
		}
		
		internal void MouseMoveEvent(object sender, MouseMoveEventArgs args)
		{
			if(Parent.CurrentScreen != null)
			{
				MouseCoords = args.Coords;
				
				//create new args
				MouseMoveEventArgs args2 = new MouseMoveEventArgs(args);
				Rectangle bounds = new Rectangle();
				
				//store all components that are guaranteed a call
				ActiveComponents.Clear();
				foreach(MouseButton button in Enum.GetValues(typeof(MouseButton)))
				{
					ActiveComponents.UnionWith(MouseActiveComponents[button]);
				}
				
				//prepare event list
				EventSet.Clear();
				RecursiveEventAdd(Parent.CurrentScreen, EventSet, InputEventType.MMove);
				
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
					if(bounds.Contains(args.Coords))
					{
						if(comp.HasListeners(InputEventType.MEnter) || comp.HasListeners(InputEventType.MExit))
						{
							//if haven't entered yet
							if(!EnteredComponents.Contains(comp))
							{
								//call enter
								if(comp.HasListeners(InputEventType.MEnter))
								{
									comp.InvokeMouseEnter(Parent, EventArgs.Empty);
								}
								EnteredComponents.Add(comp);
							}else //else prevent automatic exit
							{
								ExitedComponents.Remove(comp);
							}
						}
						
						//make coords relative to component
						args2.Coords = args.Coords - bounds.Min;
						//call event
						comp.InvokeMouseMove(Parent, args2);
						//remove from active components (we don't care if it doesn't contain the element, it'll just do nothing)
						ActiveComponents.Remove(comp);
						//auto consume if opaque
						if(comp.Opaque)
						{
							args2.Consumed = true;
						}
						//break if consumed
						if(args2.Consumed)
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
						//make coords relative to component
						args2.Coords = args.Coords - bounds.Min;
						//call event
						comp.InvokeMouseMove(Parent, args2);
					}
				}
				
				//exit all exited
				foreach(Component comp in ExitedComponents)
				{
					//call exit
					if(comp.HasListeners(InputEventType.MExit))
					{
						comp.InvokeMouseExit(Parent, EventArgs.Empty);
					}
					EnteredComponents.Remove(comp);
				}
			}
		}
		
		internal void MouseWheelEvent(object sender, MouseWheelEventArgs args)
		{
			if(Parent.CurrentScreen != null)
			{
				//create new args
				MouseWheelEventArgs args2 = new MouseWheelEventArgs(args);
				Rectangle bounds = new Rectangle();
				
				//prepare event list
				EventSet.Clear();
				RecursiveEventAdd(Parent.CurrentScreen, EventSet, InputEventType.MWheel);
				
				//perform event passing
				foreach(Component comp in EventSet)
				{
					//set bounds
					bounds.Min = (Point2D)comp.Position.Value;
					bounds.Max = (Point2D)comp.Size.Value + bounds.Min;
					//if within component bounds
					if(bounds.Contains(args.Coords))
					{
						//make coords relative to component
						args2.Coords = args.Coords - bounds.Min;
						//call event
						comp.InvokeMouseWheel(Parent, args2);
						//auto consume if opaque
						if(comp.Opaque)
						{
							args2.Consumed = true;
						}
						//break if consumed
						if(args2.Consumed)
						{
							break;
						}
					}
				}
			}
		}
		
		internal void KeyPressEvent(object sender, KeyEventArgs args)
		{
			if(Parent.CurrentScreen != null)
			{
				//create new args
				KeyEventArgs args2 = new KeyEventArgs(args);
				Rectangle bounds = new Rectangle();
				
				//prepare event list
				EventSet.Clear();
				RecursiveEventAdd(Parent.CurrentScreen, EventSet, InputEventType.KPress);
				
				//perform event passing
				foreach(Component comp in EventSet)
				{
					//set bounds
					bounds.Min = (Point2D)comp.Position.Value;
					bounds.Max = (Point2D)comp.Size.Value + bounds.Min;
					//if within component bounds
					if(bounds.Contains(MouseCoords))
					{
						//call event
						comp.InvokeKeyPress(Parent, args2);
						//add to components waiting on release
						KeyboardActiveComponents.Add(args.Button, comp);
						//auto consume if opaque
						if(comp.Opaque)
						{
							args2.Consumed = true;
						}
						//break if consumed
						if(args2.Consumed)
						{
							break;
						}
					}
				}
				
				//if unconsumed, give to focused
				if(!args2.Consumed && FocusedComponent != null)
				{
					FocusedComponent.InvokeKeyPress(Parent, args2);
				}
			}
		}
		
		internal void KeyReleaseEvent(object sender, KeyEventArgs args)
		{
			if(Parent.CurrentScreen != null)
			{
				//create new args
				KeyEventArgs args2 = new KeyEventArgs(args);
				Rectangle bounds = new Rectangle();
				
				//store all components that are guaranteed a call
				ActiveComponents.Clear();
				ActiveComponents.UnionWith(KeyboardActiveComponents[args.Button]);
				
				//prepare event list
				EventSet.Clear();
				RecursiveEventAdd(Parent.CurrentScreen, EventSet, InputEventType.KRelease);
				
				//perform event passing
				foreach(Component comp in EventSet)
				{
					//set bounds
					bounds.Min = (Point2D)comp.Position.Value;
					bounds.Max = (Point2D)comp.Size.Value + bounds.Min;
					//if within component bounds
					if(bounds.Contains(MouseCoords))
					{
						//call event
						comp.InvokeKeyRelease(Parent, args2);
						//remove from active components (we don't care if it doesn't contain the element, it'll just do nothing)
						ActiveComponents.Remove(comp);
						//auto consume if opaque
						if(comp.Opaque)
						{
							args2.Consumed = true;
						}
						//break if consumed
						if(args2.Consumed)
						{
							break;
						}
					}
				}
				
				//if unconsumed, give to focused
				if(!args2.Consumed && FocusedComponent != null)
				{
					FocusedComponent.InvokeKeyRelease(Parent, args2);
				}
				
				//satisfy waiting components
				foreach(Component comp in ActiveComponents)
				{
					//only invoke if has listener
					if(comp.HasListeners(InputEventType.KRelease))
					{
						//call event
						comp.InvokeKeyRelease(Parent, args2);
					}
				}
				
				//remove all waiters
				KeyboardActiveComponents.Clear(args.Button);
			}
		}
		
		internal void CharTypedEvent(object sender, CharEventArgs args)
		{
			if(Parent.CurrentScreen != null)
			{
				//create new args
				CharEventArgs args2 = new CharEventArgs(args);
				Rectangle bounds = new Rectangle();
				
				//prepare event list
				EventSet.Clear();
				RecursiveEventAdd(Parent.CurrentScreen, EventSet, InputEventType.CTyped);
				
				//perform event passing
				foreach(Component comp in EventSet)
				{
					//set bounds
					bounds.Min = (Point2D)comp.Position.Value;
					bounds.Max = (Point2D)comp.Size.Value + bounds.Min;
					//if within component bounds
					if(bounds.Contains(MouseCoords))
					{
						//call event
						comp.InvokeCharTyped(Parent, args2);
						//auto consume if opaque
						if(comp.Opaque)
						{
							args2.Consumed = true;
						}
						//break if consumed
						if(args2.Consumed)
						{
							break;
						}
					}
				}
				
				//if unconsumed, give to focused
				if(!args2.Consumed && FocusedComponent != null)
				{
					FocusedComponent.InvokeCharTyped(Parent, args2);
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
			if(comp.Visible)
			{
				if(!comp.Hidden && comp.HasListeners(type))
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
