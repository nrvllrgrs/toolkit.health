using UnityEngine;
using UnityEditor;
using ToolkitEngine.Health;

namespace ToolkitEditor.Health
{
    [CustomEditor(typeof(HealthLayers))]
    public class HealthLayersEditor : Editor
    {
        #region Fields

        protected SerializedProperty m_layers;

        protected SerializedProperty m_onValueChanging;
        protected SerializedProperty m_onValueChanged;
		protected SerializedProperty m_onHealed;
		protected SerializedProperty m_onDamaged;
		protected SerializedProperty m_onDying;
		protected SerializedProperty m_onDied;
        protected SerializedProperty m_onResurrected;

        #endregion

        #region Methods

        private void OnEnable()
        {
            m_layers = serializedObject.FindProperty(nameof(m_layers));

            m_onValueChanging = serializedObject.FindProperty(nameof(m_onValueChanging));
            m_onValueChanged = serializedObject.FindProperty(nameof(m_onValueChanged));
			m_onHealed = serializedObject.FindProperty(nameof(m_onHealed));
			m_onDamaged = serializedObject.FindProperty(nameof(m_onDamaged));
			m_onDying = serializedObject.FindProperty(nameof(m_onDying));
			m_onDied = serializedObject.FindProperty(nameof(m_onDied));
            m_onResurrected = serializedObject.FindProperty(nameof(m_onResurrected));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

			using (new EditorGUI.DisabledScope(true))
			{
				if (target is MonoBehaviour behaviour)
				{
					EditorGUILayout.ObjectField(EditorGUIUtility.TrTempContent("Script"), MonoScript.FromMonoBehaviour(behaviour), typeof(MonoBehaviour), false);
				}
			}

			EditorGUILayout.PropertyField(m_layers);

            EditorGUILayout.Separator();
            if (EditorGUILayoutUtility.Foldout(m_onValueChanging, "Events"))
            {
                EditorGUILayout.PropertyField(m_onValueChanging);
                EditorGUILayout.PropertyField (m_onValueChanged);
                EditorGUILayout.PropertyField(m_onHealed);
				EditorGUILayout.PropertyField(m_onDamaged);
				EditorGUILayout.PropertyField(m_onDying);
				EditorGUILayout.PropertyField(m_onDied);
                EditorGUILayout.PropertyField(m_onResurrected);
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}