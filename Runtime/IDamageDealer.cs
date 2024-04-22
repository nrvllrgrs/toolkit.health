using UnityEngine;
using UnityEngine.Events;

namespace ToolkitEngine.Health
{
	public interface IDamageDealer
	{
		Transform transform { get; }
		UnityEvent<HealthEventArgs> onDamageDealt { get; }
	}
}