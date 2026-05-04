// BVN.WinForms.Tests/LoginValidatorTests.cs
using BVN.WinForms.Services;

namespace BVN.WinForms.Tests;

/// <summary>
/// Pruebas unitarias para <see cref="LoginValidator"/>.
/// Verifica la validación de campos vacíos, nulos y con longitud insuficiente.
/// </summary>
public class LoginValidatorTests
{
    // ─── Casos VÁLIDOS ─────────────────────────────────────────────────────────

    [Fact(DisplayName = "Validate: usuario y contraseña correctos → resultado válido")]
    public void Validate_ValidCredentials_ReturnsSuccess()
    {
        var result = LoginValidator.Validate("admin", "1234");

        Assert.Equal(true, result.IsValid);
        Assert.Equal(string.Empty, result.ErrorMessage);
    }

    // ─── Usuario vacío / nulo ─────────────────────────────────────────────────

    [Fact(DisplayName = "Validate: usuario vacío → inválido con mensaje de error")]
    public void Validate_EmptyUsername_ReturnsInvalid()
    {
        var result = LoginValidator.Validate("", "1234");

        Assert.False(result.IsValid);
        Assert.Contains("Usuario", result.ErrorMessage);
    }

    [Fact(DisplayName = "Validate: usuario solo espacios → inválido")]
    public void Validate_WhitespaceUsername_ReturnsInvalid()
    {
        var result = LoginValidator.Validate("   ", "1234");

        Assert.False(result.IsValid);
        Assert.Contains("Usuario", result.ErrorMessage);
    }

    [Fact(DisplayName = "Validate: usuario nulo → inválido")]
    public void Validate_NullUsername_ReturnsInvalid()
    {
        var result = LoginValidator.Validate(null, "1234");

        Assert.False(result.IsValid);
        Assert.Contains("Usuario", result.ErrorMessage);
    }

    [Fact(DisplayName = "Validate: usuario demasiado corto → inválido con mención de longitud")]
    public void Validate_UsernameTooShort_ReturnsInvalid()
    {
        // Mínimo es 3 → "ab" es inválido
        var result = LoginValidator.Validate("ab", "1234");

        Assert.False(result.IsValid);
        Assert.Contains(LoginValidator.MinUsernameLength.ToString(), result.ErrorMessage);
    }

    // ─── Contraseña vacía / nula ───────────────────────────────────────────────

    [Fact(DisplayName = "Validate: contraseña vacía → inválido con mensaje de error")]
    public void Validate_EmptyPassword_ReturnsInvalid()
    {
        var result = LoginValidator.Validate("admin", "");

        Assert.False(result.IsValid);
        Assert.Contains("Contraseña", result.ErrorMessage);
    }

    [Fact(DisplayName = "Validate: contraseña solo espacios → inválido")]
    public void Validate_WhitespacePassword_ReturnsInvalid()
    {
        var result = LoginValidator.Validate("admin", "    ");

        Assert.False(result.IsValid);
        Assert.Contains("Contraseña", result.ErrorMessage);
    }

    [Fact(DisplayName = "Validate: contraseña nula → inválido")]
    public void Validate_NullPassword_ReturnsInvalid()
    {
        var result = LoginValidator.Validate("admin", null);

        Assert.False(result.IsValid);
        Assert.Contains("Contraseña", result.ErrorMessage);
    }

    [Fact(DisplayName = "Validate: contraseña demasiado corta → inválido con mención de longitud")]
    public void Validate_PasswordTooShort_ReturnsInvalid()
    {
        // Mínimo es 4 → "abc" es inválido
        var result = LoginValidator.Validate("admin", "abc");

        Assert.False(result.IsValid);
        Assert.Contains(LoginValidator.MinPasswordLength.ToString(), result.ErrorMessage);
    }

    // ─── Ambos vacíos ─────────────────────────────────────────────────────────

