using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.domain.Exceptions
{
    /// Exception personalizada para errores relacionados con datos invalidos de un usuario.
    public class UsuarioDTOException : Exception
    {
        public UsuarioDTOException(Exception innerException)
        : base($"Los datos ingresados no son válidos. {innerException.Message}", innerException)
        {
        }

    }
}
