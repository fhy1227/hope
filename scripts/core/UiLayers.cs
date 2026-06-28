namespace Hope.Core;

/// <summary>
/// UI <c>CanvasLayer.layer</c> 层级常量，与 <c>scenes/main.tscn</c> 中各 UI_* 节点一致。
/// 数值间留空隙，便于插入新层；新 UI 须挂到对应层，勿随意新建无名 CanvasLayer。
/// </summary>
public static class UiLayers
{
    /// <summary>常驻 HUD（血条、波次等）。</summary>
    public const int Hud = 1;

    /// <summary>叠加界面（商店、背包等，可遮挡 HUD）。</summary>
    public const int Overlay = 2;

    /// <summary>暂停菜单层。</summary>
    public const int Pause = 3;

    /// <summary>全屏转场、淡入淡出。</summary>
    public const int Transition = 4;

    /// <summary>调试 overlay，高于玩法 UI。</summary>
    public const int Debug = 10;
}
