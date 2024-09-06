using System.Collections.Generic;
using UnityEngine;

namespace Manager
{
    public class Sound : Util.Inherited.DisposableSingleton<Sound>
    {
        [System.Serializable]
        private class Clip
        {
            public string id;
            public float defaultVolum = 0.5f;
            public AudioClip audio;
            public int priority;
        }

        public enum Production
        {
            None,
            FadeIn,     //== Increasing
            FadeOut     //== Decreasing
        }

        [System.Serializable]
        private class Audio
        {
            [Header("Fade animation data")]
            public bool onProductionUpdate;
            public Production production;
            public float currentTime;
            public float originalVolum;

            [Header("Sound")]
            public AudioSource player;
            public int priority;

            public Audio(AudioSource player)
            {
                this.player = player;
            }
        }

        private struct Registration
        {
            public string id;
            public float defaultVolum;
            public bool isBgm;
            public bool isLoop;
            public Production fade;

            public Registration(string id, float defaultVolum, bool isBgm, bool isLoop, Production fade)
            {
                this.id = id;
                this.defaultVolum = defaultVolum;
                this.isBgm = isBgm;
                this.isLoop = isLoop;
                this.fade = fade;
            }
        }

        //== Inspector Only
        [SerializeField] List<Util.Inspector.UniPair<string, Clip>> clips;

        //== Converted to Dictionary to reduce search time
        Dictionary<string, Clip> database = new Dictionary<string, Clip>();

        [Header("Sound Player")]
        [SerializeField] Audio bgm;
        [SerializeField] List<Audio> sfx;

        [Header("Show Setting")]
        [SerializeField, ReadOnly] bool isBgmOn;
        [SerializeField, ReadOnly] bool isSfxOn;
        [SerializeField, ReadOnly] bool onReady;
        [SerializeField, ReadOnly] float bgmVolum = 1;
        [SerializeField, ReadOnly] float sfxVolum = 1;

        [Header("Has Data")]
        [SerializeField] float productDuration = 0.2f;
#if UNITY_EDITOR
        [SerializeField] int editSfxSoundCount;
#endif
        [SerializeField] List<Util.Inspector.UniPair<float, Clip>> fade;
        [SerializeField] Queue<Registration> callStack = new Queue<Registration>();

        #region Property list
        public bool IsBgmOn { get => isBgmOn; set => isBgmOn = value; }
        public bool IsSfxOn { get => isSfxOn; set => isSfxOn = value; }
        #endregion

        //== PlayerPrefabs save key
        private const string BgmMuteKey = "BMK_SD";
        private const string SfxMuteKey = "SMK_SD";
        private const string BgmVolumKey = "BVK_SD";
        private const string SfxVolumKey = "SVK_SD";

        //== Variable declaration for improved readability
        private const int On = 1;
        private const int Off = 0;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (bgm == null)
            {
                GameObject bgmPlayer = new GameObject("BGM Player");
                bgmPlayer.transform.SetParent(transform);
                this.bgm = new Audio(bgmPlayer.AddComponent<AudioSource>());
                this.bgm.player.playOnAwake = false;
            }
            if (sfx == null || sfx.Count == 0)
            {
                if (editSfxSoundCount <= 0) editSfxSoundCount = 5;

                if (sfx == null) sfx = new List<Audio>();

                for (int i = 0; i < editSfxSoundCount; i++)
                {
                    GameObject sfxPlayer = new GameObject("SFX Player");
                    sfxPlayer.transform.SetParent(this.transform);

                    Audio audio = new Audio(sfxPlayer.AddComponent<AudioSource>());
                    audio.player.playOnAwake = false;
                    sfx.Add(audio);
                }
            }
        }
#endif
        private void Start()
        {
            InitializeDatabase();
            LoadSetting();
            InitializeAudio();

            onReady = true;
        }
        private void OnApplicationQuit()
        {
            SaveSetting();
        }

        private void InitializeDatabase()
        {
            if (database != null) database.Clear();

            foreach (var item in clips)
            {
#if UNITY_EDITOR
                if (database.ContainsKey(item.key))
                {
                    Debug.Log($"{item.key} is contain key\n{item.value.audio.name}");
                    continue;
                }
#endif
                database.Add(item.key, item.value);
            }

            clips.Clear();
        }

        #region Load & Save : Setting
        private void LoadSetting()
        {
            int bgmon = PlayerPrefs.GetInt(BgmMuteKey, On);
            if (bgmon == On) isBgmOn = true;
            else isBgmOn = false;

            int sfxon = PlayerPrefs.GetInt(SfxMuteKey, On);
            if (sfxon == On) isSfxOn = true;
            else isSfxOn = false;

            bgmVolum = PlayerPrefs.GetFloat(BgmVolumKey, 1);
            sfxVolum = PlayerPrefs.GetFloat(SfxVolumKey, 1);
        }
        private void SaveSetting()
        {
            int bgmon = (isBgmOn) ? On : Off;
            PlayerPrefs.SetInt(BgmMuteKey, bgmon);

            int sfxon = (isSfxOn) ? On : Off;
            PlayerPrefs.SetInt(SfxMuteKey, sfxon);

            PlayerPrefs.SetFloat(BgmVolumKey, bgmVolum);
            PlayerPrefs.SetFloat(SfxVolumKey, sfxVolum);
        }
        #endregion

