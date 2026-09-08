using Shared.Domain.Entities;
using HotChocolate.Resolvers;
using Shared.Domain.Repositories;

namespace Cruises.Api.GraphQL.Types;

/// <summary>
/// Tipo GraphQL que configura la exposición de la entidad Ship en el esquema GraphQL.
/// Define los campos disponibles para consultas sobre barcos, incluyendo la resolución
/// de camarotes asociados a cada barco.
/// </summary>
public class ShipType : ObjectType<Ship>
{
    /// <summary>
    /// Configura los campos y resolvedores del tipo Ship en el esquema GraphQL.
    /// Establece que solo se exponen explícitamente los campos definidos y vincula
    /// el campo de camarotes con su resolvedores correspondiente.
    /// </summary>
    /// <param name="descriptor">Descriptor del tipo de objeto utilizado para configurar los campos disponibles.</param>
    protected override void Configure(IObjectTypeDescriptor<Ship> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Id).IsProjected(true);
        descriptor.Field(x => x.Code);
        descriptor.Field(x => x.Name);
        descriptor.Field(x => x.Description);
        descriptor.Field(x => x.Company);

        descriptor
            .Field("cabins")
            .ResolveWith<ShipResolvers>(r => r.GetCabins(default!, default!))
            .UseSorting();
    }
}

/// <summary>
/// Clase que contiene los resolvedores personalizados para la entidad Ship en GraphQL.
/// Proporciona métodos para resolver campos complejos que no se pueden mapear directamente
/// desde la entidad, como la obtención de camarotes asociados a un barco.
/// </summary>
public class ShipResolvers
{
    /// <summary>
    /// Resuelve los camarotes disponibles en un barco específico.
    /// Recupera todos los camarotes asociados al barco actual desde el repositorio de camarotes.
    /// </summary>
    /// <param name="ship">El barco padre del cual se desean obtener los camarotes.</param>
    /// <param name="repository">El repositorio de camarotes utilizado para acceder a los datos.</param>
    /// <returns>Una secuencia consultable de camarotes pertenecientes al barco especificado.</returns>
    public IQueryable<ShipCabin> GetCabins(
        [Parent] Ship ship,
        [Service] ICabinRepository repository)
    {
        return repository.GetShipCabins(ship.Id);
    }
}
