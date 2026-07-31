#nullable enable
using UnityEngine;
using UnityEngine.Events;
namespace NuoYan.DialogSystem
{
    /// <summary>
    /// 监听事件
    /// </summary>
    public class SOEventListener : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent<object?> m_EventListener;
        [SerializeField]
        private SOEventBase[] m_EventAsset;
        void OnEnable()
        {
            for (int i = 0; i < m_EventAsset.Length; i++)
            {
                if (m_EventAsset[i] == null)
                {
                    Debug.LogError("Event Asset is null");
                    continue;
                }
                m_EventAsset[i].RegisterListener(OnEventListen);
            }
        }
        void OnDisable()
        {
            for (int i = 0; i < m_EventAsset.Length; i++)
            {
                if (m_EventAsset[i] == null)
                {
                    Debug.LogError("Event Asset is null");
                    continue;
                }
                m_EventAsset[i].UnregisterListener(OnEventListen);
            }
        }

        private void OnEventListen(object? arg0)
        {
            m_EventListener?.Invoke(arg0);
        }
    }
}

