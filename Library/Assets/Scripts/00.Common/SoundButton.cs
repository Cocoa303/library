using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Common
{
    public class SoundButton : MonoBehaviour
    {
        [SerializeField] Button trigger;
        [SerializeField, ReadOnly] string soundKey;
        [SerializeField] float volume = -1;
        [SerializeField] bool isLoop;

        public string SoundKey { get => soundKey; set => soundKey = value; }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (trigger == null)
            {
                trigger = GetComponent<Button>();
            }
        }

#endif
        private void Start()
        {
            trigger.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            Cocoa.Manager.Sound.Instance.PlaySound(soundKey, volume, isLoop);
        }
    }

}
