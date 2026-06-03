using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NuoYan.DialogSystem
{
    [CreateAssetMenu(fileName = "New Dialog Node", menuName = "Dialog System/Sequence Node")]
    public class DialogueNode : DialogueNodeBase
    {
        [SerializeField] private List<DialogueLink> linkList = new();

        public List<DialogueLink> LinkList => linkList;

        public override void VaildNode()
        {
            if (linkList is null || linkList.Count == 0)
            {
                Debug.LogWarning("Node is Over");
            }
        }

        public void AddLink(DialogueLink link)
        {
            if (linkList is null)
                linkList = new List<DialogueLink>();
            linkList.Add(link);
        }

        public void RemoveLink(DialogueLink link)
        {
            if (linkList is not null)
                linkList.Remove(link);
        }

        public DialogueLink GetLink(int index)
        {
            if (linkList is not null && index >= 0 && index < linkList.Count)
                return linkList[index];
            return null;
        }

        public void ClearLinks()
        {
            if (linkList is not null)
                linkList.Clear();
        }
    }
}
