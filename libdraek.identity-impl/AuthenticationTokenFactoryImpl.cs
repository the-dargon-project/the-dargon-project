using System;
using System.Linq;
using Dargon.Draek.Identities.Dtos;

namespace Dargon.Draek.Identities {
   public class AuthenticationTokenFactoryImpl : AuthenticationTokenFactory {
      /// <summary>
      /// Creates a unique authentication token.
      /// http://stackoverflow.com/questions/14643735/how-to-generate-a-unique-token-which-expires-after-24-hours.
      /// </summary>
      /// <returns></returns>
      public string CreateToken(string tokenPrefix) {
         var time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
         var key = Guid.NewGuid().ToByteArray();
         return tokenPrefix + "_" + Convert.ToBase64String(time.Concat(key).ToArray());
      }
   }
}