using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
	[AddComponentMenu("")]
	public class OnExplosionDetonatedMessageListener : MessageListener
	{
		private void Start() => GetComponent<Explosion>()?.onDetonated.AddListener((value) =>
		{
			EventBus.Trigger(EventHooks.OnExplosionDetonated, gameObject, value);
		});
	}
}
