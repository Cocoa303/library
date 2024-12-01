using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Cocoa.Control
{
    public class Schedul : Util.Inherited.Singleton<Schedul>
    {
        #region Inner class
        [System.Serializable]
        public class Handle
        {
            private int key;
            private System.Action<int> onUpdateEvent;
            private System.Action onComplateEvent;
            private float delay;
            [ReadOnly] private float currentTime;
            private int loopCount;       //== -1 is infinite loop
            [ReadOnly] private int remainingLoopCount;

            public const int InfiniteLoop = -1;

            public int Key { get => Key; private set => Key = value; }
            public System.Action<int> OnUpdateCallback { get => onUpdateEvent; private set => onUpdateEvent = value; }
            public System.Action OnComplateEvent { get => onComplateEvent; private set => onComplateEvent = value; }
            public float Delay { get => delay; private set => delay = value; }
            public float CurrentTime { get => currentTime; private set => currentTime = value; }
            public int LoopCount { get => loopCount; private set => loopCount = value; }
            public int RemainingLoopCount { get => remainingLoopCount; private set => remainingLoopCount = value; }

            public Handle(System.Action<int> onUpdateCallback, System.Action complateEvent, float delay, int loopCount, int preRemoval)
            {
                Initialize(onUpdateCallback, complateEvent, delay, loopCount, preRemoval);
            }
            public void Initialize(System.Action<int> onUpdateCallback, System.Action onComplateEvent, float delay, int loopCount, int preRemoval)
            {
                Key = Util.Function.CreateKey();
                OnUpdateCallback = onUpdateCallback;
                OnComplateEvent = onComplateEvent;
                Delay = delay;
                LoopCount = loopCount;

                if (IsInfiniteLoop()) RemainingLoopCount = int.MaxValue;
                else RemainingLoopCount = loopCount - preRemoval;
                CurrentTime = 0.0f;
            }
            public void IncraseCurrentTime(float time)
            {
                CurrentTime += time;
            }
            public void DecraseCurrentTime(float time)
            {
                CurrentTime -= time;
            }
            public void DecraseRemainingLoopCount(int loopCount)
            {
                RemainingLoopCount -= loopCount;
            }
            public bool IsInfiniteLoop()
            {
                return loopCount == InfiniteLoop;
            }
        }
        #endregion

        [SerializeField] private List<Handle> handles = new();
        private Queue<Handle> recyclePool = new();
#if UNITY_EDITOR
        [SerializeField, ReadOnly] int handleStack = 0;
#endif

        public int Regist(System.Action<int> updateEvent, System.Action complateEvent, float delay, int loopCount = 0, int preRemoval = 0)
        {
            if (recyclePool.Count != 0)
            {
                Handle recycle = recyclePool.Dequeue();
                recycle.Initialize(updateEvent, complateEvent, delay, loopCount, preRemoval);

                handles.Add(recycle);
                UpdateHandleStack();

                return recycle.Key;
            }
            else
            {
                Handle handle = new Handle(updateEvent, complateEvent, delay, loopCount, preRemoval);
                handles.Add(handle);

                UpdateHandleStack();

                return handle.Key;
            }
        }

        private void UnRegist(int index)
        {
            recyclePool.Enqueue(handles[index]);
            handles.RemoveAt(index);

            UpdateHandleStack();
        }

        private void UpdateHandleStack()
        {
#if UNITY_EDITOR
            handleStack = handles.Count;
#endif
        }

        private void Update()
        {
            for (int index = 0; index < handles.Count; index++)
            {
                Handle handle = handles[index];

                handle.IncraseCurrentTime(Time.deltaTime);
                if (handle.CurrentTime < handle.Delay) continue;

                int count = (int)(handle.CurrentTime / handle.Delay);
                handle.DecraseCurrentTime(count * handle.Delay);

                for (int loop = 0; loop < count; loop++)
                {
                    if (handle.IsInfiniteLoop())
                    {
                        handle.OnUpdateCallback(default);
                        continue;
                    }

                    if (handle.RemainingLoopCount < 0)
                    {
                        break;
                    }

                    handle.DecraseRemainingLoopCount(1);
                    handle.OnUpdateCallback(handle.RemainingLoopCount);
                }

                if (handle.RemainingLoopCount < 0)
                {
                    handle.OnComplateEvent?.Invoke();
                    UnRegist(index);

                    //== 중도 삭제시 적용해야하는 연산.
                    index--;
                    continue;
                }

            }
        }
    }
}