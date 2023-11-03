using ToolkitEngine.Health;
using UnityEditor;
using UnityEngine;

namespace ToolkitEditor.Health
{
	[CustomPropertyDrawer(typeof(SplashDamage))]
	public class SplashDamageDrawer : DamageDrawer
    {
		protected override void DrawProperties(ref Rect position, SerializedProperty property)
		{
			EditorGUIRectLayout.PropertyField(ref position, property.FindPropertyRelative("m_factor"));
			EditorGUIRectLayout.PropertyField(ref position, property.FindPropertyRelative("m_impulse"));
			EditorGUIRectLayout.PropertyField(ref position, property.FindPropertyRelative("m_upwardModifier"));
			EditorGUIRectLayout.PropertyField(ref position, property.FindPropertyRelative("m_radius"));
			EditorGUIRectLayout.PropertyField(ref position, property.FindPropertyRelative("m_falloff"));
			EditorGUIRectLayout.PropertyField(ref position, property.FindPropertyRelative("m_bonuses"));
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float height = base.GetPropertyHeight(property, label);
			if (property.FindPropertyRelative("m_damageType").isExpanded)
			{
				height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_factor"))
					+ EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_impulse"))
					+ EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_upwardModifier"))
					+ EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_radius"))
					+ EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_falloff"))
					+ EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_bonuses"))
					+ (EditorGUIUtility.standardVerticalSpacing * 6f);
			}

			return height;
		}
	}
}