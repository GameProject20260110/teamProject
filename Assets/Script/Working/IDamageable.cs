public interface IDamageable
{
    void TakeDamageRaw(int damage, DiceContext ctx);
    void ApplyStatusEffect(StatusEffect effect);
}
