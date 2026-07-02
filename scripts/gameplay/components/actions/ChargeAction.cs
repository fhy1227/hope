using Godot;
using Hope.Components.Actions.Charge;
using Hope.Config;
using Hope.Core;

namespace Hope.Components.Actions;

/// <summary>
/// 聚气行为（输入 R 按住）：前摇 → 蓄力（可慢速移动）→ 松键或满蓄释放范围爆发 → 冷却。
/// 蓄力不足 25% 松键视为取消；受击打断前摇/蓄力。释放后 CD 2s。
/// </summary>
public sealed class ChargeAction : IPlayerAction
{
	private readonly IChargeReleaseEffect _releaseEffect;

	/// <summary>内部阶段：空闲 → 前摇 → 蓄力 → 释放 → 冷却。</summary>
	private enum Phase { Idle, WindUp, Charging, Release, Cooldown }

	private Phase _phase = Phase.Idle;
	private float _timer;
	private float _chargePercent;
	private float _cooldown;

	/// <summary>
	/// 创建聚气行为；未指定释放效果时使用默认范围爆发。
	/// </summary>
	/// <param name="releaseEffect">蓄力达标后的释放逻辑；装备词条等可注入不同实现。</param>
	public ChargeAction(IChargeReleaseEffect? releaseEffect = null)
	{
		_releaseEffect = releaseEffect ?? new AoEChargeReleaseEffect();
	}

	/// <inheritdoc />
	public PlayerActionId Id => PlayerActionId.Charge;

	/// <inheritdoc />
	public bool IsActive => _phase is Phase.WindUp or Phase.Charging or Phase.Release;

	/// <inheritdoc />
	public float CooldownRemaining => _cooldown;

	/// <inheritdoc />
	public bool BlocksMovement => _phase is Phase.WindUp or Phase.Release;

	/// <inheritdoc />
	public bool BlocksOtherActions => IsActive;

	/// <inheritdoc />
	public bool GrantsInvincibility => false;

	/// <inheritdoc />
	public float MoveSpeedMultiplier => _phase == Phase.Charging ? ParamsConfig.ChargeMoveSpeedMul : 0f;

	/// <inheritdoc />
	public bool CanStart(PlayerActionContext ctx) =>
		_phase == Phase.Idle && _cooldown <= 0f;

	/// <inheritdoc />
	public void Enter(PlayerActionContext ctx)
	{
		_phase = Phase.WindUp;
		_timer = ParamsConfig.ChargeWindupTime;
		_chargePercent = 0f;
		ctx.Player.SetActionVisual(ParamsConfig.ColorChargeEnter);
		ctx.Controller.NotifyActionStarted(Id);
	}

	/// <inheritdoc />
	public void Update(PlayerActionContext ctx, double delta)
	{
		switch (_phase)
		{
			case Phase.WindUp:
				UpdateWindUp(ctx, delta);
				break;
			case Phase.Charging:
				UpdateCharging(ctx, delta);
				break;
			case Phase.Release:
				UpdateRelease(ctx, delta);
				break;
		}
	}

	/// <inheritdoc />
	public void Exit(PlayerActionContext ctx)
	{
		if (_phase is Phase.WindUp or Phase.Charging)
		{
			Cancel(ctx);
		}
	}

	/// <inheritdoc />
	public void TickInactive(double delta)
	{
		if (_phase != Phase.Cooldown)
		{
			return;
		}

		_cooldown -= (float)delta;
		if (_cooldown <= 0f)
		{
			_cooldown = 0f;
			_phase = Phase.Idle;
		}
	}

	/// <summary>
	/// 聚气键松开时由 Controller 调用；仅在蓄力阶段有效，触发释放判定。
	/// </summary>
	public void OnInputReleased(PlayerActionContext ctx)
	{
		if (_phase == Phase.Charging)
		{
			BeginRelease(ctx);
		}
	}

	/// <summary>
	/// 玩家受击时由 Controller 调用；前摇或蓄力中取消聚气，不进入冷却（除非已释放）。
	/// </summary>
	public void OnInterrupted(PlayerActionContext ctx)
	{
		if (_phase is Phase.WindUp or Phase.Charging)
		{
			Cancel(ctx);
		}
	}

	/// <summary>前摇倒计时，结束后进入蓄力阶段。</summary>
	private void UpdateWindUp(PlayerActionContext ctx, double delta)
	{
		_timer -= (float)delta;
		if (_timer > 0f)
		{
			return;
		}

		_phase = Phase.Charging;
		_timer = 0f;
	}

	/// <summary>
	/// 蓄力中累加 charge 按住时间；松键、满蓄或键未按住时进入释放。
	/// 视觉随蓄力比例放大。
	/// </summary>
	private void UpdateCharging(PlayerActionContext ctx, double delta)
	{
		if (!Input.IsActionPressed("charge"))
		{
			BeginRelease(ctx);
			return;
		}

		_timer += (float)delta;
		_chargePercent = Mathf.Clamp(_timer / ParamsConfig.ChargeMaxTime, 0f, 1f);
		ctx.Player.SetActionVisual(
			ParamsConfig.ColorChargeCharging,
			1f + _chargePercent * ParamsConfig.ChargeVisualScaleMax);

		if (_chargePercent >= 1f)
		{
			BeginRelease(ctx);
		}
	}

	/// <summary>
	/// 判定蓄力是否达到最低释放阈值；不足则取消，否则进入释放并委托 <see cref="IChargeReleaseEffect"/>。
	/// </summary>
	private void BeginRelease(PlayerActionContext ctx)
	{
		if (_chargePercent < ParamsConfig.ChargeMinReleasePercent)
		{
			Cancel(ctx);
			return;
		}

		_phase = Phase.Release;
		_timer = ParamsConfig.ChargeReleaseTime;
		_releaseEffect.Execute(new ChargeReleaseContext
		{
			Action = ctx,
			ChargePercent = _chargePercent,
		});
		ctx.Player.FlashActionRelease(ParamsConfig.ColorChargeRelease);
	}

	/// <summary>释放阶段倒计时，结束后进入冷却。</summary>
	private void UpdateRelease(PlayerActionContext ctx, double delta)
	{
		_timer -= (float)delta;
		if (_timer <= 0f)
		{
			Finish(ctx);
		}
	}

	/// <summary>蓄力不足或被打断：重置视觉并回到 Idle，不消耗冷却。</summary>
	private void Cancel(PlayerActionContext ctx)
	{
		ctx.Player.ResetActionVisual();
		_phase = Phase.Idle;
		_chargePercent = 0f;
		ctx.Controller.NotifyActionEnded(Id);
	}

	/// <summary>正常释放结束：进入冷却。</summary>
	private void Finish(PlayerActionContext ctx)
	{
		ctx.Player.ResetActionVisual();
		_phase = Phase.Cooldown;
		_cooldown = ParamsConfig.ChargeCooldown;
		_chargePercent = 0f;
		ctx.Controller.NotifyActionEnded(Id);
	}
}
