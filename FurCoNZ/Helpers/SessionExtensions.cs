using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SauceControl.Blake2Fast;

namespace FurCoNZ.Helpers
{
    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-2.2#session-state
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// Gets a hash of the current value stored in the session. Can be used to check if the value has changed since this method was last called.
        /// </summary>
        /// <param name="session">The ISession in which the value is stored.</param>
        /// <param name="key">The key the value is stored against for the session.</param>
        /// <returns>
        /// A hash of the current session key's value. This will change dramatically if any of the key's value changes.
        /// </returns>
        public static byte[] GetValueHash(this ISession session, string key)
        {
            var valueString = session.GetString(key);
            if (String.IsNullOrWhiteSpace(valueString)) return null;
            var valueBytes = Encoding.UTF8.GetBytes(valueString);
            byte[] computedHash = Blake2b.ComputeHash(valueBytes);
            return computedHash;
        }

        /// <summary>
        /// Compares the current value stored in the session with a hash of the expected value.
        /// </summary>
        /// <param name="session">The ISession in which the value is stored.</param>
        /// <param name="key">The key the value is stored against for the session.</param>
        /// <param name="hash">The expected hash of the value. See <seealso cref="GetValueHash(ISession, string)"/>.</param>
        /// <param name="secureValidation">
        /// If true, the comparison will take a fixed amount of time regardless of if the
        /// hashes match or not (this prevents some forms of attacks if the hash is considered sensitive).
        /// If false, this comparison is done as quick as possible. This may be a security risk in some contexts.
        /// </param>
        /// <returns>
        /// If true, the supplied hash matches the current value in the session storage.
        /// If false, the supplied hash does not match the current value in the session storage.
        /// </returns>
        public static bool ValidateValueHash(this ISession session, string key, ReadOnlySpan<byte> hash, bool secureValidation = true)
        {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key), "A valid session key must be supplied.");
            if (hash == null || hash.IsEmpty) throw new ArgumentNullException(nameof(hash), "A valid hash value must be supplied.");

            ReadOnlySpan<byte> computedHash = session.GetValueHash(key);

            if (secureValidation)
            {
                return CryptographicOperations.FixedTimeEquals(hash, computedHash);
            }
            else
            {
                return hash.SequenceEqual(computedHash);
            }
        }
    }
}
