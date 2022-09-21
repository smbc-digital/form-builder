namespace form_builder.Mappers.Structure
{
    public interface IStructureMapper
    {
        Task<object> CreateBaseFormDataStructure(string form);
    }
}
