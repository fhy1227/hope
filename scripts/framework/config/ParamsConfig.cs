using Godot;

namespace Hope.Config;

/// <summary>
/// 全局参数 - 对应 tools/config/params.xlsx（自动生成，请勿手改）。
/// </summary>
public static partial class ParamsConfig
{
    /// <summary>
    /// run_max_health
    /// 对局初始生命上限
    /// </summary>
    public const float RunMaxHealth = 10f;

    /// <summary>
    /// run_speed
    /// 对局初始移速 px/s
    /// </summary>
    public const float RunSpeed = 200f;

    /// <summary>
    /// run_damage
    /// 对局初始伤害
    /// </summary>
    public const float RunDamage = 5f;

    /// <summary>
    /// run_attack_speed
    /// 对局初始攻速倍率
    /// </summary>
    public const float RunAttackSpeed = 1.2f;

    /// <summary>
    /// run_projectile_speed
    /// 对局初始弹道速度
    /// </summary>
    public const float RunProjectileSpeed = 450f;

    /// <summary>
    /// run_weapon_range
    /// 对局初始武器射程
    /// </summary>
    public const float RunWeaponRange = 320f;

    /// <summary>
    /// shop_option_count
    /// 商店每次选项数量
    /// </summary>
    public const float ShopOptionCount = 3f;

    /// <summary>
    /// shop_hp_bonus
    /// 商店升级：生命+
    /// </summary>
    public const float ShopHpBonus = 3f;

    /// <summary>
    /// shop_damage_bonus
    /// 商店升级：伤害+
    /// </summary>
    public const float ShopDamageBonus = 2f;

    /// <summary>
    /// shop_attack_speed_mul
    /// 商店升级：攻速倍率
    /// </summary>
    public const float ShopAttackSpeedMul = 1.15f;

    /// <summary>
    /// shop_speed_bonus
    /// 商店升级：移速+
    /// </summary>
    public const float ShopSpeedBonus = 20f;

    /// <summary>
    /// shop_weapon_range_bonus
    /// 商店升级：射程+
    /// </summary>
    public const float ShopWeaponRangeBonus = 40f;

    /// <summary>
    /// wave_duration
    /// 单波基础时长秒
    /// </summary>
    public const float WaveDuration = 20f;

    /// <summary>
    /// wave_duration_growth
    /// 每波增加时长秒
    /// </summary>
    public const float WaveDurationGrowth = 1f;

    /// <summary>
    /// spawn_radius_min
    /// 刷怪最小半径
    /// </summary>
    public const float SpawnRadiusMin = 360f;

    /// <summary>
    /// spawn_radius_max
    /// 刷怪最大半径
    /// </summary>
    public const float SpawnRadiusMax = 480f;

    /// <summary>
    /// spawn_base_interval
    /// 基础刷怪间隔秒
    /// </summary>
    public const float SpawnBaseInterval = 2.2f;

    /// <summary>
    /// spawn_max_alive
    /// 同屏敌人上限
    /// </summary>
    public const float SpawnMaxAlive = 30f;

    /// <summary>
    /// spawn_begin_timer
    /// 开波后首次刷怪延迟
    /// </summary>
    public const float SpawnBeginTimer = 0.5f;

    /// <summary>
    /// spawn_cap_retry_timer
    /// 达上限重试间隔
    /// </summary>
    public const float SpawnCapRetryTimer = 0.5f;

    /// <summary>
    /// spawn_interval_wave_reduce
    /// 每波缩短间隔
    /// </summary>
    public const float SpawnIntervalWaveReduce = 0.12f;

    /// <summary>
    /// spawn_interval_min
    /// 刷怪间隔下限
    /// </summary>
    public const float SpawnIntervalMin = 0.45f;

    /// <summary>
    /// weapon_default_config_id
    /// 默认武器 item id
    /// </summary>
    public const float WeaponDefaultConfigId = 1030f;

    /// <summary>
    /// weapon_sword_config_id
    /// 短剑 item id
    /// </summary>
    public const float WeaponSwordConfigId = 1031f;

    /// <summary>
    /// weapon_ranged_config_id_1
    /// 远程武器 id 1032
    /// </summary>
    public const float WeaponRangedConfigId1 = 1032f;

    /// <summary>
    /// weapon_ranged_config_id_2
    /// 远程武器 id 1033
    /// </summary>
    public const float WeaponRangedConfigId2 = 1033f;

    /// <summary>
    /// weapon_damage_scale_default
    /// 武器默认伤害倍率
    /// </summary>
    public const float WeaponDamageScaleDefault = 1f;

