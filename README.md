# Briver.Core简介

Briver.Core是一个应用程序基础框架，为应用程序开发中常见的任务，诸如对象组合、应用配置、启动过程、日志记录、调用拦截、事件总线等提供简单的解决方案。这个框架是我从事C#开发多年的经验总结提炼而来，对开发人员比较友好。

基本设计思路：
1） 自动化，按照约定的方式进行调用，减少非必要的代码，同时提供插入点，用于个性化实现
2） 可插拨，组件放到指定的目录即表示启用相应的功能，无须额外操作

此框架基于.NET Standard 2.0开发，已经发布为NuGet包，包名为Briver。

## 功能介绍

### 1. 对象组合

对象组合功能基于System.Composition包（MEF框架），并提供了元数据、自动加载、导出排序等功能，解决同一个契约有多个匹配导出可能会发生异常的问题

#### 主要类型

##### IComposition接口

表示一个支持自动组合的插件对象，实现此接口的类型，将会在系统初始化时自动加载。

这是一个空接口，不包含任何方法。

为了支持自动组合，需要为插件声明一个继承此接口的接口

##### ICompositionMetadata接口

表示组合对象的元数据信息，具体内容见CompositionAttribute特性

##### CompositionAttribute特性

用于标注实现IComposition接口的类型，实现了ICompositionMetadata接口，包含4个属性：

- Name：名称，用做主键。如果不提供，则使用类型名

- Priority：声明组合优先级，对于实现同一个接口的类型，导出时按照优先级从高到低排列。默认为0

- DisplayName：显示名称，一般为中文。如果为空，则返回Name属性的值

- Description：描述信息。默认为空

如果在类型上未声明此特性，则使用默认值

##### CompositionSupportedAttribute特性

用于声明程序集支持自动组合。

真实场景下，一个应用程序会包含非常多的程序集，其中大部分都是框架或第三方库，并不包含待组合的类型。因此本框架在进行组合时，会检查程序集是否声明了此特性，从而过滤掉无关的程序集，以提高组合性能。

##### ISystemInitialization接口

系统初始化接口，继承自IComposition接口。在系统初始化时会自动调用实现此接口的对象，达到自定义的目的。

注意：本框架中组合对象都是以单例的形式加载的，如果要实现瞬态插件，请使用工厂模式

#### 使用说明

以下示例，声明一个接口与一个实现类

```csharp
public interface IExportService : IComposition { void Execute(); }

[Composition(Priority = 0,
 Name = nameof(ExportCompositionPlugins),
 DisplayName = "导出插件",
 Description = "导出系统中所有的组合对象")]
internal class ExportCompositionPlugins : IExportService
{
 public void Execute()
 {
 }
}

```

### 2. 应用配置

应用配置基于Microsoft.Extensions.Configuration包，由于此包比较完善，未做扩展开发。本框架主要使用Json格式的配置，并对配置文件的目录做了约定（可重写）。

以下是一个本框架中Logger的配置：

```csharp
public static class Logger
{
   class Config
   {
       public LogLevel MinLevel { get; set; }
   }
   static Logger()
   {
       var config = SystemContext.Configuration.GetSection(nameof(Logger))?.Get<Config>();
```

典型的使用方式是在需要使用配置的类型中定义Config类，然后通过SystemContext.Configuration.GetSection(nameof(Logger))?.Get<Config>()方法从配置文件加载类型化的配置。

以下是它在配置文件中的内容：

```json
{
 "Logger": {
 "MinLevel": "Trace"
 },
```



### 3. 启动过程

启动过程是加载组件、应用配置、系统初始化等一系列任务的抽象。

#### 主要类型

##### Application抽象类

定义了业务系统启动时需要提供的内容

```csharp
public abstract class Application { 
/// <summary> 
/// 系统名称 
/// </summary> 
public string Name => _information.Value.Name;

/// 
/// 系统版本
/// 
public string Version => _information.Value.Version;

/// 
/// 系统显示名称
/// 
public string DisplayName => _information.Value.DisplayName;

/// 
/// 系统说明
/// 
public string Description => _information.Value.Description;

/// 
/// 基准目录（执行程序所在的目录）
/// 
public virtual string BaseDirectory { get; } = AppContext.BaseDirectory;

/// 
/// 用户目录（存放用户相关的配置等）
/// 
public virtual string UserDirectory => this.BaseDirectory;

/// 
/// 工作目录（存放日志、临时数据等）
/// 
public virtual string WorkDirectory => this.UserDirectory;


public Application()
{
    _information = new Lazy(() => this.LoadInformation().ValidateProperties());
}
/// 
/// 加载基本信息
/// 
/// 
protected abstract Information LoadInformation();

/// 
/// 执行配置
/// 
/// 
protected internal virtual void Configure(ConfigurationBuilder config)
{
    ……
}

/// 
/// 加载系统要用到的程序集
/// 
/// 
protected internal virtual IEnumerable LoadAssemblies()
{
    ……
}

public override string ToString()
{
    return JsonConvert.SerializeObject(this);
}
}
```

