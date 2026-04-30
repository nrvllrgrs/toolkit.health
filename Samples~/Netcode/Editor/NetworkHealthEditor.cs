using UnityEditor;
using ToolkitEngine.Health.Netcode;

namespace ToolkitEditor.Health.Netcode
{
	[CustomEditor(typeof(NetworkHealth))]
    public class NetworkHealthEditor : BaseToolkitEditor
    {
		#region Fields

		protected SerializedProperty m_health;

		protected SerializedProperty m_onValueChanged;
		protected SerializedProperty m_onDamaged;
		protected SerializedProperty m_onHealed;
		protected SerializedProperty m_onDying;
		protected SerializedProperty m_onDied;
		protected SerializedProperty m_onResurrected;

		#endregion

		#region Methods

		protected virtual void OnEnable()
		{
			m_health = serializedObject.FindProperty(nameof(m_health));
			
			m_onValueChanged = serializedObject.FindProperty(nameof(m_onValueChanged));
			m_onDamaged = serializedObject.FindProperty(nameof(m_onDamaged));
			m_onHealed = serializedObject.FindProperty(nameof(m_onHealed));
			m_onDying = serializedObject.FindProperty(nameof(m_onDying));
			m_onDied = serializedObject.FindProperty(nameof(m_onDied));
			m_onResurrected = serializedObject.FindProperty(nameof(m_onResurrected));
		}

		protected override void DrawProperties()
		{
			EditorGUILayout.PropertyField(m_health);
		}

		protected override void DrawEvents()
		{
			if (EditorGUILayoutUtility.Foldout(m_onValueChanged, "Events"))
			{
				EditorGUILayout.PropertyField(m_onValueChanged);
				EditorGUILayout.PropertyField(m_onDamaged);
				EditorGUILayout.PropertyField(m_onHealed);
				EditorGUILayout.PropertyField(m_onDying);
				EditorGUILayout.PropertyField(m_onDied);
				EditorGUILayout.PropertyField(m_onResurrected);
			}
		}

		#endregion
	}
}