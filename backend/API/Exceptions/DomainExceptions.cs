namespace API.Exceptions;

/// <summary>
/// 404 — entity not found (includes tenant isolation, does not leak existence).
/// Mapped by GlobalExceptionHandler to 404 ProblemDetails.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entity, object key)
        : base($"{entity} con clave '{key}' no fue encontrado.")
    {
        Entity = entity;
        Key = key;
    }

    public string Entity { get; }
    public object Key { get; }
}

/// <summary>
/// 422 — domain/business rule violation (stock below zero, etc).
/// Mapped by GlobalExceptionHandler to 422 ProblemDetails.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

/// <summary>
/// 422 — specific stock insufficient violation.
/// </summary>
public class StockInsuficienteException : DomainException
{
    public StockInsuficienteException(string producto)
        : base($"Stock insuficiente para el producto: {producto}.") { }

    public StockInsuficienteException(int stockAnterior, int delta, int stockNuevo)
        : base($"Stock insuficiente: StockActual={stockAnterior}, delta={delta} resultaria en {stockNuevo} (no se permite stock negativo).") { }
}

public class NegocioInactivoException : Exception
{
    public NegocioInactivoException()
        : base("El negocio se encuentra inactivo. Contacte al administrador.") { }
}
