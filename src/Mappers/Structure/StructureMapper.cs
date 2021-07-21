using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Factories.Schema;
using form_builder.Models.Elements;
using StockportGovUK.NetStandard.Models.FileManagement;

namespace form_builder.Mappers.Structure
{
    public class StructureMapper : IStructureMapper
    {
        private readonly ISchemaFactory _schemaFactory;

        public StructureMapper(ISchemaFactory schemaFactory) => _schemaFactory = schemaFactory;

        public async Task<object> CreateBaseFormDataStructure(string form)
        {
            var formSchema = await _schemaFactory.Build(form);
            var dataStructure = new ExpandoObject() as IDictionary<string, dynamic>;
            var elements = formSchema.Pages.SelectMany(_ => _.ValidatableElements).ToList();

            dataStructure = elements
                .Aggregate(dataStructure, (current, element) => 
                    RecursiveObjectCreate(string.IsNullOrEmpty(element.Properties.TargetMapping) ? element.Properties.QuestionId : element.Properties.TargetMapping, element, current));

            foreach (var page in formSchema.Pages)
            {
                if (page.HasIncomingValues)
                {
                    dataStructure = page.IncomingValues
                        .Aggregate(dataStructure, (current, incomingValue) => 
                            RecursiveObjectCreate(incomingValue.QuestionId, null, current));
                }

                if (page.HasPageActions)
                {
                    dataStructure = page.PageActions
                        .Where(_ => _.Type.Equals(EActionType.RetrieveExternalData))
                        .Aggregate(dataStructure, (current, action) => 
                            RecursiveObjectCreate(action.Properties.TargetQuestionId, null, current));
                }
            }

            dataStructure.Add(formSchema.GenerateReferenceNumber ? formSchema.GeneratedReferenceNumberMapping : "CaseReference", string.Empty.GetType().ConvertTypeToFormattedString());

            if (formSchema.Pages.Any(_ => _.Behaviours is not null && _.Behaviours.Any(_ => _.BehaviourType.Equals(EBehaviourType.SubmitAndPay))))
                dataStructure.Add("PaymentAmount", string.Empty.GetType().ConvertTypeToFormattedString());

            return dataStructure;
        }

        private IDictionary<string, dynamic> RecursiveObjectCreate(string targetMapping, IElement element, IDictionary<string, dynamic> dataStructure)
        {
            var splitTargets = targetMapping.Split(".");

            if (splitTargets.Length.Equals(1))
            {
                if (element is { Type: EElementType.AddAnother })
                    return CreateStructureForAddAnother(splitTargets[0], element, dataStructure);

                if (dataStructure.ContainsKey(splitTargets[0]))
                    return dataStructure;

                dataStructure.Add(splitTargets[0], GetDataType(element));
                return dataStructure;
            }

            object subObject;
            if (!dataStructure.TryGetValue(splitTargets[0], out subObject))
                subObject = new ExpandoObject();

            subObject = RecursiveObjectCreate(targetMapping.Replace($"{splitTargets[0]}.", ""), element, subObject as IDictionary<string, dynamic>);

            dataStructure.Remove(splitTargets[0]);
            dataStructure.Add(splitTargets[0], subObject);

            return dataStructure;
        }

        private IDictionary<string, dynamic> CreateStructureForAddAnother(string target, IElement element, IDictionary<string, dynamic> dataStructure)
        {
            var addAnotherStructure = new List<IDictionary<string, dynamic>>();
            var fieldsetStructure = new Dictionary<string, dynamic>();

            fieldsetStructure = element.Properties.Elements
                .Aggregate(fieldsetStructure, (current, nestedElement) => 
                    (Dictionary<string, dynamic>) RecursiveObjectCreate(string.IsNullOrEmpty(nestedElement.Properties.TargetMapping)
                        ? nestedElement.Properties.QuestionId
                        : nestedElement.Properties.TargetMapping, nestedElement, current));

            addAnotherStructure.Add(fieldsetStructure);
            dataStructure.Add(target, addAnotherStructure);

            return dataStructure;
        }

        private dynamic GetDataType(IElement element)
        {
            if (element is null)
                return "Dynamic";

            PropertyInfo[] properties;

            switch (element.Type)
            {
                case EElementType.Checkbox:
                    return new List<string> { string.Empty.GetType().ConvertTypeToFormattedString() };

                case EElementType.DateInput:
                case EElementType.DatePicker:
                    return new DateTime().GetType().ConvertTypeToFormattedString();

                case EElementType.Street:
                case EElementType.Address:
                    var address = new StockportGovUK.NetStandard.Models.Addresses.Address();
                    properties = address.GetType().GetProperties();
                    return GeneratePropertyDictionary(properties);


                case EElementType.Organisation:
                    var organisation = new StockportGovUK.NetStandard.Models.Verint.Organisation();
                    properties = organisation.GetType().GetProperties();
                    return GeneratePropertyDictionary(properties);

                case EElementType.Booking:
                    var booking = new StockportGovUK.NetStandard.Models.Booking.Booking();
                    properties = booking.GetType().GetProperties();
                    return GeneratePropertyDictionary(properties);

                case EElementType.Map:
                    return new ExpandoObject();

                case EElementType.MultipleFileUpload:
                case EElementType.FileUpload:
                    var file = new File();
                    var fileArray = new List<Dictionary<string, dynamic>>();
                    fileArray.Add(GeneratePropertyDictionary(file.GetType().GetProperties()));
                    return fileArray;

                default:
                    return string.Empty.GetType().ConvertTypeToFormattedString();
            }
        }

        private Dictionary<string, dynamic> GeneratePropertyDictionary(PropertyInfo[] props)
        {
            var propertyDictionary = new Dictionary<string, dynamic>();

            foreach (var prop in props)
            {
                if (prop.PropertyType.ToString().StartsWith("StockportGovUK.NetStandard") && !prop.PropertyType.ToString().EndsWith("[]"))
                {
                    var typeInstance = Activator.CreateInstance(prop.PropertyType);
                    var nestedProperties = typeInstance.GetType().GetProperties();
                    var nestedPropertyDictionary = new Dictionary<string, dynamic>();
                    foreach (var nestedProp in nestedProperties)
                    {
                        nestedPropertyDictionary.Add(nestedProp.Name, nestedProp.PropertyType.ConvertTypeToFormattedString());
                    }

                    propertyDictionary.Add(prop.Name, nestedPropertyDictionary);
                }
                else if (prop.PropertyType.ToString().StartsWith("StockportGovUK.NetStandard") && prop.PropertyType.ToString().EndsWith("[]"))
                {
                    var typeArray = new object[1];
                    typeArray[0] = Activator.CreateInstance(prop.PropertyType.GetElementType());
                    var arrayDictionary = new List<Dictionary<string, dynamic>>();
                    foreach (var type in typeArray)
                    {
                        var nestedProperties = type.GetType().GetProperties();
                        var nestedPropertyDictionary = new Dictionary<string, dynamic>();
                        foreach (var nestedProp in nestedProperties)
                        {
                            nestedPropertyDictionary.Add(nestedProp.Name, nestedProp.PropertyType.ConvertTypeToFormattedString());
                        }

                        arrayDictionary.Add(nestedPropertyDictionary);
                    }

                    propertyDictionary.Add(prop.Name, arrayDictionary);
                }
                else
                {
                    propertyDictionary.Add(prop.Name, prop.PropertyType.ConvertTypeToFormattedString());
                }
            }

            return propertyDictionary;
        }
    }
}
