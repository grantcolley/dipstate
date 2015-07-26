using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DevelopmentInProgress.DipState
{
    /// <summary>
    /// Generic implementation of <see cref="DipState"/>.
    /// </summary>
    /// <typeparam name="T">The type of context for the state.</typeparam>
    public class DipState<T> : DipState
    {
        /// <summary>
        /// Initializes a new generic instance of the <see cref="DipState"/> class.
        /// </summary>
        /// <param name="context">Generic state context.</param>
        /// <param name="id">Identifier of the state.</param>
        /// <param name="name">Name of the state.</param>
        /// <param name="initialiseWithParent">Indicates whether the state is initialised when its parent gets initialised.</param>
        /// <param name="canCompleteParent">Indicates whether the state's parent is completed when the state completes.</param>
        /// <param name="type">The type of state.</param>
        /// <param name="status">The status of the state.</param>
        /// <param name="canComplete">A predicate which is executed synchronously to determine whether the state can be completed i.e. passes validation.</param>
        /// <param name="canCompleteAsync">A predicate which is executed asynchronousl to determine whether the state can be completed e.g. it passes validation.</param>
        public DipState(T context, int id = 0, string name = "", bool initialiseWithParent = false,
            bool canCompleteParent = false, DipStateType type = DipStateType.Standard,
            DipStateStatus status = DipStateStatus.Uninitialised, Predicate<DipState> canComplete = null,
            Func<DipState, Task<bool>> canCompleteAsync = null)
            : base(id, name, initialiseWithParent, canCompleteParent, type, status, canComplete, canCompleteAsync)
        {
            Context = context;
        }

        /// <summary>
        /// Initializes a new generic instance of the <see cref="DipState"/> class.
        /// </summary>
        /// <param name="id">Identifier of the state.</param>
        /// <param name="name">Name of the state.</param>
        /// <param name="initialiseWithParent">Indicates whether the state is initialised when its parent gets initialised.</param>
        /// <param name="canCompleteParent">Indicates whether the state's parent is completed when the state completes.</param>
        /// <param name="type">The type of state.</param>
        /// <param name="status">The status of the state.</param>
        /// <param name="canComplete">A predicate which is executed synchronously to determine whether the state can be completed i.e. passes validation.</param>
        /// <param name="canCompleteAsync">A predicate which is executed asynchronousl to determine whether the state can be completed e.g. it passes validation.</param>
        public DipState(int id = 0, string name = "", bool initialiseWithParent = false,
            bool canCompleteParent = false, DipStateType type = DipStateType.Standard,
            DipStateStatus status = DipStateStatus.Uninitialised, Predicate<DipState> canComplete = null,
            Func<DipState, Task<bool>> canCompleteAsync = null)
            : base(id, name, initialiseWithParent, canCompleteParent, type, status, canComplete, canCompleteAsync)
        {            
        }

        /// <summary>
        /// Generic state context.
        /// </summary>
        public new T Context { get; set; }
    }

    /// <summary>
    /// DipState is a class for maintaining state in a workflow and supports both asynchronous and synchronous operations.
    /// The state has entry actions, actions that get executed when status is changed, and exit actions.  
    /// It can also have sub states, states that can be transitioned to, dependant and dependency states. 
    /// </summary>
    public class DipState
    {
        private readonly Predicate<DipState> canComplete;
        private readonly Func<DipState, Task<bool>> canCompleteAsync;
        private DipStateStatus status;

        /// <summary>
        /// Initializes a new instance of the <see cref="DipState"/> class.
        /// </summary>
        /// <param name="id">Identifier of the state.</param>
        /// <param name="name">Name of the state.</param>
        /// <param name="initialiseWithParent">Indicates whether the state is initialised when its parent gets initialised.</param>
        /// <param name="canCompleteParent">Indicates whether the state's parent is completed when the state completes.</param>
        /// <param name="type">The type of state.</param>
        /// <param name="status">The status of the state.</param>
        /// <param name="canComplete">A predicate which is executed synchronously to determine whether the state can be completed i.e. passes validation.</param>
        /// <param name="canCompleteAsync">A predicate which is executed asynchronousl to determine whether the state can be completed e.g. it passes validation.</param>
        public DipState(int id = 0, string name = "", bool initialiseWithParent = false, 
            bool canCompleteParent = false, DipStateType type = DipStateType.Standard, 
            DipStateStatus status = DipStateStatus.Uninitialised, Predicate<DipState> canComplete = null,
            Func<DipState, Task<bool>> canCompleteAsync = null)
        {
            Id = id;
            Name = name;
            Type = type;
            InitialiseWithParent = initialiseWithParent;
            CanCompleteParent = canCompleteParent;
            this.status = status;            
            this.canComplete = canComplete;
            this.canCompleteAsync = canCompleteAsync;
            Transitions = new List<DipState>();
            SubStates = new List<DipState>();
            Actions = new List<DipStateAction>();
            Dependencies = new List<DipState>();
            Dependants = new List<DipStateDependant>();
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
        /// Gets or sets a flag indicating whether the state's parent is completed when the state is completed.
        /// </summary>
        public bool CanCompleteParent { get; set; }

        /// <summary>
        /// Gets or sets the state context.
        /// </summary>
        public object Context { get; set; }

        /// <summary>
        /// Gets of sets the type of state.
        /// </summary>
        public DipStateType Type { get; set; }

        /// <summary>
        /// Gets or sets the state's parent.
        /// </summary>
        public DipState Parent { get; set; }

        /// <summary>
        /// Gets or sets the antecedent of a state i.e. the preceding state in a workflow from which it was transitioned from.
        /// </summary>
        public DipState Antecedent { get; set; }

        /// <summary>
        /// Gets or sets the state to be transitioned to.
        /// </summary>
        public DipState Transition { get; set; }

        /// <summary>
        /// Gets a list of states that the state can be transitioned to.
        /// </summary>
        public List<DipState> Transitions { get; private set; }

        /// <summary>
        /// Gets a list of states that the state is dependent on being completed before the state can be entered.
        /// The states dependencies are checked at the point of initialising that state i.e. when it is transitioned to.
        /// </summary>
        public List<DipState> Dependencies { get; private set; }

        /// <summary>
        /// Gets a list of states that are dependent on this state before they can be initialised.
        /// </summary>
        public List<DipStateDependant> Dependants { get; private set; }

        /// <summary>
        /// A list of substates belong to the state.
        /// </summary>
        public List<DipState> SubStates { get; private set; }

        /// <summary>
        /// A list of actions that are executed at different points in the lifecycle of the state e.g. on entry, when status changed and on exit.
        /// </summary>
        public List<DipStateAction> Actions { get; private set; }

        /// <summary>
        /// The state's log.
        /// </summary>
        public List<LogEntry> Log { get; private set; }

        /// <summary>
        /// Gets or sets the state's status. When the status is changed the <see cref="IsDirty"/> flag is set to true
        /// and an entry written to the state's log.
        /// </summary>
        public DipStateStatus Status
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
        /// Executes a predicate synchronously to determine whether the state can complete.
        /// </summary>
        /// <returns>Returns true if the state can be completed, else returns false. If no predicate is provided returns true.</returns>
        public bool CanComplete()
        {
            return canComplete == null || canComplete(this);
        }

        /// <summary>
        /// Executes a predicate asynchronously to determine whether the state can complete.
        /// </summary>
        /// <returns>Returns true if the state can be completed, else returns false. If no predicate is provided returns true.</returns>
        public async Task<bool> CanCompleteAsync()
        {
            if (canCompleteAsync != null)
            {
                return await canCompleteAsync(this);
            }

            return true;
        }
    }
}