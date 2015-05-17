using System;
using System.Threading;
using System.Windows;
using Castle.DynamicProxy;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services.Client;
using DargonManager;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;
using Dargon.Services;
using Dargon.Services.Clustering.Host;
using Dargon.Services.Server;

namespace Dargon.Manager {
   /// <summary>
   /// Interaction logic for App.xaml
   /// </summary>
   public partial class App : Application {
      private void Application_Startup(object sender, StartupEventArgs e) {
         Console.WriteLine("Entered Application_Startup");
         new DargonManagerApplicationEgg().Start(null);
      }
   }
}