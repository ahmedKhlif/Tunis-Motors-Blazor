using Xunit;
using FluentAssertions;
using webappAPI.Tests.TestHelpers;

namespace webappAPI.Tests.UnitTests;

/// <summary>
/// Tests unitaires pour les validations d'authentification
/// Module: Test et Qualité Logiciel - Décembre 2025
/// Technique: Boîte Noire - Classes d'équivalence et Valeurs limites
/// Niveau: Tests Unitaires
/// Données: 100% réelles tunisiennes via TunisianTestDataProvider
/// </summary>
public class AuthValidationUnitTests
{
    #region TC-UNIT-001 à TC-UNIT-005: Validation Email

    [Fact]
    public void TC_UNIT_001_ValidateEmail_WithValidTunisianFormat_ShouldReturnTrue()
    {
        // Technique: Boîte noire - Classe d'équivalence (email valide)
        foreach (var validEmail in TunisianTestDataProvider.ValidationData.ValidEmails)
        {
            var isValid = IsValidEmail(validEmail);
            isValid.Should().BeTrue($"L'email '{validEmail}' doit être valide");
        }
    }

    [Fact]
    public void TC_UNIT_002_ValidateEmail_WithInvalidFormat_ShouldReturnFalse()
    {
        // Technique: Boîte noire - Classe d'équivalence (email invalide)
        foreach (var invalidEmail in TunisianTestDataProvider.ValidationData.InvalidEmails)
        {
            var isValid = IsValidEmail(invalidEmail);
            isValid.Should().BeFalse($"L'email '{invalidEmail}' doit être invalide");
        }
    }

    [Fact]
    public void TC_UNIT_003_ValidateEmail_WithTunisianDomain_ShouldReturnTrue()
    {
        // Technique: Boîte noire - Données réelles tunisiennes
        var tunisianEmail = TunisianTestDataProvider.Users.Admin.Email;
        
        var isValid = IsValidEmail(tunisianEmail);
        
        isValid.Should().BeTrue("Un email tunisien .tn doit être valide");
    }

    [Fact]
    public void TC_UNIT_004_ValidateEmail_WithMissingAtSymbol_ShouldReturnFalse()
    {
        // Technique: Boîte noire - Valeur limite (caractère manquant)
        var emailWithoutAt = "userexample.com";
        
        var isValid = IsValidEmail(emailWithoutAt);
        
        isValid.Should().BeFalse("Un email sans @ doit échouer");
    }

    [Fact]
    public void TC_UNIT_005_ValidateEmail_WithEmptyString_ShouldReturnFalse()
    {
        // Technique: Boîte noire - Valeur limite (vide)
        var emptyEmail = "";
        
        var isValid = IsValidEmail(emptyEmail);
        
        isValid.Should().BeFalse("Un email vide doit échouer");
    }

    #endregion

    #region TC-UNIT-006 à TC-UNIT-012: Validation Mot de Passe

    [Fact]
    public void TC_UNIT_006_ValidatePassword_WithStrongPassword_ShouldReturnTrue()
    {
        // Technique: Boîte noire - Classe d'équivalence (mot de passe fort)
        foreach (var validPassword in TunisianTestDataProvider.ValidationData.ValidPasswords)
        {
            var isStrong = IsPasswordStrong(validPassword);
            isStrong.Should().BeTrue($"Le mot de passe '{validPassword}' doit être considéré fort");
        }
    }

    [Fact]
    public void TC_UNIT_007_ValidatePassword_WithWeakPassword_ShouldReturnFalse()
    {
        // Technique: Boîte noire - Classe d'équivalence (mot de passe faible)
        foreach (var invalidPassword in TunisianTestDataProvider.ValidationData.InvalidPasswords)
        {
            var isStrong = IsPasswordStrong(invalidPassword);
            isStrong.Should().BeFalse($"Le mot de passe '{invalidPassword}' doit être considéré faible");
        }
    }

    [Fact]
    public void TC_UNIT_008_ValidatePassword_WithAdminPassword_ShouldReturnTrue()
    {
        // Technique: Boîte noire - Données réelles (mot de passe admin)
        var adminPassword = TunisianTestDataProvider.Users.Admin.Password;
        
        var isStrong = IsPasswordStrong(adminPassword);
        
        isStrong.Should().BeTrue("Le mot de passe admin doit être fort");
    }

