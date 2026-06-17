namespace Hope.Core;

public interface IGameState
{
    void Enter();
    void Exit();
    void Update(double delta);
}
