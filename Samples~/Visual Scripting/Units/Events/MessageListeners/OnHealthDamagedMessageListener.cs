using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
    [AddComponentMenu("")]
    public class OnHealthDamagedMessageListener : MessageListener
    {
        private void Start() => GetComponent<IDamageReceiver>()?.onDamaged.AddListener((value) =>
        {
            EventBus.Trigger(EventHooks.OnHealthDamaged, gameObject, value);
        });
    }
}