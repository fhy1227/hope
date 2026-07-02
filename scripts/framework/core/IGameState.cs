namespace Hope.Core;

/// <summary>
/// 有限状态机中的单个状态契约。由 <see cref="StateMachine"/> 调度 Enter/Exit/Update。
/// 玩家移动层（idle/move）使用此接口；战斗行为见 <c>Hope.Components.Actions.IPlayerAction</c>，勿混入 StateMachine。
/// </summary>
public interface IGameState
{
    /// <summary>进入本状态时调用一次；用于初始化或订阅。</summary>
    void Enter();

    /// <summary>离开本状态时调用一次；用于清理或取消订阅。</summary>
    void Exit();

    /// <summary>本状态激活期间每帧调用。</summary>
    /// <param name="delta">帧间隔（秒）。</param>
    void Update(double delta);
}
