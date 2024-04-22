using UnityEngine.Events

namespace ToolkitEngine.Health
{
	public interface IDamageDealer
	{
		UnityEvent<ShooterEventArgs> onDamageDealt => m_onDamageDealt;
	}
}