    [Fact(DisplayName = "Validate: ambos campos vacíos → inválido (primero falla el usuario)")]
    public void Validate_BothEmpty_ReturnsInvalid_WithUsernameError()
    {
        var result = LoginValidator.Validate("", "");

        Assert.False(result.IsValid);
        // La validación es secuencial: usuario se evalúa primero
        Assert.Contains("Usuario", result.ErrorMessage);
    }

    // ─── Theory: tabla de casos inválidos ─────────────────────────────────────

    [Theory(DisplayName = "Validate: múltiples combinaciones inválidas")]
    [InlineData(null,    "pass1234",  "Usuario")]
    [InlineData("",     "pass1234",  "Usuario")]
    [InlineData("ab",   "pass1234",  "3")]          // longitud mínima usuario
    [InlineData("admin", null,       "Contraseña")]
    [InlineData("admin", "",         "Contraseña")]
    [InlineData("admin", "abc",      "4")]           // longitud mínima contraseña
    public void Validate_InvalidInputs_ReturnFalseWithExpectedMessage(
        string? username, string? password, string expectedFragment)
    {
        var result = LoginValidator.Validate(username, password);

        Assert.False(result.IsValid);
        Assert.Contains(expectedFragment, result.ErrorMessage);
    }

    // ─── Longitud MÁXIMA ──────────────────────────────────────────────────────

    [Fact(DisplayName = "Validate: usuario con exactamente MaxUsernameLength chars → válido")]
    public void Validate_UsernameAtMaxLength_IsValid()
    {
        // "aaaaaaaaaa" = exactamente 10 caracteres (límite justo)
        string username = new string('a', LoginValidator.MaxUsernameLength);

        var result = LoginValidator.Validate(username, "pass1234");

        Assert.True(result.IsValid, $"Se esperaba válido con {username.Length} chars, pero falló: {result.ErrorMessage}");
    }

    [Fact(DisplayName = "Validate: usuario con más de 10 chars → inválido")]
    public void Validate_UsernameLongerThan10_ReturnsInvalid()
    {
        // 11 caracteres → excede el límite de 10
        string username = new string('a', LoginValidator.MaxUsernameLength + 1);

        var result = LoginValidator.Validate(username, "pass1234");

        Assert.False(result.IsValid);
        Assert.Contains(LoginValidator.MaxUsernameLength.ToString(), result.ErrorMessage);
    }

    [Fact(DisplayName = "Validate: contraseña con exactamente MaxPasswordLength chars → válida")]
    public void Validate_PasswordAtMaxLength_IsValid()
    {
        // Exactamente 20 caracteres (límite justo)
        string password = new string('x', LoginValidator.MaxPasswordLength);

        var result = LoginValidator.Validate("admin", password);

        Assert.True(result.IsValid, $"Se esperaba válido con {password.Length} chars, pero falló: {result.ErrorMessage}");
    }

    [Fact(DisplayName = "Validate: contraseña con más de 20 chars → inválida")]
    public void Validate_PasswordLongerThan20_ReturnsInvalid()
    {
        // 21 caracteres → excede el límite de 20
        string password = new string('x', LoginValidator.MaxPasswordLength + 1);

        var result = LoginValidator.Validate("admin", password);

        Assert.False(result.IsValid);
        Assert.Contains(LoginValidator.MaxPasswordLength.ToString(), result.ErrorMessage);
    }

    [Theory(DisplayName = "Validate: longitudes exactamente en el límite superior")]
    [InlineData(10, 20, true)]   // justo en el límite → válido
    [InlineData(11, 20, false)]  // usuario excede por 1 → inválido
    [InlineData(10, 21, false)]  // contraseña excede por 1 → inválido
    [InlineData(11, 21, false)]  // ambos exceden → inválido (usuario primero)
    public void Validate_BoundaryMaxLengths(int usernameLen, int passwordLen, bool expectedValid)
    {
        string username = new string('u', usernameLen);
        string password = new string('p', passwordLen);

        var result = LoginValidator.Validate(username, password);

        Assert.Equal(expectedValid, result.IsValid);
    }
}
