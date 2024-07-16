#if UNITY_EDITOR
using Unity.Plastic.Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public partial class SheetParsing : EditorWindow
{
    [System.Serializable]
    public class KeyValue
    {
        public string name;
        public string id;

        public KeyValue(string name, string id)
        {
            this.name = name;
            this.id = id;
        }
    }
}

public partial class SheetParsing : EditorWindow
{
    public static SheetParsingData hasData;

    string gid;
    string jsonFileName;

    private const int keyIndex = 0;  
    private const int typeIndex = 1;

    [MenuItem("Tools/GoogleSheetParsing")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow(typeof(SheetParsing));
        window.maxSize = new Vector2(1200, 600);
        window.minSize = new Vector2(500, 300);
    }

    private void OnGUI()
    {
        hasData = AssetDatabase.LoadAssetAtPath("Assets/Editor/Resources/Sheet Parsing Data.asset", typeof(SheetParsingData)) as SheetParsingData;

        var btnOptions = new[] { GUILayout.Width(128), GUILayout.Height(32) };
        gid = EditorGUILayout.TextField("Gid", gid); 
        jsonFileName = EditorGUILayout.TextField("SaveFileName", jsonFileName);

        //== Tooltip
        GUILayout.Space(20);
        EditorGUILayout.LabelField("Google Gid :데이터를 가져올 시트의 GID");
        EditorGUILayout.LabelField("Save File Name : 저장할 파일 이름");     
        GUILayout.Space(20);                                                    

        if (jsonFileName == string.Empty) jsonFileName = "JsonFile";             

        if (GUILayout.Button("Parsing", btnOptions))
        {
            Parsing();
        }       
    }

    private void Parsing()
    {
        EditorCoroutineUtility.StartCoroutine(GoogleSheetParsing(jsonFileName, gid), this);
    }

