using UnityEditor;
using ToolkitEngine.Health;

namespace ToolkitEditor.Health
{
	[CustomEditor(typeof(Explosion))]
	public class ExplosionEditor : BaseToolkitEditor
    {
		#region Fields

		protected SerializedProperty m_damage;
		protected SerializedProperty m_spawner;
		protected SerializedProperty m_onDetonated;
		protected SerializedProperty m_onDamageDealt;

		#endregion

		#region Methods

		private void OnEnable()
		{
			m_damage = serializedObject.FindProperty(nameof(m_damage));
			m_spawner = serializedObject.FindProperty(nameof(m_spawner));
			m_onDetonated = serializedObject.FindProperty(nameof(m_onDetonated));
			m_onDamageDealt = serializedObject.FindProperty(nameof(m_onDamageDealt));
		}

		protected override void DrawProperties()
		{
			EditorGUILayout.PropertyField(m_damage);
			EditorGUILayout.PropertyField(m_spawner);
		}

		protected override void DrawEvents()
		{
			if (EditorGUILayoutUtility.Foldout(m_onDetonated, "Events"))
			{
				EditorGUILayout.PropertyField(m_onDetonated);
				EditorGUILayout.PropertyField(m_onDamageDealt);
				DrawNestedEvents();
			}
		}

		#endregion
	}
}