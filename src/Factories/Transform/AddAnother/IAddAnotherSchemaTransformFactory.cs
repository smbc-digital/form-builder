using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Factories.Transform.AddAnother
{
    public interface IAddAnotherSchemaTransformFactory
    {
        FormSchema Transform(FormSchema formSchema);
    }
}
