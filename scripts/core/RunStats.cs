namespace Hope.Core;

/// <summary>
/// 单局运行时数值，波间商店会修改此对象。
/// </summary>
public class RunStats
{
    public int MaxHealth { get; set; } = 10;
    public float Speed { get; set; } = 200f;
    public float Damage { get; set; } = 5f;
    public float AttackSpeed { get; set; } = 1.2f;
    public float ProjectileSpeed { get; set; } = 450f;
    public float WeaponRange { get; set; } = 320f;
    public int Gold { get; set; }
    public int Wave { get; set; }

    public RunStats Clone()
    {
        return new RunStats
        {
            MaxHealth = MaxHealth,
            Speed = Speed,
            Damage = Damage,
            AttackSpeed = AttackSpeed,
            ProjectileSpeed = ProjectileSpeed,
            WeaponRange = WeaponRange,
            Gold = Gold,
            Wave = Wave,
        };
    }
}
