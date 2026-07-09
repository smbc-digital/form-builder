namespace form_builder.Helpers.Submit;

public interface ISubmitHelper
{
    Task ConfirmBookings(MappingEntity mappingEntity, string environmentName, string caseReference);
}
