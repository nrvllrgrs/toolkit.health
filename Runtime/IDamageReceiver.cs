using UnityEngine;
using UnityEngine.Events;

namespace ToolkitEngine.Health
{
	public interface IDamageReceiver
	{
		Transform transform { get; }
		UnityEvent<HealthEventArgs> onValueChanging { get; }
		UnityEvent<HealthEventArgs> onValueChanged { get; }
		UnityEvent<HealthEventArgs> onDamaged { get; }
		UnityEvent<HealthEventArgs> onHealed { get; }
		void Apply(DamageHit damageInfo);
		void Damage(float delta, DamageType damageType = null);
		void Heal(float delta, DamageType damageType = null);
	}
}