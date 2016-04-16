namespace IROM.UI
{
	using System;
	using IROM.Dynamix;
	using IROM.Util;
	
	/// <summary>
	/// An animator that smoothes the transition of a changing double.
	/// </summary>
	public class SmoothAnimator
	{
		private readonly Dynx<double> CurrentValue = new Dynx<double>();
		private readonly Dynx<double> Start = new Dynx<double>();
		private readonly Dynx<double> End = new Dynx<double>();
		private readonly Dynx<double> Mu = new Dynx<double>();
		
		/// <summary>
		/// The Dynamix-enabled value of this animator.
		/// </summary>
		public double Value
		{
			get
			{
				return CurrentValue.Value;
			}
			set
			{
				Start.Value = CurrentValue.Value;
				End.Value = value;
				Mu.Value = 1;
			}
		}
		
		/// <summary>
		/// The target of this animator.
		/// </summary>
		public double Target
		{
			get
			{
				return End.Value;
			}
			set
			{
				Start.Value = CurrentValue.Value;
				End.Value = value;
				if(Start.Value != End.Value)
				{
					Mu.Value = 0;
				}else
				{
					Mu.Value = 1;
				}
			}
		}
		
		public SmoothAnimator()
		{
			Mu.OnFilter += d => Math.Min(d, 1);
			CurrentValue.Exp = () => Interpolation.Cosine(Start.Value, End.Value, Mu.Value);
		}
		
		/// <summary>
		/// Ticks this animator with the given percentage of the animation time.
		/// </summary>
		/// <param name="percentage">Percentage of animation to increment [0,1]</param>
		public void Tick(double percentage)
		{
			if(Mu.Value < 1)
			{
				Mu.Value += percentage;
			}
		}
	}
}
