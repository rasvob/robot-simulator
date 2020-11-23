using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.BatchSimulation
{
    public class ExperimentRunner
    {
        public int Counter { get; set; } = 0;
        public event EventHandler<int> CounterUpdated;
        protected virtual void OnCounterUpdated(int e)
        {
            CounterUpdated?.Invoke(this, e);
        }

        public ExperimentConfig Config { get; set; }

        public ExperimentRunner(ExperimentConfig config)
        {
            Config = config;
        }

        public ExperimentResults RunExperiments()
        {
            throw new NotImplementedException();
        }
    }
}
