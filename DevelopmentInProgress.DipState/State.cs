using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DevelopmentInProgress.DipState
{
    /// <summary>
    /// Generic implementation of <see cref="State"/>.
    /// </summary>
    /// <typeparam name="T">The type of context for the state.</typeparam>
    public class State<T> : State
    {
        /// <summary>
        /// Initializes a new generic instance of the <see cref="State"/> class.
        /// </summary>
        /// <param name="context">Generic state context.</param>
        /// <param name="id">Identifier of the state.</param>
        /// <param name="name">Name of the state.</param>
        /// <param name="type">The type of state.</param>
        /// <param name="status">The status of the state.</param>
        public State(T context, int id = 0, string name = "", StateType type = StateType.Standard,
            StateStatus status = StateStatus.Uninitialised)
            : base(id, name, type, status)
        {
            Context = context;
        }

        /// <summary>
        /// Initializes a new generic instance of the <see cref="State"/> class.
        /// </summary>
        /// <param name="id">Identifier of the state.</param>
        /// <param name="name">Name of the state.</param>
        /// <param name="type">The type of state.</param>
        /// <param name="status">The status of the state.</param>
        public State(int id = 0, string name = "", StateType type = StateType.Standard,
            StateStatus status = StateStatus.Uninitialised)
            : base(id, name, type, status)
        {            
        }

        /// <summary>
        /// Generic state context.
        /// </summary>
        public new T Context { get; set; }
    }

    /// <summary>
    /// State is a class for maintaining state in a workflow and supports both asynchronous and synchronous operations.
    /// The state has entry actions, actions that get executed when status is changed, and exit actions.  
    /// It can also have sub states, states that can be transitioned to, dependant and dependency states. 
    /// </summary>
    public class State
    {
        private StateStatus status;

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        /// <param name="id">Identifier of the state.</param>
        /// <param name="name">Name of the state.</param>
        /// <param name="type">The type of state.</param>
        /// <param name="status">The status of the state.</param>
        public State(int id = 0, string name = "", StateType type = StateType.Standard, 
            StateStatus status = StateStatus.Uninitialised)
        {
            Id = id;
            Name = name;
            Type = type;
            this.status = status;            
            Transitions = new List<State>();
            SubStates = new List<State>();
            Actions = new List<StateAction>();
            Dependencies = new List<State>();
            Dependants = new List<StateDependant>();
            Log = new List<LogEntry>();
        }

        /// <summary>
        /// Gets or sets the state identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the state name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the status of the state has changed.
        /// </summary>
        public bool IsDirty { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the state will be initialised when its parent is initialised.
        /// </summary>
        public bool InitialiseWithParent { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether completion is required in order for its parent to complete.
        /// </summary>
        public bool CompletionRequired { get; set; }

        /// <summary>
        /// Gets or sets the state context.
        /// </summary>
        public object Context { get; set; }

        /// <summary>
        /// Gets of sets the type of state.
        /// </summary>
        public StateType Type { get; set; }

        /// <summary>
        /// Gets or sets the state's parent.
        /// </summary>
        public State Parent { get; set; }

        /// <summary>
        /// Gets or sets the antecedent of a state i.e. the preceding state in a workflow from which it was transitioned from.
        /// </summary>
        public State Antecedent { get; set; }

        /// <summary>
        /// Gets or sets the state to be transitioned to.
        /// </summary>
        public State Transition { get; set; }

        /// <summary>
        /// Gets a list of states that the state can be transitioned to.
        /// </summary>
        public List<State> Transitions { get; private set; }

        /// <summary>
        /// Gets a list of states that the state is dependent on being completed before the state can be entered.
        /// The states dependencies are checked at the point of initialising that state i.e. when it is transitioned to.
        /// </summary>
        public List<State> Dependencies { get; private set; }

        /// <summary>
        /// Gets a list of states that are dependent on this state before they can be initialised.
        /// </summary>
        public List<StateDependant> Dependants { get; private set; }

        /// <summary>
        /// A list of substates belong to the state.
        /// </summary>
        public List<State> SubStates { get; private set; }

        /// <summary>
        /// A list of actions that are executed at different points in the lifecycle of the state e.g. on entry, when status changed and on exit.
        /// </summary>
        public List<StateAction> Actions { get; private set; }

        /// <summary>
        /// The state's log.
        /// </summary>
        public List<LogEntry> Log { get; private set; }

        /// <summary>
        /// Gets or sets the state's status. When the status is changed the <see cref="IsDirty"/> flag is set to true
        /// and an entry written to the state's log.
        /// </summary>
        public StateStatus Status
        {
            get { return status; }
            set
            {
                if (status != value)
                {
                    status = value;
                    IsDirty = true;

                    var logEntry = new LogEntry(String.Format("{0} - {1}", Name ?? String.Empty, status));
                    Log.Add(logEntry);

                    #if DEBUG

                    Debug.WriteLine(logEntry);

                    #endif
                }
            }
        }

        /// <summary>
        /// Gets or sets a predicate delegate to synchronously determine whether the state can be initialised or not.
        /// </summary>
        internal Predicate<State> CanInitialiseState { get; set; }

        /// <summary>
        /// Gets or sets a predicate delegate to synchronously determine whether the state status can be changed or not.
        /// </summary>
        internal Predicate<State> CanChangeStateStatus { get; set; }

        /// <summary>
        /// Gets or sets a predicate delegate to synchronously determine whether the state can be completed or not.
        /// </summary>
        internal Predicate<State> CanCompleteState { get; set; }

        /// <summary>
        /// Gets or sets a predicate delegate to synchronously determine whether the state can be reset or not.
        /// </summary>
        internal Predicate<State> CanResetState { get; set; }

        /// <summary>
        /// Gets or sets a predicate delegate to asynchronously determine whether the state can be initialised or not.
        /// </summary>
        internal Func<State, Task<bool>> CanInitialiseStateAsync { get; set; }

        /// <summary>
        /// Gets or sets a predicate delegate to asynchronously determine whether the state status can be changed or not.
        /// </summary>
        internal Func<State, Task<bool>> CanChangeStateStatusAsync { get; set; }

        /// <summary>
        /// Gets or sets a predicate delegate to asynchronously determine whether the state can be completed or not.
        /// </summary>
        internal Func<State, Task<bool>> CanCompleteStateAsync { get; set; }

        /// <summary>
        /// Gets or sets a predicate delegate to asynchronously determine whether the state can be reset or not.
        /// </summary>
        internal Func<State, Task<bool>> CanResetStateAsync { get; set; }
    }
}