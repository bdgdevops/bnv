// Services/LoginValidator.cs
namespace BVN.WinForms.Services;

/// <summary>
/// Resultado de una validación de credenciales de login.
/// </summary>
public record LoginValidationResult(bool IsValid, string ErrorMessage)
{
    public static LoginValidationResult Ok()    => new(true,  string.Empty);
    public static LoginValidationResult Fail(string msg) => new(false, msg);
}

/// <summary>
/// Lógica pura de validación del formulario de login.
/// Desacoplada de WinForms para permitir pruebas unitarias.
/// </summary>
public static class LoginValidator
{
    public const int MinUsernameLength = 5;
    public const int MaxUsernameLength = 10;

    public const int MinPasswordLength = 4;
    public const int MaxPasswordLength = 20;

    /// <summary>
    /// Valida las credenciales ingresadas en el formulario de login.
    /// </summary>
    /// <param name="username">Texto del campo usuario.</param>
    /// <param name="password">Texto del campo contraseña.</param>
    /// <returns><see cref="LoginValidationResult"/> con el resultado y mensaje de error si aplica.</returns>
    public static LoginValidationResult Validate(string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return LoginValidationResult.Fail("El campo Usuario es obligatorio.");

        if (username.Trim().Length < MinUsernameLength)
            return LoginValidationResult.Fail($"El usuario debe tener al menos {MinUsernameLength} caracteres.");

        if (username.Trim().Length > MaxUsernameLength)
            return LoginValidationResult.Fail($"El usuario no puede superar {MaxUsernameLength} caracteres.");

        if (string.IsNullOrWhiteSpace(password))
            return LoginValidationResult.Fail("El campo Contraseña es obligatorio.");

        if (password.Trim().Length < MinPasswordLength)
            return LoginValidationResult.Fail($"La contraseña debe tener al menos {MinPasswordLength} caracteres.");

        if (password.Trim().Length > MaxPasswordLength)
            return LoginValidationResult.Fail($"La contraseña no puede superar {MaxPasswordLength} caracteres.");

        return LoginValidationResult.Ok();
    }
}