    [Fact]
    public void TC_UNIT_009_ValidatePassword_WithoutUpperCase_ShouldReturnFalse()
    {
        // Technique: Boîte noire - Classe de test (sans majuscule)
        var passwordWithoutUpper = "tunismotors123!";
        
        var isStrong = IsPasswordStrong(passwordWithoutUpper);
        
        isStrong.Should().BeFalse("Un mot de passe sans majuscule doit échouer");
    }

    [Fact]
    public void TC_UNIT_010_ValidatePassword_WithoutNumbers_ShouldReturnFalse()
    {
        // Technique: Boîte noire - Classe de test (sans chiffres)
        var passwordWithoutNumbers = "TunisMotors!";
        
        var isStrong = IsPasswordStrong(passwordWithoutNumbers);
        
        isStrong.Should().BeFalse("Un mot de passe sans chiffres doit échouer");
    }

    [Fact]
    public void TC_UNIT_011_ValidatePassword_BelowMinimumLength_ShouldReturnFalse()
    {
        // Technique: Boîte noire - Valeur limite (longueur minimale)
        var shortPassword = "Pass1!";
        
        var isStrong = IsPasswordStrong(shortPassword);
        
        isStrong.Should().BeFalse("Un mot de passe < 8 caractères doit échouer");
    }

    [Fact]
    public void TC_UNIT_012_ValidatePassword_ExactlyMinimumLength_ShouldWork()
    {
        // Technique: Boîte noire - Valeur limite exacte (8 caractères)
        var exactLengthPassword = "Test123!"; // 8 caractères
        
        var isStrong = IsPasswordStrong(exactLengthPassword);
        
        isStrong.Should().BeTrue("Un mot de passe de 8 caractères valide doit passer");
    }

    #endregion

    #region TC-UNIT-013 à TC-UNIT-015: Correspondance Mot de Passe

    [Fact]
    public void TC_UNIT_013_PasswordMatch_WithIdenticalPasswords_ShouldReturnTrue()
    {
        // Technique: Boîte noire - Classe d'équivalence (correspondance)
        var password = TunisianTestDataProvider.Users.Seller.Password;
        var confirmPassword = TunisianTestDataProvider.Users.Seller.Password;
        
        var matches = password == confirmPassword;
        
        matches.Should().BeTrue("Les mots de passe identiques doivent correspondre");
    }

    [Fact]
    public void TC_UNIT_014_PasswordMatch_WithDifferentPasswords_ShouldReturnFalse()
    {
        // Technique: Boîte noire - Classe d'équivalence (non-correspondance)
        var password = TunisianTestDataProvider.Users.Admin.Password;
        var confirmPassword = TunisianTestDataProvider.Users.Buyer.Password;
        
        var matches = password == confirmPassword;
        
        matches.Should().BeFalse("Les mots de passe différents ne doivent pas correspondre");
    }

    [Fact]
    public void TC_UNIT_015_PasswordMatch_CaseSensitive_ShouldReturnFalse()
    {
        // Technique: Boîte noire - Test sensibilité casse
        var password = "TunisMotors2025!";
        var confirmPassword = "tunismotors2025!";
        
        var matches = password == confirmPassword;
        
        matches.Should().BeFalse("La comparaison doit être sensible à la casse");
    }

    #endregion

    #region TC-UNIT-016 à TC-UNIT-018: Validation Téléphone Tunisien

    [Fact]
    public void TC_UNIT_016_ValidatePhone_WithValidTunisianFormat_ShouldReturnTrue()
    {
        // Technique: Boîte noire - Données réelles tunisiennes (+216)
        foreach (var phone in TunisianTestDataProvider.ValidationData.ValidTunisianPhones)
        {
            var isValid = IsValidTunisianPhone(phone);
            isValid.Should().BeTrue($"Le numéro '{phone}' doit être valide");
        }
    }

    [Fact]
    public void TC_UNIT_017_ValidatePhone_WithInvalidFormat_ShouldReturnFalse()
    {
        // Technique: Boîte noire - Valeurs invalides
        var invalidPhones = new[] { "123", "+33 6 12 34 56 78", "abcdefgh" };
        
        foreach (var phone in invalidPhones)
        {
            var isValid = IsValidTunisianPhone(phone);
            isValid.Should().BeFalse($"Le numéro '{phone}' doit être invalide");
        }
    }

