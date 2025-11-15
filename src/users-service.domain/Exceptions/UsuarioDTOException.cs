using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions
{
    public class UsuarioDTOException : Exception
    {
        public UsuarioDTOException(Exception innerException)
        : base($"Los datos ingresados no son válidos. Detalle: {innerException.Message}", innerException)
        {
        }

    }
}
