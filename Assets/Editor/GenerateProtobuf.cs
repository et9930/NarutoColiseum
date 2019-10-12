using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace Editor
{
    public class GenerateProtobuf : EditorWindow
    {
        private const string GenerateProtobufMenu = "Tool/Generate Protobuf";
        private const string ProtoExec = "Assets\\Plugins\\Protobuf\\bin\\protoc.exe";
        private const string ProtoPath = "Assets\\MessagePrototype ";
        private const string CsharpPath = "Assets\\Scripts\\Generate\\Protobuf";

        [MenuItem(GenerateProtobufMenu)]
        public static void Generate()
        {

            //Clear csharp_path
            EditorUtility.DisplayProgressBar("Delete old files", "Delete old files", 0);
            DirectoryInfo csharpFolder = new DirectoryInfo(CsharpPath);
            foreach(var file in csharpFolder.GetFiles())
            {
                file.Delete();
            }        

            //Generate CSharp code
            DirectoryInfo protoFolder = new DirectoryInfo(ProtoPath);
            FileInfo[] protoFiles = protoFolder.GetFiles("*.proto");
            for (int i = 0; i < protoFiles.Length; i++)       
            {
                FileInfo file = protoFiles[i];
                EditorUtility.DisplayProgressBar("Generate proto file", file.FullName, i / (float)protoFiles.Length);            
                ProcessCommand(ProtoExec, "--proto_path=" + ProtoPath + file.Name + " --csharp_out=" + CsharpPath);
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
                start.StandardOutputEncoding = System.Text.Encoding.UTF8;
                start.StandardErrorEncoding = System.Text.Encoding.UTF8;
            }

            Process p = Process.Start(start);

            p?.WaitForExit();
            p?.Close();
        }
    }
}
