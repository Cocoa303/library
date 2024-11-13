using System.Collections;
using System.Collections.Generic;
using System.Text;
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

        private List<string> database;

        #endregion
        #region OnGUI Progress Reference
        private Component selectedScripts;
        private MonoScript typeScripts;
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
            typeScripts = EditorGUILayout.ObjectField("Target Script : ", typeScripts, typeof(MonoScript), true) as MonoScript;
            if (typeScripts == null) return;

            var externalObject = EditorGUILayout.ObjectField("Target Script : ", selectedScripts, typeof(GameObject),true) as GameObject;
            if (externalObject == null) return;
            System.Type objectType = typeScripts.GetClass();

            Component monoScript = externalObject.GetComponent(objectType);
            if (monoScript != selectedScripts)
            {
                ClearInputData();
                selectedScripts = monoScript;
            }
            if (selectedScripts == null)
            {
                return;
            }

            //== Unipair Search
            if (hasKeyVariable == null || hasKeyVariable.Count == 0)
            {
                hasKeyVariable = SearchHasKeyVariable(selectedScripts);
            }
            string selectIndexKey = InputDataCategory.SelectIndex.ToString();
            if (inputDatas[selectIndexKey] == string.Empty)
            {
                inputDatas[selectIndexKey] = 0.ToString();
            }
            int selectedIndex = EditorGUILayout.Popup("Select Option", int.Parse(inputDatas[selectIndexKey]), hasKeyVariable.ToArray());
            inputDatas[selectIndexKey] = selectedIndex.ToString();

            if (hasKeyVariable.Count == 0)
            {              
                return;
            }

            //== File 이름 설정
            string fieldName = hasKeyVariable[selectedIndex];
            string enumFileName = CreateFileName(selectedScripts.GetType(), fieldName);

            //== Enum 이름 탐색
            string enumName = CreateEnumName(enumFileName, fieldName);
            if (GUILayout.Button("파일 생성"))
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
                System.IO.File.WriteAllText(path, string.Empty,Encoding.UTF8);
                AssetDatabase.Refresh();
            }

            dataAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (dataAsset == null) return;

            string text = dataAsset.text;
            text = Regex.Replace(text, "\r|\t|", "");
            text = text.Replace("\\","");

            database = new();
            string[] lines = text.Split("\n");

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.CompareTo(string.Empty) == 0)
                {
                    break;
                }

                database.Add(line);
            }
        }
        private void ClearInputData()
        {
            inputDatas.Clear();
            InitInputData();
        }
        private List<string> SearchHasKeyVariable(Component selected)
        {
            List<string> pairs = new List<string>();

            System.Type type = selected.GetType();
            Debug.Log(type.FullName);

            var variableInfos = type.GetFields(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

            foreach (var field in variableInfos)
            {
                System.Type fieldType = field.FieldType;

                if (!fieldType.IsGenericType || !typeof(IList).IsAssignableFrom(fieldType))
                {
                    continue;
                }

                System.Type elementType = fieldType.GetGenericArguments()[0];
                if(elementType.IsGenericType == false)
                {
                    continue;
                }

                System.Type genericType = elementType.GetGenericTypeDefinition();
                if (genericType != typeof(UniPair<,>) && genericType != typeof(KeyValuePair<,>))
                {
                    continue;
                }

                pairs.Add(field.Name);
                Debug.Log(field.Name);
            }

            return pairs;
        }
        private string CreateEnumName(string className,string fieldName)
        {
            return $"{className}_{fieldName}";
        }
        private string CreateFileName(System.Type classType, string fieldName)
        {
            string[] names = classType.Name.Split('.');
            string name = names[names.Length-1];

            return $"{name}.{fieldName}.cs";
        }
        private void CreateFile()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"namespace Cocoa.Util");
            builder.AppendLine("{");
            builder.AppendLine($"\tpublic enum {inputDatas[InputDataCategory.EnumName.ToString()]}");
            builder.AppendLine("\t{");
            builder.AppendLine("\t}");
            builder.AppendLine("}");
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
