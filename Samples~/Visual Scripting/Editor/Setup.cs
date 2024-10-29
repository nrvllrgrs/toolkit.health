using System;
using System.Collections.Generic;
using ToolkitEngine.Health;
using UnityEditor;

namespace ToolkitEditor.Health.VisualScripting
{
	[InitializeOnLoad]
	public static class Setup
	{
		static Setup()
		{
			var types = new List<Type>()
			{
				// Health
				typeof(IHealth),
				typeof(IHealthRegeneration),
				typeof(ToolkitEngine.Health.Health),
				typeof(HealthComposite),
				typeof(HealthLayers),

				// Armor
				typeof(Armor),
				typeof(ArmorComposite),
				typeof(ArmorLayers),

				// Damage
				typeof(DamageType),
				typeof(Damage),
				typeof(ImpactDamage),
				typeof(SplashDamage),
				typeof(DamageHit),
				typeof(Explosion),
				typeof(IExplosive),

				// Misc
				typeof(HealthEventArgs),
				typeof(IDamageDealer),
				typeof(IDamageReceiver),
			};

			ToolkitEditor.VisualScripting.Setup.Initialize("ToolkitEngine.Health", types);
		}
	}
}