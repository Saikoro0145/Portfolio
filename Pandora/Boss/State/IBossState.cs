public interface IBossState
{
    void Enter(); void Update(float dt); void Exit(); //dt:deltaTime
    //bool CanInterrupt(StateInterrupt reason); //ステートを中断させる理由を表す識別子
}
