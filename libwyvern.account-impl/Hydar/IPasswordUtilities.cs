namespace Dargon.Wyvern.Accounts.Hydar {
   public interface IPasswordUtilities {
      /// <summary>
      /// Creates a salted PBKDF2 hash of the password.
      /// </summary>
      /// <param name="password">The password to hash.</param>
      /// <returns>The hash of the password.</returns>
      string CreateSaltedHash(string password);

      /// <summary>
      /// Validates a password given a hash of the correct one.
      /// </summary>
      /// <param name="password">The password to check.</param>
      /// <param name="correctHash">A hash of the correct password.</param>
      /// <returns>True if the password is correct. False otherwise.</returns>
      bool ValidatePassword(string password, string correctHash);
   }
}