    /// <summary>
    /// weapon_attack_speed_scale_default
    /// 武器默认攻速倍率
    /// </summary>
    public const float WeaponAttackSpeedScaleDefault = 1f;

    /// <summary>
    /// weapon_range_default
    /// 武器默认射程
    /// </summary>
    public const float WeaponRangeDefault = 300f;

    /// <summary>
    /// weapon_projectile_speed_default
    /// 武器默认弹道速度
    /// </summary>
    public const float WeaponProjectileSpeedDefault = 450f;

    /// <summary>
    /// weapon_range_ranged
    /// 远程武器射程
    /// </summary>
    public const float WeaponRangeRanged = 340f;

    /// <summary>
    /// weapon_projectile_speed_ranged
    /// 远程武器弹道速度
    /// </summary>
    public const float WeaponProjectileSpeedRanged = 480f;

    /// <summary>
    /// weapon_range_sword
    /// 短剑射程
    /// </summary>
    public const float WeaponRangeSword = 65f;

    /// <summary>
    /// weapon_range_melee_default
    /// 默认近战射程
    /// </summary>
    public const float WeaponRangeMeleeDefault = 58f;

    /// <summary>
    /// weapon_pistol_attack_speed_scale
    /// 手枪攻速倍率
    /// </summary>
    public const float WeaponPistolAttackSpeedScale = 1.2f;

    /// <summary>
    /// weapon_sword_damage_scale
    /// 短剑伤害倍率
    /// </summary>
    public const float WeaponSwordDamageScale = 1.4f;

    /// <summary>
    /// weapon_sword_attack_speed_scale
    /// 短剑攻速倍率
    /// </summary>
    public const float WeaponSwordAttackSpeedScale = 0.85f;

    /// <summary>
    /// weapon_spear_damage_scale
    /// 长矛伤害倍率
    /// </summary>
    public const float WeaponSpearDamageScale = 1.2f;

    /// <summary>
    /// weapon_spear_range
    /// 长矛射程
    /// </summary>
    public const float WeaponSpearRange = 72f;

    /// <summary>
    /// weapon_min_attack_speed_scale
    /// 攻速倍率下限
    /// </summary>
    public const float WeaponMinAttackSpeedScale = 0.1f;

    /// <summary>
    /// weapon_slot_offset
    /// 武器槽偏移像素
    /// </summary>
    public const float WeaponSlotOffset = 20f;

    /// <summary>
    /// weapon_ranged_range_ref
    /// 远程射程缩放基准
    /// </summary>
    public const float WeaponRangedRangeRef = 340f;

    /// <summary>
    /// weapon_melee_range_ref
    /// 近战射程缩放基准
    /// </summary>
    public const float WeaponMeleeRangeRef = 320f;

    /// <summary>
    /// weapon_min_attack_speed
    /// 攻击间隔攻速下限
    /// </summary>
    public const float WeaponMinAttackSpeed = 0.1f;

    /// <summary>
    /// weapon_swing_angle_peak_deg
    /// 挥砍峰值角度
    /// </summary>
    public const float WeaponSwingAnglePeakDeg = 55f;

    /// <summary>
    /// weapon_swing_angle_trough_deg
    /// 挥砍谷值角度
    /// </summary>
    public const float WeaponSwingAngleTroughDeg = 25f;

    /// <summary>
    /// weapon_swing_tween_1
    /// 挥砍 tween1 秒
    /// </summary>
    public const float WeaponSwingTween1 = 0.07f;

    /// <summary>
    /// weapon_swing_tween_2
    /// 挥砍 tween2 秒
    /// </summary>
    public const float WeaponSwingTween2 = 0.09f;

    /// <summary>
    /// weapon_swing_tween_3
    /// 挥砍 tween3 秒
    /// </summary>
    public const float WeaponSwingTween3 = 0.05f;

    /// <summary>
    /// weapon_thrust_distance
    /// 刺击位移像素
    /// </summary>
    public const float WeaponThrustDistance = 22f;

    /// <summary>
    /// weapon_thrust_tween_out
    /// 刺击伸出秒
    /// </summary>
    public const float WeaponThrustTweenOut = 0.06f;

    /// <summary>
    /// weapon_thrust_tween_back
    /// 刺击收回秒
    /// </summary>
    public const float WeaponThrustTweenBack = 0.08f;

    /// <summary>
    /// weapon_thrust_hitbox_width_ratio
    /// 刺击 hitbox 宽比
    /// </summary>
    public const float WeaponThrustHitboxWidthRatio = 0.85f;

    /// <summary>
    /// weapon_thrust_hitbox_height
    /// 刺击 hitbox 高
    /// </summary>
    public const float WeaponThrustHitboxHeight = 14f;

