using System;
using System.Collections.Generic;
using System.Linq;
using Extreme.Cartesian.Project;
using Extreme.Parallel;

namespace Extreme.Cartesian.Forward
{
    public class SimpleParallelManager
    {
        private readonly Mpi _mpi;
        private readonly ExtremeProject _project;

        private enum Tag
        {
            Command = 1,
            TaskData = 2,
        }

        private enum Command
        {
            StartTask = 1,
            Stop = 2,

            TaskIsComplete = 666,
        }

        public event EventHandler<ParallelTask> StartNewTask;

        protected virtual void OnStartNewTask(ParallelTask e)
        {
            var handler = StartNewTask;

            if (handler != null)
                handler(this, e);
        }

        public SimpleParallelManager(Mpi mpi, ExtremeProject project)
        {
            _mpi = mpi;
            _project = project;
        }

        private Mpi Mpi
        {
            get { return _mpi; }
        }

        private ExtremeProject Project
        {
            get { return _project; }
        }

        public void Run()
        {
            if (Mpi.IsMaster)
            {
                var tasks = PrepareTasks();
                RunTasks(tasks);
            }
            else
            {
                WaitForParallelCommand();
            }
        }

        private IReadOnlyCollection<ParallelTask> PrepareTasks()
            => Project.Frequencies.Select(frequency
                => ParallelTask.NewFrequencyTask(frequency, 1)).ToList();

        private void RunTasks(IReadOnlyCollection<ParallelTask> tasks)
        {
            var rankRange = Enumerable.Range(1, Mpi.Size - 1).ToList();

            var availableMpiProcesses = new Queue<int>(rankRange);
            var inWork = new List<int>();

            foreach (var task in tasks)
            {
                if (availableMpiProcesses.Count == 0)
                    WaitForFreeProcess(availableMpiProcesses);

                var rank = availableMpiProcesses.Dequeue();

                SendCommandTo(rank, Command.StartTask);
                SendTaskTo(rank, task);
                inWork.Add(rank);
            }

            foreach (var rank in rankRange)
                SendCommandTo(rank, Command.Stop);
        }

        private void WaitForParallelCommand()
        {
            // Mu0-ha-ha-ha
            while (true)
            {
                switch (RecvCommandFromMaster())
                {
                    case Command.StartTask:
                        {
                            var task = RecvTaskFromMaster();
                            OnStartNewTask(task);
                            break;
                        }

                    case Command.Stop:
                        {
                            return;
                        }

                    default:
                        throw new InvalidOperationException();
                }

                SendCompleteCommandToMaster();
            }
        }

        private void WaitForFreeProcess(Queue<int> availableMpiProcesses)
        {
            int rank = RecvCompleteCommandFromSlave();

            availableMpiProcesses.Enqueue(rank);
        }

        private void SendCompleteCommandToMaster()
        {
            SendCommandTo(Mpi.Master, Command.TaskIsComplete);
        }

        private void SendCommandTo(int rank, Command command)
        {
            Mpi.Send((int)command, rank, (int)Tag.Command, Mpi.CommWorld);
        }

        private void SendTaskTo(int rank, ParallelTask task)
        {
            Mpi.Send(task.PolarizationIndex, rank, (int)Tag.TaskData, Mpi.CommWorld);
            Mpi.Send(task.Frequency, rank, (int)Tag.TaskData, Mpi.CommWorld);
        }

        private Command RecvCommandFromMaster()
        {
            return (Command)Mpi.RecvInt(Mpi.Master, (int)Tag.Command, Mpi.CommWorld);
        }
        private int RecvCompleteCommandFromSlave()
        {
            int source;
            int command = Mpi.RecvInt(Mpi.AnySource, (int)Tag.Command, Mpi.CommWorld, out source);

            if (command == (int)Command.TaskIsComplete)
                return source;

            throw new InvalidOperationException();
        }

        private ParallelTask RecvTaskFromMaster()
        {
            var polarization = Mpi.RecvInt(Mpi.Master, (int)Tag.TaskData, Mpi.CommWorld);
            var frequency = Mpi.RecvDouble(Mpi.Master, (int)Tag.TaskData, Mpi.CommWorld);

            return ParallelTask.NewFrequencyTask(frequency, polarization);
        }
    }
}