    IEnumerator GoogleSheetParsing(string jsonFileName, string gid, bool notice = true)
    {
        string _url = string.Format("{0}/export?format=tsv&gid={1}", hasData.sheetURL, gid);
        string data = string.Empty;
        bool isFirstCall = true;
        string frontText = string.Empty;
        bool isListDictionary = (jsonFileName.Contains(hasData.listKey)) ? true : false;
        string fileName = jsonFileName.Replace(hasData.listKey, "");

        UnityWebRequest request = UnityWebRequest.Get(_url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            EditorUtility.DisplayDialog("Fail", "GoogleConnect Fail!", "OK");
            yield break;
        }

        data = request.downloadHandler.text;

        List<string> strs = data.Split("\r\n").ToList();          
        List<string> keys = strs[keyIndex].Split('\t').ToList(); 
        List<string> types = strs[typeIndex].Split('\t').ToList(); 
        JArray jArray = new JArray();

        string assetType = typeof(IExternalDataAsset).ToString();
        StringBuilder csFile = new StringBuilder("using System.Collections.Generic;\n");


        const string oneTap = "\t";
        const string doubleTap = "\t\t";
        const string tripleTap = "\t\t\t";
        csFile.Append("namespace JsonData\n{\n");
        csFile.Append(oneTap + "[System.Serializable]\n");
        csFile.Append(oneTap + "public class " + fileName + ((isListDictionary) ? " : " + assetType : "") + "\n" + oneTap + "{\n");

        for (int row = 2; row < strs.Count; row++)
        {
            JObject keyValuePairs = new JObject();
            List<string> datas = strs[row].Split("\t").ToList();

            if (datas[0].Equals(hasData.ignoreKey))
            {
               continue;
            }

            for (int column = 1; column < keys.Count; column++)
            {
                if (types[column].Equals(hasData.ignoreKey) || keys[column].Equals(""))
                {
                    continue;
                }

                switch (types[column])
                {
                    case SheetParsingData.TypeString:
                        keyValuePairs.Add(keys[column], datas[column].Equals("") ? "" : datas[column]);
                        break;
                    case SheetParsingData.TypeInt:
                        int @int = 0;

                        if (!datas[column].Equals(""))
                        {
                            int.TryParse(datas[column], out @int);
                        }
                        keyValuePairs.Add(keys[column], @int);
                        break;
                    case SheetParsingData.TypeFloat:
                        float @float = 0;

                        if (!datas[column].Equals(""))
                        {
                            float.TryParse(datas[column], out @float);
                        }

                        keyValuePairs.Add(keys[column], @float);
                        break;
                    case SheetParsingData.TypeArrayFloat:
                        JArray jArray1 = new JArray();
                        List<float> nums = new List<float>();
                        List<string> str_nums = datas[column].Split(",").ToList();

                        for (int k = 0; k < str_nums.Count; k++)
                        {
                            if (str_nums[k].Equals("")) continue;
                            float fValue = 0;
                            float.TryParse(str_nums[k], out fValue);
                            nums.Add(fValue);
                        }

                        jArray1.Add(nums);
                        keyValuePairs.Add(keys[column], jArray1);
                        break;
                    case SheetParsingData.TypeArrayString:
                        JArray jArray2 = new JArray();
                        List<string> strValues = datas[column].Split(",").ToList();
                        if (strValues.Count == 1 && strValues[0].Equals("")) { strValues.Clear(); }
                        jArray2.Add(strValues);
                        keyValuePairs.Add(keys[column], jArray2);
                        break;
                    case SheetParsingData.TypeArrayInt:
                        JArray jArray3 = new JArray();
                        List<int> nums2 = new List<int>();
                        List<string> str_nums2 = datas[column].Split(",").ToList();

                        for (int k = 0; k < str_nums2.Count; k++)
                        {
                            if (str_nums2[k].Equals("")) continue;
                            int iValue = 0;
                            int.TryParse(str_nums2[k], out iValue);
                            nums2.Add(iValue);
                        }

                        jArray3.Add(nums2);
                        keyValuePairs.Add(keys[column], jArray3);
                        break;
                    case SheetParsingData.TypeLong:
                        long @long = 0;

                        if (!datas[column].Equals(""))
                        {
                            long.TryParse(datas[column], out @long);
                        }

                        keyValuePairs.Add(keys[column], @long);
                        break;
                    case SheetParsingData.TypeByte:
                        byte @byte = 0;

                        if (!datas[column].Equals(""))
                        {
                            byte.TryParse(datas[column], out @byte);
                        }

                        keyValuePairs.Add(keys[column], @byte);
                        break;
                    case SheetParsingData.TypeBool:
                        bool boolNum = false;

                        if (!datas[column].Equals(""))
                        {
                            if (datas[column].Equals("TRUE"))
                            {
                                boolNum = true;
                            }
                            else if (datas[column].Equals("FALSE"))
                            {
                                boolNum = false;
                            }
                        }

                        keyValuePairs.Add(keys[column], boolNum);
                        break;
                    default:
                        break;
                }
            }

            jArray.Add(keyValuePairs);
        }

        StringBuilder contructor = new StringBuilder(string.Empty);
        for (int j = 1; j < keys.Count; j++)
        {
            if (types[j].Equals(hasData.ignoreKey) || keys[j].Equals(""))
            {
                continue;
            }

            switch (types[j])
            {
                case SheetParsingData.TypeString:
                    FirstCallSetting($"{keys[j]}:string");
                    csFile.Append(doubleTap + "public string " + keys[j] + ";\n");
                    contructor.Append($"{tripleTap}{keys[j]} = data.{keys[j]};\n");
                    break;
                case SheetParsingData.TypeInt:
                    FirstCallSetting($"{keys[j]}:int");
                    csFile.Append(doubleTap + "public int " + keys[j] + ";\n");
                    contructor.Append($"{tripleTap}{keys[j]} = data.{keys[j]};\n");
                    break;
                case SheetParsingData.TypeFloat:
                    FirstCallSetting($"{keys[j]}:float");
                    csFile.Append(doubleTap + "public float " + keys[j] + ";\n");
                    contructor.Append($"{tripleTap}{keys[j]} = data.{keys[j]};\n");
                    break;
                case SheetParsingData.TypeArrayFloat:
                    csFile.Append(doubleTap + "public float[] " + keys[j] + ";\n");
                    contructor.Append($"{tripleTap}{keys[j]} = data.{keys[j]};\n");
                    break;
                case SheetParsingData.TypeArrayString:
                    csFile.Append(doubleTap + "public string[] " + keys[j] + ";\n");
                    contructor.Append($"{tripleTap}{keys[j]} = data.{keys[j]};\n");
                    break;
                case SheetParsingData.TypeArrayInt:
                    csFile.Append(doubleTap + "public int[] " + keys[j] + ";\n");
                    contructor.Append($"{tripleTap}{keys[j]} = data.{keys[j]};\n");
                    break;
                case SheetParsingData.TypeByte:
                    FirstCallSetting($"{keys[j]}:byte");
                    csFile.Append(doubleTap + "public byte " + keys[j] + ";\n");
                    contructor.Append($"{tripleTap}{keys[j]} = data.{keys[j]};\n");
                    break;
                case SheetParsingData.TypeLong:
                    FirstCallSetting($"{keys[j]}:long");
                    csFile.Append(doubleTap + "public long " + keys[j] + ";\n");
                    contructor.Append($"{tripleTap}{keys[j]} = data.{keys[j]};\n");
                    break;
                case SheetParsingData.TypeBool:
                    csFile.Append(doubleTap + "public bool " + keys[j] + ";\n");
                    contructor.Append($"{tripleTap}{keys[j]} = data.{keys[j]};\n");
                    break;
                default:
                    break;
            }
        }

        string dictionaryValueType = ((isListDictionary) ? $"List<{assetType}>" : fileName);
        try
        {
            string dictionaryKeyType = frontText.Split(':')[1];
            csFile.Append(doubleTap + $"public static Dictionary<{dictionaryKeyType},{dictionaryValueType}> table = new Dictionary<{dictionaryKeyType},{dictionaryValueType}>();\n");

            if (isListDictionary)
            {
                csFile.Append(doubleTap + $"public {fileName}() {{ }}\n");
                csFile.Append(doubleTap + $"private {fileName}({assetType} asset)\n\t{{\n");
                csFile.Append($"{tripleTap}{fileName} data = asset as {fileName};\n");
                csFile.Append(contructor.ToString() + "\t}\n");
                csFile.Append(doubleTap + $"public static List<{fileName}> GetTableData({dictionaryKeyType} key)\n\t{{\n");
                csFile.Append($"{tripleTap}return table[key].ConvertAll<{fileName}>(\n\t\t\tnew System.Converter<{assetType}, {fileName}>((asset)=> {{ return new {fileName}(asset); }}));\n\t}}");
            }
        }
        catch (System.IndexOutOfRangeException)
        {
            Debug.LogError($"{fileName} Sheet의 Key - Type 순서가 올바르지 않습니다.");
            yield break;
        }

        csFile.Append("\n"+oneTap +"}\n}");

        string[] directoryPath = 
        {
            Application.dataPath + "/Resources/",
            Application.dataPath + "/Resources/JsonFiles/",
            Application.dataPath + "/Resources/DataClass/",
            Application.dataPath + "/Resources/DataClass/Text/"
        };

        for(int i = 0; i < directoryPath.Length; i++)
        {
            if (!Directory.Exists(directoryPath[i]))
            {
                Directory.CreateDirectory(directoryPath[i]);
            }
        }
        
        File.WriteAllText(Path.Combine(directoryPath[1], fileName + ".json"), jArray.ToString());
        File.WriteAllText(Path.Combine(directoryPath[2], fileName + ".cs"), csFile.ToString());
        File.WriteAllText(Path.Combine(directoryPath[3], fileName + ".txt"), "//" + frontText + ":" + ((isListDictionary) ? "true" : "false") + '\n');

        if (notice)
        {
            EditorUtility.DisplayDialog("Success", "Successfully Save!", "OK");
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        void FirstCallSetting(string frontData)
        {
            if (isFirstCall)
            {
                frontText = frontData;
                csFile.Insert(0/*Front*/, "//" + frontData + ":" + ((isListDictionary) ? "true" : "false") + '\n');
                isFirstCall = false;
            }
        }
    }
}
#endif