    /// <summary>
    /// weapon_thrust_hitbox_pos_ratio
    /// 刺击 hitbox 偏移比
    /// </summary>
    public const float WeaponThrustHitboxPosRatio = 0.42f;

    /// <summary>
    /// weapon_swing_hitbox_width_ratio
    /// 挥砍 hitbox 宽比
    /// </summary>
    public const float WeaponSwingHitboxWidthRatio = 0.75f;

    /// <summary>
    /// weapon_swing_hitbox_height
    /// 挥砍 hitbox 高
    /// </summary>
    public const float WeaponSwingHitboxHeight = 22f;

    /// <summary>
    /// weapon_swing_hitbox_pos_ratio
    /// 挥砍 hitbox 偏移比
    /// </summary>
    public const float WeaponSwingHitboxPosRatio = 0.38f;

    /// <summary>
    /// roll_duration
    /// 翻滚时长秒
    /// </summary>
    public const float RollDuration = 0.25f;

    /// <summary>
    /// roll_speed
    /// 翻滚速度
    /// </summary>
    public const float RollSpeed = 480f;

    /// <summary>
    /// roll_cooldown
    /// 翻滚冷却秒
    /// </summary>
    public const float RollCooldown = 1f;

    /// <summary>
    /// roll_visual_alpha
    /// 翻滚半透明 alpha
    /// </summary>
    public const float RollVisualAlpha = 0.55f;

    /// <summary>
    /// charge_windup_time
    /// 聚气前摇秒
    /// </summary>
    public const float ChargeWindupTime = 0.15f;

    /// <summary>
    /// charge_max_time
    /// 聚气满蓄秒
    /// </summary>
    public const float ChargeMaxTime = 2f;

    /// <summary>
    /// charge_release_time
    /// 聚气释放秒
    /// </summary>
    public const float ChargeReleaseTime = 0.2f;

    /// <summary>
    /// charge_cooldown
    /// 聚气冷却秒
    /// </summary>
    public const float ChargeCooldown = 2f;

    /// <summary>
    /// charge_min_release_percent
    /// 聚气最低释放比例
    /// </summary>
    public const float ChargeMinReleasePercent = 0.25f;

    /// <summary>
    /// charge_move_speed_mul
    /// 蓄力移速倍率
    /// </summary>
    public const float ChargeMoveSpeedMul = 0.5f;

    /// <summary>
    /// charge_visual_scale_max
    /// 蓄力视觉缩放增量
    /// </summary>
    public const float ChargeVisualScaleMax = 0.15f;

    /// <summary>
    /// charge_aoe_threshold_mid
    /// 聚气爆发中档阈值
    /// </summary>
    public const float ChargeAoeThresholdMid = 0.5f;

    /// <summary>
    /// charge_aoe_threshold_high
    /// 聚气爆发高档阈值
    /// </summary>
    public const float ChargeAoeThresholdHigh = 0.75f;

    /// <summary>
    /// charge_aoe_radius_low
    /// 聚气爆发低档半径
    /// </summary>
    public const float ChargeAoeRadiusLow = 60f;

    /// <summary>
    /// charge_aoe_damage_mul_low
    /// 聚气爆发低档伤害倍率
    /// </summary>
    public const float ChargeAoeDamageMulLow = 0.8f;

    /// <summary>
    /// charge_aoe_knockback_low
    /// 聚气爆发低档击退
    /// </summary>
    public const float ChargeAoeKnockbackLow = 80f;

    /// <summary>
    /// charge_aoe_radius_mid
    /// 聚气爆发中档半径
    /// </summary>
    public const float ChargeAoeRadiusMid = 90f;

    /// <summary>
    /// charge_aoe_damage_mul_mid
    /// 聚气爆发中档伤害倍率
    /// </summary>
    public const float ChargeAoeDamageMulMid = 1.2f;

    /// <summary>
    /// charge_aoe_knockback_mid
    /// 聚气爆发中档击退
    /// </summary>
    public const float ChargeAoeKnockbackMid = 100f;

    /// <summary>
    /// charge_aoe_radius_high
    /// 聚气爆发高档半径
    /// </summary>
    public const float ChargeAoeRadiusHigh = 120f;

    /// <summary>
    /// charge_aoe_damage_mul_high
    /// 聚气爆发高档伤害倍率
    /// </summary>
    public const float ChargeAoeDamageMulHigh = 2f;

    /// <summary>
    /// charge_aoe_knockback_high
    /// 聚气爆发高档击退
    /// </summary>
    public const float ChargeAoeKnockbackHigh = 140f;

