using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using Fody.PeImage;
using Fody.VersionResources;

namespace PatchVersion
{
    /// <summary>
    /// 此程序用来在编译后，修改AssemblyInformationVersionAttribute及文件属性中的产品版本信息
    /// 使用方法：
    /// 在项目属性的“生成事件”页中的“后期生成事件命令行”中输入如下的命令：
    /// $(ProjectDir)PatchVersion.exe $(ProjectDir)$(OutputPath)$(AssemblyName).dll
    /// 注：假定PatchVersion.exe工具已经放到项目文件夹下
    ///     $(OutputPath)变量表示目标生成输出文件夹
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine($"{nameof(PatchVersion)}: 请指定待打包的程序集文件");
                return;
            }

            var file = Path.GetFullPath(args[0]);
            try
            {
                var extension = Path.GetExtension(file).ToLower();
                if (extension != ".dll" && extension != ".exe")
                {
                    throw new Exception("扩展名无效");
                }
                if (!File.Exists(file))
                {
                    throw new Exception("文件不存在");
                }

                var information = BuildInformation(file);
                using (var stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite))
                {
                    UpdateWindowsResources(stream, information);
                    UpdateAssemblyAttributes(stream, information);
                }
                var message = new StringBuilder()
                    .AppendLine($"{nameof(PatchVersion)}: 给文件“{file}”打包版本信息成功，写入如下的内容：")
                    .Append($"\t{information}");
                Console.WriteLine(message.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(PatchVersion)}: 给文件“{file}”打包版本信息失败：{ex.Message}");
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


        private static string BuildInformation(string file)
        {
            var path = Path.GetDirectoryName(file);
            while (path != null)
            {
                if (!Directory.Exists(Path.Combine(path, ".git")))
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
                return $"编译({time}) 分支({name}) 提交({hash}) 仓库({repo})";
            }

            throw new Exception($"未找到对应的git目录");
        }

        /// <summary>
        /// 更新Windows文件中的资源，使得可以在文件“属性”对话框“详细信息”页的看到“产品版本”
        /// 注：此方法源自https://github.com/304NotModified/Fody.Stamp
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="information"></param>
        private static void UpdateWindowsResources(Stream stream, string information)
        {
            stream.Seek(0L, SeekOrigin.Begin);

            var image = new PeImage(stream);
            image.ReadHeader();
            image.CalculateCheckSum();

            var versionStream = image.GetVersionResourceStream();
            var reader = new VersionResourceReader(versionStream);
            var versions = reader.Read();

            const string entry = "ProductVersion";
            foreach (var table in versions.StringFileInfo)
            {
                if (table.Values.ContainsKey(entry))
                {
                    table.Values[entry] = information;
                }
            }

            versionStream.Position = 0;
            var writer = new VersionResourceWriter(versionStream);
            writer.Write(versions);
            image.SetVersionResourceStream(versionStream);

            image.WriteCheckSum();
        }

        private static void UpdateAssemblyAttributes(Stream stream, string information)
        {
            stream.Seek(0L, SeekOrigin.Begin);

            var module = ModuleDefMD.Load(stream);
            var attribute = module.Assembly.CustomAttributes
                .FirstOrDefault(it => it.TypeFullName == typeof(AssemblyInformationalVersionAttribute).FullName);
            if (attribute != null)
            {
                var argument = attribute.ConstructorArguments[0];
                argument = new CAArgument(argument.Type, information);
                attribute.ConstructorArguments.RemoveAt(0);
                attribute.ConstructorArguments.Add(argument);

                stream.Seek(0L, SeekOrigin.Begin);
                module.Write(stream);
            }
        }

    }
}
