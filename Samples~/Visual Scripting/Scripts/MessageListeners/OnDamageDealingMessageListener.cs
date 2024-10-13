using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
	[AddComponentMenu("")]
	public class OnDamageDealingMessageListener : MessageListener
	{
		private void Start() => GetComponent<IDamageDealer>()?.onDamageDealing.AddListener((value) =>
		{
			EventBus.Trigger(nameof(OnDamageDealing), gameObject, value);
		});
	}
}
