using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NuoYan.DialogSystem
{
    [Serializable]
    public class NodeLayoutEntry
    {
        public DialogueNodeBase node;
        public Vector2 position;
    }

    [CreateAssetMenu(fileName = "New Dialogue Graph", menuName = "Dialog System/Dialogue Graph")]
    public class DialogueGraph : ScriptableObject
    {
        #region 字段
        [SerializeField, ReadOnly] private int graphId;
        [SerializeField] private string graphName;
        [SerializeField] private DialogueNodeBase startNode;
        [SerializeField] private List<DialogueNodeBase> nodeList = new List<DialogueNodeBase>();
        [HideInInspector]
        [SerializeField]
        private List<NodeLayoutEntry> nodeLayouts = new List<NodeLayoutEntry>();
        [SerializeField, HideInInspector]
        private DialogueVariables variables = new DialogueVariables();
        #endregion

        #region 属性
        public int GraphId => graphId;
        public string GraphName => graphName;
        public DialogueNodeBase StartNode => startNode;
        public List<DialogueNodeBase> NodeList => nodeList;
        public List<NodeLayoutEntry> NodeLayouts => nodeLayouts;
        public DialogueVariables Variables => variables;
        #endregion

        #region 方法
        public void AddNode(DialogueNodeBase node)
        {
            if (nodeList is null)
                nodeList = new List<DialogueNodeBase>();
            nodeList.Add(node);
        }

        public void RemoveNode(DialogueNodeBase node)
        {
            if (nodeList is null)
            {
                Debug.LogError("Node list is null, please add node first");
                return;
            }
            nodeList.Remove(node);
            RemoveLayout(node);
        }

        public Vector2 GetLayout(DialogueNodeBase node)
        {
            if (node == null || nodeLayouts == null)
                return Vector2.zero;

            foreach (var entry in nodeLayouts)
            {
                if (entry.node == node)
                    return entry.position;
            }

            return Vector2.zero;
        }

        public void SetLayout(DialogueNodeBase node, Vector2 position)
        {
            if (node == null)
                return;

            if (nodeLayouts == null)
                nodeLayouts = new List<NodeLayoutEntry>();

            foreach (var entry in nodeLayouts)
            {
                if (entry.node == node)
                {
                    entry.position = position;
                    return;
                }
            }

            nodeLayouts.Add(new NodeLayoutEntry { node = node, position = position });
        }

        void RemoveLayout(DialogueNodeBase node)
        {
            if (nodeLayouts == null)
                return;

            nodeLayouts.RemoveAll(e => e.node == node);
        }

#if UNITY_EDITOR
        public void SetStartNode(DialogueNodeBase node)
        {
            startNode = node;
        }
#endif
        #endregion
    }
}
