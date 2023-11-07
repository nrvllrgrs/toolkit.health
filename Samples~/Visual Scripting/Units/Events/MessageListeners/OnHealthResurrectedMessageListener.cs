using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
    [AddComponentMenu("")]
    public class OnHealthResurrectedMessageListener : MessageListener
    {
        private void Start() => GetComponent<IHealth>()?.onResurrected.AddListener((value) =>
        {
            EventBus.Trigger(EventHooks.OnHealthResurrected, gameObject, value);
        });
    }
}