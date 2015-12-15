namespace IROM.UI
{
	using System;
	
	/// <summary>
	/// A screen is a reusable base component for the gui system.
	/// </summary>
	public class Screen : Panel
	{
		protected internal Frame frame;
		
		public override Frame MasterFrame
		{
			get 
			{
				return frame;
			}
		}
		
		public override Screen MasterScreen
		{
			get 
			{
				return this;
			}
		}
		
		public Screen() : base(null){}
	}
}
