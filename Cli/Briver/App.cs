using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Briver.Aspect;
using Briver.Commands;
using Briver.Framework;
using Briver.Logging;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace Briver
{
    internal class App : Application
    {
        private const string RootConfig = "Briver.json";

        /// <summary>
        /// 通过读取文件加载信息
        /// </summary>
        /// <returns></returns>
        protected override Information LoadInformation()
        {
            string BuildErrorText()
            {
                return $"运行目录“{this.BaseDirectory}”下不存在文件“{RootConfig}”或者此文件不是有效的json格式，请确保此文件存在并且使用{Encoding.UTF8.EncodingName}编码。";
            }
            var path = Path.Combine(this.BaseDirectory, RootConfig);
            if (!File.Exists(path))
            {
                throw new FrameworkException(ExceptionLevel.Fatal, BuildErrorText());
            }
            var json = File.ReadAllText(path, Encoding.UTF8);
            var info = JsonConvert.DeserializeObject<Information>(json);
            if (info == null)
            {
                throw new FrameworkException(ExceptionLevel.Fatal, BuildErrorText());
            }
            return info;
        }


    }

    [Composition(Priority = 1, DisplayName = "系统初始化器")]
    internal class AppInitiator : ISystemInitialization
    {
        void ISystemInitialization.Execute()
        {
            Logger.Info("系统已启动");
        }
    }

    internal class LogInterception : Interception
    {
        public override void Intercept(AspectContext context, AspectDelegate proceed)
        {
            Logger.Trace($"{nameof(LogInterception)}:开始执行方法{context.Method.Name}");
            proceed.Invoke();
            Logger.Trace($"{nameof(LogInterception)}:完成执行方法{context.Method.Name}");
        }
    }
}
