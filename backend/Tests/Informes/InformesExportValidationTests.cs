using API.DTO.Request.Informes;
using FluentValidation;

namespace API.Tests.Informes;

/// <summary>
/// Minimal covering tests for Informes export validation (R06) to satisfy sdd-verify UNTESTED gate.
/// Covers cantidad OOR 99 →400, invalid preset, invalid formato, and valid cases.
/// </summary>
public class InformesExportValidationTests
{
    private readonly IValidator<ExportInformesQuery> _validator = new ExportInformesQueryValidator();
    private readonly IValidator<InformesQuery> _informesValidator = new InformesQueryValidator();

    [Fact]
    public async Task ExportValidator_Cantidad99_ShouldFail400()
    {
        var query = new ExportInformesQuery(
            Tipo: "productos-top",
            Formato: "csv",
            Preset: "mes",
            FechaDesde: null,
            FechaHasta: null,
            Cantidad: 99);

        var result = await _validator.ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Cantidad" || e.ErrorMessage.Contains("cantidad", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExportValidator_InvalidPreset_ShouldFail400()
    {
        var query = new ExportInformesQuery(
            Tipo: "ventas-resumen",
            Formato: "pdf",
            Preset: "custom-invalid",
            FechaDesde: null,
            FechaHasta: null,
            Cantidad: null);

        var result = await _validator.ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.ToLower().Contains("preset") || e.ErrorMessage.Contains("Preset", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExportValidator_InvalidFormato_ShouldFail400()
    {
        var query = new ExportInformesQuery(
            Tipo: "ventas-resumen",
            Formato: "xlsx",
            Preset: "mes",
            FechaDesde: null,
            FechaHasta: null,
            Cantidad: null);

        var result = await _validator.ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.ToLower().Contains("formato") || e.ErrorMessage.Contains("Formato", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExportValidator_ValidProductosTopCantidad10_ShouldPass()
    {
        var query = new ExportInformesQuery(
            Tipo: "productos-top",
            Formato: "csv",
            Preset: "mes",
            FechaDesde: null,
            FechaHasta: null,
            Cantidad: 10);

        var result = await _validator.ValidateAsync(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task InformesValidator_PresetInvalid_ShouldFail400()
    {
        var query = new InformesQuery(
            Preset: "invalido",
            FechaDesde: null,
            FechaHasta: null,
            Cantidad: null);

        var result = await _informesValidator.ValidateAsync(query);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task InformesValidator_CantidadOOR99_ShouldFail400()
    {
        var query = new InformesQuery(
            Preset: "mes",
            FechaDesde: null,
            FechaHasta: null,
            Cantidad: 99);

        var result = await _informesValidator.ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("1 y 50"));
    }

    [Fact]
    public async Task InformesValidator_ValidPresetMes_ShouldPass()
    {
        var query = new InformesQuery(
            Preset: "mes",
            FechaDesde: null,
            FechaHasta: null,
            Cantidad: 10);

        var result = await _informesValidator.ValidateAsync(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ExportValidator_CantidadOnNonProductosTop_ShouldFail400()
    {
        var query = new ExportInformesQuery(
            Tipo: "flujo-caja",
            Formato: "csv",
            Preset: "mes",
            FechaDesde: null,
            FechaHasta: null,
            Cantidad: 10);

        var result = await _validator.ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("solo es válido para tipo productos-top"));
    }
}
