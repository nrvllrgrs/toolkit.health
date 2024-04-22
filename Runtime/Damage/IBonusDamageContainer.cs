using System.Collections.Generic;

namespace ToolkitEngine.Health
{
	public interface IBonusDamageContainer
    {
        List<Damage> bonuses { get; }
    }
}