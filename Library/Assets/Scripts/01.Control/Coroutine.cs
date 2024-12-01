using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cocoa.Object;

namespace Cocoa.Control
{
    public static class CoroutineHandler
    {
        private class Node
        {
            public string key;
            private Coroutine coroutine;
            private Coroutine registerRoutine;
            private IEnumerator enumerator;

            //== Has Flag
            public bool isRunning;
            public bool isStop;

            public Coroutine Coroutine
            {
                get => coroutine;
                set
                {
                    coroutine = value;
                }
            }

            public Coroutine RegisterRoutine
            {
                get => registerRoutine;
                set
                {
                    registerRoutine = value;
                }
            }
            public IEnumerator Enumerator
            {
                get => enumerator;
                set
                {
                    enumerator = value;
                }
            }

            public Node(string key)
            {
                this.key = key;
                coroutine = default;
                registerRoutine = default;
                enumerator = default;

                isRunning = false;
                isStop = false;
            }

            public void Clear()
            {
                key = string.Empty;
                coroutine = default;

                isRunning = false;
                isStop = false;
            }
        }

        private static CoroutineRunner runner = null;
        private static Dictionary<MonoBehaviour, List<Node>> regist = new();
        private static Dictionary<int, Node> independent = new();
        private static Dictionary<float, WaitForSeconds> waitCache = new();

        #region Optimization Purpose
        public static WaitForSeconds ForSeconds(float seconds)
        {
            if (!waitCache.ContainsKey(seconds))
            {
                waitCache.Add(seconds, new WaitForSeconds(seconds));
            }

            return waitCache[seconds];
        }
        #endregion

        #region Start Coroutine
        public static Coroutine Run(System.Func<IEnumerator> routine, bool overwrite = true, MonoBehaviour mono = null)
        {
            CheckRunner();
            NullProcessing(ref mono);

            if (!regist.TryGetValue(mono, out List<Node> list))
            {
                Node node = new Node(routine.Method.Name);
                node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine));
                regist.Add(mono, new List<Node> { node });

                return node.Coroutine;
            }

            int findIndex = list.FindIndex((node) => node.key.Equals(routine.Method.Name));
            if (findIndex != -1)
            {
                if (list[findIndex].isRunning)
                {
                    if (!overwrite) return list[findIndex].Coroutine;

                    StopCoroutine(list[findIndex].Coroutine);

                    Node node = new Node(routine.Method.Name);
                    node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine));
                    list.Add(node);