    /// <summary>
    /// stomp_windup_time
    /// 震地前摇秒
    /// </summary>
    public const float StompWindupTime = 0.1f;

    /// <summary>
    /// stomp_recovery_time
    /// 震地恢复秒
    /// </summary>
    public const float StompRecoveryTime = 0.05f;

    /// <summary>
    /// stomp_radius
    /// 震地半径
    /// </summary>
    public const float StompRadius = 80f;

    /// <summary>
    /// stomp_damage_mul
    /// 震地伤害倍率
    /// </summary>
    public const float StompDamageMul = 0.6f;

    /// <summary>
    /// stomp_knockback_speed
    /// 震地击退速度
    /// </summary>
    public const float StompKnockbackSpeed = 120f;

    /// <summary>
    /// stomp_cooldown
    /// 震地冷却秒
    /// </summary>
    public const float StompCooldown = 3f;

    /// <summary>
    /// stomp_visual_scale
    /// 震地视觉缩放
    /// </summary>
    public const float StompVisualScale = 0.85f;

    /// <summary>
    /// stomp_release_visual_scale
    /// 震地释放视觉缩放
    /// </summary>
    public const float StompReleaseVisualScale = 1.15f;

    /// <summary>
    /// parry_window_duration
    /// 格挡窗口秒
    /// </summary>
    public const float ParryWindowDuration = 0.25f;

    /// <summary>
    /// parry_perfect_window
    /// 完美格挡窗口秒
    /// </summary>
    public const float ParryPerfectWindow = 0.08f;

    /// <summary>
    /// parry_success_cooldown
    /// 格挡成功冷却
    /// </summary>
    public const float ParrySuccessCooldown = 0.8f;

    /// <summary>
    /// parry_fail_cooldown
    /// 格挡失败冷却
    /// </summary>
    public const float ParryFailCooldown = 1.5f;

    /// <summary>
    /// parry_counter_radius
    /// 格挡反制半径
    /// </summary>
    public const float ParryCounterRadius = 70f;

    /// <summary>
    /// parry_counter_damage_mul
    /// 格挡反制伤害倍率
    /// </summary>
    public const float ParryCounterDamageMul = 1f;

    /// <summary>
    /// parry_stun_duration
    /// 格挡眩晕秒
    /// </summary>
    public const float ParryStunDuration = 0.4f;

    /// <summary>
    /// parry_move_speed_mul
    /// 格挡移速倍率
    /// </summary>
    public const float ParryMoveSpeedMul = 0.3f;

    /// <summary>
    /// parry_counter_knockback
    /// 格挡反制击退
    /// </summary>
    public const float ParryCounterKnockback = 60f;

    /// <summary>
    /// drop_smart_loot_chance
    /// Smart Loot 概率
    /// </summary>
    public const float DropSmartLootChance = 0.85f;

    /// <summary>
    /// drop_area_level_base
    /// 区域等级基础
    /// </summary>
    public const float DropAreaLevelBase = 1f;

    /// <summary>
    /// drop_area_level_per_wave
    /// 每波区域等级增量
    /// </summary>
    public const float DropAreaLevelPerWave = 2f;

    /// <summary>
    /// drop_elite_magic_find
    /// 精英 MF
    /// </summary>
    public const float DropEliteMagicFind = 0.25f;

    /// <summary>
    /// drop_elite_rate_mul
    /// 精英掉落倍率
    /// </summary>
    public const float DropEliteRateMul = 2f;

    /// <summary>
    /// drop_elite_level_bonus
    /// 精英等级加成
    /// </summary>
    public const float DropEliteLevelBonus = 2f;

    /// <summary>
    /// drop_boss_magic_find
    /// Boss MF
    /// </summary>
    public const float DropBossMagicFind = 0.5f;

    /// <summary>
    /// drop_boss_rate_mul
    /// Boss 掉落倍率
    /// </summary>
    public const float DropBossRateMul = 1f;

    /// <summary>
    /// drop_boss_level_bonus
    /// Boss 等级加成
    /// </summary>
    public const float DropBossLevelBonus = 5f;

    /// <summary>
    /// elite_spawn_chance_base
    /// 精英怪基础生成概率
    /// </summary>
    public const float EliteSpawnChanceBase = 0.05f;

    /// <summary>
    /// elite_spawn_chance_per_wave
    /// 精英怪每波额外生成概率
    /// </summary>
    public const float EliteSpawnChancePerWave = 0.01f;

    /// <summary>
    /// elite_hp_mult
    /// 精英怪生命倍率
    /// </summary>
    public const float EliteHpMult = 2.5f;

    /// <summary>
    /// elite_gold_mult
    /// 精英怪金币掉落倍率
    /// </summary>
    public const float EliteGoldMult = 2f;

