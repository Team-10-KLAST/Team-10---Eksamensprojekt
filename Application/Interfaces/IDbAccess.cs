using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    // Dette interface definerer de nødvendige dataadgangsmetoder
    // og abstraherer væk fra de konkrete ADO.NET-klasser.
    public interface IDbAccess
    {
        // Metode til at udføre en SELECT (læse data)
        IDataReader ExecuteReader(string storedProcedureName, Dictionary<string, object> parameters = null);

        // Metode til at udføre en INSERT/SELECT SCOPE_IDENTITY() (tilføje og få ID)
        object ExecuteScalar(string storedProcedureName, Dictionary<string, object> parameters);

        // Metode til at udføre UPDATE/DELETE (ændre data)
        int ExecuteNonQuery(string storedProcedureName, Dictionary<string, object> parameters);
    }
}
