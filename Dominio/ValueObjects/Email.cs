using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Domain.ValueObjects
{
    public record Email
    {
        public static readonly Regex _regex = new Regex(
            @"^[a-zA-Z0-9]+(?:\.[a-zA-Z0-9]+)*@[a-zA-Z0-9]+(?:\.[a-zA-Z0-9]+)*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Value { get; private set; }

        public Email(string value)
        {
            Value = value;
        }

        public static Email Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El email no puede estar vacio.");

            if (!_regex.IsMatch(value))
                throw new ArgumentException("Formato Invalido.");

            return new Email(value);
        }

        public override string ToString() => Value;

        public override int GetHashCode() =>
            Value.ToLowerInvariant().GetHashCode();
    }
}