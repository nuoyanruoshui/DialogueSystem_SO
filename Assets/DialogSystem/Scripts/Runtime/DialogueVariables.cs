using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace NuoYan.DialogSystem
{
    [Serializable]
    public class DialogueVariables
    {
        [SerializeField] private List<FlagData> flagDataList = new();

        /// <summary>
        /// 获取标志值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public bool GetBool(string key, bool defaultValue = false)
        {
            if (string.IsNullOrEmpty(key)) return defaultValue;
            foreach (var e in flagDataList)
            {
                if (e.key == key) return e.value;
            }
            return defaultValue;
        }

        /// <summary>
        /// 设置标志值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetBool(string key, bool value)
        {
            for (int i = 0; i < flagDataList.Count; i++)
            {
                if (flagDataList[i].key == key)
                {
                    flagDataList[i] = new FlagData { key = key, value = value };
                    return;
                }
            }
            flagDataList.Add(new FlagData { key = key, value = value });
        }

        public void ClearAll()
        {
            flagDataList.Clear();
        }

        public void CopyFrom(DialogueVariables other)
        {
            if (other == null) return;
            flagDataList.Clear();
            foreach (var flag in other.flagDataList)
                flagDataList.Add(new FlagData { key = flag.key, value = flag.value });
        }

        public string[] GetAllKeys()
        {
            var keys = new string[flagDataList.Count];
            for (int i = 0; i < flagDataList.Count; i++)
                keys[i] = flagDataList[i].key;
            return keys;
        }

        /// <summary>
        /// 标志数据
        /// </summary>
        [Serializable]
        private struct FlagData
        {
            public string key;
            public bool value;
        }
    }


}