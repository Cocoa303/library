using System.Text;
using UnityEngine;
using UnityEngine.Pool;
using System.Reflection;
using System.Linq;

#if UNITY_EDITOR
namespace UnityEditor
{
    public class CSVLoader : EditorWindow
    {
        //== OnGUI Reference
        private GUILayoutOption[] buttonOption = new[] { GUILayout.Width(128), GUILayout.Height(32) };
        private TextAsset csvFile;

        //== Scriptable Object Reference
        private string savePath = "Assets/Scripts/Data";
        private string dataPath = "Assets/Resources/Data";
        private string enumPath = "Assets/Scripts/Common";

        //== NOTE : CSV의 자료형 명명규칙에 따라 해당 부분은 수정되어야합니다.
        //== 현재 방식은 (자료형)(변수명)으로 진행되어있습니다.
        private (string handle, string type)[] typeRules =
        {
            ("n_", "int"),
            ("s_", "string"),
            ("f_", "float"),
            ("l_", "long"),
            ("b_", "bool"),
            ("e_", "enum")
        };

        [MenuItem("Tools/Custom/CSV Loader")]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow(typeof(CSVLoader));
            window.minSize = new Vector2(500, 500);

        }
        public void OnGUI()
        {
            #region CSV File Reference
            EditorGUILayout.LabelField(" ▼ CSV 파일을 삽입해 주세요.");
            csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File : ", csvFile, typeof(TextAsset), false);

            if (csvFile == null) EditorGUILayout.LabelField("파일이 존재하지 않습니다.");
            else
            {
                if (!(csvFile is TextAsset)) EditorGUILayout.LabelField("해당 파일은 텍스트 파일이 아닙니다.");
                else
                {
                    //== 확장자 검사
                    string assetPath = AssetDatabase.GetAssetPath(csvFile);
                    string extension = System.IO.Path.GetExtension(assetPath);

                    if (extension.CompareTo(".csv") != 0)
                    {
                        EditorGUILayout.LabelField(" ※ 해당 파일은 CSV 파일이 아닙니다.");
                        EditorGUILayout.LabelField(" ※ 확장자를 확인해주시기 바랍니다.");
                    }
                    else
                    {
                        EditorGUILayout.Space(20);
                        if (GUILayout.Button("클래스 생성 및 초기화", buttonOption))
                        {
                            Create();
                        }
                        EditorGUILayout.Space(20);
                        EditorGUILayout.LabelField(" ▼ Infomation  ▼");
                        EditorGUILayout.Space(20);

                        ShowInfomation();
                    }
                }
            }
            #endregion
        }

        private (string[,] data, int row, int col, bool isList) CSVLoad()
        {
            string[] lines = csvFile.text.Split('\n');
            int row = lines.Length;
            int col = lines[0].Split(',').Length;

            string[,] csvData = new string[row, col];

            //== Init
            for (int i = 0; i < row; i++)
            {
                string[] rowDatas = lines[i].Split(',');

                if (rowDatas.Length != col) continue;

                for (int j = 0; j < col; j++)
                {
                    csvData[i, j] = rowDatas[j].Replace("\r", "");
                }
            }

            var ids = new System.Collections.Generic.HashSet<string>();
            for (int i = 2; i < row; i++)
            {
                ids.Add(csvData[i, 0]);
            }

            return (csvData, row, col, ((row - 2) != ids.Count));
        }

