using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using ToolkitEngine.Health;

namespace ToolkitEditor.Health
{
    [CustomEditor(typeof(ArmorComposite))]
    public class ArmorCompositeEditor : Editor
    {
        #region Fields

        protected ArmorComposite m_armorComposite;

        protected SerializedProperty m_groups;
        protected SerializedProperty m_groupAssignments;
        protected SerializedProperty m_visualize;

        protected ArmorGroupTreeView m_treeView = null;

        #endregion

        #region Methods

        private void OnEnable()
        {
            m_armorComposite = target as ArmorComposite;
            if (!Application.isPlaying)
            {
                m_armorComposite.UpdateGroups();
            }
            m_armorComposite.UpdateGroupAssignments();

            m_groups = serializedObject.FindProperty(nameof(m_groups));
            m_groupAssignments = serializedObject.FindProperty(nameof(m_groupAssignments));
            m_visualize = serializedObject.FindProperty(nameof(m_visualize));

            UpdateVisualization();
        }

        private void OnDisable()
        {
            UpdateVisualization(true);
            m_treeView = null;
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

			EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_groups);

            if (EditorGUI.EndChangeCheck() && !Application.isPlaying)
            {
                m_armorComposite.UpdateGroups();
            }

            var rect = GUILayoutUtility.GetLastRect();

            if (m_armorComposite.objects.Length > 0)
            {
                if (m_treeView == null)
                {
                    MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[]
                    {
                        new MultiColumnHeaderState.Column()
                        {
                            canSort = true,
                            headerContent = new GUIContent("Name"),
                            headerTextAlignment = TextAlignment.Center,
                            sortingArrowAlignment = TextAlignment.Right,
                            width  = EditorGUIUtility.labelWidth,
                        },
                        new MultiColumnHeaderState.Column()
                        {
                            canSort = true,
                            headerContent = new GUIContent("Group"),
                            headerTextAlignment = TextAlignment.Center,
                            sortingArrowAlignment = TextAlignment.Right,
                            width = rect.width - (EditorGUIUtility.labelWidth + 4f),
                        },
                    };

                    m_treeView = new ArmorGroupTreeView(
                        new TreeViewState(),
                        new MultiColumnHeader(
                            new MultiColumnHeaderState(columns)),
                        m_armorComposite);
                }
                else
                {
                    m_treeView.multiColumnHeader.GetColumn(0).width = EditorGUIUtility.labelWidth;
                    m_treeView.multiColumnHeader.GetColumn(1).width = rect.width - (EditorGUIUtility.labelWidth + 4f);
                }

                rect.y += EditorGUI.GetPropertyHeight(m_groups) + EditorGUIUtility.standardVerticalSpacing;
                rect.width = rect.width - 2f;
                rect.height = m_treeView.totalHeight;

                EditorGUILayout.Separator();
                m_treeView.OnGUI(rect);

                GUILayout.Space(m_treeView.totalHeight);

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(m_visualize);

				if (EditorGUI.EndChangeCheck())
				{
					UpdateVisualization();
					SceneView.RepaintAll();
				}
			}
            else
            {
                EditorGUILayout.HelpBox("Children colliders not found!", MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
			if (m_treeView == null || !m_visualize.boolValue)
				return;

			foreach (var row in m_treeView.GetRows())
			{
				if (!m_treeView.TryGetGameObject(row, out GameObject obj))
					continue;

				Material material = new Material(ToolkitEngine.ShaderUtil.defaultShader);
				if (material == null)
					break;

				if (!m_armorComposite.TryGetGroup(obj, out var group))
					break;

				material.color = group.color;
				Matrix4x4 m = Matrix4x4.TRS(obj.transform.position, obj.transform.rotation, obj.transform.lossyScale);

				foreach (var meshCollider in obj.GetComponents<MeshCollider>())
				{
                    if (meshCollider.isTrigger)
                        continue;

					if (meshCollider.sharedMesh != null)
					{
						Graphics.DrawMesh(meshCollider.sharedMesh, m, material, 0);
					}
				}

                foreach (var boxCollider in obj.GetComponents<BoxCollider>())
                {
                    if (boxCollider.isTrigger)
                        continue;

                    Graphics.DrawMesh(
                        Resources.GetBuiltinResource<Mesh>("Cube.fbx"),
                        m * Matrix4x4.TRS(boxCollider.center, Quaternion.identity, boxCollider.size),
                        material,
                        0);
                }

                foreach (var sphereCollider in obj.GetComponents<SphereCollider>())
                {
                    if (sphereCollider.isTrigger)
                        continue;

                    Graphics.DrawMesh(
                        Resources.GetBuiltinResource<Mesh>("Sphere.fbx"),
                        m * Matrix4x4.TRS(sphereCollider.center, Quaternion.identity, Vector3.one * sphereCollider.radius),
                        material,
                        0);
                }
			}
		}

		private void UpdateVisualization(bool forced = false)
		{
			if (m_armorComposite == null)
				return;

			if (forced || !m_visualize.boolValue)
			{
				foreach (var renderer in m_armorComposite.GetComponentsInChildren<Renderer>())
				{
					SceneVisibilityManager.instance.Show(renderer.gameObject, false);
				}

				if (m_treeView != null)
				{
					foreach (var row in m_treeView.GetRows())
					{
						if (!m_treeView.TryGetGameObject(row, out GameObject obj))
							continue;

						SceneVisibilityManager.instance.Show(obj.gameObject, false);
					}
				}
			}
			else
			{
				foreach (var renderer in m_armorComposite.GetComponentsInChildren<Renderer>())
				{
					SceneVisibilityManager.instance.Hide(renderer.gameObject, false);
				}

				if (m_treeView != null)
				{
					foreach (var row in m_treeView.GetRows())
					{
						if (!m_treeView.TryGetGameObject(row, out GameObject obj))
							continue;

						SceneVisibilityManager.instance.Hide(obj.gameObject, false);
					}
				}
			}
		}

		#endregion
	}