    /// <summary>
    /// elite_scale
    /// 精英怪体型缩放
    /// </summary>
    public const float EliteScale = 1.2f;

    /// <summary>
    /// drop_rarity_upgrade_chance
    /// 稀有度升档概率
    /// </summary>
    public const float DropRarityUpgradeChance = 0.05f;

    /// <summary>
    /// drop_mf_tier_factor
    /// MF 高稀有权重系数
    /// </summary>
    public const float DropMfTierFactor = 0.5f;

    /// <summary>
    /// drop_item_level_variance
    /// 物品等级波动
    /// </summary>
    public const float DropItemLevelVariance = 1f;

    /// <summary>
    /// drop_legendary_stat_mul
    /// 传奇属性倍率
    /// </summary>
    public const float DropLegendaryStatMul = 1.35f;

    /// <summary>
    /// drop_weapon_slot_type
    /// Smart Loot 武器槽类型
    /// </summary>
    public const float DropWeaponSlotType = 1f;

    /// <summary>
    /// drop_legendary_rarity_threshold
    /// 传奇稀有度阈值
    /// </summary>
    public const float DropLegendaryRarityThreshold = 4f;

    /// <summary>
    /// item_reference_level
    /// 装备 ilvl 缩放基准
    /// </summary>
    public const float ItemReferenceLevel = 50f;

    /// <summary>
    /// enemy_contact_cooldown
    /// 敌人接触伤害冷却
    /// </summary>
    public const float EnemyContactCooldown = 0.6f;

    /// <summary>
    /// enemy_gold_drop_default
    /// 敌人默认金币
    /// </summary>
    public const float EnemyGoldDropDefault = 1f;

    /// <summary>
    /// enemy_move_speed_default
    /// 敌人默认移速
    /// </summary>
    public const float EnemyMoveSpeedDefault = 90f;

    /// <summary>
    /// enemy_contact_damage_default
    /// 敌人默认接触伤害
    /// </summary>
    public const float EnemyContactDamageDefault = 1f;

    /// <summary>
    /// enemy_knockback_decay
    /// 敌人击退衰减
    /// </summary>
    public const float EnemyKnockbackDecay = 0.88f;

    /// <summary>
    /// enemy_type_normal
    /// 普通敌人类型 key
    /// </summary>
    public const string EnemyTypeNormal = "normal";

    /// <summary>
    /// enemy_type_elite
    /// 精英敌人类型 key
    /// </summary>
    public const string EnemyTypeElite = "elite";

    /// <summary>
    /// enemy_type_boss
    /// Boss 敌人类型 key
    /// </summary>
    public const string EnemyTypeBoss = "boss";

    /// <summary>
    /// player_facing_vel_threshold_sq
    /// 面朝方向速度阈值平方
    /// </summary>
    public const float PlayerFacingVelThresholdSq = 4f;

    /// <summary>
    /// player_min_damage
    /// 玩家技能伤害下限
    /// </summary>
    public const float PlayerMinDamage = 1f;

    /// <summary>
    /// player_hit_invincibility
    /// 受击无敌秒
    /// </summary>
    public const float PlayerHitInvincibility = 0.4f;

    /// <summary>
    /// player_damage_flash_duration
    /// 受击闪红恢复秒
    /// </summary>
    public const float PlayerDamageFlashDuration = 0.15f;

    /// <summary>
    /// player_action_flash_duration
    /// 行为闪光恢复秒
    /// </summary>
    public const float PlayerActionFlashDuration = 0.12f;

    /// <summary>
    /// player_death_visual_alpha
    /// 死亡视觉 alpha
    /// </summary>
    public const float PlayerDeathVisualAlpha = 0.35f;

    /// <summary>
    /// health_default_max
    /// HealthComponent 默认上限
    /// </summary>
    public const float HealthDefaultMax = 3f;

    /// <summary>
    /// health_float_text_offset_y
    /// 伤害数字 Y 偏移
    /// </summary>
    public const float HealthFloatTextOffsetY = -16f;

    /// <summary>
    /// health_bar_width
    /// 血条宽度
    /// </summary>
    public const float HealthBarWidth = 28f;

    /// <summary>
    /// health_bar_height
    /// 血条高度
    /// </summary>
    public const float HealthBarHeight = 4f;

    /// <summary>
    /// health_bar_y_offset
    /// 血条 Y 偏移
    /// </summary>
    public const float HealthBarYOffset = -18f;

    /// <summary>
    /// pickup_magnet_range
    /// 拾取磁铁范围
    /// </summary>
    public const float PickupMagnetRange = 80f;

