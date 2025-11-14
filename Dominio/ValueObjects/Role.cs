
using Domain.Enumerables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects
{

    public class Role
    {
        public string Valor { get; }

        private Role(string valor)
        {
            Valor = valor;
        }

        public static Role CrearDesdeTexto(string texto)
        {
            if (!Enum.TryParse<Rol>(texto, out var rol))
                throw new ArgumentException($"Rol inválido: {texto}");

            return new Role(texto);
        }
        public override string ToString() => Valor;
    }
}
