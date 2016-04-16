namespace IROM.UI
{
	using System;
	using System.Collections.Generic;
	using IROM.Util;
	using IROM.Dynamix;
	
	/// <summary>
	/// An invisible component that automically sizes a frame when the frame is dragged into it.
	/// </summary>
	public class FrameAnchor : Panel, IDropZone
	{
		/// <summary>
		/// True if the north edge should not be used on child frames.
		/// </summary>
		public readonly Dynx<bool> DisableNorthEdge = new Dynx<bool>();
		
		/// <summary>
		/// True if the south edge should not be used on child frames.
		/// </summary>
		public readonly Dynx<bool> DisableSouthEdge = new Dynx<bool>();
		
		/// <summary>
		/// True if the west edge should not be used on child frames.
		/// </summary>
		public readonly Dynx<bool> DisableWestEdge = new Dynx<bool>();
		
		/// <summary>
		/// True if the east edge should not be used on child frames.
		/// </summary>
		public readonly Dynx<bool> DisableEastEdge = new Dynx<bool>();
		
		/// <summary>
		/// The bounds panel. Anchored <see cref="Frame"/>s snap to this panel's size and location.
		/// </summary>
		public readonly Panel BoundsPanel = new Panel();
		
		/// <summary>
		/// The color of the preview panel.
		/// </summary>
		public readonly Dynx<ARGB> PreviewColor = new Dynx<ARGB>();
		
		/// <summary>
		/// The preview panel.
		/// </summary>
		private readonly Panel previewPanel = new PreviewPanel();
		
		/// <summary>
		/// The animator for the preview panel.
		/// </summary>
		private readonly SmoothAnimator previewAnimator = new SmoothAnimator();
		
		/// <summary>
		/// The currently hovered frame.
		/// </summary>
		private readonly Dynx<Frame> hoveredFrame = new Dynx<Frame>();
		
		/// <summary>
		/// If false, the preview panel will not be animated.
		/// </summary>
		public bool IsAnimated = true;
		
		public FrameAnchor()
		{
			Hidden.Value = true;
			InputHidden.Value = false;
			Opaque.Value = false;
			
			DisableNorthEdge.Value = false;
			DisableSouthEdge.Value = false;
			DisableWestEdge.Value = false;
			DisableEastEdge.Value = false;
			
			BoundsPanel.Parent.Value = this;
			BoundsPanel.Hidden.Value = true;
			
			PreviewColor.Exp = () => new ARGB((byte)(previewAnimator.Value * 63), RGB.White);
			
			previewPanel.Parent.Value = this;
			previewPanel.Hidden.Value = true;
			previewPanel.Opaque.Value = false;
			previewPanel.Color.Exp = () => PreviewColor.Value;
			previewPanel.Position.Exp = () => 
			{
				if(hoveredFrame.Value == null) return 0;
				Point2D vec = 0;
				vec += hoveredFrame.Value.Position.Value * (1 - previewAnimator.Value);
				vec += BoundsPanel.Position.Value * previewAnimator.Value;
				return vec;
			};
			previewPanel.Size.Exp = () =>
			{
				if(hoveredFrame.Value == null) return 0;
				Point2D vec = 0;
				vec += hoveredFrame.Value.Size.Value * (1 - previewAnimator.Value);
				vec += BoundsPanel.Size.Value * previewAnimator.Value;
				return vec;
			};
			
			OnTick += dt => previewAnimator.Tick(IsAnimated ? dt * 3 : double.PositiveInfinity);
		}
		
		public bool Handles(Component comp)
		{
			Frame frame = comp as Frame;
			return frame != null && frame.DisableResize.Exp == null && (frame.DisableResize.Value == false);
		}
		
		public void OnEnter(Component comp)
		{
			hoveredFrame.Value = comp as Frame;
			previewPanel.Hidden.Value = false;
			previewAnimator.Value = 0;
			previewAnimator.Target = 1;
		}
		
		public void OnExit(Component comp)
		{
			previewPanel.Hidden.Value = true;
			hoveredFrame.Value = null;
		}
		
		public void OnDrop(Component comp)
		{
			previewPanel.Hidden.Value = true;
			hoveredFrame.Value = null;
			
			Frame frame = comp as Frame;
			
			Point2D deltaMouse = frame.MousePosition;
			DynxState<Point2D> sizeState = frame.Size;
			DynxState<bool> northState = frame.DisableNorthEdge;
			DynxState<bool> southState = frame.DisableSouthEdge;
			DynxState<bool> westState = frame.DisableWestEdge;
			DynxState<bool> eastState = frame.DisableEastEdge;

			frame.Position.Exp = () => BoundsPanel.Position.Value;
			frame.Size.Exp = () => BoundsPanel.Size.Value;
			frame.DisableNorthEdge.Exp = () => DisableNorthEdge.Value;
			frame.DisableSouthEdge.Exp = () => DisableSouthEdge.Value;
			frame.DisableWestEdge.Exp = () => DisableWestEdge.Value;
			frame.DisableEastEdge.Exp = () => DisableEastEdge.Value;
			
			Action OnFrameMove = null;
			OnFrameMove = () =>
			{
				//remove size and position dependencies
				//frame.Position.Value = frame.Position.Value;
				//frame.Size.Value = frame.Size.Value;
				frame.Position.Value = RootObj.Value.MousePosition - deltaMouse;
				sizeState.Restore(frame.Size);
				northState.Restore(frame.DisableNorthEdge);
				southState.Restore(frame.DisableSouthEdge);
				westState.Restore(frame.DisableWestEdge);
				eastState.Restore(frame.DisableEastEdge);
				frame.OnMove -= OnFrameMove;
			};
			frame.OnMove += OnFrameMove;
		}
		
		private class PreviewPanel : Panel
		{
			public PreviewPanel()
			{
				Buffered = false;
			}
			
			protected override void Render(Image image)
			{
				//TODO
				//image.RenderSolid(Bounds, Color.Value, RenderMode.BLEND);
				image.RenderSolid(Bounds, Color.Value);
			}
		}
		
		private struct DynxState<T>
		{
			private Func<T> exp;
			private T value;
			
			/// <summary>
			/// Restores the given dynx variable to this state.
			/// </summary>
			/// <param name="dynx"></param>
			public void Restore(Dynx<T> dynx)
			{
				if(exp != null)
				{
					dynx.Exp = exp;
				}else
				{
					dynx.Value = value;
				}
			}
			
			public static implicit operator DynxState<T>(Dynx<T> dynx)
			{
				return new DynxState<T>{exp = dynx.Exp, value = dynx.Value};
			}
		}
	}
}
