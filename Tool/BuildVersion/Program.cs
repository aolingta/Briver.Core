using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace BuildVersion
{
    internal class Program
    {
        /// <summary>
        /// 要输出的文件名
        /// </summary>
        private const string OutputFile = "InformationVersion.cs";

        private static void Main(string[] args)
        {
            try
            {
                if (args.Length < 1)
                {
                    throw new Exception("请指定项目文件夹");
                }
                var projectPath = args[0];
                if (!Directory.Exists(projectPath))
                {
                    throw new Exception($"指定的项目文件夹“{projectPath}”不存在");
                }
                var projectModified = TryAmendProject(projectPath, out var projectFile);
                if (projectModified)
                {
                    throw new Exception($"项目文件“{projectFile}”已被修改，请重新执行编译操作");
                }

                var propertiesDir = Path.Combine(projectPath, "Properties");
                Directory.CreateDirectory(propertiesDir);

                var information = BuildInformation(projectPath);
                information = $"[assembly: System.Reflection.AssemblyInformationalVersion(\"{information}\")]";

                var assembly = Assembly.GetExecutingAssembly();
                var resource = "BuildVersion.BuildVersion.ReadMe.txt";
                using (var stream = assembly.GetManifestResourceStream(resource))
                using (var reader = new StreamReader(stream))
                {
                    var readme = reader.ReadToEnd();
                    var file = Path.Combine(propertiesDir, OutputFile);
                    File.WriteAllLines(file, new[] { readme, information });
                }
                Console.WriteLine($"{nameof(BuildVersion)}: 项目“{projectFile}”已重新生成“{OutputFile}”文件");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(BuildVersion)}: {ex.Message}");
                Environment.Exit(1);
            }

        }

        private static bool TryAmendProject(string projectPath, out string projectFile)
        {
            var projects = Directory.GetFiles(projectPath, "*.csproj");
            if (projects.Length == 0)
            {
                throw new Exception($"文件夹“{projectPath}”下未找到项目文件");
            }
            else if (projects.Length > 1)
            {
                throw new Exception($"文件夹“{projectPath}”下找到多个项目文件");
            }
            projectFile = projects[0];

            var xml = XElement.Load(projectFile, LoadOptions.PreserveWhitespace);
            if (xml.Attribute(nameof(BuildVersion)) != null)
            {
                return false;
            }

            AmendProjectCore(xml);
            xml.Add(new XAttribute(nameof(BuildVersion), $"{DateTime.Now:yyyy/MM/dd}"));
            using (var writer = XmlWriter.Create(projectFile, new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true, IndentChars = " ", CheckCharacters = false }))
            {
                xml.Save(writer);
            }

            return true;
        }

        private static void AmendProjectCore(XElement xml)
        {
            var isNewProjectFormat = xml.Attribute("Sdk") != null;
            XNamespace ns = XNamespace.None;
            if (!isNewProjectFormat)
            {
                ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            }

            const string propertyGroupName = "PropertyGroup";
            const string itemGroupName = "ItemGroup";
            const string inforationVersionName = "GenerateAssemblyInformationalVersionAttribute";
            const string compileName = "Compile";
            const string compileIncludeName = "Include";
            const string compileIncludeValue = "Properties\\InformationVersion.cs";

            var propertyGroup = xml.Element(ns + propertyGroupName);
            if (propertyGroup.Element(ns + inforationVersionName) == null)
            {
                propertyGroup.Add(new XElement(ns + inforationVersionName, false));
            }

            if (!isNewProjectFormat)
            {
                var compileItem = xml.Descendants(ns + compileName)
                    .FirstOrDefault(it => (string)it.Attribute(compileIncludeValue) == compileIncludeValue);
                if (compileItem == null)
                {
                    compileItem = new XElement(ns + compileName, new XAttribute(compileIncludeName, compileIncludeValue));
                    var itemGroup = new XElement(ns + itemGroupName, compileItem);

                    var previousGroup = xml.Element(ns + itemGroupName) ?? propertyGroup;
                    previousGroup.AddAfterSelf(itemGroup);
                }
            }
        }

        private static string Exec(string command, string arguments, string directory)
        {
            var info = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
            };
            var proc = Process.Start(info);
            proc.WaitForExit();
            return proc.StandardOutput.ReadToEnd().Trim();
        }

        private static string BuildInformation(string path)
        {
            while (path != null)
            {
                var git = Path.Combine(path, ".git");
                if (!Directory.Exists(git))
                {
                    path = Path.GetDirectoryName(path);
                    continue;
                }

                var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var name = Exec("git", "symbolic-ref --short -q HEAD", path);
                var hash = Exec("git", "rev-parse HEAD", path);
                var repo = Exec("git", "remote get-url --all origin", path);
                if (string.IsNullOrEmpty(repo))
                {
                    repo = path.Replace(@"\", @"\\");
                }
                AmendRepository(path, git, hash);

                return $"编译({time}) 分支({name}) 提交({hash}) 仓库({repo})";
            }

            throw new Exception($"未找到对应的git目录");
        }

        private static void AmendRepository(string projectDir, string gitDir, string uniqueName)
        {
            using (var mutex = new Mutex(true, uniqueName, out var created))
            {
                // 如果创建新互斥体失败，说明已经存在同名互斥体，则等待互斥体可用
                if (!created) { mutex.WaitOne(); }

                try
                {
                    var file = Path.Combine(gitDir, nameof(BuildVersion));
                    if (File.Exists(file)) { return; }

                    File.WriteAllText(file, "请勿删除此文件");

                    // 在.gitignore文件中添加“InformationVersion.cs”
                    var ignore = Path.Combine(projectDir, ".gitignore");
                    using (var stream = File.Open(ignore, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        var reader = new StreamReader(stream, Encoding.UTF8);
                        while (!reader.EndOfStream)
                        {
                            if (string.Equals(reader.ReadLine(), OutputFile))
                            {
                                return;
                            }
                        }

                        stream.Seek(0L, SeekOrigin.End);
                        var writer = new StreamWriter(stream, Encoding.UTF8);
                        writer.WriteLine();
                        writer.WriteLine(OutputFile);
                        writer.Flush();
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
    }
}
