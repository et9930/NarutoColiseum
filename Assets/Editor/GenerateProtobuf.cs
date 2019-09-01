using System.Diagnostics;
using System.IO;
using UnityEditor;

public class GenerateProtobuf : EditorWindow
{
    private const string generateProtobufMenu = "Tool/Generate Protobuf";
    private const string proto_exec = "Assets\\Plugins\\ProtoBuf\\bin\\protoc.exe";
    private const string proto_path = "Assets\\Plugins\\ProtoBuf\\MessagePrototype ";
    private const string csharp_path = "Assets\\Scripts\\Generate\\Protobuf";

    [MenuItem(generateProtobufMenu)]
    public static void Generate()
    {

        //Clear csharp_path
        EditorUtility.DisplayProgressBar("Delete old files", "Delete old files", 0);
        DirectoryInfo csharpFolder = new DirectoryInfo(csharp_path);
        foreach(var file in csharpFolder.GetFiles())
        {
            file.Delete();
        }        

        //Generate CSharp code
        DirectoryInfo protoFolder = new DirectoryInfo(proto_path);
        FileInfo[] protoFiles = protoFolder.GetFiles("*.proto");
        for (int i = 0; i < protoFiles.Length; i++)       
        {
            FileInfo file = protoFiles[i];
            EditorUtility.DisplayProgressBar("Generate proto file", file.FullName, i / (float)protoFiles.Length);            
            ProcessCommand(proto_exec, "--proto_path=" + proto_path + file.Name + " --csharp_out=" + csharp_path);
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    private static void ProcessCommand(string command, string argument)
    {
        ProcessStartInfo start = new ProcessStartInfo(command)
        {
            Arguments = argument,
            CreateNoWindow = false,
            ErrorDialog = true,
            UseShellExecute = true
        };

        if (start.UseShellExecute)
        {
            start.RedirectStandardOutput = false;
            start.RedirectStandardError = false;
            start.RedirectStandardInput = false;
        }
        else
        {
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = true;
            start.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
            start.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
        }

        Process p = Process.Start(start);

        p.WaitForExit();
        p.Close();
    }
}