        private void InitializeAudio()
        {
            bgm.player.mute = !isBgmOn;

            foreach (var audio in sfx)
            {
                audio.player.mute = !isSfxOn;
            }
        }

        public void PlayBGMSound(string id, float volum = -1, bool loop = true, Production product = Production.None)
        {
            if (!onReady)
            {
                Regist(new Registration(id, volum, true, loop, product));
                return;
            }

            Clip clip = GetClip(id);

            if (clip == null) { return; }

            if (volum == -1) volum = clip.defaultVolum * bgmVolum;
            bgm.player.loop = loop;
            bgm.player.clip = clip.audio;

            if (product != Production.None)
            {
                bgm.onProductionUpdate = true;
                bgm.production = product;
                bgm.currentTime = 0;

                if (bgm.production == Production.FadeIn)
                {
                    bgm.player.volume = 0;
                }
                else
                {
                    bgm.player.volume = volum;
                }
                bgm.originalVolum = volum;
            }
            else
            {
                bgm.production = Production.None;
                bgm.onProductionUpdate = false;
                bgm.player.volume = volum;
            }

            bgm.player.Play();
        }
        public void PlaySfxSound(string id, float volum = -1, bool loop = false, Production product = Production.None)
        {
            if (!onReady)
            {
                Regist(new Registration(id, volum, false, loop, product));
                return;
            }

            Clip clip = GetClip(id);

            if (clip == null) { return; }
            if (volum == -1) volum = clip.defaultVolum * sfxVolum;

            Audio sound = sfx.Find((item) => item.player.isPlaying == false);

            if (sound == null)
            {
                //== Sort in ascending order of priority
                sfx.Sort((left, right) =>
                {
                    if (left.priority < right.priority) return 1;
                    else if (left.priority == right.priority) return 0;
                    else return -1;
                });

                sound = sfx[0];
            }

            sound.player.loop = loop;
            sound.player.clip = clip.audio;

            if (product != Production.None)
            {
                sound.onProductionUpdate = true;
                sound.production = product;
                sound.currentTime = 0;

                if (sound.production == Production.FadeIn)
                {
                    sound.player.volume = 0;
                }
                else
                {
                    sound.player.volume = volum;
                }
                sound.originalVolum = volum;
            }
            else
            {
                sound.production = Production.None;
                sound.onProductionUpdate = false;
                sound.player.volume = volum;
            }

            sound.player.Play();
        }

        private void Regist(Registration registration)
        {
            callStack.Enqueue(registration);
        }
        private void UnRegist()
        {
            Registration data = callStack.Dequeue();

            if (data.isBgm)
            {
                PlayBGMSound(data.id, data.defaultVolum, data.isLoop, data.fade);
            }
            else
            {
                PlaySfxSound(data.id, data.defaultVolum, data.isLoop, data.fade);
            }
        }
        private Clip GetClip(string id)
        {
            if (database.ContainsKey(id) == false)
            {
                Debug.LogError($"Not find sound file : id [ {id} ]");
                return null;
            }
            else
            {
                if (database[id].audio == null)
                {
                    Debug.LogError($"Not find audio file : id [ {id} ]");
                    return null;
                }
            }

            return database[id];
        }
        private void AudioFade(Audio audio)
        {
            if(audio.production == Production.None)
            {
                audio.onProductionUpdate = false;
                return;
            }

            audio.currentTime += Time.deltaTime;
            float originalVolum = audio.originalVolum;
            float ratio = (audio.currentTime / productDuration);

            #region Production
            if (audio.production == Production.FadeIn)
            {
                if(1 <= ratio)
                {
                    audio.onProductionUpdate = false;
                    audio.player.volume = originalVolum;
                }
                else
                {
                    audio.player.volume = originalVolum * ratio;
                }
            }
            else if(audio.production == Production.FadeOut)
            {
                if (1 <= ratio)
                {
                    audio.onProductionUpdate = false;
                    audio.player.volume = 0;
                }
                else
                {
                    audio.player.volume = originalVolum - (originalVolum * ratio);
                }
            }
            else
            {
                Debug.Log("A new sound effect method has been added. " +
                    "Please implement the corresponding logic for the effect.");
            }
            #endregion
        }

        private void Update()
        {
            if (onReady)
            {
                //== Handle production
                if (bgm.onProductionUpdate) { AudioFade(bgm); }
                foreach (var item in sfx)
                {
                    if (item.onProductionUpdate)
                    {
                        AudioFade(item);
                    }
                }

                //== Used to handle sound calls made before the Sound Manager is initialized
                if (callStack.Count <= 0) return;
                UnRegist();
            }
        }
    }
}