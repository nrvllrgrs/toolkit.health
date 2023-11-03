using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToolkitEngine.Health
{
    [System.Serializable]
    public class ReflectedValue<T>
    {
		#region Enumerators

		public enum FetchMode
		{
			Value,
			External,
		}

		#endregion

		#region Fields

		[SerializeField]
		private FetchMode m_fetchMode;

		[SerializeField]
		private T m_value;

		[SerializeField]
		private Object m_object;

		[SerializeField]
		private Component m_component;

		[SerializeField]
		private string m_memberName;

		[SerializeField]
		private bool m_isProperty;

		#endregion

		#region Properties

		public T value
		{
			get
			{
				switch (m_fetchMode)
				{
					case FetchMode.Value:
						return m_value;

					case FetchMode.External:
						return default;
				}

				return default;
			}
		}

		#endregion
	}
}