        private void Create()
        {
            StringBuilder csFile = new StringBuilder(string.Empty);
            StringBuilder enumResult = new StringBuilder(string.Empty);
            var csv = CSVLoad();

            csFile.AppendLine("using UnityEngine;");
            if (csv.isList)
            {
                csFile.AppendLine("using System.Collections.Generic;\n");
            }
            csFile.AppendLine("namespace Data");
            csFile.AppendLine("{");
            csFile.AppendLine($"\t[CreateAssetMenu(fileName = \"Data\", menuName = \"Data/{csvFile.name}\")]");
            csFile.AppendLine($"\tpublic class {csvFile.name} : ScriptableObject");
            csFile.AppendLine("\t{");
            csFile.AppendLine(MakeInfomation(2, enumResult));
            csFile.AppendLine("\t}");
            csFile.AppendLine("}");

            //== CS 폴더 검증
            if (!System.IO.Directory.Exists(savePath))
            {
                System.IO.Directory.CreateDirectory(savePath);
            }

            //== CS 파일 생성
            System.IO.File.WriteAllText($"{savePath}/{csvFile.name}.cs", csFile.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();

            //== Enum 폴더 검증
            if (!System.IO.Directory.Exists(enumPath))
            {
                System.IO.Directory.CreateDirectory(enumPath);
            }

            //== Enum 파일 검증
            if (enumResult.Length > 0)
            {
                string enumFilePath = $"{enumPath}/Enum.cs";
                if (!System.IO.File.Exists(enumFilePath))
                {
                    //== Base 제작 
                    StringBuilder enumFile = new StringBuilder();
                    enumFile.AppendLine("namespace Common");
                    enumFile.AppendLine("{");
                    enumFile.AppendLine("\tpublic static class Enum");
                    enumFile.AppendLine("\t{");
                    enumFile.AppendLine(enumResult.ToString());
                    enumFile.AppendLine("\t}");
                    enumFile.AppendLine("}");

                    System.IO.File.WriteAllText(enumFilePath, enumFile.ToString(), Encoding.UTF8);
                    AssetDatabase.Refresh();
                }
                else
                {
                    string read = System.IO.File.ReadAllText(enumFilePath, Encoding.UTF8);
                    var datas = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>();

                    //== 파일 내용을 분해하여 자료화함
                    string[] pack = read.Split("public enum ");
                    if (pack.Length > 0)
                    {
                        //== 0 Index is : namespace Common { public static class Enum
                        for (int i = 1; i < pack.Length; i++)
                        {
                            string data = System.Text.RegularExpressions.Regex.Replace(pack[i], "({|}|,)", "");
                            string[] element = data.Split('\n');

                            for (int trim = 0; trim < element.Length; trim++)
                            {
                                element[trim] = element[trim].Trim();
                            }

                            string header = element[0];
                            datas.Add(header, new System.Collections.Generic.List<string>());
                            for (int elementIndex = 1; elementIndex < element.Length; elementIndex++)
                            {
                                datas[header].Add(element[elementIndex]);
                            }
                        }
                    }

                    string[] makePack = enumResult.ToString().Split("public enum ");
                    if (makePack.Length > 0)
                    {
                        //== 0 Index is : namespace Common { public static class Enum
                        for (int i = 1; i < makePack.Length; i++)
                        {
                            string data = System.Text.RegularExpressions.Regex.Replace(makePack[i], "({|}|,)", "");
                            string[] element = data.Split('\n');

                            for (int trim = 0; trim < element.Length; trim++)
                            {
                                element[trim] = element[trim].Trim();
                            }

                            string header = element[0];
                            if (datas.ContainsKey(header))
                            {
                                System.Collections.Generic.List<string> doesntExist = new System.Collections.Generic.List<string>();
                                for (int elementIndex = 1; elementIndex < element.Length; elementIndex++)
                                {
                                    if (!datas[header].Contains(element[elementIndex]))
                                    {
                                        datas[header].Add(element[elementIndex]);
                                    }
                                }
                                foreach (var item in datas[header])
                                {
                                    if (!element.Contains(item))
                                    {
                                        doesntExist.Add(item);
                                    }
                                }

                                if (doesntExist.Count > 0)
                                {
                                    StringBuilder log = new StringBuilder();
                                    log.AppendLine($"{header}에서 아래 항목이 존재하나, CSV 파일에서 찾을수 없습니다.");
                                    foreach (var item in doesntExist)
                                    {
                                        log.AppendLine(item);
                                    }

                                    Debug.Log(log.ToString());
                                }
                            }
                            else
                            {
                                datas.Add(header, new System.Collections.Generic.List<string>());
                                for (int elementIndex = 1; elementIndex < element.Length; elementIndex++)
                                {
                                    datas[header].Add(element[elementIndex]);
                                }
                            }
                        }
                    }

                    //== 파일 제작
                    StringBuilder enumFile = new StringBuilder();
                    enumFile.AppendLine("namespace Common");
                    enumFile.AppendLine("{");
                    enumFile.AppendLine("\tpublic static class Enum");
                    enumFile.AppendLine("\t{");
                    foreach (var header in datas.Keys)
                    {
                        enumFile.AppendLine($"\t\tpublic enum {header}");
                        enumFile.AppendLine($"\t\t{{");
                        foreach (var item in datas[header])
                        {
                            if (item.CompareTo(string.Empty) != 0)
                            {
                                enumFile.Append($"\t\t\t{item},\n");
                            }
                        }

                        //== 마지막 ',\n' 제거
                        enumFile = enumFile.Remove(enumFile.Length - 2, 2);
                        enumFile.AppendLine($"\n\t\t}}");
                    }
                    enumFile.AppendLine("\t}");
                    enumFile.AppendLine("}");

                    System.IO.File.WriteAllText(enumFilePath, enumFile.ToString(), Encoding.UTF8);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            //== 데이터 폴더 검증 및 재생성
            if (!System.IO.Directory.Exists(dataPath))
            {
                System.IO.Directory.CreateDirectory(dataPath);
            }

            string scriptablePath = dataPath + "/" + csvFile.name;
            if (System.IO.Directory.Exists(scriptablePath))
            {
                string[] assetPaths = System.IO.Directory.GetFiles(scriptablePath, "*.asset");

                //== 기존에 존재하던 파일들 제거 [ 데이터 중복과, 형식 변환 등으로 비어지는 데이터를 없게 하기 위함. ]
                foreach (var assetPath in assetPaths)
                {
                    if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath) != null)
                    {
                        AssetDatabase.DeleteAsset(assetPath);
                    }
                }
            }
            System.IO.Directory.CreateDirectory(scriptablePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var variables = ListPool<string>.Get();
            for (int col = 0; col < csv.col; col++)
            {
                variables.Add(GetVariableName(csv.data[0, col]));
            }

            Assembly assembly = Assembly.Load("Assembly-CSharp");
            System.Type type = assembly.GetType($"Data.{csvFile.name}");
            for (int row = 2; row < csv.row; row++)
            {
                if (csv.data[row, 0] == null || csv.data[row, 0].CompareTo(string.Empty) == 0) continue;
                string assetName = csv.data[row, 0];
                string path = scriptablePath + "/" + assetName + ".asset";

                //== 중복 검사 [ ID를 제외한 데이터를 List로 변경 ]
                bool duplicated = false;
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null) asset = ScriptableObject.CreateInstance(type);
                else duplicated = true;

                for (int col = 0; col < csv.col; col++)
                {
                    SetProperty(type, asset, variables[col], csv.data[row, col]);
                }

                asset.name = assetName;
                if (!duplicated)
                {
                    AssetDatabase.CreateAsset(asset, scriptablePath + "/" + assetName + ".asset");
                }

                //== 비동기 생성이 아닌, Create Asset의 특성상 새로고침 적용
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ListPool<string>.Release(variables);

            void SetProperty(System.Type type, ScriptableObject obj, string fieldName, string value)
            {
                var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);

                if (field != null)
                {
                    var fieldType = field.FieldType;

                    if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                    {
                        System.Collections.IList list = field.GetValue(obj) as System.Collections.IList;

                        //== 새로운 List 인스턴스 생성
                        if (list == null)
                        {
                            System.Type elementType = fieldType.GetGenericArguments()[0]; // 제네릭의 내부 타입 추출
                            System.Type listType = typeof(System.Collections.Generic.List<>).MakeGenericType(elementType);
                            list = (System.Collections.IList)System.Activator.CreateInstance(listType);

                            field.SetValue(obj, list);
                        }

                        //== 값이 내부 타입으로 변환 가능할 경우에 추가
                        System.Type listElementType = fieldType.GetGenericArguments()[0];
                        if (listElementType.IsEnum)
                        {
                            object convertedValue = System.Enum.Parse(listElementType, value, true);
                            list.Add(convertedValue);
                        }
                        else
                        {
                            object convertedValue = System.Convert.ChangeType(value, listElementType);
                            list.Add(convertedValue);
                        }
                    }
                    //== 열거형 검증
                    else if (fieldType.IsEnum)
                    {
                        object enumValue = System.Enum.Parse(fieldType, value.ToString());
                        field.SetValue(obj, enumValue);
                    }
                    else
                    {
                        field.SetValue(obj, System.Convert.ChangeType(value, fieldType));
                    }
                }
                else
                {
                    Debug.LogWarning($"Property {fieldName} not found or cannot be written.");
                }
            }
        }

        private void ShowInfomation()
        {
            GUIStyle textAreaStyle = new GUIStyle(EditorStyles.label);
            textAreaStyle.wordWrap = true;

            EditorGUILayout.LabelField(MakeInfomation(), textAreaStyle);
        }
        private string MakeInfomation(int tapCount = 0, StringBuilder enumResult = null)
        {
            if (csvFile != null)
            {
                StringBuilder result = new StringBuilder(string.Empty);

                string[] dataLines = csvFile.text.Split('\n');
                string variables = dataLines[0].Trim();
                string explanations = dataLines[1].Trim();
                if (string.IsNullOrEmpty(variables)) return string.Empty;
                if (string.IsNullOrEmpty(explanations)) return string.Empty;

                string[] variable = variables.Split(',');
                string[] explanation = explanations.Split(',');

                var csv = CSVLoad();

                //== 변수명 및 열거형 저장
                var enumIndex = ListPool<int>.Get();
                for (int i = 0; i < variable.Length; i++)
                {
                    bool isList = csv.isList;
                    if (i == 0 /* ID */) { isList = false; }

                    string type = GetVariableType(variable[i], isList);
                    if (type == string.Empty) continue;

                    if (type.Contains("enum"))
                    {
                        enumIndex.Add(i);
                        result.AppendLine(($"{Tap(tapCount)}public {GetVariableName(variable[i], false, isList, true)} {GetVariableName(variable[i])};\t // {explanation[i]}"));
                    }
                    else
                    {
                        result.AppendLine(($"{Tap(tapCount)}public {type} {GetVariableName(variable[i])};\t // {explanation[i]}"));
                    }
                }

                if (enumIndex.Count > 0)
                {
                    for (int i = 0; i < enumIndex.Count; i++)
                    {
                        var names = HashSetPool<string>.Get();

                        //== 변수명 / 설명 구간 제외
                        for (int j = 2; j < csv.row; j++)
                        {
                            names.Add(csv.data[j, enumIndex[i]]);
                        }

                        string enumDefinition = GetEnumDefinition(GetVariableName(csv.data[0, enumIndex[i]], false), names, tapCount);
                        if (enumResult != null)
                        {
                            enumResult.AppendLine(enumDefinition);
                        }
                        else
                        {
                            result.AppendLine(enumDefinition);
                        }


                        HashSetPool<string>.Release(names);
                    }
                }

                ListPool<int>.Release(enumIndex);

                return result.ToString();
            }
            return string.Empty;
        }

        private string GetVariableType(string data, bool isList = false)
        {
            for (int i = 0; i < typeRules.Length; i++)
            {
                if (data.Contains(typeRules[i].handle))
                {
                    if (isList)
                    {
                        return $"List<{typeRules[i].type}>";
                    }
                    else
                    {
                        return typeRules[i].type;
                    }

                }
            }

            return string.Empty;
        }
        private string GetVariableName(string data, bool toLower = true, bool isList = false, bool isEnum = false)
        {
            string result = string.Empty;
            for (int i = 0; i < typeRules.Length; i++)
            {
                if (data.Contains(typeRules[i].handle))
                {
                    string separated = data.Replace(typeRules[i].handle, string.Empty);
                    if (toLower)
                    {
                        result = char.ToLower(separated[0]) + separated.Substring(1).Trim();

                        if (isEnum)
                        {
                            result = "Common.Enum." + result;
                        }
                    }
                    else
                    {
                        result = separated.Trim();

                        if (isEnum)
                        {
                            result = "Common.Enum." + result;
                        }
                    }
                }
            }

            if (result != string.Empty && isList)
            {
                result = $"List<{result}>";
            }

            return result;
        }

        private string GetEnumDefinition(string name, in System.Collections.Generic.HashSet<string> datas, int tapCount = 1)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{Tap(tapCount)}public enum {name}\n");
            builder.Append($"{Tap(tapCount)}{{\n");

            foreach (string data in datas)
            {
                if (data == null || data.CompareTo(string.Empty) == 0) continue;
                builder.Append(Tap(tapCount + 1));
                builder.Append(data);
                builder.Append(",\n");
            }

            //== 마지막 ',\n' 제거
            builder.Remove(builder.Length - 2, 2);
            builder.Append($"\n{Tap(tapCount)}}}");

            return builder.ToString();
        }

        private string Tap(int tapCount = 0)
        {
            StringBuilder tap = new StringBuilder();

            for (int i = 0; i < tapCount; i++)
            {
                tap.Append('\t');
            }

            return tap.ToString();
        }
    }
}
#endif