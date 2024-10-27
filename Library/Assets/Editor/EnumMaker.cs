using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Util.Inspector;


#if UNITY_EDITOR

namespace UnityEditor
{
    public class CustomEnumMaker : EditorWindow
    {
        #region Data Asset Reference
        private string dataAssetPath = "Assets/Editor/Data";
        private string dataAssetName = "CustomEnumMakerData.txt";
        private TextAsset dataAsset = null;

        private Dictionary<string, List<(string field, string @enum)>> database;

        private readonly string splitUnit = "/";
        private readonly int unitCount = 3;
        #endregion
        #region OnGUI Progress Reference
        private MonoScript selectedScripts;
        private List<string> hasKeyVariable = null;
        private Dictionary<string /* */, string/**/> inputDatas = new();

        //== Name list
        private enum InputDataCategory
        {
            EnumName,
            SelectIndex
        }
        #endregion

        [MenuItem("Tools/Custom/EnumMaker")]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow(typeof(CustomEnumMaker));
            window.minSize = new Vector2(500, 500);
        }

        public void OnGUI()
        {
            if (inputDatas == null || inputDatas.Count == 0) InitInputData();

            //== Script file search
            EditorGUILayout.LabelField(" ▼ 해당 도구는 Hierachy를 기반으로 합니다.");
            EditorGUILayout.LabelField(" ▼ Inspector Data를 긁어오는 구조이기 때문에, 기본 스크립트 입력시 데이터를 추출할수없습니다.");

            var script = (MonoScript)EditorGUILayout.ObjectField("Target Script : ", selectedScripts, typeof(MonoScript), false);
            if (script != selectedScripts)
            {
                ClearInputData();
            }
            if (selectedScripts == null)
            {
                return;
            }
            //== Unipair Search
            if (hasKeyVariable == null) hasKeyVariable = SearchHasKeyVariable(selectedScripts);
            string selectIndexKey = InputDataCategory.SelectIndex.ToString();
            if (inputDatas[selectIndexKey] == string.Empty)
            {
                inputDatas[selectIndexKey] = 0.ToString();
            }
            int selectedIndex = EditorGUILayout.Popup("Select Option", int.Parse(inputDatas[selectIndexKey]), hasKeyVariable.ToArray());
            inputDatas[selectIndexKey] = selectedIndex.ToString();

            //== File 이름 설정
            string fieldName = hasKeyVariable[selectedIndex];
            string enumFileName = CreateFileName(script.GetClass(), fieldName);

            //== Enum 이름 탐색
            string enumName = SearchEnumName(enumFileName, fieldName);
            string buttonText = "파일 생성";
            if (!string.IsNullOrEmpty(enumName))
            {
                EditorGUI.BeginDisabledGroup(true);
                inputDatas[InputDataCategory.EnumName.ToString()] = EditorGUILayout.TextField("열거형 이름을 지정해주세요. ", inputDatas[InputDataCategory.EnumName.ToString()]);
                EditorGUI.EndDisabledGroup();

                buttonText = "파일 갱신";
            }
            else
            {
                //== 이름 지정
                inputDatas[InputDataCategory.EnumName.ToString()] = EditorGUILayout.TextField("열거형 이름을 지정해주세요. ", inputDatas[InputDataCategory.EnumName.ToString()]);

                if (SearchSameName(inputDatas[InputDataCategory.EnumName.ToString()], enumFileName, fieldName))
                {
                    EditorGUILayout.LabelField("[!] 중복된 열거형 이름이 존재합니다. [!]");
                }
            }

            if (GUILayout.Button(buttonText))
            {
                CreateFile();
            }
        }

        private void InitInputData()
        {
            inputDatas = new Dictionary<string, string>();

            var category = System.Enum.GetValues(typeof(InputDataCategory));
            foreach (var name in category)
            {
                inputDatas.Add(name.ToString(), string.Empty);
            }
        }
        private void DataLoad()
        {
            string[] folder = dataAssetPath.Split('/');
            string path = string.Empty;

            foreach (string file in folder)
            {
                path += folder;
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                path += '/';
            }
            path += dataAssetName;
            if (!System.IO.File.Exists(path))
            {
                System.IO.File.WriteAllText(path, string.Empty, System.Text.Encoding.UTF8);
                AssetDatabase.Refresh();
            }

            dataAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (dataAsset == null) return;

            string text = dataAsset.text;
            text = Regex.Replace(text, "\r|\t", "");

            database = new();
            string[] lines = text.Split("\n");

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.CompareTo(string.Empty) == 0)
                {
                    continue;
                }

                string[] unit = line.Split(splitUnit);
                if (unit.Length != unitCount)
                {
                    continue;
                }

                if (!database.ContainsKey(unit[0]))
                {
                    database.Add(unit[0], new List<(string member, string @enum)>());
                }

                database[unit[0]].Add(new(unit[1], unit[2]));
            }
        }
        private void ClearInputData()
        {
            foreach (var key in inputDatas.Keys)
            {
                inputDatas[key] = string.Empty;
            }
            InitInputData();
        }
        private List<string> SearchHasKeyVariable(MonoScript selected)
        {
            List<string> pairs = new List<string>();

            System.Type type = selected.GetClass();

            var variableInfos = type.GetFields(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

            foreach (var field in variableInfos)
            {
                System.Type fieldType = field.FieldType;

                if (!(fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(IList)))
                {
                    continue;
                }

                System.Type elementType = fieldType.GetGenericArguments()[0];
                if (elementType.GetGenericTypeDefinition() != typeof(UniPair<,>) &&
                    elementType.GetGenericTypeDefinition() != typeof(KeyValuePair<,>))
                {
                    continue;
                }

                pairs.Add(field.Name);
            }

            return pairs;
        }
        private string SearchEnumName(string fileName, string fieldName)
        {
            if (!database.TryGetValue(fileName, out var nodes))
            {
                return string.Empty;
            }

            foreach (var node in nodes)
            {
                if(node.field == fieldName)
                {
                    return node.@enum;
                }
            }

            return string.Empty;
        }
        private string CreateFileName(System.Type classType, string fieldName)
        {
            string[] names = classType.Name.Split('.');
            string name = names[names.Length-1];

            return $"{name}.{fieldName}.cs";
        }
        private bool SearchSameName(string enumName, string fileName, string fieldName)
        {
            string searchName = SearchEnumName(fileName, fieldName);

            if (string.IsNullOrEmpty(searchName))
            {
                return false;
            }

            foreach(var data in database)
            {
                foreach(var node in data.Value)
                {
                    if (node.field == enumName) return true;
                }
            }

            return false;
        }
        private void CreateFile()
        {

        }

        private void Update()
        {
            if (dataAsset == null)
            {
                DataLoad();
            }
        }
    }
}

#endif
