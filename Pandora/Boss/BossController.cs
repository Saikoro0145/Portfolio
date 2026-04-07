using UnityEngine;

public class BossController : MonoBehaviour
{
    [SerializeField] public Health health;

    [SerializeField] private BossDamageReceiver damageReceiver;
    private BossContextBase bossContext;
    public BossSM bossSM { get; private set; }

    private void Awake()
    {
        health.Init(10);
        damageReceiver.Init(health);
        bossContext = GetComponent<BossContextBase>();
    }

    private void Start()
    {
        bossSM = new BossSM(bossContext);
        bossSM.bossBornState.Enter();
    }

    private void Update()
    {
        bossSM.UpdateAllStates(Time.deltaTime);
        //DebugBattleState(Time.deltaTime);
    }

    //For Debugging
    private float elapsedTime = 0;
    private void DebugBattleState(float dt)
    {
        elapsedTime += dt;
        if (elapsedTime > bossContext.idleTime)
        {
            elapsedTime = 0;
            DecHP();
        }
    }

    private int counter = 0;
    private void DecHP()
    {
        counter++;
        if(counter == 3)
        {
            health.TakeDamage(10);
            counter = 0;
        }
    }
}