    public class ArmorGroupTreeView : TreeView
    {
        #region Fields

        private ArmorComposite m_model = null;

        /// <summary>
        /// Map TreeViewItem to GameObject in ArmorComposite
        /// </summary>
        private Dictionary<TreeViewItem, GameObject> m_map = new();

        private GUIStyle m_groupErrorStyle;

        #endregion

        #region Constructors

        public ArmorGroupTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, ArmorComposite model)
            : base(state, multiColumnHeader)
        {
            m_model = model;

            Reload();
            rowHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            showBorder = true;
            multiColumnHeader.sortingChanged += OnSortingChanged;

            m_groupErrorStyle = new GUIStyle(EditorStyles.textField);
            m_groupErrorStyle.normal.textColor = Color.red;
            m_groupErrorStyle.hover.textColor = Color.red;
        }

        #endregion

        #region Methods

        public bool TryGetGameObject(TreeViewItem treeViewItem, out GameObject obj)
        {
            return m_map.TryGetValue(treeViewItem, out obj);
        }

        private void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            var rows = GetRows();
            if (rows.Count <= 1)
                return;

            // No column to sort for (just use the order the data are in)
            if (multiColumnHeader.sortedColumnIndex == -1)
                return;

            bool ascending = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);

            IOrderedEnumerable<TreeViewItem> orderedRows = null;
            switch (multiColumnHeader.state.sortedColumnIndex)
            {
                // Name column
                case 0:
                    orderedRows = ascending
                        ? rows.OrderByDescending(x => x.displayName)
                        : rows.OrderBy(x => x.displayName);
                    break;

                // Group column
                case 1:
                    orderedRows = ascending
                        ? rows.OrderByDescending(x => m_model.TryGetGroupName(m_map[x], out string groupName) ? groupName : string.Empty)
                        : rows.OrderBy(x => m_model.TryGetGroupName(m_map[x], out string groupName) ? groupName : string.Empty);
                    break;
            }

            if (orderedRows == null)
                return;

            rootItem.children = orderedRows.Cast<TreeViewItem>().ToList();
            TreeToList(rootItem, rows);
            Repaint();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem()
            {
                id = 0,
                depth = -1,
                displayName = string.Empty
            };

            int id = 1;
            foreach (var obj in m_model.objects)
            {
                var item = new TreeViewItem()
                {
                    id = id++,
                    displayName = obj.name,

                };
                m_map.Add(item, obj);

                root.AddChild(item);
            }

            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item;

            // Name column
            var cellRect = args.GetCellRect(0);
            EditorGUI.LabelField(cellRect, new GUIContent(item.displayName));

            // Group column
            cellRect = args.GetCellRect(1);

            m_model.TryGetGroupName(m_map[item], out string groupName);
            var nameList = m_model.names.ToList();
            if (nameList.Count == 0)
                return;

            int selectedIndex = 0;
            if (!string.IsNullOrEmpty(groupName))
            {
                if (!nameList.Contains(groupName))
                {
                    m_model.SetGroup(m_map[item], EditorGUI.TextField(cellRect, groupName, m_groupErrorStyle));
                    return;
                }

                selectedIndex = nameList.IndexOf(groupName);
            }

            selectedIndex = Mathf.Max(selectedIndex, 0);
            selectedIndex = EditorGUI.Popup(cellRect, selectedIndex, m_model.names);

            m_model.SetGroup(m_map[item], nameList[selectedIndex]);
        }

        #endregion

        #region Sort Methods

        public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
        {
            if (root == null)
                throw new NullReferenceException("root");
            if (result == null)
                throw new NullReferenceException("result");

            result.Clear();

            if (root.children == null)
                return;

            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
            for (int i = root.children.Count - 1; i >= 0; i--)
                stack.Push(root.children[i]);

            while (stack.Count > 0)
            {
                TreeViewItem current = stack.Pop();
                result.Add(current);

                if (current.hasChildren && current.children[0] != null)
                {
                    for (int i = current.children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(current.children[i]);
                    }
                }
            }
        }

        #endregion
    }
}