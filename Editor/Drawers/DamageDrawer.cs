using UnityEngine;
using UnityEditor;
using ToolkitEngine.Health;

namespace ToolkitEditor.Health
{
    [CustomPropertyDrawer(typeof(Damage), true)]
    public class DamageDrawer : PropertyDrawer
    {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var damageTypeProp = property.FindPropertyRelative("m_damageType");
			if (EditorGUIRectLayout.Foldout(ref position, damageTypeProp, label))
			{
				++EditorGUI.indentLevel;
				EditorGUIRectLayout.PropertyField(ref position, damageTypeProp);
				EditorGUIRectLayout.PropertyField(ref position, property.FindPropertyRelative("m_value"));
				DrawProperties(ref position, property);
				--EditorGUI.indentLevel;
			}
		}

		protected virtual void DrawProperties(ref Rect position, SerializedProperty property)
		{ }

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float height = EditorGUIUtility.singleLineHeight
				+ EditorGUIUtility.standardVerticalSpacing;

			var damageTypeProp = property.FindPropertyRelative("m_damageType");
			if (damageTypeProp.isExpanded)
			{
				height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_value"))
					+ EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_damageType"))
					+ (EditorGUIUtility.standardVerticalSpacing * 2f);
			}

			return height;
		}
	}
}