                    return node.Coroutine;
                }

                list[findIndex].RegisterRoutine = runner.StartCoroutine(RegisterRoutine(list[findIndex], routine));
                return list[findIndex].Coroutine;
            }
            else
            {
                Node node = new Node(routine.Method.Name);
                node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine));
                list.Add(node);

                return node.Coroutine;
            }
        }
        public static Coroutine Run<T1>(System.Func<T1, IEnumerator> routine, T1 param, bool overwrite = true, MonoBehaviour mono = null)
        {
            CheckRunner();
            NullProcessing(ref mono);

            if (!regist.TryGetValue(mono, out List<Node> list))
            {
                Node node = new Node(routine.Method.Name);
                node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine, param));
                regist.Add(mono, new List<Node> { node });

                return node.Coroutine;
            }

            int findIndex = list.FindIndex((node) => node.key.Equals(routine.Method.Name));
            if (findIndex != -1)
            {
                if (list[findIndex].isRunning)
                {
                    if (!overwrite) return list[findIndex].Coroutine;

                    StopCoroutine(list[findIndex].Coroutine);

                    Node node = new Node(routine.Method.Name);
                    node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine, param));
                    list.Add(node);

                    return node.Coroutine;
                }

                list[findIndex].RegisterRoutine = runner.StartCoroutine(RegisterRoutine(list[findIndex], routine, param));
                return list[findIndex].Coroutine;
            }
            else
            {
                Node node = new Node(routine.Method.Name);
                node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine, param));
                list.Add(node);

                return node.Coroutine;
            }
        }
        public static Coroutine Run<T1, T2>(System.Func<T1, T2, IEnumerator> routine, T1 param1, T2 param2, bool overwrite = true, MonoBehaviour mono = null)
        {
            CheckRunner();

            NullProcessing(ref mono);

            if (!regist.TryGetValue(mono, out List<Node> list))
            {
                Node node = new Node(routine.Method.Name);
                node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine, param1, param2));
                regist.Add(mono, new List<Node> { node });

                return node.Coroutine;
            }

            int findIndex = list.FindIndex((node) => node.key.Equals(routine.Method.Name));
            if (findIndex != -1)
            {
                if (list[findIndex].isRunning)
                {
                    if (!overwrite) return list[findIndex].Coroutine;

                    StopCoroutine(list[findIndex].Coroutine);

                    Node node = new Node(routine.Method.Name);
                    node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine, param1, param2));
                    list.Add(node);

                    return node.Coroutine;
                }

                list[findIndex].RegisterRoutine = runner.StartCoroutine(RegisterRoutine(list[findIndex], routine, param1, param2));
                return list[findIndex].Coroutine;
            }
            else
            {
                Node node = new Node(routine.Method.Name);
                node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine, param1, param2));
                list.Add(node);

                return node.Coroutine;
            }
        }
        public static Coroutine Run<T1, T2, T3>(System.Func<T1, T2, T3, IEnumerator> routine, T1 param1, T2 param2, T3 param3, bool overwrite = true, MonoBehaviour mono = null)
        {
            CheckRunner();

            NullProcessing(ref mono);

            if (!regist.TryGetValue(mono, out List<Node> list))
            {
                Node node = new Node(routine.Method.Name);
                node.Coroutine = runner.StartCoroutine(RegisterRoutine(node, routine, param1, param2, param3));
                regist.Add(mono, new List<Node> { node });

                return node.Coroutine;
            }

            int findIndex = list.FindIndex((node) => node.key.Equals(routine.Method.Name));
            if (findIndex != -1)
            {
                if (list[findIndex].isRunning)
                {
                    if (!overwrite) return list[findIndex].Coroutine;

                    StopCoroutine(list[findIndex].Coroutine);

                    Node node = new Node(routine.Method.Name);
                    node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine, param1, param2, param3));
                    list.Add(node);

                    return node.Coroutine;
                }
                list[findIndex].RegisterRoutine = runner.StartCoroutine(RegisterRoutine(list[findIndex], routine, param1, param2, param3));
                return list[findIndex].Coroutine;
            }
            else
            {
                Node node = new Node(routine.Method.Name);
                node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine, param1, param2, param3));
                list.Add(node);

                return node.Coroutine;
            }
        }
        public static Coroutine Run<T1, T2, T3, T4>(System.Func<T1, T2, T3, T4, IEnumerator> routine, T1 param1, T2 param2, T3 param3, T4 param4, bool overwrite = true, MonoBehaviour mono = null)
        {
            CheckRunner();

            NullProcessing(ref mono);

            if (!regist.TryGetValue(mono, out List<Node> list))
            {
                Node node = new Node(routine.Method.Name);
                node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine, param1, param2, param3, param4));
                regist.Add(mono, new List<Node> { node });

                return node.Coroutine;
            }

            int findIndex = list.FindIndex((node) => node.key.Equals(routine.Method.Name));
            if (findIndex != -1)
            {
                if (list[findIndex].isRunning)
                {
                    if (!overwrite) return list[findIndex].Coroutine;

                    StopCoroutine(list[findIndex].Coroutine);

                    Node node = new Node(routine.Method.Name);
                    node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine, param1, param2, param3, param4));
                    list.Add(node);

                    return node.Coroutine;
                }

                list[findIndex].RegisterRoutine = runner.StartCoroutine(RegisterRoutine(list[findIndex], routine, param1, param2, param3, param4));
                return list[findIndex].Coroutine;
            }
            else
            {
                Node node = new Node(routine.Method.Name);
                node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine, param1, param2, param3, param4));
                list.Add(node);

                return node.Coroutine;
            }
        }
        public static Coroutine Run<T1, T2, T3, T4, T5>(System.Func<T1, T2, T3, T4, T5, IEnumerator> routine, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, bool overwrite = true, MonoBehaviour mono = null)
        {
            CheckRunner();

            NullProcessing(ref mono);

            if (!regist.TryGetValue(mono, out List<Node> list))
            {
                Node node = new Node(routine.Method.Name);
                node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine, param1, param2, param3, param4, param5));
                regist.Add(mono, new List<Node> { node });

                return node.Coroutine;
            }

            int findIndex = list.FindIndex((node) => node.key.Equals(routine.Method.Name));
            if (findIndex != -1)
            {
                if (list[findIndex].isRunning)
                {
                    if (!overwrite) return list[findIndex].Coroutine;

                    StopCoroutine(list[findIndex].Coroutine);

                    Node node = new Node(routine.Method.Name);
                    node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine, param1, param2, param3, param4, param5));
                    list.Add(node);

                    return node.Coroutine;
                }

                list[findIndex].RegisterRoutine = runner.StartCoroutine(RegisterRoutine(list[findIndex], routine, param1, param2, param3, param4, param5));
                return list[findIndex].Coroutine;
            }
            else
            {
                Node node = new Node(routine.Method.Name);
                node.RegisterRoutine = runner.StartCoroutine(RegisterRoutine(node, routine, param1, param2, param3, param4, param5));
                list.Add(node);

                return node.Coroutine;
            }
        }


        public static int IndependentRun(System.Func<IEnumerator> routine)
        {
            CheckRunner();

            Node node = new Node(routine.Method.Name);
            int key = Util.Function.CreateKey();

            runner.StartCoroutine(RegisterIndependentRoutine(key, node, routine));

            independent.Add(key, node);
            return key;
        }
        public static int IndependentRun<T1>(System.Func<T1, IEnumerator> routine, T1 param1)
        {
            CheckRunner();

            Node node = new Node(routine.Method.Name);
            int key = Util.Function.CreateKey();

            runner.StartCoroutine(RegisterIndependentRoutine(key, node, routine, param1));

            independent.Add(key, node);
            return key;
        }
        public static int IndependentRun<T1, T2>(System.Func<T1, T2, IEnumerator> routine, T1 param1, T2 param2)
        {
            CheckRunner();

            Node node = new Node(routine.Method.Name);
            int key = Util.Function.CreateKey();

            runner.StartCoroutine(RegisterIndependentRoutine(key, node, routine, param1, param2));

            independent.Add(key, node);
            return key;
        }
        public static int IndependentRun<T1, T2, T3>(System.Func<T1, T2, T3, IEnumerator> routine, T1 param1, T2 param2, T3 param3)
        {
            CheckRunner();

            Node node = new Node(routine.Method.Name);
            int key = Util.Function.CreateKey();

            runner.StartCoroutine(RegisterIndependentRoutine(key, node, routine, param1, param2, param3));

            independent.Add(key, node);
            return key;
        }
        public static int IndependentRun<T1, T2, T3, T4>(System.Func<T1, T2, T3, T4, IEnumerator> routine, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            CheckRunner();

            Node node = new Node(routine.Method.Name);
            int key = Util.Function.CreateKey();

            runner.StartCoroutine(RegisterIndependentRoutine(key, node, routine, param1, param2, param3, param4));

            independent.Add(key, node);
            return key;
        }
        public static int IndependentRun<T1, T2, T3, T4, T5>(System.Func<T1, T2, T3, T4, T5, IEnumerator> routine, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            CheckRunner();

            Node node = new Node(routine.Method.Name);
            int key = Util.Function.CreateKey();

            runner.StartCoroutine(RegisterIndependentRoutine(key, node, routine, param1, param2, param3, param4, param5));

            independent.Add(key, node);
            return key;
        }
        #endregion

        private static (bool result, int index) Search(Coroutine coroutine, MonoBehaviour mono = null)
        {
            NullProcessing(ref mono);

            if (!regist.TryGetValue(mono, out List<Node> list))
            {
                return (false, -1);
            }

            int findIndex = list.FindIndex((node) => node.Coroutine == coroutine);
            if (findIndex == -1) { return (false, -1); }

            return (true, findIndex);
        }
        private static Coroutine Search(int key)
        {
            if (!independent.TryGetValue(key, out Node value))
            {
                return null;
            }

            return value.Coroutine;
        }

        #region Stop Coroutine

        public static bool PauseCoroutine(Coroutine coroutine, MonoBehaviour mono = null)
        {
            NullProcessing(ref mono);

            var find = Search(coroutine, mono);
            if (!find.result) { return false; }

            if (!regist[mono][find.index].isRunning) { return false; }
            if (regist[mono][find.index].isStop) { return false; }

            regist[mono][find.index].isStop = true;
            runner.StopCoroutine(regist[mono][find.index].Enumerator);

            return true;
        }
        public static bool PauseCoroutines(MonoBehaviour mono = null)
        {
            NullProcessing(ref mono);

            if (!regist.TryGetValue(mono, out List<Node> list))
            {
                return false;
            }

            foreach (Node node in list)
            {
                if (node.isRunning)
                {
                    node.isStop = true;
                    runner.StopCoroutine(node.Enumerator);
                }
            }
            return true;
        }
        public static void PauseAllCoroutines()
        {
            CheckRunner();

            foreach (MonoBehaviour key in regist.Keys)
            {
                foreach (Node node in regist[key])
                {
                    if (node.isRunning && !node.isStop)
                    {
                        node.isStop = true;
                        runner.StopCoroutine(node.Enumerator);
                    }
                }
            }
        }
        #endregion

        #region Restart Coroutine
        public static bool RestartCoroutine(Coroutine coroutine, MonoBehaviour mono = null)
        {
            NullProcessing(ref mono);

            var find = Search(coroutine, mono);
            if (!find.result) { return false; }

            List<Node> list = regist[mono];

            if (!list[find.index].isRunning) { return false; }
            if (!list[find.index].isStop) { return false; }

            list[find.index].isStop = false;
            runner.StartCoroutine(list[find.index].Enumerator);

            return true;
        }
        public static bool RestartCoroutine(MonoBehaviour mono = null)
        {
            NullProcessing(ref mono);

            if (!regist.TryGetValue(mono, out List<Node> list))
            {
                return false;
            }

            foreach (Node node in list)
            {
                if (node.isRunning && node.isStop)
                {
                    node.isStop = false;
                    runner.StartCoroutine(node.Enumerator);
                }
            }

            return true;
        }
        public static void RestartAllCoroutines()
        {
            CheckRunner();

            foreach (MonoBehaviour key in regist.Keys)
            {
                foreach (Node node in regist[key])
                {
                    if (node.isRunning && node.isStop)
                    {
                        node.isStop = false;
                        runner.StartCoroutine(node.Enumerator);
                    }
                }
            }
        }
        #endregion

        #region Kill Coroutine
        //== Other Name : Kill Coroutine
        public static bool StopCoroutine(Coroutine coroutine, MonoBehaviour mono = null)
        {
            NullProcessing(ref mono);

            var find = Search(coroutine, mono);
            if (!find.result) { return false; }

            runner.StopCoroutine(regist[mono][find.index].RegisterRoutine);
            if (regist[mono][find.index].Coroutine != null)
            {
                runner.StopCoroutine(regist[mono][find.index].Coroutine);
            }

            regist[mono][find.index].RegisterRoutine = null;
            regist[mono][find.index].Coroutine = null;
            regist[mono].RemoveAt(find.index);

            return true;
        }
        public static bool StopCoroutines(MonoBehaviour mono = null)
        {
            NullProcessing(ref mono);

            if (!regist.TryGetValue(mono, out List<Node> list))
            {
                return false;
            }

            foreach (Node node in list)
            {
                if (node.isRunning)
                {
                    runner.StopCoroutine(node.RegisterRoutine);
                    if (node.Coroutine != null)
                    {
                        runner.StopCoroutine(node.Coroutine);
                    }
                }
            }

            regist[mono].Clear();
            return true;
        }
        public static void StopAllCoroutines()
        {
            CheckRunner();

            foreach (MonoBehaviour key in regist.Keys)
            {
                foreach (Node node in regist[key])
                {
                    if (node.isRunning && !node.isStop)
                    {
                        runner.StopCoroutine(node.RegisterRoutine);
                        if (node.Coroutine != null)
                        {
                            runner.StopCoroutine(node.Coroutine);
                        }
                    }
                }
                regist[key].Clear();
            }
        }
        #endregion

        public static bool IsFinish(Coroutine coroutine, MonoBehaviour mono = null)
        {
            NullProcessing(ref mono);

            var find = Search(coroutine, mono);
            if (!find.result) { return false; }

            if (regist[mono][find.index].isRunning) { return false; }

            return true;
        }
        public static bool IsPause(Coroutine coroutine, MonoBehaviour mono = null)
        {
            NullProcessing(ref mono);

            var find = Search(coroutine, mono);
            if (!find.result) { return false; }

            return regist[mono][find.index].isStop;
        }

        #region Enumerator Register
        private static IEnumerator RegisterRoutine(Node node, System.Func<IEnumerator> func)
        {
            node.isRunning = true;
            yield return (node.Enumerator = func());
            yield return (node.Coroutine = runner.StartCoroutine(node.Enumerator));
            node.isRunning = false;
        }
        private static IEnumerator RegisterRoutine<T1>(Node node, System.Func<T1, IEnumerator> func, T1 param)
        {
            node.isRunning = true;
            yield return (node.Enumerator = func(param));
            yield return (node.Coroutine = runner.StartCoroutine(node.Enumerator));
            node.isRunning = false;
        }
        private static IEnumerator RegisterRoutine<T1, T2>(Node node, System.Func<T1, T2, IEnumerator> func, T1 param1, T2 param2)
        {
            node.isRunning = true;
            yield return (node.Enumerator = func(param1, param2));
            yield return (node.Coroutine = runner.StartCoroutine(node.Enumerator));
            node.isRunning = false;
        }
        private static IEnumerator RegisterRoutine<T1, T2, T3>(Node node, System.Func<T1, T2, T3, IEnumerator> func, T1 param1, T2 param2, T3 param3)
        {
            node.isRunning = true;
            yield return (node.Enumerator = func(param1, param2, param3));
            yield return (node.Coroutine = runner.StartCoroutine(node.Enumerator));
            node.isRunning = false;
        }
        private static IEnumerator RegisterRoutine<T1, T2, T3, T4>(Node node, System.Func<T1, T2, T3, T4, IEnumerator> func, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            node.isRunning = true;
            yield return (node.Enumerator = func(param1, param2, param3, param4));
            yield return (node.Coroutine = runner.StartCoroutine(node.Enumerator));
            node.isRunning = false;
        }
        private static IEnumerator RegisterRoutine<T1, T2, T3, T4, T5>(Node node, System.Func<T1, T2, T3, T4, T5, IEnumerator> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            node.isRunning = true;
            yield return (node.Enumerator = func(param1, param2, param3, param4, param5));
            yield return (node.Coroutine = runner.StartCoroutine(node.Enumerator));
            node.isRunning = false;
        }

        #region Register Drift Routine
        private static IEnumerator RegisterIndependentRoutine(int key, Node node, System.Func<IEnumerator> func)
        {
            node.isRunning = true;
            yield return (node.Enumerator = func());
            yield return (node.Coroutine = runner.StartCoroutine(node.Enumerator));
            independent.Remove(key);
            node.isRunning = false;
        }
        private static IEnumerator RegisterIndependentRoutine<T1>(int key, Node node, System.Func<T1, IEnumerator> func, T1 param)
        {
            node.isRunning = true;
            yield return (node.Enumerator = func(param));
            yield return (node.Coroutine = runner.StartCoroutine(node.Enumerator));
            independent.Remove(key);
            node.isRunning = false;
        }
        private static IEnumerator RegisterIndependentRoutine<T1, T2>(int key, Node node, System.Func<T1, T2, IEnumerator> func, T1 param1, T2 param2)
        {
            node.isRunning = true;
            yield return (node.Enumerator = func(param1, param2));
            yield return (node.Coroutine = runner.StartCoroutine(node.Enumerator));
            independent.Remove(key);
            node.isRunning = false;
        }
        private static IEnumerator RegisterIndependentRoutine<T1, T2, T3>(int key, Node node, System.Func<T1, T2, T3, IEnumerator> func, T1 param1, T2 param2, T3 param3)
        {
            node.isRunning = true;
            yield return (node.Enumerator = func(param1, param2, param3));
            yield return (node.Coroutine = runner.StartCoroutine(node.Enumerator));
            independent.Remove(key);
            node.isRunning = false;
        }
        private static IEnumerator RegisterIndependentRoutine<T1, T2, T3, T4>(int key, Node node, System.Func<T1, T2, T3, T4, IEnumerator> func, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            node.isRunning = true;
            yield return (node.Enumerator = func(param1, param2, param3, param4));
            yield return (node.Coroutine = runner.StartCoroutine(node.Enumerator));
            independent.Remove(key);
            node.isRunning = false;
        }
        private static IEnumerator RegisterIndependentRoutine<T1, T2, T3, T4, T5>(int key, Node node, System.Func<T1, T2, T3, T4, T5, IEnumerator> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            node.isRunning = true;
            yield return (node.Enumerator = func(param1, param2, param3, param4, param5));
            yield return (node.Coroutine = runner.StartCoroutine(node.Enumerator));
            independent.Remove(key);
            node.isRunning = false;
        }
        #endregion

        #endregion

        private static void CheckRunner()
        {
            if (runner == null)
            {
                GameObject runner = new GameObject("Coroutine Runner");
                CoroutineHandler.runner = runner.AddComponent<CoroutineRunner>();
            }
        }

        public static void SetExternalRunner(CoroutineRunner runner)
        {
            if (CoroutineHandler.runner == null)
            {
                CoroutineHandler.runner = runner;
            }
        }

        private static void NullProcessing(ref MonoBehaviour mono)
        {
            if (mono == null) mono = runner.GetComponent<MonoBehaviour>();
        }
    }
}