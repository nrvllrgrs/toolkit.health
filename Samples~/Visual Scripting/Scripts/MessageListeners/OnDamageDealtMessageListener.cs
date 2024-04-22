using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
	[AddComponentMenu("")]
	public class OnDamageDealtMessageListener : MessageListener
	{
		private void Start() => GetComponent<IDamageDealer>()?.onDamageDealt.AddListener((value) =>
		{
			EventBus.Trigger(nameof(OnDamageDealt), gameObject, value);
		});
	}
}
