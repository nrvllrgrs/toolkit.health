using UnityEditor;
using UnityEngine;

namespace ToolkitEditor.Health
{
    [CustomEditor(typeof(ToolkitEngine.Health.Health))]
    [CanEditMultipleObjects]
    public class HealthEditor : Editor
    {
        #region Fields

        protected ToolkitEngine.Health.Health m_health;

        protected SerializedProperty m_value;
		protected SerializedProperty m_bonusValue;
		protected SerializedProperty m_maxValue;
        protected SerializedProperty m_invulnerabilityTime;
        protected SerializedProperty m_startInvulnerable;

        // Regeneration
        protected SerializedProperty m_canRegenerate;
        protected SerializedProperty m_regenerateDelay;
        protected SerializedProperty m_degenerateDelay;
		protected SerializedProperty m_rates;
        protected SerializedProperty m_stopCondition;

        // Events
        protected SerializedProperty m_onValueChanging;
        protected SerializedProperty m_onValueChanged;
        protected SerializedProperty m_onHealed;
        protected SerializedProperty m_onDamaged;
		protected SerializedProperty m_onDying;
		protected SerializedProperty m_onDied;
        protected SerializedProperty m_onResurrected;
        protected SerializedProperty m_onRegenerationChanged;

        #endregion

        #region Methods

        private void OnEnable()
        {
            m_health = (ToolkitEngine.Health.Health)target;

            m_value = serializedObject.FindProperty(nameof(m_value));
			m_bonusValue = serializedObject.FindProperty(nameof(m_bonusValue));
			m_maxValue = serializedObject.FindProperty(nameof(m_maxValue));
            m_invulnerabilityTime = serializedObject.FindProperty(nameof(m_invulnerabilityTime));
            m_startInvulnerable = serializedObject.FindProperty(nameof(m_startInvulnerable));

            // Regeneration
            m_canRegenerate = serializedObject.FindProperty(nameof(m_canRegenerate));
            m_regenerateDelay = serializedObject.FindProperty(nameof(m_regenerateDelay));
            m_degenerateDelay = serializedObject.FindProperty(nameof(m_degenerateDelay));
			m_rates = serializedObject.FindProperty(nameof(m_rates));
			m_stopCondition = serializedObject.FindProperty(nameof(m_stopCondition));

            // Events
            m_onValueChanging = serializedObject.FindProperty(nameof(m_onValueChanging));
            m_onValueChanged = serializedObject.FindProperty(nameof(m_onValueChanged));
            m_onHealed = serializedObject.FindProperty(nameof(m_onHealed));
            m_onDamaged = serializedObject.FindProperty(nameof(m_onDamaged));
			m_onDying = serializedObject.FindProperty(nameof(m_onDying));
			m_onDied = serializedObject.FindProperty(nameof(m_onDied));
            m_onResurrected = serializedObject.FindProperty(nameof(m_onResurrected));
            m_onRegenerationChanged = serializedObject.FindProperty("OnRegenerationChanged");
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

			EditorGUILayout.PropertyField(m_maxValue);

            EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.PropertyField(m_bonusValue);
			EditorGUI.EndDisabledGroup();

            m_value.floatValue = Mathf.Clamp(m_value.floatValue, 0f, m_maxValue.floatValue + m_bonusValue.floatValue);
            EditorGUILayout.Slider(m_value, 0, m_maxValue.floatValue + m_bonusValue.floatValue);
            EditorGUILayout.PropertyField(m_invulnerabilityTime);
            EditorGUILayout.PropertyField(m_startInvulnerable);

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Regeneration", EditorStyles.boldLabel);
            if (EditorGUILayoutUtility.Foldout(m_regenerateDelay, "Delay"))
            {
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(m_regenerateDelay, new GUIContent("Regenerate"));
				EditorGUILayout.PropertyField(m_degenerateDelay, new GUIContent("Degenerate"));
				--EditorGUI.indentLevel;
			}

            if (!Application.isPlaying)
            {
				EditorGUILayout.PropertyField(m_rates);
			}
            else
            {
                if (EditorGUILayoutUtility.Foldout(m_rates, m_rates.displayName))
                {
					++EditorGUI.indentLevel;
					EditorGUI.BeginDisabledGroup(true);
					foreach (var key in m_health.regenerateDamageTypes)
                    {
                        EditorGUILayout.FloatField(key?.name ?? "NONE", m_health.GetRegenerationRate(key));
					}
					EditorGUI.EndDisabledGroup();
					--EditorGUI.indentLevel;
				}
            }
			
            EditorGUILayout.PropertyField(m_stopCondition);

            EditorGUILayout.Separator();

            if (EditorGUILayoutUtility.Foldout(m_onValueChanging, "Events"))
            {
                EditorGUILayout.PropertyField(m_onValueChanging);
                EditorGUILayout.PropertyField(m_onValueChanged);
                EditorGUILayout.PropertyField(m_onHealed);
                EditorGUILayout.PropertyField(m_onDamaged);
				EditorGUILayout.PropertyField(m_onDying);
				EditorGUILayout.PropertyField(m_onDied);
                EditorGUILayout.PropertyField(m_onResurrected);
                EditorGUILayout.PropertyField(m_onRegenerationChanged);
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}