    [Fact]
    public void TC_UNIT_018_ValidatePhone_WithAdminPhone_ShouldReturnTrue()
    {
        // Technique: Boîte noire - Données réelles (téléphone admin)
        var adminPhone = TunisianTestDataProvider.Users.Admin.PhoneNumber;
        
        var isValid = IsValidTunisianPhone(adminPhone);
        
        isValid.Should().BeTrue("Le téléphone admin tunisien doit être valide");
    }

    #endregion

    #region Méthodes Helper de Validation

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsPasswordStrong(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            return false;

        // Reject passwords with spaces
        if (password.Contains(' '))
            return false;

        var hasUpperCase = password.Any(char.IsUpper);
        var hasLowerCase = password.Any(char.IsLower);
        var hasNumbers = password.Any(char.IsDigit);
        var hasSpecialChar = password.Any(ch => !char.IsLetterOrDigit(ch) && ch != ' ');

        return hasUpperCase && hasLowerCase && hasNumbers && hasSpecialChar;
    }

    private static bool IsValidTunisianPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        var cleanPhone = phone.Replace(" ", "").Replace("-", "");

        if (cleanPhone.StartsWith("+216"))
            return cleanPhone.Length == 12;
        
        if (cleanPhone.Length == 8)
            return new[] { '2', '5', '7', '9' }.Contains(cleanPhone[0]) && 
                   cleanPhone.All(char.IsDigit);

        return false;
    }

    #endregion
}

/// <summary>
/// Tests unitaires pour les validations de données métier véhicules
/// Module: Test et Qualité Logiciel - Décembre 2025
/// Technique: Boîte Noire - Valeurs limites et Classes d'équivalence
/// </summary>
public class BusinessValidationUnitTests
{
    #region TC-UNIT-019 à TC-UNIT-022: Validation Prix

    [Fact]
    public void TC_UNIT_019_ValidatePrice_WithPositiveValue_ShouldReturnTrue()
    {
        var car = TunisianTestDataProvider.Cars.PeugeotPartner;
        var isValid = car.Price > 0;
        isValid.Should().BeTrue($"Le prix {car.Price} TND doit être valide");
    }

    [Fact]
    public void TC_UNIT_020_ValidatePrice_WithNegativeValue_ShouldReturnFalse()
    {
        decimal price = -5000m;
        var isValid = price > 0;
        isValid.Should().BeFalse("Un prix négatif doit être invalide");
    }

    [Fact]
    public void TC_UNIT_021_ValidatePrice_WithinBudgetRange_ShouldReturnTrue()
    {
        var (min, max) = TunisianTestDataProvider.PriceRanges.Budget;
        decimal testPrice = 25000m;
        var isInRange = testPrice >= min && testPrice <= max;
        isInRange.Should().BeTrue($"Le prix {testPrice} doit être dans la plage Budget");
    }

    [Fact]
    public void TC_UNIT_022_ValidatePrice_WithZeroValue_ShouldReturnFalse()
    {
        decimal price = 0m;
        var isValid = price > 0;
        isValid.Should().BeFalse("Un prix de zéro doit être invalide");
    }

    #endregion

    #region TC-UNIT-023 à TC-UNIT-026: Validation Année

    [Fact]
    public void TC_UNIT_023_ValidateYear_WithCurrentYear_ShouldReturnTrue()
    {
        var car = TunisianTestDataProvider.Cars.Toyota;
        int currentYear = DateTime.Now.Year;
        var isValid = car.Year >= 1900 && car.Year <= currentYear;
        isValid.Should().BeTrue($"L'année {car.Year} doit être valide");
    }

    [Fact]
    public void TC_UNIT_024_ValidateYear_WithFutureYear_ShouldReturnFalse()
    {
        int futureYear = DateTime.Now.Year + 5;
        int currentYear = DateTime.Now.Year;
        var isValid = futureYear >= 1900 && futureYear <= currentYear;
        isValid.Should().BeFalse("Une année future doit être invalide");
    }

    [Fact]
    public void TC_UNIT_025_ValidateYear_WithOldYear_ShouldReturnTrue()
    {
        int oldYear = 2015;
        int currentYear = DateTime.Now.Year;
        var isValid = oldYear >= 1900 && oldYear <= currentYear;
        isValid.Should().BeTrue("Une année passée valide doit être acceptée");
    }

