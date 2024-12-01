using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Cocoa.Util.Container;

#if UNITY_EDITOR

namespace UnityEditor
{
    public class CustomEnumMaker : EditorWindow
    {
        #region Data Asset Reference
        private string enumCsPath = "Assets/Scripts/EnumCS";
        private TextAsset dataAsset = null;

        private List<string> database;

        #endregion
        #region OnGUI Progress Reference
        private Component selectedScripts;
        private MonoScript typeScripts;
        private List<string> hasKeyVariable = null;
        private Dictionary<string /* */, string/**/> inputDatas = new();
        private GameObject externalObject;

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

            window.ShowTab();
        }

        public void OnGUI()
        {
            if (inputDatas == null || inputDatas.Count == 0) InitInputData();

            //== Script file search
            GUILayout.Label(" ▼ 해당 도구는 Hierachy를 기반으로 합니다.");
            GUILayout.Label(" ▼ Inspector Data를 긁어오는 구조이기 때문에, 기본 스크립트 입력시 데이터를 추출할수없습니다.");

            typeScripts = EditorGUILayout.ObjectField("Target Script : ", typeScripts, typeof(MonoScript), true) as MonoScript;
            if (typeScripts == null) return;

            externalObject = EditorGUILayout.ObjectField("Target Script : ", externalObject, typeof(GameObject), true) as GameObject;
            if (externalObject == null) { return; }
            System.Type objectType = typeScripts.GetClass();

            Component monoScript = externalObject.GetComponent(objectType);
            if (monoScript != selectedScripts)
            {
                ClearInputData();
                selectedScripts = monoScript;
            }
            if (selectedScripts == null) { return; }

            //== Unipair Search
            if (hasKeyVariable == null || hasKeyVariable.Count == 0)
            {
                hasKeyVariable = new List<string>(SearchHasKeyVariable(selectedScripts));
            }

            string selectIndexKey = InputDataCategory.SelectIndex.ToString();
            if (inputDatas[selectIndexKey] == string.Empty)
            {
                inputDatas[selectIndexKey] = 0.ToString();
            }
            if (hasKeyVariable == null || hasKeyVariable.Count == 0)
            {
                return;
            }

            //== File 이름 설정
            int selectedIndex = int.Parse(inputDatas[selectIndexKey]);
            string[] array = hasKeyVariable.ToArray();

            selectedIndex = EditorGUILayout.Popup("Select Option", selectedIndex, array);
            inputDatas[selectIndexKey] = selectedIndex.ToString();

            string fieldName = hasKeyVariable[selectedIndex];
            string enumFileName = CreateFileName(selectedScripts.GetType(), fieldName);

            //== Enum 이름 탐색
            string enumName = CreateEnumName(selectedScripts.GetType().ToString(), fieldName);
            inputDatas[InputDataCategory.EnumName.ToString()] = enumName;

            if (GUILayout.Button("파일 생성"))
            {
                CreateFolder();
                CreateFile(enumFileName);
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


        private void ClearInputData()
        {
            inputDatas.Clear();
            InitInputData();
        }
        private List<string> SearchHasKeyVariable(Component selected)
        {
            List<string> pairs = new List<string>();

            System.Type type = selected.GetType();

            var variableInfos = type.GetFields(
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);

            foreach (var field in variableInfos)
            {
                System.Type fieldType = field.FieldType;

                if (!fieldType.IsGenericType || !typeof(IList).IsAssignableFrom(fieldType))
                {
                    continue;
                }

                System.Type elementType = fieldType.GetGenericArguments()[0];
                if (elementType.IsGenericType == false)
                {
                    continue;
                }

                System.Type genericType = elementType.GetGenericTypeDefinition();
                if (genericType != typeof(UniPair<,>) && genericType != typeof(KeyValuePair<,>))
                {
                    continue;
                }

                pairs.Add(field.Name);
            }

            return pairs;
        }
        private string CreateEnumName(string className, string fieldName)
        {
            string[] names = className.Split('.');
            string name = names[names.Length - 1];

            return $"{name}_{fieldName}";
        }
        private string CreateFileName(System.Type classType, string fieldName)
        {
            string[] names = classType.Name.Split('.');
            string name = names[names.Length - 1];

            return $"{name}.{fieldName}.cs";
        }
        private void CreateFolder()
        {
            string[] folder = enumCsPath.Split('/');
            string path = string.Empty;

            foreach (string file in folder)
            {
                path += file;
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                path += '/';
            }
        }

        private void CreateFile(string fileName)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"namespace Cocoa.Util");
            builder.AppendLine("{");
            builder.AppendLine($"\tpublic enum {inputDatas[InputDataCategory.EnumName.ToString()]}");
            builder.AppendLine("\t{");

            StringBuilder enumBuilder = new StringBuilder();
            string key = hasKeyVariable[int.Parse(inputDatas[InputDataCategory.SelectIndex.ToString()])];
            SerializedObject searchTarget = new SerializedObject(selectedScripts);
            SerializedProperty findProperty = searchTarget.FindProperty(key);

            for (int i = 0; i < findProperty.arraySize; i++)
            {
                SerializedProperty element = findProperty.GetArrayElementAtIndex(i);
                SerializedProperty keyValue = element.FindPropertyRelative("key");

                if (keyValue != null)
                {
                    enumBuilder.AppendLine("\t\t" + keyValue.stringValue + ",");
                }
            }

            enumBuilder.Remove(enumBuilder.Length - 3, 3);
            enumBuilder.AppendLine();
            builder.Append(enumBuilder);
            builder.AppendLine("\t}");
            builder.AppendLine("}");

            System.IO.File.WriteAllText(enumCsPath + "/" + fileName, builder.ToString(), Encoding.UTF8);

            Debug.Log("Create! \n" + builder.ToString());
        }
    }
}

#endif