using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Dataverse.Helper
{
    public static class FieldBuilderHelper
    {

        public static string BuildRelationshipField(string tableReferenced, Guid UuidReferenced) {
            return $"/{tableReferenced}({UuidReferenced})";
        }
       
    }
}
