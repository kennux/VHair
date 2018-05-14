using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;

namespace UnityTK.Benchmarking
{
    /// <summary>
    /// Represents the result of running a benchmark (<see cref="Benchmark"/>).
    /// </summary>
    public class BenchmarkResult
    {
        /// <summary>
        /// Label data structure used for benchmark labeling.
        /// </summary>
        public class Label
        {
            public string label;
            public Stopwatch stopwatch;
            public Label parent = null;
            public List<Label> children = new List<Label>();

            public Label(string label)
            {
                this.label = label;
                this.stopwatch = Stopwatch.StartNew();
            }

            public void Stop()
            {
                this.stopwatch.Stop();
            }
        }

        /// <summary>
        /// The milliseconds result of the benchmark.
        /// This equals <see cref="totalMilliseconds"/> - <see cref="overheadMilliseconds"/>.
        /// </summary>
        public double milliseconds
        {
            get { return this.totalMilliseconds - this.overheadMilliseconds; }
        }

        /// <summary>
        /// The amount of time benchmarking took total.
        /// Measured in milliseconds.
        /// </summary>
        public double totalMilliseconds
        {
            get; private set;
        }

        /// <summary>
        /// The amount of time spent on processing overhead (this is not 100% accurate and more an approximation for micro benchmarks).
        /// Can be increased with
        /// </summary>
        public double overheadMilliseconds
        {
            get; private set;
        }

        #region Labels

        private Label currentLabel;

        /// <summary>
        /// Begins a new profiler label.
        /// </summary>
        /// <param name="label">The string for the label</param>
        public void BeginLabel(string label)
        {
            UnityEngine.Profiling.Profiler.BeginSample(label);
            Label lbl = new Label(label);
            if (!ReferenceEquals(currentLabel, null))
            {
                lbl.parent = currentLabel;
                currentLabel.children.Add(lbl);
                currentLabel = lbl;
            }
            currentLabel = lbl;
        }

        /// <summary>
        /// Ends a previously begun benchmarking label <see cref="BeginLabel(string)"/>
        /// </summary>
        public void EndLabel()
        {
            if (ReferenceEquals(this.currentLabel, null))
                throw new InvalidOperationException("Cannot end label when no label was begun.");

            // Stop
            this.currentLabel.Stop();

            // Walk up the label tree
            if (!ReferenceEquals(this.currentLabel.parent, null))
                this.currentLabel = this.currentLabel.parent;

            UnityEngine.Profiling.Profiler.EndSample();
        }

        #endregion

        /// <summary>
        /// Builds a report as human readable string for this benchmark.
        /// </summary>
        /// <returns>The report string.</returns>
        public string GetReportString()
        {
            StringBuilder labelsReport = new StringBuilder();
            _LabelsReportRecursive(labelsReport, this.currentLabel);

            return string.Format("Milliseconds: {0}\r\nTotal Milliseconds: {1}\r\nOverhead Milliseconds: {2}\r\nLabels:\r\n{3}", this.milliseconds, this.totalMilliseconds, this.overheadMilliseconds, labelsReport.ToString());
        }

        private void _LabelsReportRecursive(StringBuilder labelsReport, Label label, int depth = 0)
        {
            for (int i = 0; i < depth; i++)
                labelsReport.Append("\t");
            labelsReport.AppendFormat("{0}: {1} ms", label.label, label.stopwatch.Elapsed.TotalMilliseconds);
            labelsReport.AppendLine();

            // Children recursion
            foreach (var _label in label.children)
            {
                _LabelsReportRecursive(labelsReport, _label, depth+1);
            }
        }

        private void _LabelReport(StringBuilder sb, Label label, int depth)
        {
        }

        /// <summary>
        /// Adds overhead time to this result.
        /// The overhead time is subtracted from <see cref="totalMilliseconds"/> to get 
        /// </summary>
        /// <param name="milliseconds">The milliseconds of overhead</param>
        public void AddOverhead(double milliseconds)
        {
            this.overheadMilliseconds += milliseconds;
        }

        /// <summary>
        /// Called in order to finish benchmarking.
        /// This will clear up all the benchmarking labels and set up <see cref="totalMilliseconds"/>
        /// </summary>
        public void Finish()
        {
            this.totalMilliseconds = this.currentLabel.stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
