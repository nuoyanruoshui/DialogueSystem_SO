using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NuoYan.DialogSystem
{
    public class DialogueRunner : MonoBehaviour
    {
        [SerializeField] private DialogueGraph dialogueGraph;
        DialogueVariables variables = new DialogueVariables();
        [SerializeField, ReadOnly]
        DialogueNodeBase currentNode;

        readonly List<DialogueChoice> availableChoices = new();

        public DialogueNodeBase CurrentNode => currentNode;
        public DialogueVariables Variables => variables;
        public bool IsDialogueActive => currentNode != null;
        public IReadOnlyList<DialogueChoice> AvailableChoices => availableChoices;

        // ── Events ──────────────────────────────────────────
        public event Action OnDialogueStart;
        public event Action OnDialogueEnd;
        public event Action<DialogueNodeBase> OnNodeChanged;
        public event Action<IReadOnlyList<DialogueChoice>> OnChoicesUpdated;

        public void StartDialogue(DialogueGraph graph = null)
        {
            if (graph != null)
                dialogueGraph = graph;

            if (dialogueGraph == null)
            {
                Debug.LogError("Dialogue graph is null");
                return;
            }

            variables.CopyFrom(dialogueGraph.Variables);
            GoTo(dialogueGraph.StartNode);
            OnDialogueStart?.Invoke();
        }

        public void StopDialogue()
        {
            currentNode = null;
            availableChoices.Clear();
            OnDialogueEnd?.Invoke();
        }

        public void Advance()
        {
            if (currentNode == null) return;

            if (currentNode is DialogueOptionNode)
            {
                Debug.Log("当前是选项节点，请使用 SelectOption(index) 选择");
                return;
            }

            var seqNode = currentNode as DialogueNode;
            if (seqNode == null) return;

            var link = PickLink(seqNode);
            if (link == null)
            {
                Debug.Log("没有满足条件的出口，对话结束");
                StopDialogue();
                return;
            }

            GoTo(link.toNode);
        }

        public void SelectOption(int index)
        {
            RefreshAvailableChoices();
            if (index < 0 || index >= availableChoices.Count) return;

            GoTo(availableChoices[index].toNode);
        }

        public void RefreshAvailableChoices()
        {
            availableChoices.Clear();
            var optNode = currentNode as DialogueOptionNode;
            if (optNode?.ChoiceList == null) return;

            foreach (var c in optNode.ChoiceList)
            {
                if (c == null || c.toNode == null) continue;
                if (c.CanPass(variables))
                    availableChoices.Add(c);
            }

            OnChoicesUpdated?.Invoke(availableChoices);
        }

        void GoTo(DialogueNodeBase node)
        {
            if (node == null)
            {
                StopDialogue();
                return;
            }

            currentNode = node;
            currentNode.PlayNode();
            currentNode.InvokeNodeEvents();
            OnNodeChanged?.Invoke(node);

            if (currentNode is DialogueOptionNode)
                RefreshAvailableChoices();
        }

        DialogueLink PickLink(DialogueNode node)
        {
            if (node.LinkList == null) return null;

            foreach (var link in node.LinkList)
            {
                if (link == null || link.toNode == null) continue;
                if (link.CanPass(variables))
                    return link;
            }
            return null;
        }

        // ── Unity lifecycle (optional — can be removed for full manual control) ──
        void Start()
        {
            if (dialogueGraph != null)
                StartDialogue();
        }

        void Update()
        {
            if (currentNode == null) return;

            if (Input.GetKeyDown(KeyCode.Space))
                Advance();

            if (currentNode is not DialogueOptionNode) return;

            RefreshAvailableChoices();
            for (int i = 0; i < availableChoices.Count && i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    SelectOption(i);
            }
        }
    }
}
