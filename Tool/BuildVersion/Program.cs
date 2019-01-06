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
                var propertiesDir = Path.Combine(projectPath, "Properties");
                Directory.CreateDirectory(propertiesDir);
                var informationVersionFile = Path.Combine(propertiesDir, OutputFile);
                if (!File.Exists(informationVersionFile)) //以前没有执行过
                {
                    AmendProjectFile(projectPath);
                    AmendPropertyInfo(propertiesDir);
                }

                var information = BuildInformation(projectPath);
                information = $"[assembly: System.Reflection.AssemblyInformationalVersion(\"{information}\")]";

                var assembly = Assembly.GetExecutingAssembly();
                var resource = "BuildVersion.BuildVersion.ReadMe.txt";
                using (var stream = assembly.GetManifestResourceStream(resource))
                using (var reader = new StreamReader(stream))
                {
                    var readme = reader.ReadToEnd();
                    File.WriteAllLines(informationVersionFile, new[] { readme, information });
                }

                var message = new StringBuilder()
                    .AppendLine($"{nameof(BuildVersion)}: 成功更新文件“{informationVersionFile}”")
                    .Append($"\t{information}");
                Console.WriteLine(message.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(BuildVersion)}: {ex.Message}");
            }

        }
        private static void AmendProjectFile(string path)
        {
            var project = Directory.EnumerateFiles(path, "*.csproj").SingleOrDefault();
            if (project == null)
            {
                throw new Exception();
            }
            var xml = XElement.Load(project);
            var sdk = xml.Attribute("Sdk");
            var isNewProjectFormat = sdk != null;//新版本

            const string propertyGroupName = "PropertyGroup";
            const string attributeElementName = "GenerateAssemblyInformationalVersionAttribute";
            const string compileInclude = "Properties\\InformationVersion.cs";

            XNamespace ns = XNamespace.None;
            if (!isNewProjectFormat)
            {
                ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            }

            var propertyGroup = xml.Elements(ns + propertyGroupName)
                .FirstOrDefault(it => it.Attribute("Condition") == null);
            if (propertyGroup == null)
            {
                propertyGroup = new XElement(ns + propertyGroupName);
                xml.Add(propertyGroup);
                propertyGroup.Add(new XElement(ns + attributeElementName, false));
            }
            else
            {
                var element = propertyGroup.Element(ns + attributeElementName);
                if (element != null)
                {
                    element.SetValue(false);
                }
                else
                {
                    propertyGroup.Add(new XElement(ns + attributeElementName, false));
                }
            }

            if (!isNewProjectFormat)
            {
                var informationCompile = xml.Descendants(ns + "Compile")
                    .FirstOrDefault(it => (string)it.Attribute("Include") == compileInclude);
                if (informationCompile == null)
                {
                    var itemGroup = xml.Elements(ns + "ItemGroup").FirstOrDefault(it => it.Attribute("Condition") == null);
                    if (itemGroup == null)
                    {
                        itemGroup = propertyGroup;
                    }
                    itemGroup.AddAfterSelf(
                        new XElement(ns + "ItemGroup",
                            new XElement(ns + "Compile",
                                new XAttribute("Include", compileInclude))));
                }
            }


            xml.Save(project);
        }

        private static void AmendPropertyInfo(string propertiesDir)
        {
            var assemblyInfoFile = Path.Combine(propertiesDir, "AssemblyInfo.cs");
            if (File.Exists(assemblyInfoFile))
            {
                var needRewrite = false;
                var assemblyInfo = new StringBuilder();
                foreach (var item in File.ReadAllLines(assemblyInfoFile))
                {
                    var line = item;
                    if (line.Contains("AssemblyInformationalVersion"))
                    {
                        line = "//" + line;
                        needRewrite = true;
                    }
                    assemblyInfo.AppendLine(line);
                }
                if (needRewrite)
                {
                    File.WriteAllText(assemblyInfoFile, assemblyInfo.ToString());
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
                    repo = path;
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
