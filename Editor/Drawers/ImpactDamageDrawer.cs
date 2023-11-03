using ToolkitEngine.Health;
using UnityEditor;
using UnityEngine;

namespace ToolkitEditor.Health
{
	[CustomPropertyDrawer(typeof(ImpactDamage))]
	public class ImpactDamageDrawer : DamageDrawer
	{
		protected override void DrawProperties(ref Rect position, SerializedProperty property)
		{
			EditorGUIRectLayout.PropertyField(ref position, property.FindPropertyRelative("m_factor"));
			EditorGUIRectLayout.PropertyField(ref position, property.FindPropertyRelative("m_impulse"));
			EditorGUIRectLayout.PropertyField(ref position, property.FindPropertyRelative("m_range"));
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
					+ EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_range"))
					+ EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_falloff"))
					+ EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_bonuses"))
					+ (EditorGUIUtility.standardVerticalSpacing * 5f);
			}

			return height;
		}
	}
}