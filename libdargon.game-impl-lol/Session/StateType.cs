namespace Dargon.LeagueOfLegends.Session
{
   public enum StateType
   {
      /// <summary>
      /// No League of Legends processes have launched
      /// </summary>
      Unlaunched,

      /// <summary>
      /// RADS User Kernel has launched
      /// </summary>
      Launching,

      /// <summary>
      /// The user is currently at the League of Legends launcher.
      /// </summary>
      Launcher,

      /// <summary>
      /// The user is currently at the League of Legends launcher, and the game is patching.
      /// </summary>
      Patching,

      /// <summary>
      /// The user is currently at the League of Legends AIR Client.
      /// </summary>
      PvpNetClient,

      /// <summary>
      /// The user is currently in Riot Games Server game.
      /// </summary>
      OfficialGame,

      /// <summary>
      /// The user is currently in game, but the game did not start by the AIR Client; perhaps, a
      /// third party application like LoLReplay started the game?
      /// 
      /// We detect this event by the launching of the League of Legends game client when we aren't
      /// already connected to the AIR Client, as well as with some other factors (ie: presence of
      /// applications such as LoLReplay?)
      /// </summary>
      ThirdPartyGame
   }
}
