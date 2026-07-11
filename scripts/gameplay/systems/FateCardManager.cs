using Godot;
using Hope.Config;
using Hope.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Hope.Systems;

/// <summary>
/// 命运织机核心管理器：负责抽卡、持有、连锁检测与本局效果累加。
/// 挂在 <see cref="RunManager"/> 下，仅在战斗对局生命周期内存在。
/// </summary>
public partial class FateCardManager : Node
{
    private readonly Dictionary<int, FateCardConfig> _allCardsById = new();
    private readonly List<FateCardConfig> _allCards = new();
    private readonly List<FateChainConfig> _allChains = new();
    private readonly HashSet<int> _ownedCardIds = new();
    private readonly HashSet<string> _ownedCardCodes = new();
    private readonly HashSet<int> _activeChainIds = new();
    private readonly Dictionary<string, float> _effectAccumulators = new();
    private readonly List<FateCardConfig> _currentOffer = new();
    private int _wavesWithoutRare;
    private int _wavesWithoutEpic;

    /// <summary>当前是否启用命运织机；当配置为空时自动关闭。</summary>
    public bool IsEnabled => _allCards.Count > 0;

    /// <summary>当前三选一候选卡牌（只读）。</summary>
    public IReadOnlyList<FateCardConfig> CurrentOffer => _currentOffer;

    /// <summary>当前已持有卡牌（只读）。</summary>
    public IReadOnlyCollection<int> OwnedCardIds => _ownedCardIds;

    /// <summary>本局已激活的连锁条目（只读）。</summary>
    public IReadOnlyCollection<int> ActiveChainIds => _activeChainIds;

    public override void _Ready()
    {
        LoadConfigs();
    }

    /// <summary>
    /// 按当前波次创建三选一卡池结果；同一局内不重复抽到已持有卡。
    /// </summary>
    /// <param name="currentWave">当前波次（从 1 开始）。</param>
    /// <returns>长度最多为 3 的卡牌列表。</returns>
    public IReadOnlyList<FateCardConfig> DrawCards(int currentWave)
    {
        _currentOffer.Clear();
        if (!IsEnabled)
        {
            return _currentOffer;
        }

        var pool = BuildWeightedPool(currentWave);
        while (_currentOffer.Count < 3 && pool.Count > 0)
        {
            var picked = WeightedDraw(pool);
            _currentOffer.Add(picked.Card);
            pool.RemoveAll(x => x.Card.Id == picked.Card.Id);
        }

        UpdatePityCounters(_currentOffer);
        return _currentOffer;
    }

    /// <summary>
    /// 选择并应用一张卡牌效果，同时检查并激活可达成的连锁。
    /// </summary>
    /// <param name="cardId">所选卡牌配置 Id。</param>
    /// <param name="stats">当前对局数值快照。</param>
    /// <returns>是否成功选择并应用。</returns>
    public bool SelectCard(int cardId, RunStats stats)
    {
        if (!_allCardsById.TryGetValue(cardId, out var card) || _ownedCardIds.Contains(cardId))
        {
            return false;
        }

        _ownedCardIds.Add(cardId);
        _ownedCardCodes.Add(card.CardCode);
        AccumulateEffect(card.EffectType, card.EffectValue);
        ApplyStatLikeEffect(card.EffectType, card.EffectValue, stats);
        ApplyExtraParams(card.ExtraParams, stats);

        EventBus.Instance?.EmitFateCardSelected(cardId, card.CardCode);
        TryActivateChains(stats);
        return true;
    }

    /// <summary>
    /// 查询某种效果类型的累计值。
    /// </summary>
    /// <param name="effectType">效果类型关键字。</param>
    /// <returns>累计值；不存在则为 0。</returns>
    public float GetEffectValue(string effectType)
    {
        return _effectAccumulators.GetValueOrDefault(effectType, 0f);
    }

    private void LoadConfigs()
    {
        _allCards.Clear();
        _allCardsById.Clear();
        _allChains.Clear();

        foreach (var card in ConfigManager.GetAll<FateCardConfig>())
        {
            _allCards.Add(card);
            _allCardsById[card.Id] = card;
        }

        _allChains.AddRange(ConfigManager.GetAll<FateChainConfig>());
    }

    private List<WeightedCard> BuildWeightedPool(int wave)
    {
        var pool = new List<WeightedCard>();
        foreach (var card in _allCards)
        {
            if (_ownedCardIds.Contains(card.Id))
            {
                continue;
            }

            var weight = CalculateWeight(card, wave);
            if (weight <= 0f)
            {
                continue;
            }

            pool.Add(new WeightedCard(card, weight));
        }

        ApplyRarityGuarantee(pool, wave);
        return pool;
    }

