using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
    [AddComponentMenu("")]
    public class OnHealthHealedMessageListener : MessageListener
    {
        private void Start() => GetComponent<IDamageReceiver>()?.onHealed.AddListener((value) =>
        {
            EventBus.Trigger(EventHooks.OnHealthHealed, gameObject, value);
        });
    }
}