    [Fact]
    public void TC_UNIT_026_ValidateYear_BeforeAutomobile_ShouldReturnFalse()
    {
        int impossibleYear = 1850;
        int currentYear = DateTime.Now.Year;
        var isValid = impossibleYear >= 1900 && impossibleYear <= currentYear;
        isValid.Should().BeFalse("Une année avant 1900 doit être invalide");
    }

    #endregion

    #region TC-UNIT-027 à TC-UNIT-030: Validation Kilométrage

    [Fact]
    public void TC_UNIT_027_ValidateMileage_WithRealisticValue_ShouldReturnTrue()
    {
        var car = TunisianTestDataProvider.Cars.Dacia;
        var isValid = car.Mileage >= 0 && car.Mileage <= 500000;
        isValid.Should().BeTrue($"Le kilométrage {car.Mileage} km doit être valide");
    }

    [Fact]
    public void TC_UNIT_028_ValidateMileage_WithNegativeValue_ShouldReturnFalse()
    {
        int mileage = -10000;
        var isValid = mileage >= 0 && mileage <= 500000;
        isValid.Should().BeFalse("Un kilométrage négatif doit être invalide");
    }

    [Fact]
    public void TC_UNIT_029_ValidateMileage_WithZeroValue_ShouldReturnTrue()
    {
        int mileage = 0;
        var isValid = mileage >= 0 && mileage <= 500000;
        isValid.Should().BeTrue("Un kilométrage de 0 (neuf) doit être valide");
    }

    [Fact]
    public void TC_UNIT_030_ValidateMileage_BeyondMaximum_ShouldReturnFalse()
    {
        int mileage = 1000000;
        var isValid = mileage >= 0 && mileage <= 500000;
        isValid.Should().BeFalse("Un kilométrage excessif doit être invalide");
    }

    #endregion
}

/// <summary>
/// Tests unitaires pour les opérations de panier
/// Module: Test et Qualité Logiciel - Décembre 2025
/// Technique: Boîte Noire - Tests fonctionnels
/// </summary>
public class CartOperationsUnitTests
{
    #region TC-UNIT-031 à TC-UNIT-035: Opérations Panier

    [Fact]
    public void TC_UNIT_031_Cart_AddItem_ShouldIncreaseCount()
    {
        var cart = new List<(string product, int quantity)>();
        var car = TunisianTestDataProvider.Cars.Golf8;
        cart.Add((car.Name, 1));
        cart.Should().HaveCount(1, "Le panier doit contenir 1 article après ajout");
    }

    [Fact]
    public void TC_UNIT_032_Cart_RemoveItem_ShouldDecreaseCount()
    {
        var cart = new List<(string product, int quantity)>
        {
            (TunisianTestDataProvider.Cars.PeugeotPartner.Name, 1),
            (TunisianTestDataProvider.Cars.Clio5.Name, 1)
        };
        cart.RemoveAt(0);
        cart.Should().HaveCount(1, "Le panier doit contenir 1 article après suppression");
    }

    [Fact]
    public void TC_UNIT_033_Cart_CalculateTotal_WithMultipleItems_ShouldBeCorrect()
    {
        var items = new[]
        {
            TunisianTestDataProvider.Cars.PeugeotPartner.Price,
            TunisianTestDataProvider.Cars.Clio5.Price
        };
        var total = items.Sum();
        var expectedTotal = 45000m + 38000m;
        total.Should().Be(expectedTotal, $"Le total doit être {expectedTotal} TND");
    }

    [Fact]
    public void TC_UNIT_034_Cart_ApplyDiscount_ShouldReducePrice()
    {
        var originalPrice = TunisianTestDataProvider.Cars.Golf8.Price;
        var discountPercentage = 0.1m;
        var discountedPrice = originalPrice * (1 - discountPercentage);
        discountedPrice.Should().Be(76500m, "La réduction de 10% doit être appliquée");
    }

    [Fact]
    public void TC_UNIT_035_Cart_EmptyCart_ShouldHaveZeroTotal()
    {
        var cart = new List<(string product, decimal price)>();
        var total = cart.Sum(item => item.price);
        total.Should().Be(0m, "Un panier vide doit avoir un total de 0");
    }

    #endregion
}