    private float CalculateWeight(FateCardConfig card, int wave)
    {
        var rarityFactor = card.Rarity switch
        {
            1 => 1f,
            2 => wave switch
            {
                <= 2 => 0f,
                <= 4 => 0.5f,
                <= 6 => 0.8f,
                _ => 1f,
            },
            3 => wave switch
            {
                <= 4 => 0f,
                <= 6 => 0.3f,
                _ => 0.5f,
            },
            _ => 0f,
        };

        return card.Weight * rarityFactor;
    }

    private void ApplyRarityGuarantee(List<WeightedCard> pool, int wave)
    {
        if (wave < 3 || pool.Count == 0)
        {
            return;
        }

        if (_wavesWithoutRare >= 3 && !pool.Any(x => x.Card.Rarity == 2))
        {
            TryInjectHighestWeight(pool, 2, wave);
        }

        if (wave >= 5 && _wavesWithoutEpic >= 6 && !pool.Any(x => x.Card.Rarity == 3))
        {
            TryInjectHighestWeight(pool, 3, wave);
        }
    }

    private void TryInjectHighestWeight(List<WeightedCard> pool, int rarity, int wave)
    {
        var fallback = _allCards
            .Where(x => x.Rarity == rarity && !_ownedCardIds.Contains(x.Id))
            .Select(x => new WeightedCard(x, Math.Max(1f, CalculateWeight(x, wave))))
            .OrderByDescending(x => x.Weight)
            .FirstOrDefault();

        if (fallback.Card != null && pool.All(x => x.Card.Id != fallback.Card.Id))
        {
            pool.Add(fallback);
        }
    }

    private static WeightedCard WeightedDraw(List<WeightedCard> pool)
    {
        var total = pool.Sum(x => x.Weight);
        var roll = GD.Randf() * total;
        var cursor = 0f;

        foreach (var item in pool)
        {
            cursor += item.Weight;
            if (roll <= cursor)
            {
                return item;
            }
        }

        return pool[^1];
    }

    private void UpdatePityCounters(IReadOnlyList<FateCardConfig> cards)
    {
        if (cards.Any(x => x.Rarity >= 2))
        {
            _wavesWithoutRare = 0;
        }
        else
        {
            _wavesWithoutRare += 1;
        }

        if (cards.Any(x => x.Rarity >= 3))
        {
            _wavesWithoutEpic = 0;
        }
        else
        {
            _wavesWithoutEpic += 1;
        }
    }

    private void AccumulateEffect(string effectType, float value)
    {
        var current = _effectAccumulators.GetValueOrDefault(effectType, 0f);
        _effectAccumulators[effectType] = current + value;
    }

    private static void ApplyStatLikeEffect(string effectType, float value, RunStats stats)
    {
        switch (effectType)
        {
            case "stat_max_hp":
                stats.MaxHealth += Mathf.RoundToInt(value);
                break;
            case "stat_damage":
                stats.Damage *= 1f + (value / 100f);
                break;
            case "stat_attack_speed":
                stats.AttackSpeed *= 1f + (value / 100f);
                break;
            case "stat_move_speed":
                stats.Speed *= 1f + (value / 100f);
                break;
            case "stat_armor":
                stats.Armor += Mathf.RoundToInt(value);
                break;
            case "gold_gain_mult":
                // 仅累加到 _effectAccumulators，由 RunManager 结算金币时读取
                break;
            case "instant_gold":
                stats.Gold += Mathf.RoundToInt(value);
                EventBus.Instance?.EmitGoldChanged(stats.Gold);
                break;
        }
    }

    private void ApplyExtraParams(string extraParams, RunStats stats)
    {
        if (string.IsNullOrWhiteSpace(extraParams) || extraParams == "null")
        {
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(extraParams);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (!prop.Value.TryGetSingle(out var value))
                {
                    continue;
                }

                AccumulateEffect(prop.Name, value);
                ApplyStatLikeEffect(prop.Name, value, stats);
            }
        }
        catch (JsonException ex)
        {
            GD.PushWarning($"[FateCardManager] 无法解析 extra_params: {extraParams} ({ex.Message})");
        }
    }

    private void TryActivateChains(RunStats stats)
    {
        foreach (var chain in _allChains)
        {
            if (_activeChainIds.Contains(chain.Id))
            {
                continue;
            }

            if (!_ownedCardCodes.Contains(chain.CardCode1)
                || !_ownedCardCodes.Contains(chain.CardCode2)
                || !_ownedCardCodes.Contains(chain.CardCode3))
            {
                continue;
            }

            _activeChainIds.Add(chain.Id);
            AccumulateEffect(chain.EffectType, chain.EffectValue);
            ApplyStatLikeEffect(chain.EffectType, chain.EffectValue, stats);
            EventBus.Instance?.EmitFateChainActivated(chain.Id, chain.ChainName);
        }
    }

    private readonly struct WeightedCard
    {
        public WeightedCard(FateCardConfig card, float weight)
        {
            Card = card;
            Weight = weight;
        }

        public FateCardConfig Card { get; }

        public float Weight { get; }
    }
}
