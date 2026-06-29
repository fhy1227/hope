using Godot;

namespace Hope.Components;

/// <summary>
/// 世界空间浮动伤害数字：上浮、淡出后自毁。
/// </summary>
public partial class DamageNumber : Node2D
{
    private const float Duration = 0.75f;
    private const float RiseSpeed = 55f;
    private const int FontSize = 16;

    private string _text = "";
    private Color _color;
    private Vector2 _velocity;
    private float _elapsed;

    public void Setup(int amount, bool isPlayer)
    {
        _text = amount.ToString();
        _color = isPlayer
            ? new Color(1f, 0.4f, 0.4f)
            : new Color(1f, 0.92f, 0.45f);

        Position = new Vector2((float)GD.RandRange(-10f, 10f), (float)GD.RandRange(-22f, -16f));
        _velocity = new Vector2((float)GD.RandRange(-18f, 18f), -RiseSpeed);
        Scale = Vector2.One * 0.55f;

        var tween = CreateTween();
        tween.TweenProperty(this, "scale", Vector2.One, 0.1)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }

    public override void _Process(double delta)
    {
        var dt = (float)delta;
        _elapsed += dt;
        Position += _velocity * dt;
        _velocity *= 0.92f;
        QueueRedraw();

        if (_elapsed >= Duration)
        {
            QueueFree();
        }
    }

    public override void _Draw()
    {
        var font = ThemeDB.FallbackFont;
        var size = font.GetStringSize(_text, HorizontalAlignment.Center, -1, FontSize);
        var alpha = Mathf.Clamp(1f - _elapsed / Duration, 0f, 1f);
        var color = new Color(_color.R, _color.G, _color.B, alpha);
        DrawString(font, new Vector2(-size.X / 2f, size.Y / 4f), _text, HorizontalAlignment.Center, -1, FontSize, color);
    }
}
