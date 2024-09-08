using System.Collections.Generic;
using UnityEngine;

namespace Manager
{
    public class Sound : Util.Inherited.DisposableSingleton<Sound>
    {
        public enum Type
        {
            BGM,
            EFFECT,
            VOICE,
            MASTER
        }
        [System.Serializable]
        private class Clip
        {
            public string id;
            public Type type;
            public AudioClip audio;
            public int priority;
            public float defaultVolum = 0.5f;
        }

        [System.Serializable]
        public class State
        {
            public bool mute;
            public float volum;
            [ReadOnly] public string baseKey;

            public State(bool mute, float volum, string baseKey)
            {
                this.mute = mute;
                this.volum = volum;
                this.baseKey = baseKey;
            }

            public State Clone()
            {
                return new State(mute, volum, baseKey);
            }
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
            [ReadOnly] public float currentTime;
            [ReadOnly] public float originalVolum;

            [Header("Sound")]
            public int priority;
            [ReadOnly] public AudioSource player;
            [ReadOnly] public string clipID;

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

        [Header("Setting")]
        [SerializeField] bool fullStackIsSkip;
        [SerializeField, ReadOnly] bool isPause;
        [SerializeField, ReadOnly] bool onReady;
        Dictionary<Type, State> states;

        [Header("Has Data")]
        [SerializeField] float productDuration = 0.2f;
#if UNITY_EDITOR
        [SerializeField] int editSfxCount;
#endif
        [SerializeField] Queue<Registration> callStack = new Queue<Registration>();

        //== Variable declaration for improved readability
        private const int On = 1;
        private const int Off = 0;
        private const string Mute = "MUTE";
        private const string Volum = "VOLUM";

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
                if (editSfxCount <= 0) editSfxCount = 5;

                if (sfx == null) sfx = new List<Audio>();

                for (int i = 0; i < editSfxCount; i++)
                {
                    GameObject sfxPlayer = new GameObject("SFX Player");
                    sfxPlayer.transform.SetParent(this.transform);

                    Audio audio = new Audio(sfxPlayer.AddComponent<AudioSource>());
                    audio.player.playOnAwake = false;
                    sfx.Add(audio);
                }
            }

