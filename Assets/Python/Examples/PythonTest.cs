using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using UnityEngine;

public class PythonTest : MonoBehaviour
{
    ScriptEngine engine = Python.CreateEngine();

    void Start()
    {
        ScriptScope scope = engine.CreateScope();

        string code = "array = [3, 5, 7, 8]";
        ScriptSource source = engine.CreateScriptSourceFromString(code);
        ScriptSource sourceFromFile = engine.CreateScriptSourceFromFile("C:\\Users\\emirs\\Desktop\\bacckgroundChanger.py");
        source.Execute(scope);
        sourceFromFile.Execute(scope);

        List numbers = scope.GetVariable<List>("array");

        Debug.Log(numbers.Count);
    }

}
