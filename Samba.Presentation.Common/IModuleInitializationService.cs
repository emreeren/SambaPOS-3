using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;

namespace Samba.Presentation.Common
{
    public enum ModuleInitializationStage
    {
        PreInitialization,
        Initialization,
        PostInitialization,
        StartUp
    }

    public interface IStagedSequenceService<TStageEnum>
    {
        void RegisterForStage(Action action, TStageEnum stage);
    }


    public class StagedSequenceService<TStageEnum> : IStagedSequenceService<TStageEnum>
    {
        private readonly List<Action>[] _stages;

        public StagedSequenceService()
        {
            _stages = new List<Action>[NumberOfEnumValues()];

            for (int i = 0; i < _stages.Length; ++i)
            {
                _stages[i] = new List<Action>();
            }
        }

        public virtual void ProcessSequence()
        {
            foreach (var stage in _stages)
            {
                foreach (var action in stage)
                {
                    action();
                }
            }
        }

        public virtual void RegisterForStage(Action action, TStageEnum stage)
        {
            _stages[Convert.ToInt32(stage)].Add(action);
        }

        static int NumberOfEnumValues()
        {
            return typeof(TStageEnum).GetFields(BindingFlags.Public | BindingFlags.Static).Length;
        }
    }

    public interface IModuleInitializationService : IStagedSequenceService<ModuleInitializationStage>
    {
        void Initialize();
    }

    [Export(typeof(IModuleInitializationService))]
    public class ModuleInitializationService : StagedSequenceService<ModuleInitializationStage>, IModuleInitializationService
    {
        public virtual void Initialize()
        {
            base.ProcessSequence();
        }
    }
}
