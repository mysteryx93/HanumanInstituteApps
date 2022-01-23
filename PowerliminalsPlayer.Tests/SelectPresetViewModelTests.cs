using HanumanInstitute.Common.Services;
using HanumanInstitute.PowerliminalsPlayer.Models;
using HanumanInstitute.PowerliminalsPlayer.ViewModels;
using Moq;
using Xunit;

// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.PowerliminalsPlayer.Tests;

public class SelectPresetViewModelTests
{
    protected AppSettingsData Settings { get; set; } = new();
    
    protected ISettingsProvider<AppSettingsData> SettingsProvider => _settingsProvider ??= 
        Mock.Of<ISettingsProvider<AppSettingsData>>(x => x.Value == Settings);
    private ISettingsProvider<AppSettingsData> _settingsProvider;

    protected SelectPresetViewModel Model => _model ??= new SelectPresetViewModel(SettingsProvider);
    private SelectPresetViewModel _model;

    protected void AddPreset(string name = Preset1) => Settings.Presets.Add(new PresetItem(Preset1));
    private const string Preset1 = "Preset1"; 
    
    [Fact]
    public void Load_ModeLoadEmpty_PresetNameEmpty()
    {
        Model.Load(false);
        
        Assert.Null(Model.SelectedItem);
        Assert.Empty(Model.PresetName);
    }
    
    [Fact]
    public void Load_ModeLoadWithData_SelectFirst()
    {
        AddPreset();
        
        Model.Load(false);
        
        Assert.NotNull(Model.SelectedItem);
        Assert.Equal(Preset1, Model.PresetName);
    }
    
    [Fact]
    public void Load_ModeSaveEmpty_DefaultPresetName()
    {
        Model.Load(true);
        
        Assert.Null(Model.SelectedItem);
        Assert.Equal("New Preset", Model.PresetName);
    }

    [Fact]
    public void Load_ModeSaveWithData_DefaultPresetName()
    {
        AddPreset();
        
        Model.Load(true);
        
        Assert.Null(Model.SelectedItem);
        Assert.Equal("New Preset", Model.PresetName);
    }

    [Fact]
    public void CancelCommand_NoSelection_InvokeRequestClose()
    {
        var called = 0;
        Model.RequestClose += (_, _) => called++; 
        
        Model.Load(false);
        Model.CancelCommand.Execute(null);
        
        Assert.Equal(1, called);
    }

    [Fact]
    public void CancelCommand_NoSelection_DialogResultFalse()
    {
        Model.Load(false);
        Model.CancelCommand.Execute(null);
        
        Assert.False(Model.DialogResult);
    }
    
    [Fact]
    public void ConfirmCommand_NoSelection_DoNotInvokeRequestClose()
    {
        var called = 0;
        Model.RequestClose += (_, _) => called++; 

        Model.Load(false);
        Model.ConfirmCommand.Execute(null);
        
        Assert.Equal(0, called);
    }

    [Fact]
    public void ConfirmCommand_LoadSelection_ReturnsSelection()
    {
        AddPreset();

        Model.Load(false);
        Model.ConfirmCommand.Execute(null);
        
        Assert.True(Model.DialogResult);
        Assert.NotNull(Model.SelectedItem);
    }
    
    [Fact]
    public void ConfirmCommand_SavePreset_ReturnsPresetName()
    {
        AddPreset();
        var name = "Saving";

        Model.Load(true);
        Model.PresetName = name;
        Model.ConfirmCommand.Execute(null);
        
        Assert.True(Model.DialogResult);
        Assert.Equal(name, Model.PresetName);
    }
}
