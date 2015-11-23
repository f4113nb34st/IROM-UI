namespace IROM.UI
{
	using System;
	using System.Collections.Generic;
	using IROM.Util;
	
	/// <summary>
	/// Base class for dynamically calculated UIVariables.
	/// </summary>
	public abstract class UIVariable<T, W> where T : struct where W : struct
	{
		/// <summary>
		/// The dependancies of this <see cref="UIVariable{T, W}">UIVariable</see>.
		/// </summary>
		internal readonly Dictionary<string, ParentPair> ParentGroup = new Dictionary<string, ParentPair>();
		
		/// <summary>
		/// A list of value filters.
		/// </summary>
		public readonly List<Filter> Filters = new List<Filter>();
		
		/// Backing fields
		private T BaseOffset;
		
		/// <summary>
		/// Called whenever this variable changes.
		/// </summary>
		public event EventHandler OnChange;
		
		/// <summary>
		/// The current value of this <see cref="UIVariable{T, W}">UIVariable</see>.
		/// </summary>
		public virtual T Value
		{
			get;
			protected set;
		}
		
		/// <summary>
		/// The offset to add to the resulting value.
		/// Negative values subtract.
		/// </summary>
		public virtual T Offset
		{
			get{return BaseOffset;}
			set
			{
				BaseOffset = value; 
				Update();
			}
		}
		
		/// <summary>
		/// Custom value filtering method.
		/// </summary>
		/// <param name="value">The calculated value.</param>
		/// <returns>The value to use.</returns>
		public delegate T Filter(T value);
		
		/// <summary>
		/// Updates the value of this <see cref="UIVariable{T, W}">UIVariable</see>.
		/// </summary>
		public virtual void Update()
		{
			T tempValue = Offset;
			foreach(ParentPair pair in ParentGroup.Values)
			{
				if(pair.Value != null)
				{
					tempValue = Modify(tempValue, pair.Value.Value, pair.Weight);
				}
			}
			foreach(Filter filter in Filters)
			{
				tempValue = filter(tempValue);
			}
			if(!Equals(Value, tempValue))
			{
				Value = tempValue;
				if(OnChange != null) OnChange(this, EventArgs.Empty);
			}
		}
		
		/// <summary>
		/// Returns the weight with the given tag.
		/// </summary>
		/// <param name="tag">The tag.</param>
		/// <returns>The weight, or 0 if it does not exist.</returns>
		public virtual W GetWeight(string tag)
		{
			if(ParentGroup.ContainsKey(tag))
			{
				return ParentGroup[tag].Weight;
			}else
			{
				return default(W);
			}
		}
		
		/// <summary>
		/// Sets the weight with the given tag.
		/// </summary>
		/// <param name="tag">The tag.</param>
		/// <param name="weight">The new weight.</param>
		public virtual void SetWeight(string tag, W weight)
		{
			if(ParentGroup.ContainsKey(tag))
			{
				ParentGroup[tag].Weight = weight;
			}else
			{
				ParentGroup[tag] = new ParentPair(null, weight);
			}
			//update variable
			Update();
		}
		
		/// <summary>
		/// Returns the parent with the given tag.
		/// </summary>
		/// <param name="tag">The tag.</param>
		/// <returns>The parent, or null if it does not exist.</returns>
		public UIVariable<T, W> GetParent(string tag)
		{
			if(ParentGroup.ContainsKey(tag))
			{
				return ParentGroup[tag].Value;
			}else
			{
				return null;
			}
		}
		
		/// <summary>
		/// Sets the parent with the given tag.
		/// </summary>
		/// <param name="tag">The tag.</param>
		/// <param name="parent">The new parent.</param>
		public void SetParent(string tag, UIVariable<T, W> parent)
		{
			if(ParentGroup.ContainsKey(tag))
			{
				if(ParentGroup[tag].Value != null)
				{
					//unsubscribe old parent
					ParentGroup[tag].Value.OnChange -= OnChangeEvent;
				}
				//set new reference
				ParentGroup[tag].Value = parent;
			}else
			{
				ParentGroup[tag] = new ParentPair(parent, default(W));
			}
			//subscribe to parent reference
			parent.OnChange += OnChangeEvent;
			//update variable
			Update();
		}
		
		/// <summary>
		/// Sets the parent with the given tag and weight.
		/// </summary>
		/// <param name="tag">The tag.</param>
		/// <param name="parent">The new parent.</param>
		/// <param name="weight">The new weight.</param>
		public void SetParent(string tag, UIVariable<T, W> parent, W weight)
		{
			SetParent(tag, parent);
			SetWeight(tag, weight);
		}
		
		/// <summary>
		/// Clears all parents.
		/// </summary>
		public void ClearParents()
		{
			ParentGroup.Clear();
		}
		
		private void OnChangeEvent(object sender, EventArgs args)
		{
			Update();
		}
		
		/// <summary>
		/// Applies the influence of the given parent.
		/// </summary>
		/// <param name="currentValue">The value to modify.</param>
		/// <param name="parent">The parent.</param>
		/// <param name="weight">The weight.</param>
		/// <returns></returns>
		protected abstract T Modify(T currentValue, T parent, W weight);
		
		/// <summary>
		/// Returns true if the two values given are equal.
		/// </summary>
		/// <param name="newValue">The new value.</param>
		/// <param name="oldValue">The old value.</param>
		/// <returns>True if equal.</returns>
		protected abstract bool Equals(T newValue, T oldValue);
		
		/// <summary>
		/// A simple parent variable/weight pair.
		/// </summary>
		public class ParentPair
		{
			public UIVariable<T, W> Value;
			public W Weight;
			
			public ParentPair(UIVariable<T, W> value, W weight)
			{
				Value = value;
				Weight = weight;
			}
		}
	}
}
