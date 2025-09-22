using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapLibrary.DAL.Events
{
    public record ObjectRemoved(Object @object) : EventBase(@object);
}
