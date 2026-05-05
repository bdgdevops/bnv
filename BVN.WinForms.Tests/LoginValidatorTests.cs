using System;
using System.Collections.Generic;
using System.Text;
using BVN.WinForms.Services;

namespace BVN.WinForms.Tests
{
    public class LoginValidatorTests
    {
        [Fact(DisplayName = "Validate: usuario y contraseña correctos")]
        public void Validate_ValidCredentials_ReturnsSuccess()
        {
            var result = LoginValidator.Validate("admin", "password123");
            Assert.True(result.IsValid);
            Assert.Equal(string.Empty, result.ErrorMessage);
        }

        [Fact(DisplayName = "Validate: usuario demasiado corto → inválido con mención de longitud")]
        public void Validate_UsernameTooShort_ReturnsInvalid()
        {
            // Mínimo es 3 → "ab" es inválido
            var result = LoginValidator.Validate("ab", "1234");

            Assert.False(result.IsValid);
            Assert.Contains(LoginValidator.MinUsernameLength.ToString(), result.ErrorMessage);
        }

        [Fact(DisplayName = "Validate: contraseña solo espacios → inválido")]
        public void Validate_WhitespacePassword_ReturnsInvalid()
        {
            var result = LoginValidator.Validate("admin", "           ");

            Assert.False(result.IsValid);
            Assert.Contains("Contraseña", result.ErrorMessage);
        }

    }
}