主要包括四个部分的内容：

**1）基本信息**


    Name：系统名称（英文），用于标识业务系统
    DisplayName：显示名称，一般会显示在业务系统标题栏上
    Version：系统版本
    Description：系统说明
这些基本信息是通过抽象方法LoadInformation()来加载的。以下是从特定文件（UIF.Json）中加载的内容

```json
{
 "Name": "UCP",
 "Version": "1.0.0.0",
 "DisplayName": "统一清算平台",
 "Description": ""
}
```

**2）应用目录**

本框架将应用目录分为三种，根据数据的属性分开存储。

    BaseDirectory：基准目录（执行程序所在的目录），业务系统打包时包含的文件都位于这个目录下
    UserDirectory：用户目录（存放用户相关的配置等），在运行时根据用户的使用情况，会生成一些自定义的配置，如：界面布局、个人偏好等，通常存储在Windows的用户专属文件夹中
    WorkDirectory：工作目录（存放日志、临时数据等），此目录下文件都是业务系统运行时产生的临时数据，可随时删除

**3）加载配置**

虚方法void Configure(ConfigurationBuilder config)，默认从(BaseDirectory)\Config和(UserDirectory)\Config里加载.json文件。

**4）加载程序集**

虚方法IEnumerable<Assembly> LoadAssemblies()，默认从BaseDirectory加载所有.dll文件。

    SystemContext静态类
此类表示应用程序的上下文环境，包含以下几个内容：
    初始化方法
void Initialize(Application application)，接收一个继承自Application类的自定义类，执行整个框架的初始化，一般在Main方法开始的位置调用此方法
    Application属性
公开Application类的实例
    Configuration属性
公共配置信息
    组件导出方法
提供三个方法：
1）    IEnumerable<T> GetExports<T>() where T:IComposition，获取所有实现指定接口的导出
2）    T GetHeadExport<T> where T:IComposition，获取实现指定接口并且优先级最高的导出
3）    void SatisfyImports(object target)，自动完成指定对象的导入，如下示例：

```csharp
class Demo
{
   [ImportMany]
   private List<IAppPlugin> plugins;
 
   public Demo()
   {
       Briver.Framework.SystemContext.SatisfyImports(this);
   }
}
```



### 4. 日志记录

日志是业务系统的必备功能，本框架的日志功能具有以下的特色：

1）    可插拨，框架默认提供了两个日志记录器，分别是写入文件和VisualStudio调试输出，也可以自行实现新日志记录器
2）    异步处理，日志记录是在一个单独的线程上执行批量写入操作，不会占用业务处理线程的时间 
3）    记录代码位置，默认会将调用日志记录时所在代码文件、调用者方法、代码行数保存下来，通过日志直接找到对应的代码，方便迅速定位问题

调用示例：

```csharp
Logger.Debug("准备打包业务数据", JsonConvert.SerializeObject(oBizStruct, Newtonsoft.Json.Formatting.Indented));
```

日志内容：

```json
时间：14:21:15.590
级别：Debug
位置：D:\GIT\eTechFD\UIF\01Fronted\01SRC\PluginFramework\CGate.cs@TransBizStructToBtpack#255
消息：准备打包业务数据
{
 "i_systemid": "0810",
 "i_ipaddr": "172.25.209.50",
 "i_funcid": "30100101",
 "o_errcode": "",
 "o_errmsg": "",
 "o_authmodeid": "",
 "o_authmodename": "",
 "o_isdefault": "",
 "o_systemname": ""
}
```



### 5.    调用拦截

调用拦截（AOP，面向切面编程）是解耦业务代码和非业务代码的重要手段，目前有两种主要的风格：

1）静态织入，在编译时将拦截代码注入到程序集中，优点是速度快，缺点是调用不便
2）动态代理，在运行时通过动态生成代理类实现拦截，优点是方便扩展、调试方便，缺点是速度略慢（有一个生成代理类的过程）
本框架使用动态代理的方式实现调用拦截。
核心逻辑是将目标转换成动态对象（dynamic），利用表达式树（System.Linq.Expressions）生成动态代理，结合前面讲的组件自动加载的功能，实现方法调用的拦截功能，达到切面编程的目的。

