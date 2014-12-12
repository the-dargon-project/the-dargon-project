using Dargon.InjectedModule;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.LeagueOfLegends.RADS;
using Dargon.LeagueOfLegends.Session;
using Dargon.Modifications;
using NMockito;
using System.Collections.Generic;
using Xunit;

namespace Dargon.LeagueOfLegends.Lifecycle
{
   public class LeagueLifecycleServiceImplTests : NMockitoInstance
   {
      private LeagueLifecycleServiceImpl testObj;

      [Mock] private readonly LeagueModificationRepositoryService leagueModificationRepositoryService = null;
      [Mock] private readonly InjectedModuleService injectedModuleService = null;
      [Mock] private readonly LeagueModificationResolutionService leagueModificationResolutionService = null;
      [Mock] private readonly LeagueModificationObjectCompilerService leagueModificationObjectCompilerService = null;
      [Mock] private readonly LeagueModificationCommandListCompilerService leagueModificationCommandListCompilerService = null;
      [Mock] private readonly LeagueGameModificationLinkerService leagueGameModificationLinkerService = null;
      [Mock] private readonly LeagueSessionService leagueSessionService = null;
      [Mock] private readonly RadsService radsService = null;
      [Mock] private readonly ILeagueInjectedModuleConfigurationFactory leagueInjectedModuleConfigurationFactory = null;
      
      [Mock] private readonly IModification firstModification = null;
      [Mock] private readonly IModification secondModification = null;
      private IEnumerable<IModification> modifications;

      public LeagueLifecycleServiceImplTests()
      {
         testObj = new LeagueLifecycleServiceImpl(injectedModuleService, leagueModificationRepositoryService, leagueModificationResolutionService, leagueModificationObjectCompilerService, leagueModificationCommandListCompilerService, leagueGameModificationLinkerService, leagueSessionService, radsService, leagueInjectedModuleConfigurationFactory);

         modifications = new[] { firstModification, secondModification };
         When(leagueModificationRepositoryService.EnumerateModifications()).ThenReturn(modifications);
      }

      [Fact]
      public void InitializeSubscribesToSessionServiceSessionCreatedTest()
      {
         testObj.Initialize();
         Verify(leagueSessionService).SessionCreated += testObj.HandleLeagueSessionCreated;
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void HandlePreclientProcessLaunchedTest()
      {
         const int processId = 13337;

         var preclientConfiguration = CreateUntrackedMock<IInjectedModuleConfiguration>();
         When(leagueInjectedModuleConfigurationFactory.GetPreclientConfiguration()).ThenReturn(preclientConfiguration);

         testObj.HandlePreclientProcessLaunched(processId);

         Verify(leagueInjectedModuleConfigurationFactory).GetPreclientConfiguration();
         Verify(injectedModuleService).InjectToProcess(processId, preclientConfiguration);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void HandleUninitializedToPreclientPhaseTransitionTest()
      {
         var firstResolutionTask = CreateMock<IResolutionTask>();
         var secondResolutionTask = CreateMock<IResolutionTask>();
         When(leagueModificationResolutionService.StartModificationResolution(firstModification, ModificationTargetType.Client)).ThenReturn(firstResolutionTask);
         When(leagueModificationResolutionService.StartModificationResolution(secondModification, ModificationTargetType.Client)).ThenReturn(secondResolutionTask);

         var firstCompilationTask = CreateMock<ICompilationTask>();
         var secondCompilationTask = CreateMock<ICompilationTask>();
         When(leagueModificationObjectCompilerService.CompileObjects(firstModification, ModificationTargetType.Client)).ThenReturn(firstCompilationTask);
         When(leagueModificationObjectCompilerService.CompileObjects(secondModification, ModificationTargetType.Client)).ThenReturn(secondCompilationTask);
         
         testObj.HandleUninitializedToPreclientPhaseTransition(CreateMock<ILeagueSession>(), new LeagueSessionPhaseChangedArgs(0, 0));

         Verify(leagueModificationRepositoryService, Times(1)).EnumerateModifications();
         Verify(leagueModificationResolutionService, Times(1), AfterPrevious()).StartModificationResolution(firstModification, ModificationTargetType.Client);
         Verify(leagueModificationResolutionService, Times(1), WithPrevious()).StartModificationResolution(secondModification, ModificationTargetType.Client);
         Verify(radsService, Times(1), AfterPrevious()).Suspend();
         Verify(firstResolutionTask, Times(1), AfterPrevious()).WaitForChainCompletion();
         Verify(secondResolutionTask, Times(1), WithPrevious()).WaitForChainCompletion();
         Verify(leagueModificationObjectCompilerService, Times(1), AfterPrevious()).CompileObjects(firstModification, ModificationTargetType.Client);
         Verify(leagueModificationObjectCompilerService, Times(1), WithPrevious()).CompileObjects(secondModification, ModificationTargetType.Client);
         Verify(firstCompilationTask, Times(1), AfterPrevious()).WaitForChainCompletion();
         Verify(secondCompilationTask, Times(1), WithPrevious()).WaitForChainCompletion();
         VerifyNoMoreInteractions();
      }
   }
}