    /// <summary>
    /// pickup_magnet_speed
    /// 拾取磁铁速度
    /// </summary>
    public const float PickupMagnetSpeed = 280f;

    /// <summary>
    /// pickup_collect_distance
    /// 拾取判定距离
    /// </summary>
    public const float PickupCollectDistance = 8f;

    /// <summary>
    /// pickup_gold_default
    /// 金币拾取默认量
    /// </summary>
    public const float PickupGoldDefault = 1f;

    /// <summary>
    /// projectile_speed_default
    /// 弹道默认速度
    /// </summary>
    public const float ProjectileSpeedDefault = 450f;

    /// <summary>
    /// projectile_damage_default
    /// 弹道默认伤害
    /// </summary>
    public const float ProjectileDamageDefault = 1f;

    /// <summary>
    /// projectile_lifetime
    /// 弹道存活秒
    /// </summary>
    public const float ProjectileLifetime = 2f;

    /// <summary>
    /// inventory_max_slots
    /// 背包格数
    /// </summary>
    public const float InventoryMaxSlots = 20f;

    /// <summary>
    /// inventory_grid_columns
    /// 背包网格列数
    /// </summary>
    public const float InventoryGridColumns = 5f;

    /// <summary>
    /// inventory_low_rarity_max
    /// 一键出售低品质上限
    /// </summary>
    public const float InventoryLowRarityMax = 2f;

    /// <summary>
    /// inventory_quality_bg_alpha
    /// 品质背景 alpha
    /// </summary>
    public const float InventoryQualityBgAlpha = 0.35f;

    /// <summary>
    /// inventory_slot_corner_radius
    /// 背包格圆角
    /// </summary>
    public const float InventorySlotCornerRadius = 4f;

    /// <summary>
    /// damage_number_duration
    /// 伤害数字持续秒
    /// </summary>
    public const float DamageNumberDuration = 0.75f;

    /// <summary>
    /// damage_number_rise_speed
    /// 伤害数字上浮速度
    /// </summary>
    public const float DamageNumberRiseSpeed = 55f;

    /// <summary>
    /// damage_number_font_size
    /// 伤害数字字号
    /// </summary>
    public const float DamageNumberFontSize = 16f;

    /// <summary>
    /// damage_number_jitter_x
    /// 伤害数字 X 抖动
    /// </summary>
    public const float DamageNumberJitterX = 8f;

    /// <summary>
    /// damage_number_jitter_y
    /// 伤害数字 Y 抖动
    /// </summary>
    public const float DamageNumberJitterY = 2f;

    /// <summary>
    /// damage_number_vel_jitter_x
    /// 伤害数字速度 X 抖动
    /// </summary>
    public const float DamageNumberVelJitterX = 18f;

    /// <summary>
    /// damage_number_initial_scale
    /// 伤害数字初始缩放
    /// </summary>
    public const float DamageNumberInitialScale = 0.55f;

    /// <summary>
    /// damage_number_scale_tween
    /// 伤害数字缩放 tween 秒
    /// </summary>
    public const float DamageNumberScaleTween = 0.1f;

    /// <summary>
    /// damage_number_velocity_decay
    /// 伤害数字速度衰减
    /// </summary>
    public const float DamageNumberVelocityDecay = 0.92f;

    /// <summary>
    /// circle_flash_duration
    /// 圈闪光持续秒
    /// </summary>
    public const float CircleFlashDuration = 0.18f;

    /// <summary>
    /// circle_flash_start_alpha
    /// 圈闪光起始 alpha
    /// </summary>
    public const float CircleFlashStartAlpha = 0.42f;

    /// <summary>
    /// circle_flash_end_scale
    /// 圈闪光结束缩放
    /// </summary>
    public const float CircleFlashEndScale = 1.06f;

    /// <summary>
    /// circle_flash_start_scale
    /// 圈闪光起始缩放
    /// </summary>
    public const float CircleFlashStartScale = 0.94f;

    /// <summary>
    /// numeric_min_move_speed
    /// 移速下限
    /// </summary>
    public const float NumericMinMoveSpeed = 50f;

    /// <summary>
    /// numeric_min_weapon_range
    /// 射程下限
    /// </summary>
    public const float NumericMinWeaponRange = 50f;

    /// <summary>
    /// audio_sfx_pool_size
    /// 音效池大小
    /// </summary>
    public const float AudioSfxPoolSize = 8f;

    /// <summary>
    /// shop_panel_z_index
    /// 商店面板 ZIndex
    /// </summary>
    public const float ShopPanelZIndex = 100f;

