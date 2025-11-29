
using users_service.domain.Enumerables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.domain.ValueObjects
{
    /// Value Object que representa el rol de un usuario.
    public class Role
    {
        public string Valor { get; }

        private Role(string valor)
        {
            Valor = valor;
        }

        /// Crea una nueva instancia de DuracionEvento.
        /// Valores administrados: Usuario, Organizador, Administrador, Soporte.
        public static Role CrearDesdeTexto(string texto)
        {
            if (!Enum.TryParse<Rol>(texto, out var rol))
                throw new ArgumentException($"Rol inválido: {texto}");

            return new Role(texto);
        }
        public override string ToString() => Valor;
    }
}
