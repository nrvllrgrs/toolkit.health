using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Health.VisualScripting
{
    [AddComponentMenu("")]
    public class OnHealthDiedMessageListener : MessageListener
    {
        private void Start() => GetComponent<IHealth>()?.onDied.AddListener((value) =>
        {
            EventBus.Trigger(EventHooks.OnHealthDied, gameObject, value);
        });
    }
}