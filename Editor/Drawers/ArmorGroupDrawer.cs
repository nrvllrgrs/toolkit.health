using UnityEngine;
using UnityEditor;
using ToolkitEngine.Health;

namespace ToolkitEditor.Health
{
    [CustomPropertyDrawer(typeof(ArmorComposite.ArmorGroup))]
    public class ArmorGroupDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var nameProp = property.FindPropertyRelative("m_name");
            string foldoutLabel = !string.IsNullOrWhiteSpace(nameProp.stringValue) ? nameProp.stringValue : "Undefined";
			if (!EditorGUIRectLayout.Foldout(ref position, nameProp, foldoutLabel))
				return;

            ++EditorGUI.indentLevel;

            EditorGUIRectLayout.PropertyField(ref position, nameProp);
			EditorGUIRectLayout.PropertyField(ref position, property.FindPropertyRelative("m_factor"));
			EditorGUIRectLayout.PropertyField(ref position, property.FindPropertyRelative("m_color"));
			EditorGUIRectLayout.PropertyField(ref position, property.FindPropertyRelative("m_vulnerabilities"));

            var onHitProp = property.FindPropertyRelative("m_onHit");
            if (EditorGUIRectLayout.Foldout(ref position, onHitProp, "Events"))
            {
                EditorGUIRectLayout.PropertyField(ref position, onHitProp);
            }

			--EditorGUI.indentLevel;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
			float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

			var nameProp = property.FindPropertyRelative("m_name");
            if (!nameProp.isExpanded)
                return height;

            height += EditorGUI.GetPropertyHeight(nameProp)
                + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_factor"))
                + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_color"))
                + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_vulnerabilities"))
                + EditorGUIUtility.singleLineHeight
                + (EditorGUIUtility.standardVerticalSpacing * 5f);

			var onHitProp = property.FindPropertyRelative("m_onHit");
            if (onHitProp.isExpanded)
            {
                height += EditorGUI.GetPropertyHeight(onHitProp)
                    + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
		}
    }
}