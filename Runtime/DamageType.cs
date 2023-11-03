using UnityEngine;
using Sirenix.OdinInspector;

namespace ToolkitEngine.Health
{
    [CreateAssetMenu(menuName = "Toolkit/Health/Damage Type")]
    public class DamageType : ScriptableObject
    {
        #region Fields

        [SerializeField, ReadOnly]
        private string m_id = System.Guid.NewGuid().ToString();

        #endregion

        #region Properties

        public string ID => m_id;

        #endregion
    }
}