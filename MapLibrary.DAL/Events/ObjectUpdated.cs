using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapLibrary.DAL.Events
{
    public record ObjectUpdated(Object @object) : EventBase(@object);
}
