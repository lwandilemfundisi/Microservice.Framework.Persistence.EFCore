using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microservice.Framework.Persistence.EFCore.Attributes
{
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = true
        )]
    public class MapTo : Attribute
    {
        public MapTo(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
