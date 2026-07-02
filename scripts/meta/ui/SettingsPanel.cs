using Godot;
using Hope.Config;

namespace Hope.UI;

/// <summary>
/// 设置面板：主音量调节。
/// </summary>
public partial class SettingsPanel : PanelContainer
{
    private HSlider _volumeSlider = null!;
    private Label _volumeLabel = null!;

    public override void _Ready()
    {
        _volumeSlider = GetNode<HSlider>("%VolumeSlider");
        _volumeLabel = GetNode<Label>("%VolumeLabel");

        var busIndex = AudioServer.GetBusIndex(ParamsConfig.AudioMasterBus);
        var linear = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(busIndex));
        _volumeSlider.Value = linear;
        UpdateVolumeLabel(linear);

        _volumeSlider.ValueChanged += OnVolumeChanged;
        GetNode<Button>("%BackButton").Pressed += HideSettings;

        Visible = false;
    }

    public void ShowSettings()
    {
        Visible = true;
    }

    public void HideSettings()
    {
        Visible = false;
    }

    private void OnVolumeChanged(double value)
    {
        var linear = (float)value;
        var busIndex = AudioServer.GetBusIndex(ParamsConfig.AudioMasterBus);
        AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(linear));
        UpdateVolumeLabel(linear);
    }

    private void UpdateVolumeLabel(float linear)
    {
        _volumeLabel.Text = $"主音量 {Mathf.RoundToInt(linear * 100)}%";
    }
}
