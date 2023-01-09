using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace SWE_A1Grandner_MTCG.Exceptions
{
    internal class DatabaseCorruptedException : NpgsqlException
    {
    }
}