#### 主要类型

##### AspectContext类

拦截调用的环境信息，包括如下的属性：
    SeqNo：序列号，用于标识一次拦截
    Target：目标对象
    Method：调用的方法
    Binding：绑定（方法、get属性、set属性等）
    Arguments：调用的参数
    Interceptions：拦截时将用到的拦截器
    Execution：AspectDelegate类型
    AspectDelegate委托
表示当前拦截要调用的下一个拦截方法，需要在拦截器的拦截方法中调用，从而形成完整的调用链

##### Interception抽象类

全局拦截器的基类，将自动应用到所有的拦截调用上，主要用于通用功能，如：日志、异常处理等。



核心方法：

```csharp
void Intercept(AspectContext context, AspectDelegate proceed)
```

以下是一个示例实现：

```csharp
[Composition(Priority = 3)]
internal class LogInterception : Interception
{
 public override void Intercept(AspectContext context, AspectDelegate proceed)
 {
   Logger.Info($"调用方法{context.Method}");
   proceed.Invoke();
 }
}
```

##### InterceptionAttribute抽象特性类

特定拦截器的基类，以声明特性的方式应用到指定的类型或者方法上，主要用于与业务相关的功能，如：权限验证。
InterceptionAttribute的实现可以参考Interception类

##### AspectDynamic类

内部类，用于生成动态代理，不作详细介绍



#### 使用说明

本框架提供一个扩展方法，用于启用调用拦截：

```csharp
dynamic Aspect<T>(this T @this) where T : class
```




以调用CGate的SyncRequest方法为例：
public bool SyncRequest(object objIn, ref List<object> lstOut, ref bool bNextFlag)

正常调用如下：

```csharp
List<object> lstOut = null;
bool bNext = false;
CGate gate = new CGate(appContext, funcid);
gate.SyncRequest(stru, ref lstOut, ref bNext);
```

如果启用拦截，则使用如下的方式：

```csharp
List<object> lstOut = null;
bool bNext = false;
CGate gate = new CGate(appContext, funcid);
gate.Aspect().SyncRequest(stru, ref lstOut, ref bNext);
```

差别在于，调用目标方法之前，先调用Aspect()扩展方法。



### 6. 事件总线

事件总线是本框架的特色功能，实现弱引用的事件通知。
常规的事件模型存在两个弊端，使得其灵活性不够：
1）强引用，对象A订阅了对象B的事件，则B持有A的强引用，如果B存活，由A也不会被垃圾回收，如此一来，容易造成内存泄露。所以，通常只订阅私有成员的事件，保证订阅者和被订阅者有同样的生命周期。
2）强耦合，对象A能够调用对象B，有一个前提是A引用了B的程序集，这就形成了强耦合的关系。但是对于插件模型来说，这种限制过于严格了。订阅者通常并不知道具体插件（被订阅者）的存在，因此订阅也就无从谈起。

事件总线正是为了解决这两个问题而设计的。首先，被订阅者只持有订阅者的弱引用，不会影响订阅者的垃圾回收；其次，将订阅者对被订阅者的依赖解耦成它们共同依赖事件参数，而事件参数可以定义在公共库里。

#### 主要类型

```csharp
public static class EventBus
{
 public static SubscriptionToken Subscribe<T>(EventHandler<T> handler) where T : EventArgs
 public static void Unsubscribe(SubscriptionToken token)
 public static void Publish<T>(object sender, T e) where T : EventArgs
```

与常规事件相似，主要包含三个方法：
    Subscribe：订阅事件，返回订阅句柄，解订阅时用到
    Unsubscribe：解订阅事件（非必须）
    Publish：发布事件



#### 使用说明

1）定义事件参数

```csharp
public class MyEventArgs : EventArgs { }
```


2）订阅事件

```csharp
public class Subscriber : Form
{
 public Subscriber()
 {
   EventBus.Subscribe<MyEventArgs>(this.OnEvent);
 }
 private void OnEvent(object sender, MyEventArgs e)
 {
 }
}
```



3）发布事件

```csharp
public class Publisher
{
 void Execute()
 {
 EventBus.Publish(this, new MyEventArgs());
 }
}
```

注意：EventBus已经处理了线程间同步的问题，因此可以直接在工作线程上发布事件。如果订阅者是控件的话，一般在它的构造函数里订阅事件。

