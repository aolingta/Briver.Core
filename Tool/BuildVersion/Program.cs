using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                var path = args[0];
                if (!Directory.Exists(path))
                {
                    throw new Exception($"指定的项目文件夹“{path}”不存在");
                }
                var directory = Path.Combine(path, "Properties");
                Directory.CreateDirectory(directory);
                var file = Path.Combine(directory, OutputFile);

                var information = BuildInformation(path);
                information = $"[assembly: System.Reflection.AssemblyInformationalVersion(\"{information}\")]";

                var assembly = Assembly.GetExecutingAssembly();
                var resource = "BuildVersion.BuildVersion.ReadMe.txt";
                using (var stream = assembly.GetManifestResourceStream(resource))
                using (var reader = new StreamReader(stream))
                {
                    var readme = reader.ReadToEnd();
                    File.WriteAllLines(file, new[] { readme, information });
                }

                var message = new StringBuilder()
                    .AppendLine($"{nameof(BuildVersion)}: 成功更新文件“{file}”")
                    .Append($"\t{information}");
                Console.WriteLine(message.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(BuildVersion)}: {ex.ToString()}");
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

        private static void AmendRepository(string projectPath, string gitPath, string uniqueName)
        {
            using (var mutex = new Mutex(true, uniqueName, out var created))
            {
                // 如果创建新互斥体失败，说明已经存在同名互斥体，则等待互斥体可用
                if (!created) { mutex.WaitOne(); }

                try
                {
                    var file = Path.Combine(gitPath, nameof(BuildVersion));
                    if (File.Exists(file)) { return; }

                    File.WriteAllText(file, "请勿删除此文件");
                    var ignore = Path.Combine(projectPath, ".gitignore");
                    using (var stream = File.Open(ignore, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        var exist = false;
                        var reader = new StreamReader(stream, Encoding.UTF8);
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            if (string.Equals(line, OutputFile))
                            {
                                exist = true;
                                break;
                            }
                        }
                        if (exist) { return; }

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