    /// <summary>
    /// save_max_profile_slots
    /// 存档栏位数
    /// </summary>
    public const float SaveMaxProfileSlots = 3f;

    /// <summary>
    /// save_default_slot_index
    /// 默认存档栏
    /// </summary>
    public const float SaveDefaultSlotIndex = 0f;

    /// <summary>
    /// anim_player_idle_fps
    /// 玩家 idle FPS
    /// </summary>
    public const float AnimPlayerIdleFps = 6f;

    /// <summary>
    /// anim_player_walk_fps
    /// 玩家 walk FPS
    /// </summary>
    public const float AnimPlayerWalkFps = 10f;

    /// <summary>
    /// anim_player_roll_fps
    /// 玩家 roll FPS
    /// </summary>
    public const float AnimPlayerRollFps = 20f;

    /// <summary>
    /// anim_player_charge_fps
    /// 玩家 charge FPS
    /// </summary>
    public const float AnimPlayerChargeFps = 8f;

    /// <summary>
    /// anim_player_charge_release_fps
    /// 玩家 charge_release FPS
    /// </summary>
    public const float AnimPlayerChargeReleaseFps = 12f;

    /// <summary>
    /// anim_player_stomp_fps
    /// 玩家 stomp FPS
    /// </summary>
    public const float AnimPlayerStompFps = 14f;

    /// <summary>
    /// anim_player_parry_fps
    /// 玩家 parry FPS
    /// </summary>
    public const float AnimPlayerParryFps = 12f;

    /// <summary>
    /// anim_player_walk_frames
    /// 玩家 walk 帧数
    /// </summary>
    public const float AnimPlayerWalkFrames = 10f;

    /// <summary>
    /// anim_player_roll_frames
    /// 玩家 roll 帧数
    /// </summary>
    public const float AnimPlayerRollFrames = 10f;

    /// <summary>
    /// anim_player_charge_frames
    /// 玩家 charge 帧数
    /// </summary>
    public const float AnimPlayerChargeFrames = 8f;

    /// <summary>
    /// anim_player_stomp_frames
    /// 玩家 stomp 帧数
    /// </summary>
    public const float AnimPlayerStompFrames = 14f;

    /// <summary>
    /// anim_player_parry_frames
    /// 玩家 parry 帧数
    /// </summary>
    public const float AnimPlayerParryFrames = 6f;

    /// <summary>
    /// anim_enemy_idle_fps
    /// 敌人 idle FPS
    /// </summary>
    public const float AnimEnemyIdleFps = 6f;

    /// <summary>
    /// anim_enemy_walk_fps
    /// 敌人 walk FPS
    /// </summary>
    public const float AnimEnemyWalkFps = 10f;

    /// <summary>
    /// anim_enemy_attack_fps
    /// 敌人 attack FPS
    /// </summary>
    public const float AnimEnemyAttackFps = 12f;

    /// <summary>
    /// anim_enemy_defeated_fps
    /// 敌人 defeated FPS
    /// </summary>
    public const float AnimEnemyDefeatedFps = 12f;

    /// <summary>
    /// anim_enemy_walk_frames
    /// 敌人 walk 帧数
    /// </summary>
    public const float AnimEnemyWalkFrames = 10f;

    /// <summary>
    /// anim_enemy_attack_frames
    /// 敌人 attack 帧数
    /// </summary>
    public const float AnimEnemyAttackFrames = 15f;

    /// <summary>
    /// anim_enemy_defeated_frames
    /// 敌人 defeated 帧数
    /// </summary>
    public const float AnimEnemyDefeatedFrames = 14f;

    /// <summary>
    /// color_weapon_pistol
    /// 手枪视觉色
    /// </summary>
    public static readonly Color ColorWeaponPistol = new(1f, 0.85f, 0.3f);

    /// <summary>
    /// color_weapon_sword
    /// 短剑视觉色
    /// </summary>
    public static readonly Color ColorWeaponSword = new(0.85f, 0.9f, 1f);

    /// <summary>
    /// color_weapon_spear
    /// 长矛视觉色
    /// </summary>
    public static readonly Color ColorWeaponSpear = new(0.75f, 0.8f, 0.95f);

    /// <summary>
    /// color_roll_visual
    /// 翻滚视觉色
    /// </summary>
    public static readonly Color ColorRollVisual = new(1f, 1f, 1f, 0.55f);

    /// <summary>
    /// color_charge_enter
    /// 聚气进入色
    /// </summary>
    public static readonly Color ColorChargeEnter = new(0.85f, 0.95f, 1f);

    /// <summary>
    /// color_charge_charging
    /// 聚气蓄力色
    /// </summary>
    public static readonly Color ColorChargeCharging = new(0.7f, 0.85f, 1f);