            if (clips != null)
            {
                foreach (var item in clips)
                {
                    var clip = item.value;
                    if (clip.type == Type.MASTER)
                    {
                        Debug.LogError($"Sound Clip cannot be of MASTER type.\n" +
                            $"Clip id is [ {clip.id} ]");
                    }
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
        private void InitializeState()
        {
            if (states != null && states.Count != 0) return;

            states = new Dictionary<Type, State>();

            var types = System.Enum.GetValues(typeof(Type));
            foreach (var value in types)
            {
                Type type = (Type)value;

                //== PlayerPrefabs save key
                string baseKey = $"SET_SOUND_KEY_{type}_";
                State state = new State(false, 1.0f, baseKey);
                states.Add(type, state);
            }
        }

        #region Load & Save : Setting
        private void LoadSetting()
        {
            foreach (var item in states.Values)
            {
                State state = item;
                int readMute = PlayerPrefs.GetInt(state.baseKey + Mute, Off);
                bool mute = (readMute == On) ? true : false;
                float volum = PlayerPrefs.GetFloat(state.baseKey + Volum, 1.0f);

                state.mute = mute;
                state.volum = volum;
            }

            State master = states[Type.MASTER];
            AudioListener.volume = master.volum;
        }
        private void SaveSetting()
        {
            foreach (var state in states.Values)
            {
                SaveState(state);
            }
        }
        private void SaveState(State state)
        {
            PlayerPrefs.SetInt(state.baseKey + Mute, (state.mute) ? On : Off);
            PlayerPrefs.SetFloat(state.baseKey + Volum, state.volum);
        }
        #endregion

        private void InitializeAudio()
        {
            bgm.player.mute = states[Type.BGM].mute || states[Type.MASTER].mute;
        }

        public void ChangState(Type type, bool mute, float volum)
        {
            State state = states[type];

            //== Updated the list of currently playing sounds
            if (type == Type.MASTER)
            {
                AudioListener.volume = volum;
            }
            else if (type == Type.BGM)
            {
                UpdateAudio(bgm, in state, in mute, in volum);
            }
            else
            {
                foreach (var item in sfx)
                {
                    if (item.clipID.CompareTo(string.Empty) == 0) continue;
                    Clip clip = database[item.clipID];

                    if (clip.type != type) continue;
                    UpdateAudio(item, in state, in mute, in volum);
                }
            }

            state.mute = mute;
            state.volum = volum;

            SaveState(state);

            void UpdateAudio(Audio audio,in State state,in bool mute,in float volum)
            {
                if (audio.onProductionUpdate)
                {
                    float originalVolum = audio.originalVolum;
                    if (state.volum != 0) originalVolum /= state.volum;
                    else originalVolum = 0;

                    audio.originalVolum = originalVolum * volum;
                }
                else
                {
                    float originalVolum = audio.player.volume;
                    if (state.volum != 0) originalVolum /= state.volum;
                    else originalVolum = 0;

                    audio.player.volume = originalVolum * volum;
                }

                audio.player.mute = mute;
            }
        }

        //== Return only a clone to prevent direct external modification
        public State GetStateClone(Type type)
        {
            return states[type].Clone();
        }

        public void PlaySound(string id, float volum = -1, bool loop = true, Production product = Production.None)
        {
            if (!onReady)
            {
                Regist(new Registration(id, volum, true, loop, product));
                return;
            }

            Clip clip = GetClip(id);

            if (clip == null) { return; }

            if (clip.type == Type.BGM) PlayBgm(clip, volum, loop, product);
            else PlaySfx(clip, volum, loop, product);
        }
        private void PlaySfx(Clip clip, float volum = -1, bool loop = false, Production product = Production.None)
        {
            State state = states[clip.type];

            if (volum == -1) volum = clip.defaultVolum * state.volum;
            else volum *= state.volum;

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

            SetAudio(sound, state, clip, volum, loop, product);

            sound.player.Play();
        }
        private void PlayBgm(Clip clip, float volum = -1, bool loop = false, Production product = Production.None)
        {
            if (volum == -1) volum = clip.defaultVolum * states[Type.BGM].volum;
            else volum *= states[Type.BGM].volum;

            SetAudio(bgm, states[Type.BGM], clip, volum, loop, product);
            bgm.player.Play();
        }
        private void SetAudio(Audio audio, State state, Clip clip, float volum = -1, bool loop = false, Production product = Production.None)
        {
            audio.player.loop = loop;
            audio.player.clip = clip.audio;
            audio.player.mute = state.mute || states[Type.MASTER].mute;
            audio.clipID = clip.id; 

            if (product != Production.None)
            {
                audio.onProductionUpdate = true;
                audio.production = product;
                audio.currentTime = 0;

                if (audio.production == Production.FadeIn)
                {
                    audio.player.volume = 0;
                }
                else if(audio.production == Production.FadeOut)
                {
                    audio.player.volume = volum;
                }
                else
                {
                    Debug.Log("A new sound effect method has been added. " +
                        "Please implement the corresponding logic for the effect.");
                }
                audio.originalVolum = volum;
            }
            else
            {
                audio.production = Production.None;
                audio.onProductionUpdate = false;
                audio.player.volume = volum;
            }
        }

        public void PauseAllSound()
        {
            AudioListener.pause = true;
        }
        public void UnPauseAllSound()
        {
            AudioListener.pause = false;
        }

        private void Regist(Registration registration)
        {
            callStack.Enqueue(registration);
        }
        private void UnRegist()
        {
            Registration data = callStack.Dequeue();

            PlaySound(data.id, data.defaultVolum, data.isLoop, data.fade);
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
        private void AudioProduct(Audio audio)
        {
            if (audio.production == Production.None)
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
                if (1 <= ratio)
                {
                    audio.onProductionUpdate = false;
                    audio.player.volume = originalVolum;
                }
                else
                {
                    audio.player.volume = originalVolum * ratio;
                }
            }
            else if (audio.production == Production.FadeOut)
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
                if (bgm.onProductionUpdate) { AudioProduct(bgm); }
                foreach (var item in sfx)
                {
                    if (item.onProductionUpdate)
                    {
                        AudioProduct(item);
                    }
                }

                //== Used to handle sound calls made before the Sound Manager is initialized
                if (callStack.Count <= 0) return;
                UnRegist();
            }
        }
    }
}