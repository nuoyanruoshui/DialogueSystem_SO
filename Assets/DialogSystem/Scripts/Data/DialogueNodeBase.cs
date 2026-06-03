using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NuoYan.DialogSystem
{
    public abstract class DialogueNodeBase : ScriptableObject
    {
        [SerializeField, ReadOnly] private int nodeId;
        [SerializeField] private SpeakEnums speakType;
        [SerializeField] private string speakerName;
        [SerializeField] private string dialogText;
        [SerializeField]
        private List<SOEventBase> m_NodeEvents = new List<SOEventBase>();

        public int NodeId => nodeId;
        public SpeakEnums SpeakType => speakType;
        public string SpeakerName => speakerName;
        public string DialogText { get => dialogText; set => dialogText = value; }
        public bool HasEvents => m_NodeEvents != null && m_NodeEvents.Count > 0;
        public int EventCount => m_NodeEvents?.Count ?? 0;
        public IReadOnlyList<SOEventBase> NodeEvents => m_NodeEvents;

        public virtual void PlayNode()
        {
            Debug.Log($"[{nodeId}] {speakerName}: {dialogText}");
        }

        public abstract void VaildNode();

        public static int GenerateNodeId()
        {
            return (int)System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        public void InvokeNodeEvents()
        {
            foreach (SOEventBase nodeEvent in m_NodeEvents)
            {
                if (nodeEvent != null)
                {
                    nodeEvent.RaiseEvent(this);
                }
            }
        }
    }
}
