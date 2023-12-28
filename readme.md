# 模拟ASP.NET CORE MVC

 原文地址：<https://mp.weixin.qq.com/s/Qw4JwT_F0FB2YSsxLyYkmg>

## 请求的整个处理过程

### 一、描述Action方法

 MVC应用提供的功能体现在一个个Action方法上，所以MVC框架定义了专门的类型ActionDescriptor来描述每个有效的Action方法。但是Action方法和ActionDescriptor对象并非一对一的关系，而是一对多的关系。具体来说，采用“约定路由”的Action方法对应一个ActionDescriptor对象，采用“特性路由”，MVC框架会针对每个注册的路由创建一个ActionDescriptor。

### 二、注册路由终结点

 MVC利用“路由”对外提供服务，它会将每个ActionDescriptor转换成“零到多个”路由终结点。ActionDescriptor与终结点之间的对应关系为什么是“零到多”，而不是“一对一”或者“一对多”呢？这也与Action方法采用的路由默认有关，采用特性路由的ActionDescriptor（RouteTemplateProvider 属性不等于Null）总是对应着一个确定的路由，但是如何为采用“约定路由”的ActionDescriptor创建对应的终结点，则取决于多少个约定路由与之匹配。针对每一个基于“约定”路由的ActionDescriptor，系统会为每个与之匹配的路由创建对应的终结点。如果没有匹配的约定路由，对应的Action方法自然就不会有对应的终结点
 每个路由终结点由“路由模式”和“路由处理器”这两个核心元素构成，前者对应一个RoutePattern对象，由注册的路由信息构建而成，后者体现为一个用来处理请求的RequestDelegate委托。一个MVC应用绝大部分的请求处理工作都落在IActionInvoker对象上，所以作为路由处理器的RequestDelegate委托只需要将请求处理任务“移交”给这个对象就可以了

### 三、绑定Action方法参数

 现在我们完成了路由（终结点）注册，此时匹配的请求总是会被路由到对应的终结点，后者将利用IActionInvokerFactory工厂创建的IActionInvoker对象来处理请求。IActionInvoker最终需要调用对应的Action方法，但是要完成针对目标方法的调用，得先绑定其所有参数，MVC框架为此构建了一套名为“模型绑定（Model Binding）”的系统来完成参数绑定的任务，毫无疑问这是MVC框架最为复杂的部分。在我么简化的模拟框架中，我们将针对单个参数的绑定交给IArgumentBinder对象来完成。

 默认实现的ArgumentBinder类型完成了最基本的参数绑定功能，它可以帮助我们完成源自依赖服务、请求查询字符串、路由参数、主体内容（默认采用JSON反序列化）和默认值的参数绑定

### 四、执行Action方法

 在模拟框架中，针对目标Action方法的执行体现在如下所示的IActionMethodExecutor接口的Execute方法上，该方法的三个参数分别代表Controller对象、描述目标Action方法的ActionDescriptor和通过“参数绑定”得到的参数列表。Execute方法的返回值就是执行目标Action方法的返回值。如下所示的实现类型ActionMethodExecutor 利用“表达式树”的方式将Action方法对应的MethodInfo转换成对应的Func<object, object?[], object?>委托，并利用后者执行Action方法

### 五、响应执行结果

 当我们利用IActionMethodExecutor对象成功执行Action方法后，需要进一步处理其返回值。为了统一处理执行Action方法的结果，于是有了如下这个IActionResult接口，具体的处理逻辑实现在ExecuteResultAsync方法中，方法的唯一参数依然是当前ActionContext上下文

### 六、编排整个处理流程

 ActionInvoker利用IActionMethodExecutor对象成功执行Action方法，并利用IActionResultConverter对象将返回结果转换成IActionResult对象，最终通过执行这个对象完成针对请求的响应工作
