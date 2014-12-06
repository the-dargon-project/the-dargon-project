using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dargon.Draek.Identities.Dtos;

namespace Dargon.Draek.Identities {
   public interface AuthenticationTokenFactory {
      string CreateToken(string tokenPrefix);
   }
}
