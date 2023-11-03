using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
    [AddComponentMenu("")]
    public class OnHealthValueChangedMessageListener : MessageListener
    {
        private void Start() => GetComponent<IDamageReceiver>()?.onValueChanged.AddListener((value) =>
        {
            EventBus.Trigger(EventHooks.OnHealthValueChanged, gameObject, value);
        });
    }
}