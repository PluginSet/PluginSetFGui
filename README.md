# FairyGUI 封装插件

## 一、设计目的
[FairyGUI](https://www.fairygui.com)是一套不错UI解决方案，编辑器符合美术操作习惯，有强大的适配能力。接口设计简单，配合控制器的使用可以让UI逻辑编写更轻松简洁。

在此基础上，该插件作了再次封装，使开发者在界面编写时不再需要考虑界面相关资源的加载与释放。并且对FairyGUI部分接口稍作修改，使其更具有扩展性。

## 二、 设计思路
1. 统一```FairyGUI.UIPackage```的加载/卸载接口
2. 窗口与其依赖的资源包作绑定，打开窗口时自动加载包，关闭窗口时自动卸载包
3. 管理所有窗口，自动控制窗口间的显示层级，单例界面不重复创建
4. 可使用FairyGUI自带的分支功能作为多语言切换方案
5. 修改FairyGUI部分代码，让其支持动态切换分支

## 三、 主要实现
### ```GCustomLoader``` 扩展加载器
通用自定义```FairyGUI.GLoader```扩展，支持加载远程图片，动态切换分支时重新加载内容

该类还提供了```AddBoardFrame```可以为加载器加载一个底层的点击组件，以防止在加载器中的组件设置为【点击空白区域穿透】后该加载器点击会穿透

### ```UILayer``` UI层级
```UILayer```继承自```FairyGUI.GRoot```，用来管理界面及其相应层级。该插件使用FairyGUI时是多GRoot模式，因此```GRoot.Inst```对象不再被使用，所有界面会被添加到相应的UI层级显示。

同一个```UILayer```对象上的所有界面，层级规则与```FairyGUI.GRoot```一致。在界面没有设置```sortingOrder```时，所有的非```modal```界面会在```modal```界面的下层，而```modal```界面或非```modal```之间的层级为先创建的在底层，后创建的在上层。也可以使用```BringToFront```将界面提到同类的最上层。

### ```IPackageLoader``` UI包加载接口
该接口定义了UI包加载的通用接口，包括：
 1. ```PreparePackage``` 准备目标资源包的相关资源，如AssetBundle等，返回一个异步操作
 2. ```LoadPackage``` 加载指定资源包，返回```FairyGUI.UIPackage```
 3. ```ReloadPackage``` 重新加载指定资源包
 4. ```ReleasePackage``` 释放资源包

 该插件中有```PackageLoaderEditorImpl```和```PackageLoaderImpl```两个```IPackageLoader```接口示例，分别用于编辑器环境下资源加载与生产环境下的包资源加载

 ### ```FairyGUIUtils``` 通用方法集
 使用该类中的包加载相关方法前，需先调用```SetPackageLoad```设置项目使用的```IPackageLoader```实例。

 除了UI包加载相关的接口，该类还提供了设置默认字体的方法```SetDefaultFont```，修改默认字体后，该接口会同步到UI舞台上的所有节点更新字体。```MakeFullScreen```方法能将指定UI对象设置为全屏大小，该方法不会受安全区设置影响表现结果。

### ```UnionPackage``` ```FairyGUI.UIPackage```封装
```UnionPackage```封装了```FairyGUI.UIPackage```，可以对资源包增加/减少引用从而控制资源包是否释放，方便管理资源的加载与释放。

另外，```UnionPackage```还修改了```FairyGUI.UIPackage```中的少量代码，让资源包可以动态添加分支包。在FairyGUI原码中，资源分支应当与主干资源包同处一个```UIPackage```，这个会导致有分支的资源包必须同时加载所有分支，而其中大多数分支资源并不一定会用到。动态添加分支包，可以不在同的包中制作相同名称的组件，按需加载分支包，切换分支后释放未使用到的分支资源。

### ```UIWindowBase``` ```FairyGUI.Window```窗口封装
```UIWindowBase```封装了```FairyGUI.Window```中的接口，完善了窗口的生命周期管理，前在窗口初始化时，会根据传入的参数加载所有依赖的资源包，创建并设置组件，显示到舞台。当界面退出时，销毁组件，释放所有依赖的资源包。

### ```UIWindow``` ```UIWindowBase```再封装
C#中的非```modal```界面可以继承```UIWindow```来处理界面生命周期内相应的逻辑。其生命周期中主要的几个重载方法如下：

1. ```OnUIInit``` 界面创建成功后初始化
2. ```SetData``` 界面初始化后/界面再次进入时，设置界面数据
3. ```OnShown``` 界面创建初始化并设置数据后，显示到舞台
4. ```OnHide``` 界面隐藏退出舞台
5. ```OnDispose``` 界面销毁

***PS:继承```UIWindow```的非```modal```界面大小会设置为与其所在根节点相同***

### ```UIModal``` ```UIWindow```再封装
C#中的```modal```界面可以继承```UIModal```来处理界面生命周期内相应的逻辑。

在使用```UIModal```之前，需要先设置```UIModal.ModalContentUrl```属性，指定装载组件的容器。在```UIModal```创建时，```FairyGUI.Window.pane```属性是装载组件生成的实例，该装载组件必须要有一个名为```content```的```FairyGUI.GLoader```容器，在界面初始化时，传入的组件会由这个容器装载。因此，该界面的适配规则，则指定的装载组件控制。

```UIModal```实例有与```UIWindow```相同的生命周期函数。

### ```UIToast``` Toast示例
这是一个Toast提示框实现示例，项目可根据自身需求实现。

### ```UIManager``` 界面管理器
```UIManager```是界面管理器，所有的界面应通过管理器提供的接口打开，方便管理所有UI。```UIManager```在使用前，需要先初始化，初始化可以直接调用```UIManager.Init```，也可根据安全区域大小初始化```UIManager.InitWithSafeArea```。初始化时，```UIManager```会创建四个不同的层级，使用不同的显示界面接口，会对应显示到不同的UI层级。四个层级分别对应：

1. ```ShowView``` 显示到```LayerView```，一般显示最底层非模态窗口
2. ```ShowModal``` 显示到```LayerModal```，一般显示模态窗口
3. ```ShowToast``` 在```LayerToast```显示toast框，在所有常规界面最上层
4. ```ShowLoading``` 显示到```LayerLoading```，一般显示需要遮盖所有界面的加载界面

```UIManager```没有提供关闭指定单个界面的接口，只提供了根据```tag```（目前使用的```Window.data```来记录```tag```）或```name```来关闭一组UI。单个UI关闭需要界面自身处理（比如派发一个事件）。

***PS:```ShowModal```接口并不代表该接口只能显示模态窗口，而是在相应的层级显示界面，也可以使用```ShowModal```接口显示非模态窗口，但需要注意```FairyGUI.GRoot```中的界面层级规则。另外在管理器初始化后，项目可根据自身需求增加更多的UI层级，这些UI层级也会在```UIManager.Purge```时被销毁掉***

### ```PluginFairyGUI``` 运行时插件
* 插件初始化时，会先将```PluginSetConfig```中FairyGUI相关的配置设置到```FairyGUI```框架中，并监听```PluginFairyGUIConst.NOTIFY_CHANGE_BRANCH```事件，当需要切换分支时，在插件中派发该事件即可。
* 插件启动时，预设```GLoader```和```GLoader3D```的扩展。
* 插件重启/销毁时，会关闭所有界面，释放所有已加载资源。

### ```PluginSetFGUIBuildParams``` 插件自定义的构建属性
 * ```packagesInStartScene``` 游戏启动时需要加载的资源包，这些包不会在重启游戏时释放。如果某些界面是放在```Resources```文件夹下，在启动时需要直接使用，可以通过该属性加载并保持资源。
 * ```FairyGUIPaths``` FairyGUI导出的资源目录
 * ```AutoSetFairyBundles``` 是否自动设置FairyGUIPaths中的AssetBundleName。如果勾选，构建AssetBundle时，插件会将这些资源目录中的所有**子文件夹**按**目录名称**组织AssetBundleBuild
 * ```BundleNameFormatter``` FairyGUI资源文件构建AssetBundle时，使用的名称格式化方法，例如文件夹package所在的AssetBundle会是string.Format(BundleNameFormatter, package)返回的结果
 * ```EnableSpine``` 启用Spine开关 （TODO）
 * ```EnableDragon``` 启用龙骨动画开关，启用后，将打开```FAIRYGUI_DRAGONBONES```宏启用龙骨动画相关代码
 * ```EnableTMPro``` 启用TMPro开关 (TODO)
 * ```bringWindowToFrontOnClick``` 运行时是否将被点击的窗品提到最上层。FairyGUI窗口默认自动将被点击的窗口提到最上层，通过该属性可以预设该开关。

### ```PluginSetFGUIConfig``` ```FairyGUI.UIConfig```副本
该配置效果同```FairyGUI.UIConfig```，构建时会将该配置所有数的居拷贝到```PluginSetConfig```中，在插件运行时，会将该数据转换为```FairyGUI.UIConfig```并设置到```FairyGUI```中。

### 构建流程相关
* 响应```OnSyncEditorSetting```，在预设编辑器配置时，会根据配置添加宏。如果开启了龙骨动画，会检测所有FairyGUI资源目录中是否创建了龙骨动画依赖的```BoneData```
* 响应```OnSyncExportSetting```，同步配置到```PluginSetConfig```
* 响应```OnBuildBundles```和```OnBuildPatches```，最先预设fairyGUI资源目录，在构建AssetBundle前，将fairyGUI资源按照需求设置AssetBundleName
* 响应```OnBuildBundlesCompleted```，在构建AssetBundle完成后，清除fairyGUI资源的AssetBundleName

### 其它修改
* 龙骨动画的叠加模式着色器算法会在每次渲染前抓屏一次，严重影响渲染性能，已将该Shader替换为FairyGUI中的叠加算法