    /// <summary>
    /// color_charge_release
    /// 聚气释放闪色
    /// </summary>
    public static readonly Color ColorChargeRelease = new(1f, 0.95f, 0.7f);

    /// <summary>
    /// color_stomp_enter
    /// 震地进入色
    /// </summary>
    public static readonly Color ColorStompEnter = new(0.75f, 0.85f, 1f);

    /// <summary>
    /// color_stomp_release
    /// 震地释放色
    /// </summary>
    public static readonly Color ColorStompRelease = new(0.6f, 0.8f, 1f);

    /// <summary>
    /// color_parry_enter
    /// 格挡进入色
    /// </summary>
    public static readonly Color ColorParryEnter = new(0.9f, 1f, 0.95f);

    /// <summary>
    /// color_parry_perfect
    /// 完美格挡闪色
    /// </summary>
    public static readonly Color ColorParryPerfect = new(1f, 1f, 0.6f);

    /// <summary>
    /// color_parry_normal
    /// 普通格挡闪色
    /// </summary>
    public static readonly Color ColorParryNormal = new(0.85f, 1f, 0.85f);

    /// <summary>
    /// color_player_damage
    /// 玩家受击色
    /// </summary>
    public static readonly Color ColorPlayerDamage = new(1f, 0.45f, 0.45f);

    /// <summary>
    /// color_damage_number_player
    /// 玩家伤害数字色
    /// </summary>
    public static readonly Color ColorDamageNumberPlayer = new(1f, 0.4f, 0.4f);

    /// <summary>
    /// color_damage_number_enemy
    /// 敌人伤害数字色
    /// </summary>
    public static readonly Color ColorDamageNumberEnemy = new(1f, 0.92f, 0.45f);

    /// <summary>
    /// color_circle_flash_default
    /// 圈闪光默认色
    /// </summary>
    public static readonly Color ColorCircleFlashDefault = new(0.75f, 0.9f, 1f);

    /// <summary>
    /// color_health_bar_fill
    /// 血条填充色
    /// </summary>
    public static readonly Color ColorHealthBarFill = new(0.35f, 0.9f, 0.45f);

    /// <summary>
    /// color_health_bar_bg
    /// 血条背景色
    /// </summary>
    public static readonly Color ColorHealthBarBg = new(0.1f, 0.1f, 0.12f, 0.9f);

    /// <summary>
    /// color_inventory_empty_slot
    /// 空槽文字色
    /// </summary>
    public static readonly Color ColorInventoryEmptySlot = new(0.5f, 0.5f, 0.5f);

    /// <summary>
    /// color_quality_white
    /// 品质色-白
    /// </summary>
    public static readonly Color ColorQualityWhite = new(0.627f, 0.627f, 0.627f);

    /// <summary>
    /// color_quality_blue
    /// 品质色-蓝
    /// </summary>
    public static readonly Color ColorQualityBlue = new(0.188f, 0.447f, 0.953f);

    /// <summary>
    /// color_quality_yellow
    /// 品质色-黄
    /// </summary>
    public static readonly Color ColorQualityYellow = new(0.961f, 0.651f, 0.137f);

    /// <summary>
    /// color_quality_orange
    /// 品质色-橙
    /// </summary>
    public static readonly Color ColorQualityOrange = new(0.639f, 0.208f, 0.933f);

    /// <summary>
    /// color_quality_gold
    /// 品质色-暗金
    /// </summary>
    public static readonly Color ColorQualityGold = new(1f, 0.843f, 0f);

    /// <summary>
    /// path_player_sprite_dir
    /// 玩家精灵目录
    /// </summary>
    public const string PathPlayerSpriteDir = "res://assets/textures/characters/archer_gold.sprites/";

    /// <summary>
    /// path_enemy_sprite_dir
    /// 敌人精灵目录
    /// </summary>
    public const string PathEnemySpriteDir = "res://assets/textures/enemy/enemy01.sprites/";

    /// <summary>
    /// path_enemy_sprite_prefix
    /// 敌人精灵前缀
    /// </summary>
    public const string PathEnemySpritePrefix = "long_daoke";

    /// <summary>
    /// path_inventory_item_slot
    /// 背包格场景
    /// </summary>
    public const string PathInventoryItemSlot = "res://scenes/gameplay/inventory_item_slot.tscn";

    /// <summary>
    /// path_config_dir
    /// 配置 JSON 目录
    /// </summary>
    public const string PathConfigDir = "res://assets/config/";

    /// <summary>
    /// audio_master_bus
    /// 主音量总线名
    /// </summary>
    public const string AudioMasterBus = "Master";
}
